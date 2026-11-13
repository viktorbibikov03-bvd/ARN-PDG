using System;

namespace VrdoPdg.Infrastructure
{
    /// <summary>
    /// Постоянные границы графика напряжения отдельного КП, например КП класса напряжения 220 кВ
    /// </summary>
    public sealed class ControlPointBounds
    {
        /// <summary>
        /// Создает границы графика КП
        /// </summary>
        /// <param name="controlPointName">Наименование КП в БД</param>
        /// <param name="lowerBound">Нижняя граница графика, кВ</param>
        /// <param name="upperBound">Верхняя граница графика, кВ</param>
        public ControlPointBounds(string controlPointName, double lowerBound, double upperBound)
        {
            if (string.IsNullOrWhiteSpace(controlPointName))
            {
                throw new ArgumentException("Не задано наименование КП", nameof(controlPointName));
            }

            if (lowerBound > upperBound)
            {
                throw new ArgumentException("Нижняя граница графика напряжения больше верхней");
            }

            ControlPointName = controlPointName.Trim();
            LowerBound = lowerBound;
            UpperBound = upperBound;
        }

        /// <summary>
        /// Наименование КП в БД
        /// </summary>
        public string ControlPointName { get; }

        /// <summary>
        /// Нижняя граница графика, кВ
        /// </summary>
        public double LowerBound { get; }

        /// <summary>
        /// Верхняя граница графика, кВ
        /// </summary>
        public double UpperBound { get; }
    }
}
