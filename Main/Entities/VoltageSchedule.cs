using System.Collections.Generic;
using System.Linq;

namespace VrdoPdg.Model.Entities
{
    /// <summary>
    /// Графики напряжений в КП: границы допустимой области по каждому КП и часу суток
    /// </summary>
    public sealed class VoltageSchedule
    {
        /// <summary>
        /// Границы графиков по коду КП и часу суток
        /// </summary>
        private readonly Dictionary<(int ControlPointId, int Hour), VoltageBounds> _bounds;

        /// <summary>
        /// Создает графики напряжений
        /// </summary>
        /// <param name="bounds">Границы графиков по КП и часам.</param>
        public VoltageSchedule(IEnumerable<VoltageBounds> bounds)
        {
            _bounds = new Dictionary<(int ControlPointId, int Hour), VoltageBounds>();
            foreach (VoltageBounds item in bounds)
            {
                _bounds[(item.ControlPointId, item.Hour)] = item;
            }
        }

        /// <summary>
        /// Возвращает границы графика КП в заданном часе
        /// </summary>
        /// <param name="controlPointId">Код КП.</param>
        /// <param name="hour">Час суток (1–24).</param>
        /// <returns>Границы графика или <c>null</c>, если график не задан.</returns>
        public VoltageBounds? GetBounds(int controlPointId, int hour)
        {
            VoltageBounds? bounds;
            return _bounds.TryGetValue((controlPointId, hour), out bounds)
                ? bounds
                : null;
        }

        /// <summary>
        /// Проверяет, задан ли график напряжения КП на все часы суток.
        /// </summary>
        /// <param name="controlPointId">Код КП.</param>
        /// <param name="hourCount">Количество часов в сутках.</param>
        /// <returns><c>true</c>, если границы заданы на все часы.</returns>
        public bool IsComplete(int controlPointId, int hourCount)
        {
            for (int hour = 1; hour <= hourCount; hour++)
            {
                if (!_bounds.ContainsKey((controlPointId, hour)))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
