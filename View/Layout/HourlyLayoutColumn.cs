namespace VrdoPdg.View.Layout
{
    /// <summary>
    /// Столбец таблицы по часам: вид значения и номер узла или реактора в МТ.
    /// </summary>
    internal sealed class HourlyLayoutColumn
    {
        /// <summary>
        /// Создает столбец.
        /// </summary>
        /// <param name="kind">Вид значения.</param>
        /// <param name="number">Номер узла (ny) или реактора (Id).</param>
        public HourlyLayoutColumn(HourlyLayoutColumnKind kind, int number)
        {
            Kind = kind;
            Number = number;
        }

        /// <summary>
        /// Вид значения.
        /// </summary>
        public HourlyLayoutColumnKind Kind { get; }

        /// <summary>
        /// Номер узла (ny) или реактора (Id).
        /// </summary>
        public int Number { get; }

        /// <summary>
        /// Признак столбца реактора.
        /// </summary>
        public bool IsReactor => Kind == HourlyLayoutColumnKind.Reactor;
    }
}
