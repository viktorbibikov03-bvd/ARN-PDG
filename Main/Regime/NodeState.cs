namespace VrdoPdg.Model.Regime
{
    /// <summary>
    /// Значения строки узла таблицы «Узлы» МТ
    /// </summary>
    public sealed class NodeState
    {
        /// <summary>
        /// Создает значения строки узла
        /// </summary>
        /// <param name="isOn">Признак включенного узла</param>
        /// <param name="voltage">Расчетное напряжение (vras), кВ</param>
        /// <param name="setVoltage">Заданное напряжение (vzd), кВ</param>
        /// <param name="minReactivePower">Минимальная генерация Q (qmin), Мвар</param>
        public NodeState(bool isOn, double voltage, double setVoltage, double minReactivePower)
        {
            IsOn = isOn;
            Voltage = voltage;
            SetVoltage = setVoltage;
            MinReactivePower = minReactivePower;
        }

        /// <summary>
        /// Признак включенного узла
        /// </summary>
        public bool IsOn { get; }

        /// <summary>
        /// Расчетное напряжение (vras), кВ
        /// </summary>
        public double Voltage { get; }

        /// <summary>
        /// Заданное напряжение (vzd), кВ
        /// </summary>
        public double SetVoltage { get; }

        /// <summary>
        /// Минимальная генерация Q (qmin), Мвар
        /// </summary>
        public double MinReactivePower { get; }
    }
}
