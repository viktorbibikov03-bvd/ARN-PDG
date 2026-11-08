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
        /// <param name="deviceNumber">Номер узла, реактора или генератора.</param>
        public EfficiencyRow(int? priority, DeviceType deviceType, int? deviceNumber)
        {
            Priority = priority;
            DeviceType = deviceType;
            DeviceNumber = deviceNumber;
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
        /// Номер узла (управляемые), реактора (коммутируемые) или генератора (генераторы).
        /// </summary>
        public int? DeviceNumber { get; set; }

        /// <summary>
        /// Создает копию строки.
        /// </summary>
        /// <returns>Копия строки.</returns>
        public EfficiencyRow Clone()
        {
            return new EfficiencyRow(Priority, DeviceType, DeviceNumber);
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
