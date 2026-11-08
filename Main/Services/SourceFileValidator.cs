using System;
using System.IO;

namespace VrdoPdg.Model.Services
{
    /// <summary>
    /// Проверка наличия, доступности и формата файлов исходных данных.
    /// </summary>
    public static class SourceFileValidator
    {
        /// <summary>
        /// Расширение файла МТ в формате СМЗУ.
        /// </summary>
        public const string MegapointExtension = ".mptsmz";

        /// <summary>
        /// Расширение файла топологии smp_smzu.csv.
        /// </summary>
        public const string SmpSmzuExtension = ".csv";

        /// <summary>
        /// Проверяет файл исходных данных.
        /// </summary>
        /// <param name="path">Путь к файлу.</param>
        /// <param name="expectedExtension">Ожидаемое расширение файла.</param>
        /// <exception cref="DataFileException">Файл не прошел проверку.</exception>
        public static void Validate(string path, string expectedExtension)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new DataFileException(DataFileError.NotFound, string.Empty, "путь к файлу не задан");
            }

            string extension = Path.GetExtension(path);
            if (!string.Equals(extension, expectedExtension, StringComparison.OrdinalIgnoreCase))
            {
                throw new DataFileException(
                    DataFileError.UnknownFormat,
                    path,
                    "ожидается файл с расширением " + expectedExtension);
            }

            if (!File.Exists(path))
            {
                throw new DataFileException(DataFileError.NotFound, path, null);
            }

            try
            {
                using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    if (!stream.CanRead)
                    {
                        throw new DataFileException(DataFileError.AccessDenied, path, null);
                    }
                }
            }
            catch (UnauthorizedAccessException exception)
            {
                throw new DataFileException(DataFileError.AccessDenied, path, exception.Message, exception);
            }
            catch (IOException exception)
            {
                throw new DataFileException(DataFileError.AccessDenied, path, exception.Message, exception);
            }
        }
    }
}
