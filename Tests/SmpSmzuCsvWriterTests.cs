using System.Collections.Generic;
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
