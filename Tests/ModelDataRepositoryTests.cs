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
    /// Тесты репозитория БД. Тесты чтения и записи выполняются на копии рабочей БД, путь к которой
    /// задается переменной среды <c>VRDO_PDG_TEST_DATABASE</c>; без нее эти тесты пропускаются.
    /// Проверки не зависят от состава данных в БД.
    /// </summary>
    [TestFixture]
    public sealed class ModelDataRepositoryTests
    {
        /// <summary>
        /// Переменная среды с путем к рабочей БД.
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
        /// PQ-диаграмма генератора сохраняется и считывается по коду СКРМ.
        /// </summary>
        [Test]
        public void SavePQDiagram_GeneratorPoints_RoundTrips()
        {
            ModelDataRepository repository = OpenWorkingCopy();
            Device? generator = repository.GetDevices().FirstOrDefault(device => device.Type == DeviceType.Generator);
            if (generator == null)
            {
                Assert.Ignore("В рабочей БД нет генераторов");
            }

            repository.SavePQDiagram(generator!.Id, new[] { new PQPoint(0, -100, 150), new PQPoint(200, -60, 120) });

            PQDiagram diagram = repository.GetPQDiagrams()[generator.Id];

            Assert.That(diagram.Points, Has.Count.EqualTo(2));
            Assert.That(diagram.GetLimits(100).Max, Is.EqualTo(135).Within(1e-9));
        }

        /// <summary>
        /// Копирует рабочую БД во временный каталог и открывает копию. Без переменной среды
        /// с путем к рабочей БД тест пропускается.
        /// </summary>
        /// <returns>Репозиторий копии рабочей БД.</returns>
        private ModelDataRepository OpenWorkingCopy()
        {
            string? source = Environment.GetEnvironmentVariable(DatabaseVariable);
            if (string.IsNullOrWhiteSpace(source) || !File.Exists(source))
            {
                Assert.Ignore("Не задан путь к рабочей БД: переменная среды " + DatabaseVariable);
            }

            File.Copy(source!, _databasePath, true);
            return new ModelDataRepository(_databasePath);
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
