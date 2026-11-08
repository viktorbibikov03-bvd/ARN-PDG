using VrdoPdg.Model.Entities;
using VrdoPdg.Model.Regime;

namespace VrdoPdg.View.Controls
{
    /// <summary>
    /// Источник значений таблицы по часам: режим и границы графиков, по которым
    /// выделяются недопустимые значения напряжения.
    /// </summary>
    internal sealed class HourlyValueSource
    {
        /// <summary>
        /// Создает источник значений.
        /// </summary>
        /// <param name="regime">Режим по часам.</param>
        /// <param name="schedule">Графики напряжений в КП.</param>
        public HourlyValueSource(RegimeSnapshot regime, VoltageSchedule schedule)
        {
            Regime = regime;
            Schedule = schedule;
        }

        /// <summary>
        /// Режим по часам.
        /// </summary>
        public RegimeSnapshot Regime { get; }

        /// <summary>
        /// Графики напряжений в КП.
        /// </summary>
        public VoltageSchedule Schedule { get; }
    }
}
