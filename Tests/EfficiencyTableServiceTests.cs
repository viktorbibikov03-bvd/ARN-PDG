using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using VrdoPdg.Model;
using VrdoPdg.Model.Entities;
using VrdoPdg.Model.Protocol;
using VrdoPdg.Model.Regime;
using VrdoPdg.Model.Services;
using VrdoPdg.Tests.Fakes;

namespace VrdoPdg.Tests
{
    /// <summary>
    /// Тесты проверки, сохранения и восстановления таблицы эффективности.
    /// </summary>
    [TestFixture]
    public sealed class EfficiencyTableServiceTests
    {
        /// <summary>
        /// Справочник СКРМ.
        /// </summary>
        private static readonly IReadOnlyList<Device> _devices = new[]
        {
            new Device(1, DeviceType.Controlled, "СТК-1 ПС 500 кВ Рубцовская", 6269, null),
            new Device(2, DeviceType.Switched, "Р-500 №1 Рубцовская", 7, SwitchedDeviceKind.Reactor),
            new Device(3, DeviceType.Switched, "Р-500 №2 Рубцовская", 8, SwitchedDeviceKind.Reactor),
            new Device(4, DeviceType.Generator, "ТГ-1 Барнаульская ТЭЦ-3", 18, null)
        };

        /// <summary>
        /// Сопоставление строк со СКРМ без загруженной МТ: СКРМ проверяются по БД
        /// </summary>
        private static readonly EfficiencyDeviceResolver _resolver = new EfficiencyDeviceResolver(_devices, null);

        /// <summary>
        /// Корректная таблица не содержит ошибок.
        /// </summary>
        [Test]
        public void Validate_CorrectTable_ReturnsNoErrors()
        {
            List<EfficiencyRow> rows = new List<EfficiencyRow>
            {
                new EfficiencyRow(1, DeviceType.Controlled, 6269),
                new EfficiencyRow(2, DeviceType.Switched, 7),
                new EfficiencyRow(3, DeviceType.Generator, 18)
            };

            Assert.That(EfficiencyTableService.Validate(rows, _resolver), Is.Empty);
        }

        /// <summary>
        /// Повторяющийся приоритет отмечается в обеих строках.
        /// </summary>
        [Test]
        public void Validate_DuplicatePriority_MarksBothRows()
        {
            List<EfficiencyRow> rows = new List<EfficiencyRow>
            {
                new EfficiencyRow(1, DeviceType.Controlled, 6269),
                new EfficiencyRow(3, DeviceType.Switched, 7),
                new EfficiencyRow(3, DeviceType.Switched, 8)
            };

            IReadOnlyList<EfficiencyValidationError> errors = EfficiencyTableService.Validate(rows, _resolver);

            Assert.That(errors.Select(error => error.RowIndex), Is.EquivalentTo(new[] { 1, 2 }));
            Assert.That(errors.All(error => error.Message == "Приоритет 3 повторяется"), Is.True);
        }

        /// <summary>
        /// СКРМ, которого нет в БД, отмечается ошибкой с номером реактора.
        /// </summary>
        [Test]
        public void Validate_UnknownReactor_ReportsReactorNotFound()
        {
            List<EfficiencyRow> rows = new List<EfficiencyRow>
            {
                new EfficiencyRow(1, DeviceType.Switched, 6299)
            };

            IReadOnlyList<EfficiencyValidationError> errors = EfficiencyTableService.Validate(rows, _resolver);

            Assert.That(errors, Has.Count.EqualTo(1));
            Assert.That(errors[0].RowIndex, Is.Zero);
            Assert.That(errors[0].Message, Is.EqualTo("Реактор 6299 не найден в БД"));
        }

        /// <summary>
        /// Пропуск в последовательности приоритетов — ошибка всей таблицы.
        /// </summary>
        [Test]
        public void Validate_PriorityGap_ReportsTableError()
        {
            List<EfficiencyRow> rows = new List<EfficiencyRow>
            {
                new EfficiencyRow(1, DeviceType.Controlled, 6269),
                new EfficiencyRow(3, DeviceType.Switched, 7)
            };

            IReadOnlyList<EfficiencyValidationError> errors = EfficiencyTableService.Validate(rows, _resolver);

            Assert.That(errors, Has.Count.EqualTo(1));
            Assert.That(errors[0].RowIndex, Is.EqualTo(EfficiencyValidationError.TableRowIndex));
        }

        /// <summary>
        /// Повторно добавленное СКРМ отмечается ошибкой.
        /// </summary>
        [Test]
        public void Validate_DuplicateDevice_ReportsDuplicate()
        {
            List<EfficiencyRow> rows = new List<EfficiencyRow>
            {
                new EfficiencyRow(1, DeviceType.Switched, 7),
                new EfficiencyRow(2, DeviceType.Switched, 7)
            };

            IReadOnlyList<EfficiencyValidationError> errors = EfficiencyTableService.Validate(rows, _resolver);

            Assert.That(errors, Has.Count.EqualTo(1));
            Assert.That(errors[0].RowIndex, Is.EqualTo(1));
        }

        /// <summary>
        /// Таблица по умолчанию содержит СКРМ согласно ПУР в порядке приоритетов ПУР.
        /// </summary>
        [Test]
        public void GetDefaultTable_ChangedTable_ReturnsTableByDefaultPriorities()
        {
            FakeModelDataRepository repository = CreateRepository();
            EfficiencyTableService service = new EfficiencyTableService(repository, new CalculationProtocol());

            IReadOnlyList<EfficiencyRow> rows = service.GetDefaultTable(1, service.CreateResolver(null));

            Assert.That(rows.Select(row => row.DeviceNumber), Is.EqualTo(new int?[] { 6269, 7, 8 }));
            Assert.That(rows.Select(row => row.Priority), Is.EqualTo(new int?[] { 1, 2, 3 }));
        }

        /// <summary>
        /// Действующая таблица упорядочена по действующему приоритету и не содержит исключенных СКРМ.
        /// </summary>
        [Test]
        public void GetActiveTable_ChangedTable_ReturnsActiveRowsByPriority()
        {
            FakeModelDataRepository repository = CreateRepository();
            EfficiencyTableService service = new EfficiencyTableService(repository, new CalculationProtocol());

            IReadOnlyList<EfficiencyRow> rows = service.GetActiveTable(1, service.CreateResolver(null));

            Assert.That(rows.Select(row => row.DeviceNumber), Is.EqualTo(new int?[] { 7, 6269, 18 }));
        }

        /// <summary>
        /// Некорректная таблица не сохраняется.
        /// </summary>
        [Test]
        public void Save_InvalidTable_ThrowsAndKeepsStoredTable()
        {
            FakeModelDataRepository repository = CreateRepository();
            EfficiencyTableService service = new EfficiencyTableService(repository, new CalculationProtocol());
            List<EfficiencyRow> rows = new List<EfficiencyRow> { new EfficiencyRow(1, DeviceType.Switched, 6299) };
            ControlPoint controlPoint = new ControlPoint(1, 1, "Рубцовская", new ControlPointNode[0]);

            VrdoException exception = Assert.Throws<VrdoException>(
                () => service.Save(controlPoint, rows, service.CreateResolver(null)))!;

            Assert.That(exception.Message, Is.EqualTo("Изменения не сохранены: данные некорректны"));
            Assert.That(repository.EfficiencyEntries, Has.Count.EqualTo(4));
        }

        /// <summary>
        /// Строки без приоритета и без номера СКРМ отмечаются ошибками.
        /// </summary>
        [Test]
        public void Validate_EmptyCells_ReturnsErrors()
        {
            List<EfficiencyRow> rows = new List<EfficiencyRow>
            {
                new EfficiencyRow(null, DeviceType.Controlled, 6269),
                new EfficiencyRow(0, DeviceType.Switched, 7),
                new EfficiencyRow(1, DeviceType.Switched, null)
            };

            IReadOnlyList<EfficiencyValidationError> errors = EfficiencyTableService.Validate(rows, _resolver);

            Assert.That(errors.Count(error => error.Message == "Не задан приоритет"), Is.EqualTo(2));
            Assert.That(
                errors.Single(error => error.RowIndex == 2).Message,
                Is.EqualTo("Не задан номер узла или реактора"));
        }

        /// <summary>
        /// Корректная таблица сохраняется с кодами СКРМ из БД, сохранение записывается в протокол;
        /// справочники и число СКРМ в действующих таблицах берутся из БД.
        /// </summary>
        [Test]
        public void Save_CorrectTable_SavesPrioritiesAndAddsProtocolEntry()
        {
            FakeModelDataRepository repository = CreateRepository();
            repository.ControlPoints.Add(new ControlPoint(1, 1, "Рубцовская", new ControlPointNode[0]));
            CalculationProtocol protocol = new CalculationProtocol();
            EfficiencyTableService service = new EfficiencyTableService(repository, protocol);
            List<EfficiencyRow> rows = new List<EfficiencyRow>
            {
                new EfficiencyRow(2, DeviceType.Switched, 8),
                new EfficiencyRow(1, DeviceType.Generator, 18)
            };

            service.Save(service.GetControlPoints()[0], rows, service.CreateResolver(null));

            Assert.That(repository.Devices, Has.Count.EqualTo(4));
            Assert.That(service.GetActiveDeviceCounts()[1], Is.EqualTo(2));
            Assert.That(
                service.GetActiveTable(1, service.CreateResolver(null)).Select(row => row.DeviceNumber),
                Is.EqualTo(new int?[] { 18, 8 }));
            Assert.That(
                protocol.GetEntries().Single().Message,
                Is.EqualTo("Таблица эффективности КП «Рубцовская» сохранена"));
            Assert.That(protocol.GetEntries().Single().Details, Is.EqualTo("СКРМ в таблице: 2"));
        }

        /// <summary>
        /// Таблицы для расчета содержат только СКРМ с действующим приоритетом, известные в справочнике.
        /// </summary>
        [Test]
        public void BuildActiveTables_UnknownDevices_AreSkipped()
        {
            EfficiencyEntry[] entries =
            {
                new EfficiencyEntry(1, 1, 2, 1),
                new EfficiencyEntry(1, 99, 1, null),
                new EfficiencyEntry(1, 3, null, 3)
            };

            IReadOnlyDictionary<int, IReadOnlyList<EfficiencyItem>> tables =
                EfficiencyTableService.BuildActiveTables(entries, _devices);

            EfficiencyItem item = tables[1].Single();
            Assert.That(item.Device.Id, Is.EqualTo(1));
            Assert.That(item.Priority, Is.EqualTo(2));
            Assert.That(item.DefaultPriority, Is.EqualTo(1));
        }

        /// <summary>
        /// Аргументы конструктора и КП сохраняемой таблицы обязательны.
        /// </summary>
        [Test]
        public void Constructor_NullArguments_Throw()
        {
            FakeModelDataRepository repository = CreateRepository();
            EfficiencyTableService service = new EfficiencyTableService(repository, new CalculationProtocol());

            Assert.Throws<ArgumentNullException>(() => new EfficiencyTableService(null!, new CalculationProtocol()));
            Assert.Throws<ArgumentNullException>(() => new EfficiencyTableService(repository, null!));
            Assert.Throws<ArgumentNullException>(
                () => service.Save(null!, new EfficiencyRow[0], service.CreateResolver(null)));
            Assert.Throws<ArgumentNullException>(() => service.Save(
                new ControlPoint(1, 1, "Рубцовская", new ControlPointNode[0]), new EfficiencyRow[0], null!));
            Assert.Throws<ArgumentNullException>(() => new EfficiencyDeviceResolver(null!, null));
        }

        /// <summary>
        /// СКРМ МТ, которых нет в БД, при сохранении таблицы добавляются в БД, генератор — номером
        /// генератора (Num); таблица выводит генератор номером узла подключения
        /// </summary>
        [Test]
        public void Save_DevicesFromMegapoint_AddsDevicesToDatabase()
        {
            FakeModelDataRepository repository = CreateRepository();
            CalculationProtocol protocol = new CalculationProtocol();
            EfficiencyTableService service = new EfficiencyTableService(repository, protocol);
            MegapointCatalog catalog = new MegapointCatalog(
                new Dictionary<int, string> { { 6269, "Рубцовская" }, { 6300, "Рубцовская 220" } },
                new Dictionary<int, string> { { 7, "Р-500 №1" } },
                new[] { new MegapointGenerator(313, 6400, "Бийская ТЭЦ, ТГ-9") });
            EfficiencyDeviceResolver resolver = service.CreateResolver(catalog);
            List<EfficiencyRow> rows = new List<EfficiencyRow>
            {
                new EfficiencyRow(1, DeviceType.Switched, 7),
                new EfficiencyRow(2, DeviceType.Controlled, 6300),
                new EfficiencyRow(3, DeviceType.Generator, 6400)
            };

            service.Save(new ControlPoint(1, 1, "Рубцовская", new ControlPointNode[0]), rows, resolver);

            Device generator = repository.Devices.Single(device => device.Type == DeviceType.Generator
                && device.Number == 313);
            Assert.That(generator.Name, Is.EqualTo("Бийская ТЭЦ, ТГ-9"));
            Assert.That(repository.Devices.Any(device => device.Type == DeviceType.Controlled
                && device.Number == 6300), Is.True);
            IReadOnlyList<EfficiencyRow> saved = service.GetActiveTable(1, service.CreateResolver(catalog));
            Assert.That(saved.Select(row => row.DeviceNumber), Is.EqualTo(new int?[] { 7, 6300, 6400 }));
            Assert.That(protocol.GetEntries().Single().Details, Does.Contain(
                "добавлены в БД из МТ: Рубцовская 220 (узел 6300), Бийская ТЭЦ, ТГ-9 (генератор 313)"));
        }

        /// <summary>
        /// Создает репозиторий с таблицей КП 1: действующие приоритеты отличаются от ПУР,
        /// реактор 8 исключен из действующей таблицы, генератор добавлен технологом.
        /// </summary>
        /// <returns>Репозиторий.</returns>
        private static FakeModelDataRepository CreateRepository()
        {
            FakeModelDataRepository repository = new FakeModelDataRepository();
            repository.Devices.AddRange(_devices);
            repository.EfficiencyEntries.Add(new EfficiencyEntry(1, 1, 2, 1));
            repository.EfficiencyEntries.Add(new EfficiencyEntry(1, 2, 1, 2));
            repository.EfficiencyEntries.Add(new EfficiencyEntry(1, 3, null, 3));
            repository.EfficiencyEntries.Add(new EfficiencyEntry(1, 4, 3, null));
            return repository;
        }
    }
}
