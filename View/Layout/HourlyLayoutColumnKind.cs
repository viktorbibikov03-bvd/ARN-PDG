namespace VrdoPdg.View.Layout
{
    /// <summary>
    /// Вид столбца таблицы по часам: какое значение МТ выводится в столбце.
    /// </summary>
    internal enum HourlyLayoutColumnKind
    {
        /// <summary>
        /// Напряжение на шинах: расчетное напряжение узла (vras таблицы «Узлы»).
        /// </summary>
        BusVoltage,

        /// <summary>
        /// Заданное напряжение узла управляемого СКРМ (vzd таблицы «Узлы»).
        /// </summary>
        SetVoltage,

        /// <summary>
        /// Загрузка управляемого СКРМ: минимальная генерация Q узла (qmin таблицы «Узлы»).
        /// </summary>
        ReactivePower,

        /// <summary>
        /// Состояние реактора (sta таблицы «Реакторы»): «Вкл» или «Откл».
        /// </summary>
        Reactor,

        /// <summary>
        /// Напряжение генератора: заданное напряжение узла генератора (vzd), если узел включен.
        /// </summary>
        GeneratorVoltage
    }
}
