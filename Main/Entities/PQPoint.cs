namespace VrdoPdg.Model.Entities
{
    /// <summary>
    /// Точка PQ-диаграммы генератора: допустимый диапазон реактивной мощности
    /// при заданной активной мощности.
    /// </summary>
    public sealed class PQPoint
    {
        /// <summary>
        /// Создает точку PQ-диаграммы.
        /// </summary>
        /// <param name="activePower">Активная мощность, МВт.</param>
        /// <param name="minReactivePower">Минимальная реактивная мощность, Мвар.</param>
        /// <param name="maxReactivePower">Максимальная реактивная мощность, Мвар.</param>
        public PQPoint(double activePower, double minReactivePower, double maxReactivePower)
        {
            ActivePower = activePower;
            MinReactivePower = minReactivePower;
            MaxReactivePower = maxReactivePower;
        }

        /// <summary>
        /// Активная мощность, МВт.
        /// </summary>
        public double ActivePower { get; }

        /// <summary>
        /// Минимальная реактивная мощность, Мвар.
        /// </summary>
        public double MinReactivePower { get; }

        /// <summary>
        /// Максимальная реактивная мощность, Мвар.
        /// </summary>
        public double MaxReactivePower { get; }
    }
}
