using VrdoPdg.Model.Entities;

namespace VrdoPdg.Model.Services
{
    /// <summary>
    /// Редактируемая строка таблицы эффективности КП.
    /// </summary>
    public sealed class EfficiencyRow
    {
        /// <summary>
        /// Создает строку таблицы эффективности.
        /// </summary>
        /// <param name="priority">Действующий приоритет.</param>
        /// <param name="deviceType">Тип СКРМ.</param>
        /// <param name="deviceNumber">Номер узла, реактора или узла подключения генератора</param>
        /// <param name="deviceId">Код СКРМ в БД; <c>null</c> — строка добавлена или изменена пользователем</param>
        public EfficiencyRow(int? priority, DeviceType deviceType, int? deviceNumber, int? deviceId = null)
        {
            Priority = priority;
            DeviceType = deviceType;
            DeviceNumber = deviceNumber;
            DeviceId = deviceId;
        }

        /// <summary>
        /// Действующий приоритет.
        /// </summary>
        public int? Priority { get; set; }

        /// <summary>
        /// Тип СКРМ.
        /// </summary>
        public DeviceType DeviceType { get; set; }

        /// <summary>
        /// Номер узла (управляемые), реактора (коммутируемые) или узла подключения генератора
        /// по таблице «Узлы» МТ (генераторы); если МТ не загружена — номер генератора
        /// </summary>
        public int? DeviceNumber { get; set; }

        /// <summary>
        /// Код СКРМ в БД, загруженного в строку; сбрасывается при изменении типа или номера СКРМ.
        /// Позволяет различать генераторы, подключенные к одному узлу
        /// </summary>
        public int? DeviceId { get; set; }

        /// <summary>
        /// Создает копию строки.
        /// </summary>
        /// <returns>Копия строки.</returns>
        public EfficiencyRow Clone()
        {
            return new EfficiencyRow(Priority, DeviceType, DeviceNumber, DeviceId);
        }

        /// <summary>
        /// Проверяет, совпадает ли строка с другой строкой по приоритету и СКРМ.
        /// </summary>
        /// <param name="other">Другая строка.</param>
        /// <returns><c>true</c>, если строки совпадают.</returns>
        public bool IsSameAs(EfficiencyRow? other)
        {
            return other != null
                && other.Priority == Priority
                && other.DeviceType == DeviceType
                && other.DeviceNumber == DeviceNumber;
        }
    }
}
