using System;
using System.Collections.Generic;
using System.Linq;

namespace VrdoPdg.Model.Protocol
{
    /// <summary>
    /// Протокол расчета: хранит записи о действиях системы в иерархии
    /// «КП → час → действия с СКРМ и результаты расчета УР».
    /// </summary>
    /// <remarks>
    /// Записи добавляются сервисами прикладного уровня из рабочего потока,
    /// поэтому все операции протокола потокобезопасны.
    /// </remarks>
    public sealed class CalculationProtocol
    {
        /// <summary>
        /// Объект синхронизации доступа к записям.
        /// </summary>
        private readonly object _syncRoot = new object();

        /// <summary>
        /// Записи в хронологическом порядке.
        /// </summary>
        private readonly List<ProtocolEntry> _entries = new List<ProtocolEntry>();

        /// <summary>
        /// Количество записей по меткам.
        /// </summary>
        private readonly Dictionary<ProtocolTag, int> _counts = new Dictionary<ProtocolTag, int>();

        /// <summary>
        /// Возникает при добавлении записи в протокол. Обработчик вызывается
        /// в потоке, добавившем запись.
        /// </summary>
        public event EventHandler<ProtocolEntryEventArgs>? EntryAdded;

        /// <summary>
        /// Общее количество записей.
        /// </summary>
        public int Count
        {
            get
            {
                lock (_syncRoot)
                {
                    return _entries.Count;
                }
            }
        }

        /// <summary>
        /// Добавляет запись в протокол.
        /// </summary>
        /// <param name="tag">Метка типа записи.</param>
        /// <param name="message">Текст сообщения.</param>
        /// <param name="details">Пояснение к сообщению.</param>
        /// <param name="parent">Родительская запись.</param>
        /// <returns>Добавленная запись.</returns>
        public ProtocolEntry Add(ProtocolTag tag, string message, string? details = null, ProtocolEntry? parent = null)
        {
            ProtocolEntry entry;
            lock (_syncRoot)
            {
                entry = new ProtocolEntry(
                    _entries.Count + 1,
                    DateTime.Now,
                    tag,
                    message ?? string.Empty,
                    details ?? string.Empty,
                    parent);
                _entries.Add(entry);

                int count;
                _counts.TryGetValue(tag, out count);
                _counts[tag] = count + 1;
            }

            EntryAdded?.Invoke(this, new ProtocolEntryEventArgs(entry));
            return entry;
        }

        /// <summary>
        /// Возвращает все записи в хронологическом порядке.
        /// </summary>
        /// <returns>Копия списка записей.</returns>
        public IReadOnlyList<ProtocolEntry> GetEntries()
        {
            lock (_syncRoot)
            {
                return _entries.ToList();
            }
        }

        /// <summary>
        /// Возвращает количество записей с заданной меткой.
        /// </summary>
        /// <param name="tag">Метка типа записи.</param>
        /// <returns>Количество записей.</returns>
        public int GetCount(ProtocolTag tag)
        {
            lock (_syncRoot)
            {
                int count;
                return _counts.TryGetValue(tag, out count)
                    ? count
                    : 0;
            }
        }

        /// <summary>
        /// Проверяет, вложена ли запись в заданную родительскую запись.
        /// </summary>
        /// <param name="entry">Проверяемая запись.</param>
        /// <param name="parent">Родительская запись.</param>
        /// <returns><c>true</c>, если запись вложена в родительскую на любом уровне.</returns>
        private static bool IsDescendant(ProtocolEntry entry, ProtocolEntry parent)
        {
            for (ProtocolEntry? current = entry.Parent; current != null; current = current.Parent)
            {
                if (ReferenceEquals(current, parent))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
