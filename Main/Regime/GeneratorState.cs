namespace VrdoPdg.Model.Regime
{
    /// <summary>
    /// Состояние и загрузка генератора по данным таблицы «Генераторы УР» МТ.
    /// </summary>
    public sealed class GeneratorState
    {
        /// <summary>
        /// Создает состояние генератора.
        /// </summary>
        /// <param name="isOn">Признак состояния «Включено».</param>
        /// <param name="nodeNumber">Номер узла подключения генератора.</param>
        /// <param name="activePower">Активная мощность, МВт.</param>
        /// <param name="reactivePower">Реактивная мощность, Мвар.</param>
        /// <param name="setVoltage">Заданное напряжение на шинах генератора, кВ.</param>
        /// <param name="minReactivePower">Нижний предел реактивной мощности из таблицы генераторов, Мвар.</param>
        /// <param name="maxReactivePower">Верхний предел реактивной мощности из таблицы генераторов, Мвар.</param>
        /// <param name="pqDiagramNumber">Номер PQ-диаграммы (0 — не задана).</param>
        public GeneratorState(
            bool isOn,
            int nodeNumber,
            double activePower,
            double reactivePower,
            double setVoltage,
            double minReactivePower,
            double maxReactivePower,
            int pqDiagramNumber)
        {
            IsOn = isOn;
            NodeNumber = nodeNumber;
            ActivePower = activePower;
            ReactivePower = reactivePower;
            SetVoltage = setVoltage;
            MinReactivePower = minReactivePower;
            MaxReactivePower = maxReactivePower;
            PQDiagramNumber = pqDiagramNumber;
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
        /// Нижний предел реактивной мощности из таблицы генераторов, Мвар.
        /// </summary>
        public double MinReactivePower { get; }

        /// <summary>
        /// Верхний предел реактивной мощности из таблицы генераторов, Мвар.
        /// </summary>
        public double MaxReactivePower { get; }

        /// <summary>
        /// Номер PQ-диаграммы генератора (0 — диаграмма не задана).
        /// </summary>
        public int PQDiagramNumber { get; }
    }
}
