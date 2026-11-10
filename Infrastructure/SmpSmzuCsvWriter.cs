using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

using VrdoPdg.Model.SmpSmzu;

namespace VrdoPdg.Infrastructure
{
    /// <summary>
    /// Модуль формирования файла smp_smzu.csv: формирует строки записей согласно инструкции
    /// и дополняет ими второй блок файла (записи о ручных изменениях состояния СКРМ).
    /// </summary>
    public sealed class SmpSmzuCsvWriter : ISmpSmzuCsvWriter
    {
        /// <summary>
        /// Десятичный разделитель, используемый, если его нельзя определить по исходному файлу.
        /// </summary>
        private readonly char _defaultDecimalSeparator;

        /// <summary>
        /// Создает модуль формирования файла с десятичной запятой по умолчанию.
        /// </summary>
        public SmpSmzuCsvWriter()
            : this(',')
        {
        }

        /// <summary>
        /// Создает модуль формирования файла.
        /// </summary>
        /// <param name="defaultDecimalSeparator">Десятичный разделитель по умолчанию: точка или запятая.</param>
        public SmpSmzuCsvWriter(char defaultDecimalSeparator)
        {
            if (defaultDecimalSeparator != '.' && defaultDecimalSeparator != ',')
            {
                throw new ArgumentException(
                    "Десятичный разделитель должен быть точкой или запятой",
                    nameof(defaultDecimalSeparator));
            }

            _defaultDecimalSeparator = defaultDecimalSeparator;
        }

        /// <inheritdoc/>
        public void Write(string path, SmpSmzuDocument source, IReadOnlyList<SmpSmzuRecord> addedRecords)
        {
            IReadOnlyList<string> lines = ComposeLines(source, addedRecords);
            string? directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string text = string.Join(source.NewLine, lines) + source.NewLine;
            File.WriteAllText(path, text, source.Encoding);
        }

        /// <summary>
        /// Формирует строки дополненного файла: строки исходного файла сохраняются без изменений,
        /// новые записи добавляются в конец второго блока. Если в файле один блок,
        /// перед новыми записями добавляется пустая строка-разделитель.
        /// </summary>
        /// <param name="source">Содержимое исходного файла.</param>
        /// <param name="addedRecords">Добавляемые записи.</param>
        /// <returns>Строки файла.</returns>
        public IReadOnlyList<string> ComposeLines(SmpSmzuDocument source, IReadOnlyList<SmpSmzuRecord> addedRecords)
        {
            List<string> lines = source.Lines.ToList();
            if (addedRecords.Count == 0)
            {
                return lines;
            }

            while (lines.Count > 0 && SmpSmzuCsvReader.IsBlank(lines[lines.Count - 1]))
            {
                lines.RemoveAt(lines.Count - 1);
            }

            if (!HasSecondBlock(lines))
            {
                lines.Add(string.Empty);
            }

            char decimalSeparator = source.DecimalSeparator ?? _defaultDecimalSeparator;
            lines.AddRange(addedRecords.Select(record => FormatRecord(record, decimalSeparator)));
            return lines;
        }

        /// <summary>
        /// Формирует строку записи
        /// </summary>
        /// <param name="record">Запись</param>
        /// <param name="decimalSeparator">Десятичный разделитель значений</param>
        /// <returns>Строка записи</returns>
        public static string FormatRecord(SmpSmzuRecord record, char decimalSeparator)
        {
            string[] fields =
            {
                record.TypeCode.ToString("00", CultureInfo.InvariantCulture),
                record.StartHour.ToString(CultureInfo.InvariantCulture),
                record.EndHour.ToString(CultureInfo.InvariantCulture),
                record.Address1.ToString(CultureInfo.InvariantCulture),
                record.Address2.ToString(CultureInfo.InvariantCulture),
                record.Address3.ToString(CultureInfo.InvariantCulture),
                FormatValue(record.Value1, decimalSeparator),
                FormatValue(record.Value2, decimalSeparator),
                FormatValue(record.Value3, decimalSeparator)
            };
            return string.Join(SmpSmzuCsvReader.FieldSeparator.ToString(), fields);
        }

        /// <summary>
        /// Форматирует значение параметра: целые числа без дробной части,
        /// дробные — не более трех знаков после разделителя.
        /// </summary>
        /// <param name="value">Значение.</param>
        /// <param name="decimalSeparator">Десятичный разделитель.</param>
        /// <returns>Текст поля; пустая строка, если значение не задано.</returns>
        private static string FormatValue(double? value, char decimalSeparator)
        {
            if (!value.HasValue)
            {
                return string.Empty;
            }

            double rounded = Math.Round(value.Value, 3);
            if (rounded == 0)
            {
                return "0";
            }

            string text = rounded.ToString("0.###", CultureInfo.InvariantCulture);
            return decimalSeparator == '.'
                ? text
                : text.Replace('.', decimalSeparator);
        }

        /// <summary>
        /// Проверяет, есть ли в файле второй блок: непустая строка после пустой строки-разделителя.
        /// </summary>
        /// <param name="lines">Строки файла без завершающих пустых строк.</param>
        /// <returns><c>true</c>, если второй блок есть.</returns>
        private static bool HasSecondBlock(IReadOnlyList<string> lines)
        {
            bool isSeparatorFound = false;
            foreach (string line in lines)
            {
                if (SmpSmzuCsvReader.IsBlank(line))
                {
                    isSeparatorFound = true;
                }
                else if (isSeparatorFound)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
