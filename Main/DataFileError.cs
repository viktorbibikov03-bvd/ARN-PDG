namespace VrdoPdg.Model
{
    /// <summary>
    /// Вид ошибки файла исходных данных.
    /// </summary>
    public enum DataFileError
    {
        /// <summary>
        /// Не найден файл с исходными данными.
        /// </summary>
        NotFound,

        /// <summary>
        /// Формат файла не распознан.
        /// </summary>
        UnknownFormat,

        /// <summary>
        /// Нет доступа к файлу.
        /// </summary>
        AccessDenied,

        /// <summary>
        /// Неправильно заполнен файл исходных данных.
        /// </summary>
        InvalidContent
    }
}
