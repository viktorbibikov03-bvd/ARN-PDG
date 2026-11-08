using System;

namespace VrdoPdg.Model
{
    /// <summary>
    /// Ошибка файла исходных данных: файл не найден, формат не распознан,
    /// нет доступа к файлу или файл неправильно заполнен.
    /// </summary>
    public sealed class DataFileException : VrdoException
    {
        /// <summary>
        /// Создает исключение.
        /// </summary>
        /// <param name="error">Вид ошибки.</param>
        /// <param name="filePath">Путь к файлу.</param>
        /// <param name="details">Пояснение к ошибке.</param>
        public DataFileException(DataFileError error, string filePath, string? details)
            : base(GetMessage(error), BuildDetails(filePath, details))
        {
            Error = error;
            FilePath = filePath ?? string.Empty;
        }

        /// <summary>
        /// Создает исключение с внутренним исключением.
        /// </summary>
        /// <param name="error">Вид ошибки.</param>
        /// <param name="filePath">Путь к файлу.</param>
        /// <param name="details">Пояснение к ошибке.</param>
        /// <param name="innerException">Исключение, вызвавшее ошибку.</param>
        public DataFileException(DataFileError error, string filePath, string? details, Exception innerException)
            : base(GetMessage(error), BuildDetails(filePath, details), innerException)
        {
            Error = error;
            FilePath = filePath ?? string.Empty;
        }

        /// <summary>
        /// Вид ошибки.
        /// </summary>
        public DataFileError Error { get; }

        /// <summary>
        /// Путь к файлу.
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// Возвращает сообщение об ошибке файла для протокола и пользователя.
        /// </summary>
        /// <param name="error">Вид ошибки.</param>
        /// <returns>Текст сообщения.</returns>
        public static string GetMessage(DataFileError error)
        {
            switch (error)
            {
                case DataFileError.NotFound:
                    return "Не найден файл с исходными данными";
                case DataFileError.UnknownFormat:
                    return "Формат файла не распознан";
                case DataFileError.AccessDenied:
                    return "Нет доступа к файлу";
                default:
                    return "Неправильно заполнен файл исходных данных";
            }
        }

        /// <summary>
        /// Формирует пояснение из пути к файлу и дополнительного текста.
        /// </summary>
        /// <param name="filePath">Путь к файлу.</param>
        /// <param name="details">Дополнительный текст.</param>
        /// <returns>Пояснение к ошибке.</returns>
        private static string BuildDetails(string filePath, string? details)
        {
            return string.IsNullOrEmpty(details)
                ? filePath ?? string.Empty
                : filePath + " — " + details;
        }
    }
}
