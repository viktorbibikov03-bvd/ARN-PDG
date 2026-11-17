using System;
using System.Collections.Generic;
using System.Linq;

namespace VrdoPdg.Model.Protocol
{
    /// <summary>
    /// Протокол расчета: хранит записи о действиях системы в иерархии
    /// «КП → час → действия с СКРМ и результаты расчета УР»
    /// </summary>
    /// <remarks>
    /// Записи добавляются сервисами прикладного уровня из рабочего потока,
    /// поэтому все операции протокола потокобезопасны
    /// </remarks>
    public sealed class CalculationProtocol
    {
        /// <summary>
        /// Объект синхронизации доступа к записям
        /// </summary>
        private readonly object _syncRoot = new object();

        /// <summary>
        /// Записи в хронологическом порядке
        /// </summary>
        private readonly List<ProtocolEntry> _entries = new List<ProtocolEntry>();

        /// <summary>
        /// Количество записей по меткам
        /// </summary>
        private readonly Dictionary<ProtocolTag, int> _counts = new Dictionary<ProtocolTag, int>();

        /// <summary>
        /// Последний выданный номер записи. При очистке не сбрасывается.
        /// </summary>
        private int _lastEntryId;

        /// <summary>
        /// Номер, до которого записи были удалены включительно.
        /// </summary>
        private int _clearedThroughId;

        /// <summary>
        /// Возникает при добавлении записи в протокол. Обработчик вызывается
        /// в потоке, добавившем запись
        /// </summary>
        public event EventHandler<ProtocolEntryEventArgs>? EntryAdded;

        /// <summary>
        /// Общее количество записей
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
        /// Добавляет запись в протокол
        /// </summary>
        /// <param name="tag">Метка типа записи</param>
        /// <param name="message">Текст сообщения</param>
        /// <param name="details">Пояснение к сообщению</param>
        /// <param name="parent">Родительская запись</param>
        /// <returns>Добавленная запись</returns>
        public ProtocolEntry Add(ProtocolTag tag,
            string message, string? details = null,
            ProtocolEntry? parent = null)
        {
            ProtocolEntry entry;

            lock (_syncRoot)
            {
                if (parent != null && parent.Id <= _clearedThroughId)
                {
                    parent = null;
                }

                entry = new ProtocolEntry(
                    ++_lastEntryId,
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
        /// Возвращает все записи в хронологическом порядке
        /// </summary>
        /// <returns>Копия списка записей</returns>
        public IReadOnlyList<ProtocolEntry> GetEntries()
        {
            lock (_syncRoot)
            {
                return _entries.ToList();
            }
        }

        /// <summary>
        /// Возвращает количество записей с заданной меткой
        /// </summary>
        /// <param name="tag">Метка типа записи</param>
        /// <returns>Количество записей</returns>
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
        /// Удаляет записи протокола и сбрасывает счетчики меток.
        /// </summary>
        /// <returns>
        /// Последний удаленный номер записи.
        /// </returns>
        public int Clear()
        {
            lock (_syncRoot)
            {
                _entries.Clear();
                _counts.Clear();

                _clearedThroughId = _lastEntryId;

                return _clearedThroughId;
            }
        }
    }
}
