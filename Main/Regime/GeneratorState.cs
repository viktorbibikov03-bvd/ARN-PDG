namespace VrdoPdg.Model.Regime
{
    /// <summary>
    /// Состояние и загрузка генератора: таблица «Генераторы УР» и строка узла подключения
    /// в таблице «Узлы» МТ.
    /// </summary>
    public sealed class GeneratorState
    {
        /// <summary>
        /// Создает состояние генератора.
        /// </summary>
        /// <param name="isOn">Признак состояния «Включено».</param>
        /// <param name="nodeNumber">Номер узла подключения генератора.</param>
        /// <param name="activePower">Активная мощность, МВт.</param>
        /// <param name="reactivePower">Реактивная мощность генерации узла (qg), Мвар.</param>
        /// <param name="setVoltage">Заданное напряжение узла (vzd), кВ.</param>
        /// <param name="minReactivePower">Минимальная генерация Q узла (qmin), Мвар.</param>
        /// <param name="maxReactivePower">Максимальная генерация Q узла (qmax), Мвар.</param>
        public GeneratorState(
            bool isOn,
            int nodeNumber,
            double activePower,
            double reactivePower,
            double setVoltage,
            double minReactivePower,
            double maxReactivePower)
        {
            IsOn = isOn;
            NodeNumber = nodeNumber;
            ActivePower = activePower;
            ReactivePower = reactivePower;
            SetVoltage = setVoltage;
            MinReactivePower = minReactivePower;
            MaxReactivePower = maxReactivePower;
        }

        /// <summary>
        /// Признак состояния «Включено».
        /// </summary>
        public bool IsOn { get; }

        /// <summary>
        /// Номер узла подключения генератора.
        /// </summary>
        public int NodeNumber { get; }

        /// <summary>
        /// Активная мощность, МВт.
        /// </summary>
        public double ActivePower { get; }

        /// <summary>
        /// Реактивная мощность, Мвар.
        /// </summary>
        public double ReactivePower { get; }

        /// <summary>
        /// Заданное напряжение на шинах генератора, кВ.
        /// </summary>
        public double SetVoltage { get; }

        /// <summary>
        /// Минимальная генерация Q узла подключения (qmin), Мвар.
        /// </summary>
        public double MinReactivePower { get; }

        /// <summary>
        /// Максимальная генерация Q узла подключения (qmax), Мвар.
        /// </summary>
        public double MaxReactivePower { get; }
    }
}
