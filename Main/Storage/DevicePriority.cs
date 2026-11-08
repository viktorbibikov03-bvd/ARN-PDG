namespace VrdoPdg.Model.Storage
{
    /// <summary>
    /// Действующий приоритет СКРМ в таблице эффективности КП.
    /// </summary>
    public sealed class DevicePriority
    {
        /// <summary>
        /// Создает приоритет СКРМ.
        /// </summary>
        /// <param name="deviceId">Код СКРМ.</param>
        /// <param name="priority">Действующий приоритет.</param>
        public DevicePriority(int deviceId, int priority)
        {
            DeviceId = deviceId;
            Priority = priority;
        }

        /// <summary>
        /// Код СКРМ.
        /// </summary>
        public int DeviceId { get; }

        /// <summary>
        /// Действующий приоритет.
        /// </summary>
        public int Priority { get; }
    }
}
