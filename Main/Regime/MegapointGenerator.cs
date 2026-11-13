namespace VrdoPdg.Model.Regime
{
    /// <summary>
    /// Генератор таблицы «Генераторы УР» МТ
    /// </summary>
    public sealed class MegapointGenerator
    {
        /// <summary>
        /// Создает генератор
        /// </summary>
        /// <param name="number">Номер генератора (Num)</param>
        /// <param name="nodeNumber">Номер узла подключения (Node) — номер узла ny таблицы «Узлы»</param>
        /// <param name="name">Наименование генератора</param>
        public MegapointGenerator(int number, int nodeNumber, string name)
        {
            Number = number;
            NodeNumber = nodeNumber;
            Name = name ?? string.Empty;
        }

        /// <summary>
        /// Номер генератора (Num)
        /// </summary>
        public int Number { get; }

        /// <summary>
        /// Номер узла подключения (Node) — номер узла ny таблицы «Узлы»
        /// </summary>
        public int NodeNumber { get; }

        /// <summary>
        /// Наименование генератора
        /// </summary>
        public string Name { get; }
    }
}
