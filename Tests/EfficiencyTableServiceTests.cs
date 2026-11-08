using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using VrdoPdg.Model;
using VrdoPdg.Model.Entities;
using VrdoPdg.Model.Protocol;
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

            Assert.That(EfficiencyTableService.Validate(rows, _devices), Is.Empty);
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

            IReadOnlyList<EfficiencyValidationError> errors = EfficiencyTableService.Validate(rows, _devices);

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

            IReadOnlyList<EfficiencyValidationError> errors = EfficiencyTableService.Validate(rows, _devices);

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

            IReadOnlyList<EfficiencyValidationError> errors = EfficiencyTableService.Validate(rows, _devices);

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

            IReadOnlyList<EfficiencyValidationError> errors = EfficiencyTableService.Validate(rows, _devices);

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

            IReadOnlyList<EfficiencyRow> rows = service.GetDefaultTable(1);

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

            IReadOnlyList<EfficiencyRow> rows = service.GetActiveTable(1);

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

            VrdoException exception = Assert.Throws<VrdoException>(() => service.Save(controlPoint, rows))!;

            Assert.That(exception.Message, Is.EqualTo("Изменения не сохранены: данные некорректны"));
            Assert.That(repository.EfficiencyEntries, Has.Count.EqualTo(4));
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
