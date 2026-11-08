using System;
using System.IO;
using System.Linq;

using NUnit.Framework;

using VrdoPdg.Infrastructure;
using VrdoPdg.Model;
using VrdoPdg.Model.Entities;
using VrdoPdg.Model.Protocol;
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

            Assert.That(scenario.Archive.CurrentSession, Is.SameAs(session));
            Assert.That(session.SmpSmzuDocument.Records, Has.Count.EqualTo(2));
            Assert.That(session.IsFixed(RegulationScenario.ReactorId, 6), Is.False);
            Assert.That(session.IsFixed(RegulationScenario.ReactorId, 7), Is.True);
            Assert.That(session.Schedule.GetBounds(RegulationScenario.ControlPointId, 24)!.UpperBound, Is.EqualTo(525));
            Assert.That(session.InitialRegime.GetVoltage(RegulationScenario.MainNode, 12), Is.EqualTo(515));
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
            Assert.That(scenario.Archive.CurrentSession, Is.Null);
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
                scenario.Archive,
                scenario.Protocol);

            SessionData session = service.Load(megapointPath, smpSmzuPath);

            Assert.That(session.Schedule.GetBounds(RegulationScenario.ControlPointId, 1)!.UpperBound, Is.EqualTo(520));
            Assert.That(scenario.Protocol.GetEntries().Any(entry => entry.Tag == ProtocolTag.Warning
                && entry.Message == DataLoadService.ScheduleNotUpdatedMessage), Is.True);
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
                scenario.Archive,
                scenario.Protocol);
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
