namespace VrdoPdg.Model.Regime
{
    /// <summary>
    /// Значения наблюдаемого узла таблицы «Узлы» МТ в определенном часе суток
    /// </summary>
    public sealed class ObservedNodeState
    {
        /// <summary>
        /// Создает значения узла.
        /// </summary>
        /// <param name="nodeNumber">Номер узла</param>
        /// <param name="hour">Час суток</param>
        /// <param name="isOn">Признак включенного узла</param>
        /// <param name="voltage">Расчетное напряжение, кВ; 0 для отключенного узла</param>
        /// <param name="setVoltage">Заданное напряжение, кВ</param>
        /// <param name="minReactivePower">Минимальная генерация Q, Мвар</param>
        public ObservedNodeState(
            int nodeNumber,
            int hour,
            bool isOn,
            double voltage,
            double setVoltage,
            double minReactivePower)
        {
            NodeNumber = nodeNumber;
            Hour = hour;
            IsOn = isOn;
            Voltage = voltage;
            SetVoltage = setVoltage;
            MinReactivePower = minReactivePower;
        }

        /// <summary>
        /// Номер узла
        /// </summary>
        public int NodeNumber { get; }

        /// <summary>
        /// Час суток
        /// </summary>
        public int Hour { get; }

        /// <summary>
        /// Признак включенного узла
        /// </summary>
        public bool IsOn { get; }

        /// <summary>
        /// Расчетное напряжение, кВ; 0 для отключенного узла
        /// </summary>
        public double Voltage { get; }

        /// <summary>
        /// Заданное напряжение, кВ
        /// </summary>
        public double SetVoltage { get; }

        /// <summary>
        /// Минимальная генерация Q, Мвар
        /// </summary>
        public double MinReactivePower { get; }
    }
}
