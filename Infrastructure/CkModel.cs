using System;
using System.Collections.Generic;
using System.Linq;

using VrdoPdg.Model;
using VrdoPdg.Model.Entities;
using VrdoPdg.Model.Regime;
using VrdoPdg.Model.Schedules;

namespace VrdoPdg.Infrastructure
{
    /// <summary>
    /// Модуль взаимодействия с ОИК «СК-11»: получение границ графиков напряжений в КП.
    /// </summary>
    /// <remarks>
    /// Запрос к Public API ОИК «СК-11» пока не реализован: для всех КП и часов
    /// возвращаются постоянные границы (по умолчанию 500–525 кВ, как в файле Анализ.xlsx),
    /// для отдельных КП, например класса напряжения 220 кВ, — свои постоянные границы
    /// </remarks>
    public sealed class CkModel : ICkModel
    {
        /// <summary>
        /// Нижняя граница графика напряжения по умолчанию, кВ.
        /// </summary>
        public const double DefaultLowerBound = 500;

        /// <summary>
        /// Верхняя граница графика напряжения по умолчанию, кВ.
        /// </summary>
        public const double DefaultUpperBound = 525;

        /// <summary>
        /// Нижняя граница графика напряжения, кВ.
        /// </summary>
        private readonly double _lowerBound;

        /// <summary>
        /// Верхняя граница графика напряжения, кВ.
        /// </summary>
        private readonly double _upperBound;

        /// <summary>
        /// Границы графиков отдельных КП по наименованию КП
        /// </summary>
        private readonly Dictionary<string, ControlPointBounds> _controlPointBounds;

        /// <summary>
        /// Создает модуль с границами графиков по умолчанию (500–525 кВ).
        /// </summary>
        public CkModel()
            : this(DefaultLowerBound, DefaultUpperBound)
        {
        }

        /// <summary>
        /// Создает модуль с заданными постоянными границами графиков.
        /// </summary>
        /// <param name="lowerBound">Нижняя граница графика, кВ.</param>
        /// <param name="upperBound">Верхняя граница графика, кВ.</param>
        public CkModel(double lowerBound, double upperBound)
            : this(lowerBound, upperBound, new ControlPointBounds[0])
        {
        }

        /// <summary>
        /// Создает модуль с заданными постоянными границами графиков и границами отдельных КП
        /// </summary>
        /// <param name="lowerBound">Нижняя граница графика остальных КП, кВ</param>
        /// <param name="upperBound">Верхняя граница графика остальных КП, кВ</param>
        /// <param name="controlPointBounds">Границы графиков отдельных КП</param>
        public CkModel(double lowerBound, double upperBound, IEnumerable<ControlPointBounds> controlPointBounds)
        {
            if (lowerBound > upperBound)
            {
                throw new ArgumentException("Нижняя граница графика напряжения больше верхней");
            }

            if (controlPointBounds == null)
            {
                throw new ArgumentNullException(nameof(controlPointBounds));
            }

            _lowerBound = lowerBound;
            _upperBound = upperBound;
            _controlPointBounds = new Dictionary<string, ControlPointBounds>(StringComparer.OrdinalIgnoreCase);
            foreach (ControlPointBounds bounds in controlPointBounds)
            {
                _controlPointBounds[bounds.ControlPointName] = bounds;
            }
        }

        /// <inheritdoc/>
        public string SourceName => "ОИК «СК-11»: временно используются границы "
            + ValueFormat.ShortPower(_lowerBound) + "–" + ValueFormat.ShortPower(_upperBound) + " кВ"
            + (_controlPointBounds.Count == 0
                ? string.Empty
                : ", для КП " + string.Join(", ", _controlPointBounds.Values.Select(FormatBounds)));

        /// <inheritdoc/>
        public IReadOnlyList<VoltageBounds> GetVoltageSchedule(IReadOnlyList<ControlPoint> controlPoints, DateTime date)
        {
            if (controlPoints == null)
            {
                throw new ArgumentNullException(nameof(controlPoints));
            }

            DateTime receivedAt = DateTime.Now;
            List<VoltageBounds> result = new List<VoltageBounds>();
            foreach (ControlPoint controlPoint in controlPoints)
            {
                ControlPointBounds? bounds;
                _controlPointBounds.TryGetValue(controlPoint.Name.Trim(), out bounds);
                double lowerBound = bounds?.LowerBound ?? _lowerBound;
                double upperBound = bounds?.UpperBound ?? _upperBound;
                for (int hour = 1; hour <= RegimeSnapshot.HourCount; hour++)
                {
                    result.Add(new VoltageBounds(controlPoint.Id, hour, lowerBound, upperBound, receivedAt));
                }
            }

            return result;
        }

        /// <summary>
        /// Форматирует границы графика КП для описания источника
        /// </summary>
        /// <param name="bounds">Границы графика КП</param>
        /// <returns>Наименование КП и границы</returns>
        private static string FormatBounds(ControlPointBounds bounds)
        {
            return bounds.ControlPointName + " " + ValueFormat.ShortPower(bounds.LowerBound) + "–"
                + ValueFormat.ShortPower(bounds.UpperBound) + " кВ";
        }
    }
}
