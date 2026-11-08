using System;

namespace VrdoPdg.Model.Protocol
{
    /// <summary>
    /// Аргументы события добавления записи в протокол расчета.
    /// </summary>
    public sealed class ProtocolEntryEventArgs : EventArgs
    {
        /// <summary>
        /// Создает аргументы события.
        /// </summary>
        /// <param name="entry">Добавленная запись.</param>
        public ProtocolEntryEventArgs(ProtocolEntry entry)
        {
            Entry = entry;
        }

        /// <summary>
        /// Добавленная запись.
        /// </summary>
        public ProtocolEntry Entry { get; }
    }
}
