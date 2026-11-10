using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using NUnit.Framework;

using VrdoPdg.Infrastructure;
using VrdoPdg.Model;
using VrdoPdg.Model.Entities;
using VrdoPdg.Model.Protocol;
using VrdoPdg.Model.Regime;
using VrdoPdg.Model.Services;
using VrdoPdg.Model.SmpSmzu;
using VrdoPdg.Tests.Fakes;

namespace VrdoPdg.Tests
{
    /// <summary>
    /// Тесты формирования записей файла smp_smzu.csv по результатам расчета.
    /// </summary>
    [TestFixture]
    public sealed class ResultExportServiceTests
    {
        /// <summary>
        /// Изменения СКРМ в часе формируют записи с кодами 03, 20 и 01.
        /// </summary>
        [Test]
        public void CreateRecords_ChangedDevices_CreatesRecordsByDeviceType()
        {
            RegulationScenario scenario = new RegulationScenario();
            scenario.SetVoltage(4, 535);
            scenario.FixedDeviceHours.Add((RegulationScenario.ReactorId, 4));
            CalculationResult result = scenario.Run();
            ResultExportService service = new ResultExportService(new SmpSmzuCsvWriter(','), new CalculationProtocol());

            List<string> lines = service.CreateRecords(result)
                .Select(record => SmpSmzuCsvWriter.FormatRecord(record, ','))
                .ToList();

            Assert.That(lines, Is.EqualTo(new[]
            {
                "03;4;4;520;0;0;-100;-100;0",
                "20;4;4;125;0;0;0;0;0",
                "01;4;4;6000;0;0;233,9;0;0"
            }));
        }

        /// <summary>
        /// Одинаковые изменения в соседних часах объединяются в одну запись.
        /// </summary>
        [Test]
        public void CreateRecords_SameChangeInConsecutiveHours_MergesHours()
        {
            RegulationScenario scenario = new RegulationScenario();
            for (int hour = 7; hour <= 24; hour++)
            {
                scenario.SetVoltage(hour, 535);
            }

            CalculationResult result = scenario.Run();
            ResultExportService service = new ResultExportService(new SmpSmzuCsvWriter('.'), new CalculationProtocol());

            List<string> lines = service.CreateRecords(result)
                .Select(record => SmpSmzuCsvWriter.FormatRecord(record, '.'))
                .ToList();

            Assert.That(lines, Is.EqualTo(new[]
            {
                "03;7;24;520;0;0;-100;-100;0",
                "20;7;24;9;0;0;1;0;0"
            }));
        }

        /// <summary>
        /// Без изменений СКРМ записи не формируются.
        /// </summary>
        [Test]
        public void CreateRecords_NoChanges_ReturnsEmptyList()
        {
            RegulationScenario scenario = new RegulationScenario();
            CalculationResult result = scenario.Run();
            ResultExportService service = new ResultExportService(new SmpSmzuCsvWriter(), new CalculationProtocol());

            Assert.That(service.CreateRecords(result), Is.Empty);
        }

        /// <summary>
        /// Сохранение дополняет исходные строки файла второй частью с записями и пишет в протокол.
        /// </summary>
        [Test]
        public void Export_ChangedDevices_WritesSourceLinesAndRecords()
        {
            RegulationScenario scenario = new RegulationScenario();
            scenario.SetVoltage(1, 535);
            CalculationResult result = scenario.Run();
            ResultExportService service = new ResultExportService(new SmpSmzuCsvWriter(), scenario.Protocol);
            string path = Path.Combine(Path.GetTempPath(), "vrdo_" + Path.GetRandomFileName(), "smp_smzu.csv");
            try
            {
                int count = service.Export(result, path);

                string[] lines = File.ReadAllLines(path);
                Assert.That(count, Is.EqualTo(2));
                Assert.That(lines, Is.EqualTo(new[]
                {
                    "20;1;24;185;0;0;1;0;0",
                    string.Empty,
                    "03;1;1;520;0;0;-100;-100;0",
                    "20;1;1;9;0;0;1;0;0"
                }));
                Assert.That(
                    scenario.Protocol.GetEntries().Last().Message,
                    Is.EqualTo("Результаты расчета №1 сохранены"));
            }
            finally
            {
                Directory.Delete(Path.GetDirectoryName(path)!, true);
            }
        }

        /// <summary>
        /// Путь сохранения по умолчанию совпадает с путем загруженного файла smp_smzu.csv.
        /// </summary>
        [Test]
        public void GetDefaultPath_Result_ReturnsLoadedFilePath()
        {
            CalculationResult result = new RegulationScenario().Run();

            Assert.That(
                ResultExportService.GetDefaultPath(result),
                Is.EqualTo(@"D:\BarsMDP\Results\131026\BarsCSV\smp_smzu.csv"));
        }

        /// <summary>
        /// Ошибка записи файла сообщается ошибкой «Нет доступа к файлу» и записывается в протокол.
        /// </summary>
        [Test]
        public void Export_DirectoryPath_ThrowsAccessDeniedAndAddsProtocolEntry()
        {
            RegulationScenario scenario = new RegulationScenario();
            CalculationResult result = scenario.Run();
            ResultExportService service = new ResultExportService(new SmpSmzuCsvWriter(), scenario.Protocol);
            string path = Path.Combine(Path.GetTempPath(), "vrdo_" + Path.GetRandomFileName());
            Directory.CreateDirectory(path);
            try
            {
                DataFileException exception = Assert.Throws<DataFileException>(() => service.Export(result, path))!;

                Assert.That(exception.Error, Is.EqualTo(DataFileError.AccessDenied));
                Assert.That(scenario.Protocol.GetEntries().Last().Tag, Is.EqualTo(ProtocolTag.Error));
            }
            finally
            {
                Directory.Delete(path, true);
            }
        }

        /// <summary>
        /// Записи не формируются для генератора без узла подключения и для часов без режима
        /// до или после расчета; пустые пределы управляемого СКРМ не записываются.
        /// </summary>
        [Test]
        public void CreateRecords_MissingData_SkipsDevicesAndHours()
        {
            Device controlled = new Device(1, DeviceType.Controlled, "СТК-1", 520, null);
            Device generator = new Device(41, DeviceType.Generator, "ТГ-5", 13, null);
            RegimeSnapshot before = new RegimeSnapshot();
            RegimeSnapshot after = new RegimeSnapshot();
            before.SetMode(new DeviceMode(1, 1, true, 0, -100, 100, null, null, false));
            after.SetMode(new DeviceMode(1, 1, true, 0, null, null, null, null, false));
            before.SetMode(new DeviceMode(1, 2, true, 0, -100, 100, null, null, false));
            before.SetMode(new DeviceMode(1, 3, true, 0, null, null, null, null, false));
            after.SetMode(new DeviceMode(1, 3, true, 0, null, null, null, null, false));
            before.SetMode(new DeviceMode(41, 1, true, 0, -50, 50, 236, 10, false));
            after.SetMode(new DeviceMode(41, 1, true, 0, -50, 50, 235.7, 10, false));
            CalculationResult result = CreateResult(new[] { controlled, generator }, before, after);
            ResultExportService service = new ResultExportService(new SmpSmzuCsvWriter(), new CalculationProtocol());

            Assert.That(service.CreateRecords(result), Is.Empty);
        }

        /// <summary>
        /// Аргументы конструктора и расчет обязательны.
        /// </summary>
        [Test]
        public void Methods_NullArguments_Throw()
        {
            ResultExportService service = new ResultExportService(new SmpSmzuCsvWriter(), new CalculationProtocol());

            Assert.Throws<ArgumentNullException>(() => new ResultExportService(null!, new CalculationProtocol()));
            Assert.Throws<ArgumentNullException>(() => new ResultExportService(new SmpSmzuCsvWriter(), null!));
            Assert.Throws<ArgumentNullException>(() => service.CreateRecords(null!));
            Assert.Throws<ArgumentNullException>(() => service.Export(null!, "smp_smzu.csv"));
        }

        /// <summary>
        /// Создает расчет по режимам СКРМ до и после расчета без узлов подключения генераторов.
        /// </summary>
        /// <param name="devices">Справочник СКРМ.</param>
        /// <param name="before">Режим до расчета.</param>
        /// <param name="after">Режим после расчета.</param>
        /// <returns>Расчет.</returns>
        private static CalculationResult CreateResult(
            IReadOnlyList<Device> devices,
            RegimeSnapshot before,
            RegimeSnapshot after)
        {
            SmpSmzuDocument document = new SmpSmzuDocument(
                new string[0],
                new SmpSmzuRecord[0],
                "\r\n",
                new UTF8Encoding(false),
                null);
            SessionData session = new SessionData(
                "smzu_mega_act.mptsmz",
                "smp_smzu.csv",
                document,
                new PowerSystem[0],
                new ControlPoint[0],
                devices,
                new VoltageSchedule(new VoltageBounds[0]),
                before,
                new Dictionary<int, int>(),
                new (int DeviceId, int Hour)[0],
                DateTime.Now);
            return new CalculationResult(1, DateTime.Now, DateTime.Now, session, after, new UnresolvedViolation[0]);
        }
    }
}
