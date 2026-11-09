using System.Collections.Generic;

namespace VrdoPdg.View.Layout
{
    /// <summary>
    /// Блок таблицы по часам: КП, выводимые в одной таблице из 24 строк.
    /// </summary>
    internal sealed class HourlyLayoutBlock
    {
        /// <summary>
        /// Создает блок.
        /// </summary>
        /// <param name="controlPoints">КП блока.</param>
        public HourlyLayoutBlock(IReadOnlyList<HourlyLayoutControlPoint> controlPoints)
        {
            ControlPoints = controlPoints;
        }

        /// <summary>
        /// КП блока.
        /// </summary>
        public IReadOnlyList<HourlyLayoutControlPoint> ControlPoints { get; }
    }
}
