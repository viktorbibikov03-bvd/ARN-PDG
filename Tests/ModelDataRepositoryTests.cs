using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

using Microsoft.Data.Sqlite;

using NUnit.Framework;

using VrdoPdg.Infrastructure;
using VrdoPdg.Model.Entities;
using VrdoPdg.Model.Storage;

namespace VrdoPdg.Tests
{
    /// <summary>
    /// Тесты репозитория БД на скриптах создания БД и заполнения данных КП «Барнаульская».
    /// </summary>
    [TestFixture]
    public sealed class ModelDataRepositoryTests
    {
        /// <summary>
        /// Каталог временной БД теста.
        /// </summary>
        private string _directory = string.Empty;

        /// <summary>
        /// Путь к временной БД теста.
        /// </summary>
        private string _databasePath = string.Empty;

        /// <summary>
        /// Репозиторий временной БД.
        /// </summary>
        private ModelDataRepository _repository = null!;

        /// <summary>
        /// Создает БД и заполняет ее данными КП «Барнаульская».
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _directory = Path.Combine(Path.GetTempPath(), "vrdo_db_" + Path.GetRandomFileName());
            Directory.CreateDirectory(_directory);
            _databasePath = Path.Combine(_directory, "VrdoPdg.db");
            _repository = new ModelDataRepository(_databasePath);
            _repository.EnsureCreated();
            Execute(File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData",
                "fill_barnaulskaya.sql")));
        }

        /// <summary>
        /// Удаляет временную БД.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            SqliteConnection.ClearAllPools();
            Directory.Delete(_directory, true);
        }

        /// <summary>
        /// Новая БД содержит справочник энергосистем; повторный вызов не изменяет БД.
        /// </summary>
        [Test]
        public void EnsureCreated_NewDatabase_CreatesPowerSystems()
        {
            _repository.EnsureCreated();

            IReadOnlyList<PowerSystem> powerSystems = _repository.GetPowerSystems();

            Assert.That(powerSystems.Select(item => item.Name),
                Is.EqualTo(new[] { "ОЭС Сибири", "ЕЭС Казахстана", "Внешние ЭС" }));
        }

        /// <summary>
        /// КП «Барнаульская» содержит основной узел 564 и дополнительный узел 563.
        /// </summary>
        [Test]
        public void GetControlPoints_FilledDatabase_ReturnsNodes()
        {
            ControlPoint controlPoint = _repository.GetControlPoints().Single();

            Assert.That(controlPoint.Name, Is.EqualTo("Барнаульская"));
            Assert.That(controlPoint.Nodes.Select(node => node.Number), Is.EqualTo(new[] { 563, 564 }));
            Assert.That(controlPoint.MainNode!.Number, Is.EqualTo(564));
        }

        /// <summary>
        /// Справочник СКРМ содержит 4 управляемых СКРМ, 23 коммутируемых и 30 генераторов.
        /// </summary>
        [Test]
        public void GetDevices_FilledDatabase_ReturnsDevicesWithNumbers()
        {
            IReadOnlyList<Device> devices = _repository.GetDevices();

            Assert.That(devices.Count(device => device.Type == DeviceType.Controlled), Is.EqualTo(4));
            Assert.That(devices.Count(device => device.Type == DeviceType.Switched), Is.EqualTo(23));
            Assert.That(devices.Count(device => device.Type == DeviceType.Generator), Is.EqualTo(30));
            Device capacitor = devices.Single(device => device.Number == 125 && device.Type == DeviceType.Switched);
            Assert.That(capacitor.IsCapacitorBank, Is.True);
        }

        /// <summary>
        /// Перестановка приоритетов сохраняется без нарушения уникальности; исключенное СКРМ из ПУР
        /// остается с пустым приоритетом, а добавленное технологом СКРМ удаляется.
        /// </summary>
        [Test]
        public void SaveEfficiencyTable_ChangedPriorities_KeepsDefaultPriorities()
        {
            int controlPointId = _repository.GetControlPoints().Single().Id;
            List<EfficiencyEntry> entries = _repository.GetEfficiencyEntries()
                .OrderBy(entry => entry.Priority)
                .ToList();
            List<DevicePriority> priorities = new List<DevicePriority>
            {
                new DevicePriority(entries[1].DeviceId, 1),
                new DevicePriority(entries[0].DeviceId, 2)
            };
            for (int index = 2; index < entries.Count - 1; index++)
            {
                priorities.Add(new DevicePriority(entries[index].DeviceId, index + 1));
            }

            _repository.SaveEfficiencyTable(controlPointId, priorities);

            List<EfficiencyEntry> saved = _repository.GetEfficiencyEntries().ToList();
            Assert.That(saved, Has.Count.EqualTo(entries.Count));
            Assert.That(saved.Single(entry => entry.DeviceId == entries[1].DeviceId).Priority, Is.EqualTo(1));
            Assert.That(saved.Single(entry => entry.DeviceId == entries[0].DeviceId).Priority, Is.EqualTo(2));
            EfficiencyEntry removed = saved.Single(entry => entry.DeviceId == entries[entries.Count - 1].DeviceId);
            Assert.That(removed.Priority, Is.Null);
            Assert.That(removed.DefaultPriority, Is.EqualTo(entries.Count));
        }

        /// <summary>
        /// СКРМ, добавленное технологом и не входящее в ПУР, удаляется из таблицы при исключении.
        /// </summary>
        [Test]
        public void SaveEfficiencyTable_RemovedDeviceNotInDefaultTable_DeletesRow()
        {
            int controlPointId = _repository.GetControlPoints().Single().Id;
            List<EfficiencyEntry> entries = _repository.GetEfficiencyEntries()
                .OrderBy(entry => entry.Priority)
                .ToList();
            List<DevicePriority> priorities = entries
                .Select(entry => new DevicePriority(entry.DeviceId, entry.Priority.GetValueOrDefault()))
                .ToList();
            Execute("UPDATE Efficiency_table SET priority_pur = NULL WHERE id_dev = "
                + entries[0].DeviceId.ToString(CultureInfo.InvariantCulture));
            priorities.RemoveAt(0);
            for (int index = 0; index < priorities.Count; index++)
            {
                priorities[index] = new DevicePriority(priorities[index].DeviceId, index + 1);
            }

            _repository.SaveEfficiencyTable(controlPointId, priorities);

            Assert.That(
                _repository.GetEfficiencyEntries().Any(entry => entry.DeviceId == entries[0].DeviceId),
                Is.False);
        }

        /// <summary>
        /// Границы графиков сохраняются и заменяются при повторном сохранении.
        /// </summary>
        [Test]
        public void SaveVoltageSchedule_RepeatedSave_ReplacesBounds()
        {
            int controlPointId = _repository.GetControlPoints().Single().Id;
            DateTime receivedAt = new DateTime(2026, 10, 13, 9, 30, 0);
            _repository.SaveVoltageSchedule(new[] { new VoltageBounds(controlPointId, 1, 500, 525, receivedAt) });
            _repository.SaveVoltageSchedule(new[] { new VoltageBounds(controlPointId, 1, 505, 520, receivedAt) });

            VoltageBounds bounds = _repository.GetVoltageSchedule().Single();

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
            Device device = _repository.GetDevices().First();
            _repository.SaveRegime(
                new[] { new NodeVoltage(564, 1, 530), new NodeVoltage(564, 2, 520) },
                new[] { new DeviceMode(device.Id, 1, true, -180, -180, 0, 525, null, true) });
            _repository.SaveRegime(
                new[] { new NodeVoltage(563, 1, 0) },
                new DeviceMode[0]);

            Assert.That(Scalar("SELECT COUNT(*) FROM Node_voltage"), Is.EqualTo(1));
            Assert.That(Scalar("SELECT COUNT(*) FROM Device_mode"), Is.Zero);
        }

        /// <summary>
        /// PQ-диаграмма генератора сохраняется и считывается по коду СКРМ.
        /// </summary>
        [Test]
        public void SavePQDiagram_GeneratorPoints_RoundTrips()
        {
            Device generator = _repository.GetDevices().First(device => device.Type == DeviceType.Generator);
            _repository.SavePQDiagram(generator.Id, new[] { new PQPoint(0, -100, 150), new PQPoint(200, -60, 120) });

            PQDiagram diagram = _repository.GetPQDiagrams()[generator.Id];

            Assert.That(diagram.Points, Has.Count.EqualTo(2));
            Assert.That(diagram.GetLimits(100).Max, Is.EqualTo(135).Within(1e-9));
        }

        /// <summary>
        /// Выполняет SQL-скрипт во временной БД.
        /// </summary>
        /// <param name="sql">Текст скрипта.</param>
        private void Execute(string sql)
        {
            using (SqliteConnection connection = new SqliteConnection("Data Source=" + _databasePath))
            {
                connection.Open();
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = "PRAGMA foreign_keys = ON; " + sql;
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Выполняет запрос, возвращающий число.
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
