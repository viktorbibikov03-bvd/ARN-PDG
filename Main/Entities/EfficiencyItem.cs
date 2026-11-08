namespace VrdoPdg.Model.Entities
{
    /// <summary>
    /// СКРМ действующей таблицы эффективности КП с приоритетом.
    /// </summary>
    public sealed class EfficiencyItem
    {
        /// <summary>
        /// Создает элемент таблицы эффективности.
        /// </summary>
        /// <param name="device">СКРМ.</param>
        /// <param name="priority">Действующий приоритет.</param>
        /// <param name="defaultPriority">Приоритет согласно ПУР.</param>
        public EfficiencyItem(Device device, int priority, int? defaultPriority)
        {
            Device = device;
            Priority = priority;
            DefaultPriority = defaultPriority;
        }

        /// <summary>
        /// СКРМ.
        /// </summary>
        public Device Device { get; }

        /// <summary>
        /// Действующий приоритет.
        /// </summary>
        public int Priority { get; }

        /// <summary>
        /// Приоритет согласно ПУР; <c>null</c>, если СКРМ в ПУР не входит.
        /// </summary>
        public int? DefaultPriority { get; }
    }
}
