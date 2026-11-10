using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

using Microsoft.Data.Sqlite;

using NUnit.Framework;

using VrdoPdg.Infrastructure;
using VrdoPdg.Model;
using VrdoPdg.Model.Entities;
using VrdoPdg.Model.Storage;

namespace VrdoPdg.Tests
{
    /// <summary>
    /// Тесты репозитория БД. Тесты чтения и записи выполняются на копии тестовой БД
    /// Tests/TestData/VrdoPdg.db или рабочей БД, путь к которой задается переменной среды
    /// <c>VRDO_PDG_TEST_DATABASE</c>. Проверки не зависят от состава данных в БД.
    /// </summary>
    [TestFixture]
    public sealed class ModelDataRepositoryTests
    {
        /// <summary>
        /// Переменная среды с путем к рабочей БД, на копии которой выполняются тесты вместо тестовой БД.
        /// </summary>
        private const string DatabaseVariable = "VRDO_PDG_TEST_DATABASE";

        /// <summary>
        /// Каталог временных файлов теста.
        /// </summary>
        private string _directory = string.Empty;

        /// <summary>
        /// Путь к копии БД теста.
        /// </summary>
        private string _databasePath = string.Empty;

        /// <summary>
        /// Создает каталог временных файлов.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _directory = Path.Combine(Path.GetTempPath(), "vrdo_db_" + Path.GetRandomFileName());
            Directory.CreateDirectory(_directory);
            _databasePath = Path.Combine(_directory, "VrdoPdg.db");
        }

        /// <summary>
        /// Удаляет временные файлы.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            SqliteConnection.ClearAllPools();
            Directory.Delete(_directory, true);
        }

        /// <summary>
        /// Отсутствующий файл БД не создается, а сообщается об ошибке.
        /// </summary>
        [Test]
        public void CheckDatabase_MissingFile_ThrowsAndDoesNotCreateFile()
        {
            ModelDataRepository repository = new ModelDataRepository(_databasePath);

            VrdoException exception = Assert.Throws<VrdoException>(repository.CheckDatabase)!;

            Assert.That(exception.Message, Is.EqualTo("Не найден файл базы данных"));
            Assert.That(File.Exists(_databasePath), Is.False);
        }

        /// <summary>
        /// Файл без таблиц системы отклоняется с перечнем отсутствующих таблиц.
        /// </summary>
        [Test]
        public void CheckDatabase_EmptyFile_ThrowsWithMissingTables()
        {
            File.WriteAllBytes(_databasePath, new byte[0]);
            ModelDataRepository repository = new ModelDataRepository(_databasePath);

            VrdoException exception = Assert.Throws<VrdoException>(repository.CheckDatabase)!;

            Assert.That(exception.Message, Is.EqualTo("База данных не содержит таблиц системы"));
            Assert.That(exception.Details, Does.Contain("Control_point"));
        }

        /// <summary>
        /// Рабочая БД проходит проверку и содержит справочник энергосистем.
        /// </summary>
        [Test]
        public void CheckDatabase_WorkingDatabase_Succeeds()
        {
            ModelDataRepository repository = OpenWorkingCopy();

            Assert.DoesNotThrow(repository.CheckDatabase);
            Assert.That(repository.GetPowerSystems(), Is.Not.Empty);
        }

        /// <summary>
        /// КП считываются с узлами, основной узел входит в состав узлов КП.
        /// </summary>
        [Test]
        public void GetControlPoints_WorkingDatabase_ReturnsNodes()
        {
            ModelDataRepository repository = OpenWorkingCopy();

            IReadOnlyList<ControlPoint> controlPoints = repository.GetControlPoints();

            Assert.That(controlPoints, Is.Not.Empty);
            foreach (ControlPoint controlPoint in controlPoints)
            {
                Assert.That(controlPoint.Name, Is.Not.Empty);
                Assert.That(controlPoint.Nodes, Is.Not.Empty);
                if (controlPoint.MainNode != null)
                {
                    Assert.That(controlPoint.Nodes, Does.Contain(controlPoint.MainNode));
                }
            }
        }

        /// <summary>
        /// СКРМ считываются с номерами в МТ и видом коммутируемых СКРМ.
        /// </summary>
        [Test]
        public void GetDevices_WorkingDatabase_ReturnsDevicesWithNumbers()
        {
            ModelDataRepository repository = OpenWorkingCopy();

            IReadOnlyList<Device> devices = repository.GetDevices();

            Assert.That(devices, Is.Not.Empty);
            Assert.That(devices.All(device => device.Number > 0), Is.True);
            Assert.That(
                devices.Where(device => device.Type == DeviceType.Switched).All(device => device.SwitchedKind != null),
                Is.True);
        }

        /// <summary>
        /// Перестановка двух первых приоритетов сохраняется без нарушения уникальности,
        /// приоритеты по умолчанию (ПУР) не меняются.
        /// </summary>
        [Test]
        public void SaveEfficiencyTable_SwappedPriorities_KeepsDefaultPriorities()
        {
            ModelDataRepository repository = OpenWorkingCopy();
            IGrouping<int, EfficiencyEntry>? table = repository.GetEfficiencyEntries()
                .Where(entry => entry.Priority.HasValue)
                .GroupBy(entry => entry.ControlPointId)
                .FirstOrDefault(group => group.Count() >= 2);
            if (table == null)
            {
                Assert.Ignore("В рабочей БД нет таблицы эффективности с двумя СКРМ");
            }

            List<EfficiencyEntry> entries = table!.OrderBy(entry => entry.Priority).ToList();
            List<DevicePriority> priorities = entries
                .Select((entry, index) => new DevicePriority(entry.DeviceId, index + 1))
                .ToList();
            priorities[0] = new DevicePriority(entries[0].DeviceId, 2);
            priorities[1] = new DevicePriority(entries[1].DeviceId, 1);

            repository.SaveEfficiencyTable(table.Key, priorities);

            List<EfficiencyEntry> saved = repository.GetEfficiencyEntries()
                .Where(entry => entry.ControlPointId == table.Key && entry.Priority.HasValue)
                .ToList();
            Assert.That(saved, Has.Count.EqualTo(entries.Count));
            Assert.That(saved.Single(entry => entry.DeviceId == entries[0].DeviceId).Priority, Is.EqualTo(2));
            Assert.That(saved.Single(entry => entry.DeviceId == entries[1].DeviceId).Priority, Is.EqualTo(1));
            foreach (EfficiencyEntry entry in entries)
            {
                Assert.That(
                    saved.Single(item => item.DeviceId == entry.DeviceId).DefaultPriority,
                    Is.EqualTo(entry.DefaultPriority));
            }
        }

        /// <summary>
        /// Границы графиков сохраняются и заменяются при повторном сохранении.
        /// </summary>
        [Test]
        public void SaveVoltageSchedule_RepeatedSave_ReplacesBounds()
        {
            ModelDataRepository repository = OpenWorkingCopy();
            int controlPointId = repository.GetControlPoints().First().Id;
            DateTime receivedAt = new DateTime(2026, 10, 13, 9, 30, 0);
            repository.SaveVoltageSchedule(new[] { new VoltageBounds(controlPointId, 1, 500, 525, receivedAt) });
            repository.SaveVoltageSchedule(new[] { new VoltageBounds(controlPointId, 1, 505, 520, receivedAt) });

            VoltageBounds bounds = repository.GetVoltageSchedule()
                .Single(item => item.ControlPointId == controlPointId && item.Hour == 1);

            Assert.That(bounds.LowerBound, Is.EqualTo(505));
            Assert.That(bounds.UpperBound, Is.EqualTo(520));
            Assert.That(bounds.ReceivedAt, Is.EqualTo(receivedAt));
        }

        /// <summary>
        /// Данные МТ последней загрузки заменяют ранее сохраненные.
        /// </summary>
        [Test]
        public void SaveRegime_RepeatedSave_ReplacesData()
        {
            ModelDataRepository repository = OpenWorkingCopy();
            ControlPoint controlPoint = repository.GetControlPoints().First();
            int nodeNumber = controlPoint.Nodes[0].Number;
            Device device = repository.GetDevices().First();
            repository.SaveRegime(
                new[] { new NodeVoltage(nodeNumber, 1, 530), new NodeVoltage(nodeNumber, 2, 520) },
                new[] { new DeviceMode(device.Id, 1, true, -180, -180, 0, 525, null, true) });
            repository.SaveRegime(
                new[] { new NodeVoltage(nodeNumber, 1, 0) },
                new DeviceMode[0]);

            Assert.That(Scalar("SELECT COUNT(*) FROM Node_voltage"), Is.EqualTo(1));
            Assert.That(Scalar("SELECT COUNT(*) FROM Device_mode"), Is.Zero);
        }

        /// <summary>
        /// Путь к файлу БД обязателен.
        /// </summary>
        [Test]
        public void Constructor_EmptyPath_Throws()
        {
            Assert.Throws<ArgumentException>(() => new ModelDataRepository(" "));
        }

        /// <summary>
        /// Методы записи не принимают пустые перечни.
        /// </summary>
        [Test]
        public void SaveMethods_NullArguments_Throw()
        {
            ModelDataRepository repository = new ModelDataRepository(_databasePath);

            Assert.Throws<ArgumentNullException>(() => repository.SaveEfficiencyTable(1, null!));
            Assert.Throws<ArgumentNullException>(() => repository.SaveVoltageSchedule(null!));
            Assert.Throws<ArgumentNullException>(() => repository.SaveRegime(null!, new DeviceMode[0]));
            Assert.Throws<ArgumentNullException>(() => repository.SaveRegime(new NodeVoltage[0], null!));
        }

        /// <summary>
        /// СКРМ без строки подтипа и с неизвестным типом пропускаются, коммутируемые СКРМ
        /// считываются с видом «Реактор» или «БСК».
        /// </summary>
        [Test]
        public void GetDevices_DevicesWithoutSubtype_AreSkipped()
        {
            ModelDataRepository repository = OpenWorkingCopy();
            int count = repository.GetDevices().Count;
            Execute("INSERT INTO Device (id_dev, type_dev, name_dev) VALUES "
                + "(9001, 'Управляемые', 'без узла'), (9002, 'Коммутируемые', 'без реактора'), "
                + "(9003, 'Генераторы', 'без генератора'), (9004, 'Коммутируемые', 'БСК')");
            Execute("INSERT INTO Switched_device (id_dev, num_reactor, kind_sd) VALUES (9004, 9004, 'БСК')");
            Execute("PRAGMA ignore_check_constraints = ON; "
                + "INSERT INTO Device (id_dev, type_dev, name_dev) VALUES (9005, 'Неизвестные', 'тип')");

            IReadOnlyList<Device> devices = repository.GetDevices();

            Assert.That(devices, Has.Count.EqualTo(count + 1));
            Device capacitor = devices.Single(device => device.Id == 9004);
            Assert.That(capacitor.Type, Is.EqualTo(DeviceType.Switched));
            Assert.That(capacitor.SwitchedKind, Is.EqualTo(SwitchedDeviceKind.CapacitorBank));
            Assert.That(capacitor.Number, Is.EqualTo(9004));
        }

        /// <summary>
        /// СКРМ, исключенное из таблицы эффективности, теряет действующий приоритет: строка
        /// с приоритетом по умолчанию сохраняется, остальные удаляются; добавленное СКРМ
        /// сохраняется без приоритета по умолчанию.
        /// </summary>
        [Test]
        public void SaveEfficiencyTable_RemovedDevices_KeepOnlyDefaultPriorities()
        {
            ModelDataRepository repository = OpenWorkingCopy();
            Execute("INSERT INTO Control_point (id_cp, id_ps, name_cp) VALUES (9001, 1, 'Тестовый КП')");
            Execute("INSERT INTO Efficiency_table (id_cp, id_dev, priority, priority_pur) VALUES "
                + "(9001, 1, 1, 1), (9001, 2, 2, NULL), (9001, 3, 3, 2)");

            repository.SaveEfficiencyTable(9001, new[] { new DevicePriority(3, 1), new DevicePriority(4, 2) });

            List<EfficiencyEntry> saved = repository.GetEfficiencyEntries()
                .Where(entry => entry.ControlPointId == 9001)
                .OrderBy(entry => entry.DeviceId)
                .ToList();
            Assert.That(saved, Has.Count.EqualTo(3));
            Assert.That(saved[0].DeviceId, Is.EqualTo(1));
            Assert.That(saved[0].Priority, Is.Null);
            Assert.That(saved[0].DefaultPriority, Is.EqualTo(1));
            Assert.That(saved[1].DeviceId, Is.EqualTo(3));
            Assert.That(saved[1].Priority, Is.EqualTo(1));
            Assert.That(saved[1].DefaultPriority, Is.EqualTo(2));
            Assert.That(saved[2].DeviceId, Is.EqualTo(4));
            Assert.That(saved[2].Priority, Is.EqualTo(2));
            Assert.That(saved[2].DefaultPriority, Is.Null);
        }

        /// <summary>
        /// Дата получения графика в формате, отличном от формата системы, разбирается,
        /// неразборчивая дата заменяется минимальной.
        /// </summary>
        [Test]
        public void GetVoltageSchedule_OtherDateFormats_AreParsed()
        {
            ModelDataRepository repository = OpenWorkingCopy();
            int controlPointId = repository.GetControlPoints().First().Id;
            Execute("INSERT INTO Voltage_schedule (id_cp, hour, u_low, u_high, received_at) VALUES "
                + "(" + controlPointId + ", 1, 500, 525, '2026-10-13T09:30:00'), "
                + "(" + controlPointId + ", 2, 500, 525, 'вчера')");

            List<VoltageBounds> schedule = repository.GetVoltageSchedule()
                .Where(item => item.ControlPointId == controlPointId)
                .ToList();

            Assert.That(schedule[0].ReceivedAt, Is.EqualTo(new DateTime(2026, 10, 13, 9, 30, 0)));
            Assert.That(schedule[1].ReceivedAt, Is.EqualTo(DateTime.MinValue));
        }

        /// <summary>
        /// Режимы СКРМ сохраняются со всеми значениями, в том числе пустыми и признаками.
        /// </summary>
        [Test]
        public void SaveRegime_Modes_AreSavedWithFlags()
        {
            ModelDataRepository repository = OpenWorkingCopy();
            Device device = repository.GetDevices().First();
            repository.SaveRegime(
                new NodeVoltage[0],
                new[]
                {
                    new DeviceMode(device.Id, 1, true, -180, -180, 0, 525, null, true),
                    new DeviceMode(device.Id, 2, false, null, null, null, null, 120, false)
                });

            Assert.That(Scalar("SELECT COUNT(*) FROM Device_mode WHERE is_on = 1 AND is_fixed = 1"), Is.EqualTo(1));
            Assert.That(Scalar("SELECT COUNT(*) FROM Device_mode WHERE is_on = 0 AND q_dev IS NULL"), Is.EqualTo(1));
        }

        /// <summary>
        /// Копирует БД во временный каталог и открывает копию: рабочую БД, если задана переменная
        /// среды с путем к ней, иначе тестовую БД проекта тестов.
        /// </summary>
        /// <returns>Репозиторий копии БД.</returns>
        private ModelDataRepository OpenWorkingCopy()
        {
            string? source = Environment.GetEnvironmentVariable(DatabaseVariable);
            if (string.IsNullOrWhiteSpace(source))
            {
                source = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "VrdoPdg.db");
            }

            File.Copy(source, _databasePath, true);
            return new ModelDataRepository(_databasePath);
        }

        /// <summary>
        /// Выполняет запрос к копии БД, не возвращающий данных.
        /// </summary>
        /// <param name="sql">Текст запроса.</param>
        private void Execute(string sql)
        {
            using (SqliteConnection connection = new SqliteConnection("Data Source=" + _databasePath))
            {
                connection.Open();
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = sql;
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Выполняет запрос к копии БД, возвращающий число.
        /// </summary>
        /// <param name="sql">Текст запроса.</param>
        /// <returns>Результат запроса.</returns>
        private long Scalar(string sql)
        {
            using (SqliteConnection connection = new SqliteConnection("Data Source=" + _databasePath))
            {
                connection.Open();
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = sql;
                    return Convert.ToInt64(command.ExecuteScalar(), CultureInfo.InvariantCulture);
                }
            }
        }
    }
}
