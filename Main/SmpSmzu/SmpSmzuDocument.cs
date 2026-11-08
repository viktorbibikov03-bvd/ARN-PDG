using System.Collections.Generic;
using System.Text;

namespace VrdoPdg.Model.SmpSmzu
{
    /// <summary>
    /// Содержимое файла smp_smzu.csv: исходные строки и разобранные записи.
    /// </summary>
    public sealed class SmpSmzuDocument
    {
        /// <summary>
        /// Создает содержимое файла smp_smzu.csv.
        /// </summary>
        /// <param name="lines">Исходные строки файла без символов перевода строки.</param>
        /// <param name="records">Разобранные записи.</param>
        /// <param name="newLine">Последовательность перевода строки, используемая в файле.</param>
        /// <param name="encoding">Кодировка файла.</param>
        /// <param name="decimalSeparator">Десятичный разделитель значений; <c>null</c>, если не определен.</param>
        public SmpSmzuDocument(
            IReadOnlyList<string> lines,
            IReadOnlyList<SmpSmzuRecord> records,
            string newLine,
            Encoding encoding,
            char? decimalSeparator)
        {
            Lines = lines;
            Records = records;
            NewLine = newLine;
            Encoding = encoding;
            DecimalSeparator = decimalSeparator;
        }

        /// <summary>
        /// Исходные строки файла без символов перевода строки.
        /// </summary>
        public IReadOnlyList<string> Lines { get; }

        /// <summary>
        /// Разобранные записи файла.
        /// </summary>
        public IReadOnlyList<SmpSmzuRecord> Records { get; }

        /// <summary>
        /// Последовательность перевода строки, используемая в файле.
        /// </summary>
        public string NewLine { get; }

        /// <summary>
        /// Кодировка файла, в которой сохраняется дополненный файл.
        /// </summary>
        public Encoding Encoding { get; }

        /// <summary>
        /// Десятичный разделитель значений, определенный по содержимому файла;
        /// <c>null</c>, если в файле нет дробных значений.
        /// </summary>
        public char? DecimalSeparator { get; }
    }
}
