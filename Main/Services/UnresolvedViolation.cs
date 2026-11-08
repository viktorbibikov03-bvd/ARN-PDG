using VrdoPdg.Model.Entities;

namespace VrdoPdg.Model.Services
{
    /// <summary>
    /// КП и час, в которых напряжение не введено в допустимую область.
    /// </summary>
    public sealed class UnresolvedViolation
    {
        /// <summary>
        /// Создает сведения о невведенном режиме.
        /// </summary>
        /// <param name="controlPoint">КП.</param>
        /// <param name="hour">Час суток (1–24).</param>
        /// <param name="voltage">Напряжение после расчета, кВ.</param>
        /// <param name="bounds">Границы графика напряжения.</param>
        public UnresolvedViolation(ControlPoint controlPoint, int hour, double voltage, VoltageBounds bounds)
        {
            ControlPoint = controlPoint;
            Hour = hour;
            Voltage = voltage;
            Bounds = bounds;
        }

        /// <summary>
        /// КП.
        /// </summary>
        public ControlPoint ControlPoint { get; }

        /// <summary>
        /// Час суток (1–24).
        /// </summary>
        public int Hour { get; }

        /// <summary>
        /// Напряжение после расчета, кВ.
        /// </summary>
        public double Voltage { get; }

        /// <summary>
        /// Границы графика напряжения.
        /// </summary>
        public VoltageBounds Bounds { get; }
    }
}
