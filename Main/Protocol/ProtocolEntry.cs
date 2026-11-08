using System;

namespace VrdoPdg.Model.Protocol
{
    /// <summary>
    /// Запись протокола расчета.
    /// </summary>
    public sealed class ProtocolEntry
    {
        /// <summary>
        /// Создает запись протокола.
        /// </summary>
        /// <param name="id">Порядковый номер записи.</param>
        /// <param name="time">Время записи.</param>
        /// <param name="tag">Метка типа записи.</param>
        /// <param name="message">Текст сообщения.</param>
        /// <param name="details">Пояснение к сообщению.</param>
        /// <param name="parent">Родительская запись.</param>
        internal ProtocolEntry(
            int id,
            DateTime time,
            ProtocolTag tag,
            string message,
            string details,
            ProtocolEntry? parent)
        {
            Id = id;
            Time = time;
            Tag = tag;
            Message = message;
            Details = details;
            Parent = parent;
            Level = parent == null
                ? 0
                : parent.Level + 1;
        }

        /// <summary>
        /// Порядковый номер записи в протоколе.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Время записи.
        /// </summary>
        public DateTime Time { get; }

        /// <summary>
        /// Метка типа записи.
        /// </summary>
        public ProtocolTag Tag { get; }

        /// <summary>
        /// Текст сообщения.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Пояснение к сообщению, выводимое серым шрифтом.
        /// </summary>
        public string Details { get; }

        /// <summary>
        /// Родительская запись: КП или час; <c>null</c> для записей верхнего уровня.
        /// </summary>
        public ProtocolEntry? Parent { get; }

        /// <summary>
        /// Уровень вложенности записи (0 — верхний уровень).
        /// </summary>
        public int Level { get; }
    }
}
