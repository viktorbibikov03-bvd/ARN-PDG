using System;
using System.IO;
using System.Linq;

using NUnit.Framework;

using VrdoPdg.Infrastructure;
using VrdoPdg.Model;
using VrdoPdg.Model.Entities;
using VrdoPdg.Model.Protocol;
using VrdoPdg.Model.Regime;
using VrdoPdg.Model.Services;
using VrdoPdg.Tests.Fakes;

namespace VrdoPdg.Tests
{
    /// <summary>
    /// Тесты сервиса загрузки данных с тестовой моделью режима.
    /// </summary>
    [TestFixture]
    public sealed class DataLoadServiceTests
    {
        /// <summary>
        /// Каталог временных файлов теста.
        /// </summary>
        private string _directory = string.Empty;

        /// <summary>
        /// Создает каталог временных файлов.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _directory = Path.Combine(Path.GetTempPath(), "vrdo_load_" + Path.GetRandomFileName());
            Directory.CreateDirectory(_directory);
        }

        /// <summary>
        /// Удаляет каталог временных файлов.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            Directory.Delete(_directory, true);
        }

        /// <summary>
        /// Загрузка формирует сеанс: режим по часам, границы 500–525 кВ, заданные в файле
        /// состояния реакторов; данные МТ сохраняются в БД.
        /// </summary>
        [Test]
        public void Load_ValidFiles_CreatesSessionAndSavesRegime()
        {
            RegulationScenario scenario = new RegulationScenario();
            string megapointPath = CreateFile("smzu_mega_act.mptsmz", string.Empty);
            string smpSmzuPath = CreateFile("smp_smzu.csv", "20;7;24;9;0;0;1;0;0\r\n04;1;24;907;908;0;1;0;0\r\n");
            DataLoadService service = CreateService(scenario);

            SessionData session = service.Load(megapointPath, smpSmzuPath);

            Assert.That(session.SmpSmzuDocument.Records, Has.Count.EqualTo(2));
            Assert.That(session.IsFixed(RegulationScenario.ReactorId, 6), Is.False);
            Assert.That(session.IsFixed(RegulationScenario.ReactorId, 7), Is.True);
            Assert.That(session.Schedule.GetBounds(RegulationScenario.ControlPointId, 24)!.UpperBound, Is.EqualTo(525));
            Assert.That(session.InitialRegime.GetVoltage(RegulationScenario.MainNode, 12), Is.EqualTo(515));
            ObservedNodeState mainNode = session.InitialRegime.GetObservedNode(RegulationScenario.MainNode, 12)!;
            Assert.That(mainNode.IsOn, Is.True);
            Assert.That(mainNode.Voltage, Is.EqualTo(515));
            Assert.That(
                session.InitialRegime.GetObservedNode(RegulationScenario.ControlledNode, 1)!.MinReactivePower,
                Is.EqualTo(-100));
            Assert.That(session.InitialRegime.GetObservedNode(RegulationScenario.MissingNode, 1), Is.Null);
            Assert.That(session.InitialRegime.GetObservedReactor(RegulationScenario.ReactorNumber, 1)!.IsOn, Is.False);
            Assert.That(
                session.GetGeneratorNode(RegulationScenario.GeneratorId),
                Is.EqualTo(RegulationScenario.GeneratorNode));
            Assert.That(scenario.Repository.SavedVoltages, Has.Count.EqualTo(48));
            Assert.That(scenario.Repository.SavedModes, Has.Count.EqualTo(96));
            Assert.That(scenario.Repository.SavedModes.Count(mode => mode.IsFixed), Is.EqualTo(18));
            Assert.That(
                scenario.Protocol.GetEntries().Last().Message,
                Is.EqualTo("Загрузка исходных данных завершена"));
        }

        /// <summary>
        /// Файл другого формата не загружается; в протокол записывается ошибка.
        /// </summary>
        [Test]
        public void Load_WrongExtension_ThrowsUnknownFormat()
        {
            RegulationScenario scenario = new RegulationScenario();
            string megapointPath = CreateFile("smzu_mega_act.txt", string.Empty);
            string smpSmzuPath = CreateFile("smp_smzu.csv", string.Empty);

            DataFileException exception = Assert.Throws<DataFileException>(
                () => CreateService(scenario).Load(megapointPath, smpSmzuPath))!;

            Assert.That(exception.Error, Is.EqualTo(DataFileError.UnknownFormat));
            Assert.That(scenario.Protocol.GetEntries().Last().Tag, Is.EqualTo(ProtocolTag.Error));
        }

        /// <summary>
        /// Отсутствующий файл приводит к ошибке «Не найден файл с исходными данными».
        /// </summary>
        [Test]
        public void Load_MissingFile_ThrowsNotFound()
        {
            RegulationScenario scenario = new RegulationScenario();
            string megapointPath = CreateFile("smzu_mega_act.mptsmz", string.Empty);

            DataFileException exception = Assert.Throws<DataFileException>(
                () => CreateService(scenario).Load(megapointPath, Path.Combine(_directory, "smp_smzu.csv")))!;

            Assert.That(exception.Message, Is.EqualTo("Не найден файл с исходными данными"));
        }

        /// <summary>
        /// При недоступности ОИК «СК-11» используются границы, сохраненные в БД,
        /// а в протокол записывается предупреждение.
        /// </summary>
        [Test]
        public void Load_CkUnavailable_UsesSavedBoundsAndWarns()
        {
            RegulationScenario scenario = new RegulationScenario();
            for (int hour = 1; hour <= 24; hour++)
            {
                scenario.Repository.Schedule.Add(new VoltageBounds(
                    RegulationScenario.ControlPointId, hour, 505, 520, new DateTime(2026, 10, 12, 18, 0, 0)));
            }

            string megapointPath = CreateFile("smzu_mega_act.mptsmz", string.Empty);
            string smpSmzuPath = CreateFile("smp_smzu.csv", string.Empty);
            DataLoadService service = new DataLoadService(
                scenario.Adapter,
                scenario.Repository,
                new UnavailableCkModel(),
                new SmpSmzuCsvReader(),
                scenario.Protocol,
                RegulationScenario.Observed);

            SessionData session = service.Load(megapointPath, smpSmzuPath);

            Assert.That(session.Schedule.GetBounds(RegulationScenario.ControlPointId, 1)!.UpperBound, Is.EqualTo(520));
            Assert.That(scenario.Protocol.GetEntries().Any(entry => entry.Tag == ProtocolTag.Warning
                && entry.Message == DataLoadService.ScheduleNotUpdatedMessage), Is.True);
        }

        /// <summary>
        /// Недостатки исходных данных записываются в протокол предупреждениями, загрузка
        /// продолжается: КП без основного узла, узлы и СКРМ, не найденные в МТ, несошедшийся
        /// час МТ, отсутствие графика КП и сохраненных границ.
        /// </summary>
        [Test]
        public void Load_IncompleteData_AddsWarnings()
        {
            RegulationScenario scenario = new RegulationScenario();
            scenario.Repository.ControlPoints.Add(
                new ControlPoint(2, 1, "Рубцовская", new[] { new ControlPointNode(7777, false) }));
            scenario.Repository.Devices.Add(new Device(70, DeviceType.Controlled, "СТК-2", 7001, null));
            scenario.Repository.Devices.Add(
                new Device(71, DeviceType.Switched, "Р-2", 7002, SwitchedDeviceKind.Reactor));
            scenario.Repository.Devices.Add(new Device(72, DeviceType.Generator, "ТГ-7", 7003, null));
            scenario.Adapter.DivergentHours.Add(5);
            string megapointPath = CreateFile("smzu_mega_act.mptsmz", string.Empty);
            string smpSmzuPath = CreateFile("smp_smzu.csv", "20;1;24;7002;0;0;1;0;0\r\n20;1;24;555;0;0;1;0;0\r\n");
            DataLoadService service = new DataLoadService(
                scenario.Adapter,
                scenario.Repository,
                new UnavailableCkModel(),
                new SmpSmzuCsvReader(),
                scenario.Protocol,
                RegulationScenario.Observed);

            SessionData session = service.Load(megapointPath, smpSmzuPath);

            string[] warnings = scenario.Protocol.GetEntries()
                .Where(entry => entry.Tag == ProtocolTag.Warning)
                .Select(entry => entry.Message + ": " + entry.Details)
                .ToArray();
            Assert.That(warnings, Does.Contain("Не задан основной узел КП: Рубцовская — КП не участвует в расчете"));
            Assert.That(warnings, Does.Contain("Узел КП не найден в МТ или отключен: Рубцовская: узел 7777"));
            Assert.That(warnings, Does.Contain("СКРМ не найдено в МТ: СТК-2 (узел 7001)"));
            Assert.That(warnings, Does.Contain("СКРМ не найдено в МТ: Р-2 (реактор 7002)"));
            Assert.That(warnings, Does.Contain("СКРМ не найдено в МТ: ТГ-7 (генератор 7003)"));
            Assert.That(
                warnings,
                Does.Contain("Час 5: расчет режима МТ не сошелся: значения напряжений часа могут быть недостоверны"));
            Assert.That(
                warnings,
                Does.Contain("Не задан график напряжения КП: Барнаульская — КП не участвует в расчете"));
            ProtocolEntry savedBounds = scenario.Protocol.GetEntries()
                .Single(entry => entry.Message == "Использованы границы графиков, сохраненные в БД");
            Assert.That(savedBounds.Details, Is.EqualTo("сохраненных границ нет"));
            Assert.That(session.FixedDeviceHours, Has.Count.EqualTo(24));
            Assert.That(session.GetGeneratorNode(72), Is.Null);
            Assert.That(session.PowerSystems, Has.Count.EqualTo(1));
            Assert.That(session.LoadedAt, Is.EqualTo(DateTime.Now).Within(TimeSpan.FromMinutes(1)));
        }

        /// <summary>
        /// БД без КП загружается с предупреждением о том, что ввод режима выполняться не будет.
        /// </summary>
        [Test]
        public void Load_NoControlPoints_Warns()
        {
            RegulationScenario scenario = new RegulationScenario();
            scenario.Repository.ControlPoints.Clear();
            string megapointPath = CreateFile("smzu_mega_act.mptsmz", string.Empty);
            string smpSmzuPath = CreateFile("smp_smzu.csv", string.Empty);

            SessionData session = CreateService(scenario).Load(megapointPath, smpSmzuPath);

            Assert.That(session.ControlPoints, Is.Empty);
            Assert.That(
                scenario.Protocol.GetEntries().Any(entry => entry.Message == "В базе данных нет контрольных пунктов"),
                Is.True);
        }

        /// <summary>
        /// Ошибки обращения к БД сообщаются ошибкой работы с БД и записываются в протокол.
        /// </summary>
        /// <param name="method">Метод репозитория, завершающийся ошибкой.</param>
        [TestCase("GetControlPoints")]
        [TestCase("SaveVoltageSchedule")]
        [TestCase("SaveRegime")]
        public void Load_DatabaseFails_ThrowsDatabaseError(string method)
        {
            RegulationScenario scenario = new RegulationScenario();
            scenario.Repository.FailingMethods.Add(method);
            string megapointPath = CreateFile("smzu_mega_act.mptsmz", string.Empty);
            string smpSmzuPath = CreateFile("smp_smzu.csv", string.Empty);

            VrdoException exception = Assert.Throws<VrdoException>(
                () => CreateService(scenario).Load(megapointPath, smpSmzuPath))!;

            Assert.That(exception.Message, Is.EqualTo(DataLoadService.DatabaseErrorMessage));
            Assert.That(exception.Details, Is.EqualTo("database is locked"));
            Assert.That(scenario.Protocol.GetEntries().Last().Tag, Is.EqualTo(ProtocolTag.Error));
        }

        /// <summary>
        /// Ошибка ПО «Барс» при открытии МТ и непредвиденная ошибка чтения файла smp_smzu.csv
        /// сообщаются ошибками системы.
        /// </summary>
        [Test]
        public void Load_ReadersFail_ThrowsSystemErrors()
        {
            RegulationScenario scenario = new RegulationScenario();
            string megapointPath = CreateFile("smzu_mega_act.mptsmz", string.Empty);
            string smpSmzuPath = CreateFile("smp_smzu.csv", string.Empty);
            DataLoadService failingReader = new DataLoadService(
                scenario.Adapter,
                scenario.Repository,
                new CkModel(),
                new FailingSmpSmzuCsvReader(),
                scenario.Protocol,
                RegulationScenario.Observed);

            DataFileException fileError = Assert.Throws<DataFileException>(
                () => failingReader.Load(megapointPath, smpSmzuPath))!;
            scenario.Adapter.OpenException = new InvalidOperationException("Нет лицензии");
            VrdoException barsError = Assert.Throws<VrdoException>(
                () => CreateService(scenario).Load(megapointPath, smpSmzuPath))!;

            Assert.That(fileError.Error, Is.EqualTo(DataFileError.InvalidContent));
            Assert.That(fileError.Details, Does.EndWith("Неожиданный конец файла"));
            Assert.That(barsError.Message, Is.EqualTo(DataLoadService.BarsErrorMessage));
            Assert.That(barsError.Details, Is.EqualTo("Нет лицензии"));
        }

        /// <summary>
        /// Все зависимости сервиса обязательны.
        /// </summary>
        [Test]
        public void Constructor_NullArguments_Throw()
        {
            RegulationScenario scenario = new RegulationScenario();
            FakeRastrAdapter adapter = scenario.Adapter;
            FakeModelDataRepository repository = scenario.Repository;
            CkModel ckModel = new CkModel();
            SmpSmzuCsvReader reader = new SmpSmzuCsvReader();
            CalculationProtocol protocol = scenario.Protocol;
            ObservedObjects observed = RegulationScenario.Observed;

            Assert.Throws<ArgumentNullException>(
                () => new DataLoadService(null!, repository, ckModel, reader, protocol, observed));
            Assert.Throws<ArgumentNullException>(
                () => new DataLoadService(adapter, null!, ckModel, reader, protocol, observed));
            Assert.Throws<ArgumentNullException>(
                () => new DataLoadService(adapter, repository, null!, reader, protocol, observed));
            Assert.Throws<ArgumentNullException>(
                () => new DataLoadService(adapter, repository, ckModel, null!, protocol, observed));
            Assert.Throws<ArgumentNullException>(
                () => new DataLoadService(adapter, repository, ckModel, reader, null!, observed));
            Assert.Throws<ArgumentNullException>(
                () => new DataLoadService(adapter, repository, ckModel, reader, protocol, null!));
        }

        /// <summary>
        /// Создает сервис загрузки данных для тестовых исходных данных.
        /// </summary>
        /// <param name="scenario">Тестовые исходные данные.</param>
        /// <returns>Сервис загрузки данных.</returns>
        private static DataLoadService CreateService(RegulationScenario scenario)
        {
            return new DataLoadService(
                scenario.Adapter,
                scenario.Repository,
                new CkModel(),
                new SmpSmzuCsvReader(),
                scenario.Protocol,
                RegulationScenario.Observed);
        }

        /// <summary>
        /// Создает временный файл.
        /// </summary>
        /// <param name="name">Имя файла.</param>
        /// <param name="content">Содержимое файла.</param>
        /// <returns>Путь к файлу.</returns>
        private string CreateFile(string name, string content)
        {
            string path = Path.Combine(_directory, name);
            File.WriteAllText(path, content);
            return path;
        }
    }
}
