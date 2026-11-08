using System.Collections.Generic;
using System.Linq;

namespace VrdoPdg.Model.Entities
{
    /// <summary>
    /// PQ-диаграмма генератора: зависимость пределов реактивной мощности
    /// от активной мощности, заданная набором точек.
    /// </summary>
    public sealed class PQDiagram
    {
        /// <summary>
        /// Создает PQ-диаграмму.
        /// </summary>
        /// <param name="points">Точки PQ-диаграммы.</param>
        public PQDiagram(IEnumerable<PQPoint> points)
        {
            Points = points
                .OrderBy(point => point.ActivePower)
                .ToList();
        }

        /// <summary>
        /// Пустая PQ-диаграмма.
        /// </summary>
        public static PQDiagram Empty { get; } = new PQDiagram(new PQPoint[0]);

        /// <summary>
        /// Точки PQ-диаграммы, упорядоченные по возрастанию активной мощности.
        /// </summary>
        public IReadOnlyList<PQPoint> Points { get; }

        /// <summary>
        /// Признак пустой PQ-диаграммы.
        /// </summary>
        public bool IsEmpty => Points.Count == 0;

        /// <summary>
        /// Определяет пределы реактивной мощности при заданной активной мощности
        /// линейной интерполяцией между точками диаграммы. За пределами диапазона
        /// активной мощности диаграммы используются крайние точки.
        /// </summary>
        /// <param name="activePower">Активная мощность генератора, МВт.</param>
        /// <returns>Пределы реактивной мощности.</returns>
        public ReactivePowerLimits GetLimits(double activePower)
        {
            if (IsEmpty)
            {
                return new ReactivePowerLimits(0, 0);
            }

            PQPoint first = Points[0];
            if (activePower <= first.ActivePower)
            {
                return new ReactivePowerLimits(first.MinReactivePower, first.MaxReactivePower);
            }

            PQPoint last = Points[Points.Count - 1];
            if (activePower >= last.ActivePower)
            {
                return new ReactivePowerLimits(last.MinReactivePower, last.MaxReactivePower);
            }

            for (int i = 1; i < Points.Count; i++)
            {
                PQPoint right = Points[i];
                if (activePower > right.ActivePower)
                {
                    continue;
                }

                PQPoint left = Points[i - 1];
                double width = right.ActivePower - left.ActivePower;
                double ratio = width > 0
                    ? (activePower - left.ActivePower) / width
                    : 0;
                double min = left.MinReactivePower + (right.MinReactivePower - left.MinReactivePower) * ratio;
                double max = left.MaxReactivePower + (right.MaxReactivePower - left.MaxReactivePower) * ratio;
                return new ReactivePowerLimits(min, max);
            }

            return new ReactivePowerLimits(last.MinReactivePower, last.MaxReactivePower);
        }
    }
}
