namespace VrdoPdg.Model.Entities
{
    /// <summary>
    /// Строка таблицы эффективности: СКРМ, влияющее на напряжение в КП,
    /// с действующим приоритетом и приоритетом по умолчанию (согласно ПУР).
    /// </summary>
    public sealed class EfficiencyEntry
    {
        /// <summary>
        /// Создает строку таблицы эффективности.
        /// </summary>
        /// <param name="controlPointId">Код КП.</param>
        /// <param name="deviceId">Код СКРМ.</param>
        /// <param name="priority">Действующий приоритет.</param>
        /// <param name="defaultPriority">Приоритет согласно ПУР.</param>
        public EfficiencyEntry(int controlPointId, int deviceId, int? priority, int? defaultPriority)
        {
            ControlPointId = controlPointId;
            DeviceId = deviceId;
            Priority = priority;
            DefaultPriority = defaultPriority;
        }

        /// <summary>
        /// Код КП.
        /// </summary>
        public int ControlPointId { get; }

        /// <summary>
        /// Код СКРМ.
        /// </summary>
        public int DeviceId { get; }

        /// <summary>
        /// Действующий приоритет; <c>null</c>, если СКРМ исключено технологом
        /// из действующей таблицы.
        /// </summary>
        public int? Priority { get; }

        /// <summary>
        /// Приоритет согласно ПУР; <c>null</c>, если СКРМ добавлено технологом
        /// и в ПУР не входит.
        /// </summary>
        public int? DefaultPriority { get; }
    }
}
