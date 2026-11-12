using System.Collections.Generic;

namespace VrdoPdg.Model.Protocol
{
    /// <summary>
    /// Подписи меток записей протокола расчета
    /// </summary>
    public static class ProtocolTags
    {
        /// <summary>
        /// Все метки в порядке вывода на панели фильтров
        /// </summary>
        public static IReadOnlyList<ProtocolTag> All { get; } = new[]
        {
            ProtocolTag.Vrdo,
            ProtocolTag.Hour,
            ProtocolTag.Device,
            ProtocolTag.Regime,
            ProtocolTag.Warning,
            ProtocolTag.Info,
            ProtocolTag.Error
        };

        /// <summary>
        /// Возвращает подпись метки
        /// </summary>
        /// <param name="tag">Метка записи</param>
        /// <returns>Подпись метки: ВРДО, ЧАС, СКРМ, УР, WARN, INFO или ERR</returns>
        public static string GetLabel(ProtocolTag tag)
        {
            switch (tag)
            {
                case ProtocolTag.Vrdo:
                    return "ВРДО";
                case ProtocolTag.Hour:
                    return "ЧАС";
                case ProtocolTag.Device:
                    return "СКРМ";
                case ProtocolTag.Regime:
                    return "УР";
                case ProtocolTag.Warning:
                    return "WARN";
                case ProtocolTag.Info:
                    return "INFO";
                default:
                    return "ERR";
            }
        }
    }
}
