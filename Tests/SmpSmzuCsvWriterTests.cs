using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using NUnit.Framework;

using VrdoPdg.Infrastructure;
using VrdoPdg.Model.SmpSmzu;

namespace VrdoPdg.Tests
{
    /// <summary>
    /// Тесты формирования файла smp_smzu.csv.
    /// </summary>
    [TestFixture]
    public sealed class SmpSmzuCsvWriterTests
    {
        /// <summary>
        /// Записи, добавляемые в тестах.
        /// </summary>
        private static readonly SmpSmzuRecord[] _records =
        {
            new SmpSmzuRecord(20, 7, 24, 185, 0, 0, 1, 0, 0, 0),
            new SmpSmzuRecord(1, 4, 19, 263, 0, 0, 235.7, 0, 0, 0)
        };

        /// <summary>
        /// Запись формируется по формату инструкции с кодом из двух цифр.
        /// </summary>
        [Test]
        public void FormatRecord_ValuesWithDecimalSeparator_FormatsFields()
        {
            Assert.That(SmpSmzuCsvWriter.FormatRecord(_records[0], ','), Is.EqualTo("20;7;24;185;0;0;1;0;0"));
            Assert.That(SmpSmzuCsvWriter.FormatRecord(_records[1], ','), Is.EqualTo("01;4;19;263;0;0;235,7;0;0"));
            Assert.That(SmpSmzuCsvWriter.FormatRecord(_records[1], '.'), Is.EqualTo("01;4;19;263;0;0;235.7;0;0"));
        }

        /// <summary>
        /// В файле один блок: перед новыми записями добавляется пустая строка-разделитель.
        /// </summary>
        [Test]
        public void ComposeLines_SingleBlock_AddsSecondBlock()
        {
            SmpSmzuDocument source = CreateDocument("20;1;24;5;0;0;1;0;0", "04;7;20;381;295;1;0;0;0");

            IReadOnlyList<string> lines = new SmpSmzuCsvWriter(',').ComposeLines(source, _records);

            Assert.That(lines, Is.EqualTo(new[]
            {
                "20;1;24;5;0;0;1;0;0",
                "04;7;20;381;295;1;0;0;0",
                string.Empty,
                "20;7;24;185;0;0;1;0;0",
                "01;4;19;263;0;0;235,7;0;0"
            }));
        }

        /// <summary>
        /// Второй блок уже есть: записи добавляются в его конец, завершающие пустые строки удаляются.
        /// </summary>
        [Test]
        public void ComposeLines_SecondBlockExists_AppendsToSecondBlock()
        {
            SmpSmzuDocument source = CreateDocument(
                "20;1;24;5;0;0;1;0;0",
                string.Empty,
                "20;1;3;6;0;0;0;0;0",
                string.Empty);

            IReadOnlyList<string> lines = new SmpSmzuCsvWriter('.').ComposeLines(source, _records);

            Assert.That(lines, Is.EqualTo(new[]
            {
                "20;1;24;5;0;0;1;0;0",
                string.Empty,
                "20;1;3;6;0;0;0;0;0",
                "20;7;24;185;0;0;1;0;0",
                "01;4;19;263;0;0;235.7;0;0"
            }));
        }

        /// <summary>
        /// Без новых записей строки исходного файла не изменяются.
        /// </summary>
        [Test]
        public void ComposeLines_NoRecords_KeepsSourceLines()
        {
            SmpSmzuDocument source = CreateDocument("20;1;24;5;0;0;1;0;0", string.Empty);

            IReadOnlyList<string> lines = new SmpSmzuCsvWriter().ComposeLines(source, new SmpSmzuRecord[0]);

            Assert.That(lines, Is.EqualTo(source.Lines));
        }

        /// <summary>
        /// Десятичный разделитель исходного файла имеет приоритет над разделителем по умолчанию.
        /// </summary>
        [Test]
        public void ComposeLines_SourceDecimalSeparator_UsesSourceSeparator()
        {
            SmpSmzuDocument source = new SmpSmzuDocument(
                new[] { "01;1;24;263;0;0;218.5;0;0" },
                new SmpSmzuRecord[0],
                "\r\n",
                new UTF8Encoding(false),
                '.');

            IReadOnlyList<string> lines = new SmpSmzuCsvWriter(',').ComposeLines(source, new[] { _records[1] });

            Assert.That(lines[lines.Count - 1], Is.EqualTo("01;4;19;263;0;0;235.7;0;0"));
        }

        /// <summary>
        /// В пустой файл записи добавляются после пустой строки-разделителя.
        /// </summary>
        [Test]
        public void ComposeLines_EmptySource_AddsSecondBlock()
        {
            IReadOnlyList<string> lines = new SmpSmzuCsvWriter('.').ComposeLines(CreateDocument(), _records);

            Assert.That(lines, Is.EqualTo(new[]
            {
                string.Empty,
                "20;7;24;185;0;0;1;0;0",
                "01;4;19;263;0;0;235.7;0;0"
            }));
        }

        /// <summary>
        /// Десятичным разделителем по умолчанию может быть только точка или запятая.
        /// </summary>
        [Test]
        public void Constructor_InvalidSeparator_Throws()
        {
            Assert.Throws<ArgumentException>(() => new SmpSmzuCsvWriter(';'));
            Assert.DoesNotThrow(() => new SmpSmzuCsvWriter());
        }

        /// <summary>
        /// Пустые значения записываются пустыми полями.
        /// </summary>
        [Test]
        public void FormatRecord_EmptyValues_WritesEmptyFields()
        {
            SmpSmzuRecord record = new SmpSmzuRecord(16, 7, 16, 613, 0, 0, null, 50, null, 0);

            Assert.That(SmpSmzuCsvWriter.FormatRecord(record, ','), Is.EqualTo("16;7;16;613;0;0;;50;"));
        }

        /// <summary>
        /// Файл записывается в новый каталог с переводами строк и кодировкой исходного файла;
        /// файл без каталога записывается в текущий каталог.
        /// </summary>
        [Test]
        public void Write_NewDirectory_CreatesDirectoryAndFile()
        {
            string directory = Path.Combine(Path.GetTempPath(), "vrdo_out_" + Path.GetRandomFileName());
            string currentDirectory = Environment.CurrentDirectory;
            try
            {
                string path = Path.Combine(directory, "BarsCSV", "smp_smzu.csv");
                SmpSmzuDocument source = CreateDocument("20;1;24;5;0;0;1;0;0");

                new SmpSmzuCsvWriter(',').Write(path, source, _records);
                Directory.CreateDirectory(directory);
                Environment.CurrentDirectory = directory;
                new SmpSmzuCsvWriter(',').Write("local.csv", source, new SmpSmzuRecord[0]);

                Assert.That(
                    File.ReadAllText(path),
                    Is.EqualTo("20;1;24;5;0;0;1;0;0\r\n\r\n20;7;24;185;0;0;1;0;0\r\n01;4;19;263;0;0;235,7;0;0\r\n"));
                Assert.That(
                    File.ReadAllText(Path.Combine(directory, "local.csv")),
                    Is.EqualTo("20;1;24;5;0;0;1;0;0\r\n"));
            }
            finally
            {
                Environment.CurrentDirectory = currentDirectory;
                Directory.Delete(directory, true);
            }
        }

        /// <summary>
        /// Создает содержимое файла из строк.
        /// </summary>
        /// <param name="lines">Строки файла.</param>
        /// <returns>Содержимое файла.</returns>
        private static SmpSmzuDocument CreateDocument(params string[] lines)
        {
            return new SmpSmzuDocument(lines, new SmpSmzuRecord[0], "\r\n", new UTF8Encoding(false), null);
        }
    }
}
