namespace VrdoPdg.View
{
    /// <summary>
    /// Настройка постоянных границ графика напряжения отдельного КП в файле appsettings.json
    /// </summary>
    internal sealed class ControlPointBoundsSetting
    {
        /// <summary>
        /// Наименование КП в БД
        /// </summary>
        public string ControlPoint { get; set; } = string.Empty;

        /// <summary>
        /// Нижняя граница графика, кВ
        /// </summary>
        public double LowerBound { get; set; }

        /// <summary>
        /// Верхняя граница графика, кВ
        /// </summary>
        public double UpperBound { get; set; }
    }
}
