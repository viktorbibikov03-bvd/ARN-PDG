using System.Collections.Generic;
using System.IO;
using System.Linq;

using NUnit.Framework;

using VrdoPdg.Infrastructure;
using VrdoPdg.Model.Protocol;
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
    }
}
