using System.Collections.Generic;

using VrdoPdg.Model.Entities;
using VrdoPdg.Model.Regime;
using VrdoPdg.Model.Services;

namespace VrdoPdg.View.Layout
{
    /// <summary>
    /// Источник значений таблицы по часам: режим до или после расчета и графики напряжений КП БД.
    /// </summary>
    internal sealed class HourlyValueSource
    {
        /// <summary>
        /// Коды КП БД по номерам их узлов.
        /// </summary>
        private readonly Dictionary<int, int> _controlPointByNode = new Dictionary<int, int>();

        /// <summary>
        /// Графики напряжений в КП.
        /// </summary>
        private readonly VoltageSchedule _schedule;

        /// <summary>
        /// Создает источник значений.
        /// </summary>
        /// <param name="regime">Режим по часам.</param>
        /// <param name="session">Исходные данные сеанса: КП и графики напряжений.</param>
        public HourlyValueSource(RegimeSnapshot regime, SessionData session)
        {
            Regime = regime;
            _schedule = session.Schedule;
            foreach (ControlPoint controlPoint in session.ControlPoints)
            {
                foreach (ControlPointNode node in controlPoint.Nodes)
                {
                    _controlPointByNode.TryAdd(node.Number, controlPoint.Id);
                }
            }
        }

        /// <summary>
        /// Режим по часам.
        /// </summary>
        public RegimeSnapshot Regime { get; }

        /// <summary>
        /// Возвращает границы графика напряжения КП БД, в который входит узел.
        /// </summary>
        /// <param name="nodeNumber">Номер узла.</param>
        /// <param name="hour">Час суток (1–24).</param>
        /// <returns>Границы графика или <c>null</c>, если узел не входит в КП БД.</returns>
        public VoltageBounds? GetScheduleBounds(int nodeNumber, int hour)
        {
            int controlPointId;
            return _controlPointByNode.TryGetValue(nodeNumber, out controlPointId)
                ? _schedule.GetBounds(controlPointId, hour)
                : null;
        }
    }
}
