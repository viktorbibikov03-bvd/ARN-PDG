using System;

namespace VrdoPdg.Model
{
    /// <summary>
    /// Ошибка выполнения операции АС «ВРДО ПДГ»
    /// Сообщение исключения предназначено для вывода пользователю
    /// </summary>
    public class VrdoException : Exception
    {
        /// <summary>
        /// Создает исключение
        /// </summary>
        /// <param name="message">Сообщение для пользователя</param>
        /// <param name="details">Пояснение к сообщению</param>
        public VrdoException(string message, string? details)
            : base(message)
        {
            Details = details ?? string.Empty;
        }

        /// <summary>
        /// Создает исключение с внутренним исключением
        /// </summary>
        /// <param name="message">Сообщение для пользователя</param>
        /// <param name="details">Пояснение к сообщению</param>
        /// <param name="innerException">Исключение, вызвавшее ошибку</param>
        public VrdoException(string message, string? details, Exception innerException)
            : base(message, innerException)
        {
            Details = details ?? string.Empty;
        }

        /// <summary>
        /// Пояснение к сообщению: путь к файлу, номер строки, текст исходной ошибки.
        /// </summary>
        public string Details { get; }
    }
}
