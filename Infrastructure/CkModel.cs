using System;
using System.Collections.Generic;

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
    /// возвращаются постоянные границы (по умолчанию 500–525 кВ, как в файле Анализ.xlsx).
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
        {
            if (lowerBound > upperBound)
            {
                throw new ArgumentException("Нижняя граница графика напряжения больше верхней");
            }

            _lowerBound = lowerBound;
            _upperBound = upperBound;
        }

        /// <inheritdoc/>
        public string SourceName => "ОИК «СК-11»: временно используются границы "
            + ValueFormat.ShortPower(_lowerBound) + "–" + ValueFormat.ShortPower(_upperBound) + " кВ";

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
                for (int hour = 1; hour <= RegimeSnapshot.HourCount; hour++)
                {
                    result.Add(new VoltageBounds(controlPoint.Id, hour, _lowerBound, _upperBound, receivedAt));
                }
            }

            return result;
        }
    }
}
