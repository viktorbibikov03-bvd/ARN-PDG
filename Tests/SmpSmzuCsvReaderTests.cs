using System.IO;
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
        /// Файл читается с диска; отсутствующий файл или каталог сообщается ошибкой «не найден»,
        /// каталог вместо файла — ошибкой доступа.
        /// </summary>
        [Test]
        public void Read_Paths_ReadsFileOrReportsError()
        {
            string directory = Path.Combine(Path.GetTempPath(), "vrdo_csv_" + Path.GetRandomFileName());
            Directory.CreateDirectory(directory);
            try
            {
                string path = Path.Combine(directory, "smp_smzu.csv");
                File.WriteAllText(path, "20;1;24;185;0;0;1;0;0\r\n");
                SmpSmzuCsvReader reader = new SmpSmzuCsvReader();

                Assert.That(reader.Read(path).Records, Has.Count.EqualTo(1));
                Assert.That(
                    Assert.Throws<DataFileException>(() => reader.Read(Path.Combine(directory, "нет.csv")))!.Error,
                    Is.EqualTo(DataFileError.NotFound));
                Assert.That(
                    Assert.Throws<DataFileException>(() => reader.Read(Path.Combine(directory, "нет", "f.csv")))!.Error,
                    Is.EqualTo(DataFileError.NotFound));
                Assert.That(
                    Assert.Throws<DataFileException>(() => reader.Read(directory))!.Error,
                    Is.EqualTo(DataFileError.AccessDenied));
                using (new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    Assert.That(
                        Assert.Throws<DataFileException>(() => reader.Read(path))!.Error,
                        Is.EqualTo(DataFileError.AccessDenied));
                }
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        /// <summary>
        /// Файл в кодировке Windows-1251 разбирается, кодировка сохраняется в документе.
        /// </summary>
        [Test]
        public void Parse_Windows1251_KeepsEncoding()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding windows1251 = Encoding.GetEncoding(1251);
            byte[] bytes = windows1251.GetBytes("Тип;Начало;Конец\r\n20;1;24;185;0;0;1;0;0");

            SmpSmzuDocument document = new SmpSmzuCsvReader().Parse(bytes, "smp_smzu.csv");

            Assert.That(document.Encoding.CodePage, Is.EqualTo(1251));
            Assert.That(document.Lines[0], Is.EqualTo("Тип;Начало;Конец"));
            Assert.That(document.Records, Has.Count.EqualTo(1));
        }

        /// <summary>
        /// Строки из разделителей пропускаются, пустые адреса и значения допускаются,
        /// строка из четырех полей дополняется пустыми полями.
        /// </summary>
        [Test]
        public void Parse_EmptyFields_AreAllowed()
        {
            SmpSmzuDocument document = Parse(" ; ;\r\n20;1;24;185;;;1;;\r\n20;2;24;186");

            Assert.That(document.Records, Has.Count.EqualTo(2));
            Assert.That(document.Records[0].Address2, Is.Zero);
            Assert.That(document.Records[0].Value2, Is.Null);
            Assert.That(document.Records[1].Value1, Is.Null);
            Assert.That(document.Lines, Has.Count.EqualTo(3));
        }

        /// <summary>
        /// Ошибки числа полей, адресов и значений перечисляются по строкам, в сообщение попадают
        /// первые пять ошибок и общее число ошибок.
        /// </summary>
        [Test]
        public void Parse_ManyInvalidLines_ReportsFirstErrorsAndCount()
        {
            string text = "20;1;24\r\n"
                + "20;1;24;185;0;0;1;0;0;0\r\n"
                + "20;1;24;185;x;0;1;0;0\r\n"
                + "20;1;24;185;0;x;1;0;0\r\n"
                + "20;1;24;185;0;0;x;0;0\r\n"
                + "20;1;24;185;0;0;1;1e;0\r\n";

            DataFileException exception = Assert.Throws<DataFileException>(() => Parse(text))!;

            Assert.That(exception.Details, Does.Contain("ожидается от 4 до 9 полей, получено 3"));
            Assert.That(exception.Details, Does.Contain("получено 10"));
            Assert.That(exception.Details, Does.Contain("строка 3: адреса элемента должны быть целыми числами"));
            Assert.That(exception.Details, Does.Contain("строка 5: значения параметров должны быть числами"));
            Assert.That(exception.Details, Does.Not.Contain("строка 6"));
            Assert.That(exception.Details, Does.Contain("всего ошибок: 6"));
        }

        /// <summary>
        /// Пустой файл разбирается в документ без строк.
        /// </summary>
        [Test]
        public void Parse_EmptyFile_ReturnsEmptyDocument()
        {
            SmpSmzuDocument document = Parse(string.Empty);

            Assert.That(document.Lines, Is.Empty);
            Assert.That(document.Records, Is.Empty);
            Assert.That(document.NewLine, Is.EqualTo("\r\n"));
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
