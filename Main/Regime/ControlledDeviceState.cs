namespace VrdoPdg.Model.Regime
{
    /// <summary>
    /// Состояние и загрузка управляемого СКРМ по данным таблицы «Узлы» МТ
    /// </summary>
    public sealed class ControlledDeviceState
    {
        /// <summary>
        /// Создает состояние управляемого СКРМ
        /// </summary>
        /// <param name="isOn">Признак состояния «Включено»</param>
        /// <param name="reactivePower">Загрузка по реактивной мощности, Мвар</param>
        /// <param name="minReactivePower">Нижний предел реактивной мощности, Мвар</param>
        /// <param name="maxReactivePower">Верхний предел реактивной мощности, Мвар</param>
        /// <param name="setVoltage">Заданное напряжение в узле, кВ</param>
        public ControlledDeviceState(
            bool isOn,
            double reactivePower,
            double minReactivePower,
            double maxReactivePower,
            double setVoltage)
        {
            IsOn = isOn;
            ReactivePower = reactivePower;
            MinReactivePower = minReactivePower;
            MaxReactivePower = maxReactivePower;
            SetVoltage = setVoltage;
        }

        /// <summary>
        /// Признак состояния «Включено»
        /// </summary>
        public bool IsOn { get; }

        /// <summary>
        /// Загрузка по реактивной мощности, Мвар
        /// </summary>
        public double ReactivePower { get; }

        /// <summary>
        /// Нижний предел реактивной мощности (Qmin), Мвар
        /// </summary>
        public double MinReactivePower { get; }

        /// <summary>
        /// Верхний предел реактивной мощности (Qmax), Мвар
        /// </summary>
        public double MaxReactivePower { get; }

        /// <summary>
        /// Заданное напряжение в узле, кВ
        /// </summary>
        public double SetVoltage { get; }
    }
}
