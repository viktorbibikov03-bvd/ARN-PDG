namespace VrdoPdg.Model.Entities
{
    /// <summary>
    /// Пределы реактивной мощности, Мвар
    /// </summary>
    public readonly struct ReactivePowerLimits
    {
        /// <summary>
        /// Создает пределы реактивной мощности.
        /// </summary>
        /// <param name="min">Нижний предел, Мвар</param>
        /// <param name="max">Верхний предел, Мвар</param>
        public ReactivePowerLimits(double min, double max)
        {
            Min = min;
            Max = max;
        }

        /// <summary>
        /// Нижний предел реактивной мощности, Мвар
        /// </summary>
        public double Min { get; }

        /// <summary>
        /// Верхний предел реактивной мощности, Мвар
        /// </summary>
        public double Max { get; }

        /// <summary>
        /// Ширина диапазона регулирования, Мвар
        /// </summary>
        public double Range => Max - Min;
    }
}
