using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using VrdoPdg.Model.Entities;
using VrdoPdg.Model.Regime;
using VrdoPdg.Model.Services;

namespace VrdoPdg.Tests
{
    /// <summary>
    /// Тесты сопоставления строк таблицы эффективности со СКРМ БД и загруженной МТ
    /// </summary>
    [TestFixture]
    public sealed class EfficiencyDeviceResolverTests
    {
        /// <summary>
        /// Справочник СКРМ из БД: генератор хранится номером генератора (Num)
        /// </summary>
        private static readonly IReadOnlyList<Device> _devices = new[]
        {
            new Device(1, DeviceType.Controlled, "СТК-1 ПС 500 кВ Рубцовская", 6269, null),
            new Device(2, DeviceType.Switched, "Р-500 №1 Рубцовская", 7, SwitchedDeviceKind.Reactor),
            new Device(3, DeviceType.Generator, "Саяно-Шушенская ГЭС, Г1", 291, null),
            new Device(4, DeviceType.Generator, "Саяно-Шушенская ГЭС, Г9", 299, null)
        };

        /// <summary>
        /// Перечень объектов МТ: генераторы 291 и 292 подключены к узлу 6001, генератор 299 — к узлу 6009
        /// </summary>
        private static readonly MegapointCatalog _catalog = new MegapointCatalog(
            new Dictionary<int, string> { { 6269, "Рубцовская 500" }, { 6270, "Рубцовская 220" }, { 6001, "СШГЭС Г1-2" } },
            new Dictionary<int, string> { { 7, "Р-500 №1" }, { 8, "БСК-2 220 кВ" }, { 9, string.Empty } },
            new[]
            {
                new MegapointGenerator(291, 6001, "СШГЭС Г1"),
                new MegapointGenerator(292, 6001, "СШГЭС Г2"),
                new MegapointGenerator(299, 6009, "СШГЭС Г9")
            });

        /// <summary>
        /// Генераторы выводятся номером узла подключения по МТ, остальные СКРМ — своим номером;
        /// без МТ генератор выводится номером генератора
        /// </summary>
        [Test]
        public void CreateRow_Generator_ShowsNodeNumber()
        {
            EfficiencyDeviceResolver resolver = new EfficiencyDeviceResolver(_devices, _catalog);
            EfficiencyDeviceResolver withoutMegapoint = new EfficiencyDeviceResolver(_devices, null);

            EfficiencyRow generator = resolver.CreateRow(3, _devices[2]);

            Assert.That(resolver.IsMegapointLoaded, Is.True);
            Assert.That(generator.DeviceNumber, Is.EqualTo(6001));
            Assert.That(generator.DeviceId, Is.EqualTo(3));
            Assert.That(resolver.CreateRow(1, _devices[0]).DeviceNumber, Is.EqualTo(6269));
            Assert.That(withoutMegapoint.IsMegapointLoaded, Is.False);
            Assert.That(withoutMegapoint.CreateRow(3, _devices[2]).DeviceNumber, Is.EqualTo(291));
            Assert.That(
                resolver.GetDisplayNumber(new Device(5, DeviceType.Generator, "нет в МТ", 400, null)),
                Is.EqualTo(400));
        }

        /// <summary>
        /// Строки с одним узлом генераторов сопоставляются разным генераторам узла: сначала генератору
        /// из БД, затем генератору МТ, которого нет в БД; лишняя строка — повтор СКРМ
        /// </summary>
        [Test]
        public void Resolve_SeveralGeneratorsAtNode_UsesEachGeneratorOnce()
        {
            EfficiencyDeviceResolver resolver = new EfficiencyDeviceResolver(_devices, _catalog);
            EfficiencyRow[] rows =
            {
                new EfficiencyRow(1, DeviceType.Generator, 6001),
                new EfficiencyRow(2, DeviceType.Generator, 6001),
                new EfficiencyRow(3, DeviceType.Generator, 6001)
            };
            List<EfficiencyValidationError> errors = new List<EfficiencyValidationError>();

            IReadOnlyList<EfficiencyDeviceMatch?> matches = resolver.Resolve(rows, errors);

            Assert.That(matches[0]!.Device, Is.SameAs(_devices[2]));
            Assert.That(matches[1]!.IsNew, Is.True);
            Assert.That(matches[1]!.Number, Is.EqualTo(292));
            Assert.That(matches[1]!.Name, Is.EqualTo("СШГЭС Г2"));
            Assert.That(matches[2], Is.Null);
            Assert.That(errors.Single().RowIndex, Is.EqualTo(2));
            Assert.That(errors.Single().Message, Is.EqualTo("СКРМ уже есть в таблице (строка 1)"));
        }

        /// <summary>
        /// Строка, загруженная из БД, сохраняет свой генератор, даже если первым в узле идет другой
        /// </summary>
        [Test]
        public void Resolve_RowFromDatabase_KeepsOwnGenerator()
        {
            Device second = new Device(6, DeviceType.Generator, "Саяно-Шушенская ГЭС, Г2", 292, null);
            EfficiencyDeviceResolver resolver = new EfficiencyDeviceResolver(
                _devices.Concat(new[] { second }).ToList(),
                _catalog);
            EfficiencyRow[] rows = { resolver.CreateRow(1, second), resolver.CreateRow(2, _devices[2]) };

            IReadOnlyList<EfficiencyDeviceMatch?> matches =
                resolver.Resolve(rows, new List<EfficiencyValidationError>());

            Assert.That(matches[0]!.Device, Is.SameAs(second));
            Assert.That(matches[1]!.Device, Is.SameAs(_devices[2]));
        }

        /// <summary>
        /// СКРМ, которого нет в БД, но есть в МТ, сопоставляется СКРМ МТ: наименование берется из МТ,
        /// вид коммутируемого СКРМ определяется по наименованию; пустое наименование заменяется номером
        /// </summary>
        [Test]
        public void Resolve_DevicesOnlyInMegapoint_CreatesNewDevices()
        {
            EfficiencyDeviceResolver resolver = new EfficiencyDeviceResolver(_devices, _catalog);
            EfficiencyRow[] rows =
            {
                new EfficiencyRow(1, DeviceType.Controlled, 6270),
                new EfficiencyRow(2, DeviceType.Switched, 8),
                new EfficiencyRow(3, DeviceType.Switched, 9),
                resolver.CreateRow(4, _devices[1])
            };

            IReadOnlyList<EfficiencyDeviceMatch?> matches =
                resolver.Resolve(rows, new List<EfficiencyValidationError>());

            Assert.That(matches[0]!.IsNew, Is.True);
            Assert.That(matches[0]!.Name, Is.EqualTo("Рубцовская 220"));
            Assert.That(matches[1]!.SwitchedKind, Is.EqualTo(SwitchedDeviceKind.CapacitorBank));
            Assert.That(matches[2]!.Name, Is.EqualTo("реактор 9"));
            Assert.That(matches[2]!.SwitchedKind, Is.EqualTo(SwitchedDeviceKind.Reactor));
            Assert.That(matches[3]!.Device, Is.SameAs(_devices[1]));
            Assert.That(resolver.GetName(rows[1]), Is.EqualTo("БСК-2 220 кВ"));
        }

        /// <summary>
        /// При загруженной МТ СКРМ, которых нет в МТ, отклоняются, даже если они есть в БД;
        /// без МТ СКРМ проверяются по БД
        /// </summary>
        [Test]
        public void Resolve_DevicesMissingInMegapoint_ReportsErrors()
        {
            Device absent = new Device(5, DeviceType.Controlled, "СТК-3", 7777, null);
            List<Device> devices = _devices.Concat(new[] { absent }).ToList();
            EfficiencyDeviceResolver resolver = new EfficiencyDeviceResolver(devices, _catalog);
            EfficiencyDeviceResolver withoutMegapoint = new EfficiencyDeviceResolver(devices, null);
            EfficiencyRow[] rows =
            {
                new EfficiencyRow(1, DeviceType.Controlled, 7777, 5),
                new EfficiencyRow(2, DeviceType.Switched, 99),
                new EfficiencyRow(3, DeviceType.Generator, 6270),
                new EfficiencyRow(4, DeviceType.Generator, null)
            };
            List<EfficiencyValidationError> errors = new List<EfficiencyValidationError>();
            List<EfficiencyValidationError> databaseErrors = new List<EfficiencyValidationError>();

            resolver.Resolve(rows, errors);
            withoutMegapoint.Resolve(rows, databaseErrors);

            Assert.That(errors.Select(error => error.Message), Is.EqualTo(new[]
            {
                "Узел 7777 не найден в МТ",
                "Реактор 99 не найден в МТ",
                "Генератор в узле 6270 не найден в МТ"
            }));
            Assert.That(databaseErrors.Select(error => error.Message), Is.EqualTo(new[]
            {
                "Реактор 99 не найден в БД",
                "Генератор 6270 не найден в БД"
            }));
            Assert.That(resolver.GetName(rows[0]), Is.Empty);
            Assert.That(resolver.GetName(rows[3]), Is.Empty);
        }
    }
}
