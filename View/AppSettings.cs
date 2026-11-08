using System;
using System.IO;
using System.Text.Json;

namespace VrdoPdg.View
{
    /// <summary>
    /// Настройки системы, считываемые из файла appsettings.json рядом с исполняемым файлом.
    /// </summary>
    internal sealed class AppSettings
    {
        /// <summary>
        /// Имя файла настроек.
        /// </summary>
        public const string FileName = "appsettings.json";

        /// <summary>
        /// Путь к файлу БД SQLite; относительный путь отсчитывается от каталога программы.
        /// </summary>
        public string DatabasePath { get; set; } = "VrdoPdg.db";

        /// <summary>
        /// Каталог результатов ПО Барс-МДП (D:\BarsMDP\Results).
        /// </summary>
        public string ResultsDirectory { get; set; } = @"D:\BarsMDP\Results";

        /// <summary>
        /// Десятичный разделитель значений в файле smp_smzu.csv, если его нельзя определить
        /// по исходному файлу: «,» или «.».
        /// </summary>
        public string CsvDecimalSeparator { get; set; } = ",";

        /// <summary>
        /// Нижняя граница графиков напряжений, используемая до реализации запроса к ОИК «СК-11», кВ.
        /// </summary>
        public double LowerVoltageBound { get; set; } = 500;

        /// <summary>
        /// Верхняя граница графиков напряжений, используемая до реализации запроса к ОИК «СК-11», кВ.
        /// </summary>
        public double UpperVoltageBound { get; set; } = 525;

        /// <summary>
        /// Загружает настройки из файла; при отсутствии или ошибке чтения файла
        /// используются значения по умолчанию.
        /// </summary>
        /// <param name="path">Путь к файлу настроек.</param>
        /// <returns>Настройки.</returns>
        public static AppSettings Load(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    AppSettings? settings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(path));
                    if (settings != null)
                    {
                        return settings;
                    }
                }
            }
            catch (Exception exception)
                when (exception is IOException
                    || exception is UnauthorizedAccessException
                    || exception is JsonException)
            {
                return new AppSettings();
            }

            return new AppSettings();
        }

        /// <summary>
        /// Возвращает полный путь к файлу БД.
        /// </summary>
        /// <returns>Полный путь.</returns>
        public string GetDatabasePath()
        {
            return Path.IsPathRooted(DatabasePath)
                ? DatabasePath
                : Path.Combine(AppContext.BaseDirectory, DatabasePath);
        }

        /// <summary>
        /// Возвращает десятичный разделитель значений файла smp_smzu.csv.
        /// </summary>
        /// <returns>Точка или запятая.</returns>
        public char GetCsvDecimalSeparator()
        {
            return CsvDecimalSeparator == "."
                ? '.'
                : ',';
        }
    }
}
