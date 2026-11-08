using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

using VrdoPdg.Model;
using VrdoPdg.Model.Regime;
using VrdoPdg.Model.SmpSmzu;

namespace VrdoPdg.Infrastructure
{
    /// <summary>
    /// Модуль чтения файла smp_smzu.csv: разбирает строки файла в записи.
    /// Поля записи разделены точкой с запятой; блоки записей разделены пустыми строками.
    /// </summary>
    public sealed class SmpSmzuCsvReader : ISmpSmzuCsvReader
    {
        /// <summary>
        /// Разделитель полей записи.
        /// </summary>
        public const char FieldSeparator = ';';

        /// <summary>
        /// Наименьшее число полей записи: тип, час начала, час конца, адрес 1.
        /// </summary>
        private const int MinFieldCount = 4;

        /// <summary>
        /// Наибольшее число полей записи.
        /// </summary>
        private const int MaxFieldCount = 9;

        /// <summary>
        /// Наибольшее число ошибок, выводимых в сообщении.
        /// </summary>
        private const int MaxReportedErrors = 5;

        /// <summary>
        /// Кодовая страница Windows-1251.
        /// </summary>
        private const int Windows1251CodePage = 1251;

        /// <inheritdoc/>
        public SmpSmzuDocument Read(string path)
        {
            byte[] bytes;
            try
            {
                bytes = File.ReadAllBytes(path);
            }
            catch (FileNotFoundException exception)
            {
                throw new DataFileException(DataFileError.NotFound, path, null, exception);
            }
            catch (DirectoryNotFoundException exception)
            {
                throw new DataFileException(DataFileError.NotFound, path, null, exception);
            }
            catch (UnauthorizedAccessException exception)
            {
                throw new DataFileException(DataFileError.AccessDenied, path, exception.Message, exception);
            }
            catch (IOException exception)
            {
                throw new DataFileException(DataFileError.AccessDenied, path, exception.Message, exception);
            }

            return Parse(bytes, path);
        }

        /// <summary>
        /// Разбирает содержимое файла smp_smzu.csv.
        /// </summary>
        /// <param name="bytes">Содержимое файла.</param>
        /// <param name="path">Путь к файлу для сообщений об ошибках.</param>
        /// <returns>Содержимое файла.</returns>
        /// <exception cref="DataFileException">Файл неправильно заполнен.</exception>
        public SmpSmzuDocument Parse(byte[] bytes, string path)
        {
            Encoding encoding;
            string text = Decode(bytes, out encoding);
            string newLine = text.Contains("\r\n")
                ? "\r\n"
                : text.Contains("\n")
                    ? "\n"
                    : "\r\n";

            List<string> lines = text
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
                .ToList();
            if (lines.Count > 0 && lines[lines.Count - 1].Length == 0)
            {
                lines.RemoveAt(lines.Count - 1);
            }

            List<SmpSmzuRecord> records = new List<SmpSmzuRecord>();
            List<string> errors = new List<string>();
            char? decimalSeparator = null;
            bool isFirstContentLine = true;
            for (int index = 0; index < lines.Count; index++)
            {
                string line = lines[index];
                if (IsBlank(line))
                {
                    continue;
                }

                string[] fields = line.Split(FieldSeparator);
                bool isHeader = isFirstContentLine && fields[0].Any(char.IsLetter);
                isFirstContentLine = false;
                if (isHeader)
                {
                    continue;
                }

                string? error;
                SmpSmzuRecord? record = ParseRecord(fields, index + 1, ref decimalSeparator, out error);
                if (record == null)
                {
                    errors.Add("строка " + (index + 1) + ": " + error);
                }
                else
                {
                    records.Add(record);
                }
            }

            if (errors.Count > 0)
            {
                string details = string.Join("; ", errors.Take(MaxReportedErrors));
                if (errors.Count > MaxReportedErrors)
                {
                    details += "; всего ошибок: " + errors.Count;
                }

                throw new DataFileException(DataFileError.InvalidContent, path, details);
            }

            return new SmpSmzuDocument(lines, records, newLine, encoding, decimalSeparator);
        }

        /// <summary>
        /// Проверяет, является ли строка пустой (разделителем блоков).
        /// </summary>
        /// <param name="line">Строка файла.</param>
        /// <returns><c>true</c>, если строка не содержит значений.</returns>
        public static bool IsBlank(string line)
        {
            return line.All(symbol => char.IsWhiteSpace(symbol) || symbol == FieldSeparator);
        }

        /// <summary>
        /// Определяет кодировку и декодирует содержимое файла: UTF-8 с меткой порядка байтов,
        /// UTF-8 без метки или Windows-1251.
        /// </summary>
        /// <param name="bytes">Содержимое файла.</param>
        /// <param name="encoding">Определенная кодировка.</param>
        /// <returns>Текст файла.</returns>
        private static string Decode(byte[] bytes, out Encoding encoding)
        {
            if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            {
                encoding = new UTF8Encoding(true);
                return Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
            }

            try
            {
                UTF8Encoding strictEncoding = new UTF8Encoding(false, true);
                string text = strictEncoding.GetString(bytes);
                encoding = new UTF8Encoding(false);
                return text;
            }
            catch (DecoderFallbackException)
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                encoding = Encoding.GetEncoding(Windows1251CodePage);
                return encoding.GetString(bytes);
            }
        }

        /// <summary>
        /// Разбирает поля записи.
        /// </summary>
        /// <param name="fields">Поля строки.</param>
        /// <param name="lineNumber">Номер строки.</param>
        /// <param name="decimalSeparator">Десятичный разделитель значений, определенный по файлу.</param>
        /// <param name="error">Описание ошибки разбора.</param>
        /// <returns>Запись или <c>null</c>, если строка неправильно заполнена.</returns>
        private static SmpSmzuRecord? ParseRecord(
            string[] fields,
            int lineNumber,
            ref char? decimalSeparator,
            out string? error)
        {
            error = null;
            if (fields.Length < MinFieldCount || fields.Length > MaxFieldCount)
            {
                error = "ожидается от " + MinFieldCount + " до " + MaxFieldCount + " полей, получено " + fields.Length;
                return null;
            }

            int typeCode;
            int startHour;
            int endHour;
            int address1;
            if (!TryParseInteger(fields[0], out typeCode)
                || !TryParseInteger(fields[1], out startHour)
                || !TryParseInteger(fields[2], out endHour)
                || !TryParseInteger(fields[3], out address1))
            {
                error = "тип записи, часы и номер элемента должны быть целыми числами";
                return null;
            }

            if (startHour < 1 || endHour > RegimeSnapshot.HourCount || startHour > endHour)
            {
                error = "неверный диапазон часов " + startHour + "–" + endHour;
                return null;
            }

            int address2;
            int address3;
            if (!TryParseOptionalInteger(GetField(fields, 4), out address2)
                || !TryParseOptionalInteger(GetField(fields, 5), out address3))
            {
                error = "адреса элемента должны быть целыми числами";
                return null;
            }

            double? value1;
            double? value2;
            double? value3;
            if (!TryParseValue(GetField(fields, 6), ref decimalSeparator, out value1)
                || !TryParseValue(GetField(fields, 7), ref decimalSeparator, out value2)
                || !TryParseValue(GetField(fields, 8), ref decimalSeparator, out value3))
            {
                error = "значения параметров должны быть числами";
                return null;
            }

            return new SmpSmzuRecord(
                typeCode,
                startHour,
                endHour,
                address1,
                address2,
                address3,
                value1,
                value2,
                value3,
                lineNumber);
        }

        /// <summary>
        /// Возвращает поле строки или пустую строку, если поля нет.
        /// </summary>
        /// <param name="fields">Поля строки.</param>
        /// <param name="index">Индекс поля.</param>
        /// <returns>Значение поля.</returns>
        private static string GetField(string[] fields, int index)
        {
            return index < fields.Length
                ? fields[index].Trim()
                : string.Empty;
        }

        /// <summary>
        /// Разбирает целое число.
        /// </summary>
        /// <param name="text">Текст поля.</param>
        /// <param name="value">Значение.</param>
        /// <returns><c>true</c>, если поле содержит целое число.</returns>
        private static bool TryParseInteger(string text, out int value)
        {
            return int.TryParse(text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }

        /// <summary>
        /// Разбирает необязательное целое число; пустое поле означает 0.
        /// </summary>
        /// <param name="text">Текст поля.</param>
        /// <param name="value">Значение.</param>
        /// <returns><c>true</c>, если поле пустое или содержит целое число.</returns>
        private static bool TryParseOptionalInteger(string text, out int value)
        {
            if (text.Length == 0)
            {
                value = 0;
                return true;
            }

            return TryParseInteger(text, out value);
        }

        /// <summary>
        /// Разбирает значение параметра с десятичной точкой или запятой; пустое поле — значение не задано.
        /// </summary>
        /// <param name="text">Текст поля.</param>
        /// <param name="decimalSeparator">Десятичный разделитель, определенный по файлу.</param>
        /// <param name="value">Значение.</param>
        /// <returns><c>true</c>, если поле пустое или содержит число.</returns>
        private static bool TryParseValue(string text, ref char? decimalSeparator, out double? value)
        {
            value = null;
            if (text.Length == 0)
            {
                return true;
            }

            if (!decimalSeparator.HasValue)
            {
                if (text.Contains(','))
                {
                    decimalSeparator = ',';
                }
                else if (text.Contains('.'))
                {
                    decimalSeparator = '.';
                }
            }

            double result;
            if (!double.TryParse(
                text.Replace(',', '.'),
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out result))
            {
                return false;
            }

            value = result;
            return true;
        }
    }
}
