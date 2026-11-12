using System;

namespace VrdoPdg.Model.Entities
{
    /// <summary>
    /// Верхняя и нижняя границы графика напряжения КП в определенном часе суток
    /// </summary>
    public sealed class VoltageBounds
    {
        /// <summary>
        /// Допустимая погрешность сравнения напряжения с границами графика, кВ
        /// </summary>
        public const double Tolerance = 0.001;

        /// <summary>
        /// Создает границы графика напряжения
        /// </summary>
        /// <param name="controlPointId">Код КП</param>
        /// <param name="hour">Час суток</param>
        /// <param name="lowerBound">Нижняя граница графика, кВ</param>
        /// <param name="upperBound">Верхняя граница графика, кВ</param>
        /// <param name="receivedAt">Дата и время получения границ</param>
        public VoltageBounds(int controlPointId, int hour, double lowerBound, double upperBound, DateTime receivedAt)
        {
            if (lowerBound > upperBound)
            {
                throw new ArgumentException("Нижняя граница графика напряжения больше верхней");
            }

            ControlPointId = controlPointId;
            Hour = hour;
            LowerBound = lowerBound;
            UpperBound = upperBound;
            ReceivedAt = receivedAt;
        }

        /// <summary>
        /// Код КП
        /// </summary>
        public int ControlPointId { get; }

        /// <summary>
        /// Час суток
        /// </summary>
        public int Hour { get; }

        /// <summary>
        /// Нижняя граница графика, кВ
        /// </summary>
        public double LowerBound { get; }

        /// <summary>
        /// Верхняя граница графика, кВ
        /// </summary>
        public double UpperBound { get; }

        /// <summary>
        /// Дата и время получения границ из ОИК «СК-11»
        /// </summary>
        public DateTime ReceivedAt { get; }

        /// <summary>
        /// Проверяет, превышает ли напряжение верхнюю границу графика
        /// </summary>
        /// <param name="voltage">Напряжение, кВ.</param>
        /// <returns><c>true</c>, если напряжение выше верхней границы.</returns>
        public bool IsAbove(double voltage)
        {
            return voltage > UpperBound + Tolerance;
        }

        /// <summary>
        /// Проверяет, ниже ли напряжение нижней границы графика
        /// </summary>
        /// <param name="voltage">Напряжение, кВ</param>
        /// <returns><c>true</c>, если напряжение ниже нижней границы</returns>
        public bool IsBelow(double voltage)
        {
            return voltage < LowerBound - Tolerance;
        }

        /// <summary>
        /// Проверяет, находится ли напряжение в допустимой области
        /// </summary>
        /// <param name="voltage">Напряжение, кВ</param>
        /// <returns><c>true</c>, если напряжение в границах графика</returns>
        public bool Contains(double voltage)
        {
            return !IsAbove(voltage) && !IsBelow(voltage);
        }

        /// <summary>
        /// Возвращает отклонение напряжения от допустимой области
        /// </summary>
        /// <param name="voltage">Напряжение, кВ</param>
        /// <returns>Превышение верхней границы или снижение ниже нижней границы, кВ; 0 в границах графика</returns>
        public double GetDeviation(double voltage)
        {
            if (IsAbove(voltage))
            {
                return voltage - UpperBound;
            }

            return IsBelow(voltage)
                ? LowerBound - voltage
                : 0;
        }
    }
}
