namespace VrdoPdg.View.Controls
{
    /// <summary>
    /// Вид столбца таблицы по часам.
    /// </summary>
    internal enum HourlyColumnKind
    {
        /// <summary>
        /// Час суток.
        /// </summary>
        Hour,

        /// <summary>
        /// Напряжение в узле КП (U сш).
        /// </summary>
        Voltage,

        /// <summary>
        /// Состояние или загрузка СКРМ.
        /// </summary>
        Device
    }
}
