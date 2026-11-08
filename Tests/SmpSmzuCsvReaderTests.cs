using System.Text;

using NUnit.Framework;

using VrdoPdg.Infrastructure;
using VrdoPdg.Model;
using VrdoPdg.Model.SmpSmzu;

namespace VrdoPdg.Tests
{
    /// <summary>
    /// Тесты разбора файла smp_smzu.csv.
    /// </summary>
    [TestFixture]
    public sealed class SmpSmzuCsvReaderTests
    {
        /// <summary>
        /// Записи всех типов из приложения Б разбираются с кодами, часами, адресами и значениями.
        /// </summary>
        [Test]
        public void Parse_RecordsFromInstruction_ParsesAllFields()
        {
            string text = "03;5;20;331;0;0;50;-28;0\r\n"
                + "20;1;3;5;0;0;1;0;0\r\n"
                + "00;1;14;669;0;0;0;0;0\r\n"
                + "\r\n"
                + "01;4;19;263;0;0;218;0;0\r\n"
                + "12;11;24;805;0;0;1;0;0\r\n"
                + "16;7;16;613;0;0;;50;\r\n"
                + "04;7;20;381;295;1;0;0;0\r\n";

            SmpSmzuDocument document = Parse(text);

            Assert.That(document.Records, Has.Count.EqualTo(7));
            Assert.That(document.Lines, Has.Count.EqualTo(8));
            Assert.That(document.NewLine, Is.EqualTo("\r\n"));
            SmpSmzuRecord limits = document.Records[0];
            Assert.That(limits.Type, Is.EqualTo(SmpSmzuRecordType.NodeReactivePowerLimits));
            Assert.That(limits.StartHour, Is.EqualTo(5));
            Assert.That(limits.EndHour, Is.EqualTo(20));
            Assert.That(limits.Address1, Is.EqualTo(331));
            Assert.That(limits.Value1, Is.EqualTo(50));
            Assert.That(limits.Value2, Is.EqualTo(-28));
            Assert.That(document.Records[1].Type, Is.EqualTo(SmpSmzuRecordType.Reactor));
            Assert.That(document.Records[2].Type, Is.EqualTo(SmpSmzuRecordType.NodeState));
            Assert.That(document.Records[3].Type, Is.EqualTo(SmpSmzuRecordType.NodeVoltage));
            Assert.That(document.Records[4].Type, Is.EqualTo(SmpSmzuRecordType.BalancingNode));
            SmpSmzuRecord activePower = document.Records[5];
            Assert.That(activePower.Type, Is.EqualTo(SmpSmzuRecordType.NodeActivePowerLimits));
            Assert.That(activePower.Value1, Is.Null);
            Assert.That(activePower.Value2, Is.EqualTo(50));
            Assert.That(activePower.Value3, Is.Null);
            SmpSmzuRecord branch = document.Records[6];
            Assert.That(branch.Type, Is.EqualTo(SmpSmzuRecordType.BranchState));
            Assert.That(branch.Address2, Is.EqualTo(295));
            Assert.That(branch.Address3, Is.EqualTo(1));
            Assert.That(branch.LineNumber, Is.EqualTo(8));
        }

        /// <summary>
        /// Десятичный разделитель определяется по значениям файла.
        /// </summary>
        [TestCase("01;1;24;263;0;0;218,5;0;0", ',')]
        [TestCase("01;1;24;263;0;0;218.5;0;0", '.')]
        public void Parse_FractionalValue_DetectsDecimalSeparator(string line, char separator)
        {
            SmpSmzuDocument document = Parse(line + "\n");

            Assert.That(document.DecimalSeparator, Is.EqualTo(separator));
            Assert.That(document.Records[0].Value1, Is.EqualTo(218.5));
            Assert.That(document.NewLine, Is.EqualTo("\n"));
        }

        /// <summary>
        /// Строка с нечисловыми полями приводит к ошибке «Неправильно заполнен файл исходных данных».
        /// </summary>
        [Test]
        public void Parse_InvalidLine_ThrowsInvalidContentWithLineNumber()
        {
            DataFileException exception = Assert.Throws<DataFileException>(
                () => Parse("20;1;24;185;0;0;1;0;0\r\n20;x;24;185;0;0;1;0;0\r\n"))!;

            Assert.That(exception.Error, Is.EqualTo(DataFileError.InvalidContent));
            Assert.That(exception.Message, Is.EqualTo("Неправильно заполнен файл исходных данных"));
            Assert.That(exception.Details, Does.Contain("строка 2"));
        }

        /// <summary>
        /// Часы вне диапазона 1–24 приводят к ошибке.
        /// </summary>
        [Test]
        public void Parse_HoursOutOfRange_ThrowsInvalidContent()
        {
            DataFileException exception = Assert.Throws<DataFileException>(
                () => Parse("20;0;25;185;0;0;1;0;0\r\n"))!;

            Assert.That(exception.Details, Does.Contain("диапазон часов"));
        }

        /// <summary>
        /// Первая строка с текстом считается заголовком и пропускается.
        /// </summary>
        [Test]
        public void Parse_HeaderLine_SkipsHeader()
        {
            SmpSmzuDocument document = Parse("Тип;Начало;Конец;Номер;;;Значение;;\r\n20;1;24;185;0;0;1;0;0\r\n");

            Assert.That(document.Records, Has.Count.EqualTo(1));
            Assert.That(document.Lines, Has.Count.EqualTo(2));
        }

        /// <summary>
        /// Метка порядка байтов UTF-8 сохраняется в кодировке документа.
        /// </summary>
        [Test]
        public void Parse_Utf8WithBom_KeepsBomEncoding()
        {
            byte[] preamble = new UTF8Encoding(true).GetPreamble();
            byte[] content = Encoding.UTF8.GetBytes("20;1;24;185;0;0;1;0;0\r\n");
            byte[] bytes = new byte[preamble.Length + content.Length];
            preamble.CopyTo(bytes, 0);
            content.CopyTo(bytes, preamble.Length);

            SmpSmzuDocument document = new SmpSmzuCsvReader().Parse(bytes, "smp_smzu.csv");

            Assert.That(document.Encoding.GetPreamble(), Is.EqualTo(preamble));
            Assert.That(document.Records, Has.Count.EqualTo(1));
        }

        /// <summary>
        /// Разбирает текст файла.
        /// </summary>
        /// <param name="text">Текст файла.</param>
        /// <returns>Содержимое файла.</returns>
        private static SmpSmzuDocument Parse(string text)
        {
            return new SmpSmzuCsvReader().Parse(Encoding.UTF8.GetBytes(text), "smp_smzu.csv");
        }
    }
}
