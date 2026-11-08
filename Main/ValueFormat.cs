using System.Globalization;

namespace VrdoPdg.Model
{
    /// <summary>
    /// Форматирование значений режима для протокола и пользовательского интерфейса.
    /// </summary>
    public static class ValueFormat
    {
        /// <summary>
        /// Культура форматирования значений (десятичный разделитель — запятая).
        /// </summary>
        public static CultureInfo Culture { get; } = CultureInfo.GetCultureInfo("ru-RU");

        /// <summary>
        /// Форматирует напряжение с двумя знаками после запятой.
        /// </summary>
        /// <param name="voltage">Напряжение, кВ.</param>
        /// <returns>Строка вида «527,56».</returns>
        public static string Voltage(double voltage)
        {
            return voltage.ToString("0.00", Culture);
        }

        /// <summary>
        /// Форматирует напряжение с одним знаком после запятой для таблиц.
        /// </summary>
        /// <param name="voltage">Напряжение, кВ.</param>
        /// <returns>Строка вида «527,6».</returns>
        public static string ShortVoltage(double voltage)
        {
            return voltage.ToString("0.0", Culture);
        }

        /// <summary>
        /// Форматирует мощность с одним знаком после запятой.
        /// </summary>
        /// <param name="power">Мощность, Мвар или МВт.</param>
        /// <returns>Строка вида «-180,0».</returns>
        public static string Power(double power)
        {
            return power.ToString("0.0", Culture);
        }

        /// <summary>
        /// Форматирует мощность без лишних нулей для таблиц.
        /// </summary>
        /// <param name="power">Мощность, Мвар или МВт.</param>
        /// <returns>Строка вида «-180» или «15,5».</returns>
        public static string ShortPower(double power)
        {
            return power.ToString("0.#", Culture);
        }
    }
}
