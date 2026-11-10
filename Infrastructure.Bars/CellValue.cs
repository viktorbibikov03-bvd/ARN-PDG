using ASTRALib;

namespace VrdoPdg.Infrastructure.Bars
{
    /// <summary>
    /// Исходное значение ячейки таблицы МТ, запомненное для отмены изменения
    /// </summary>
    internal sealed class CellValue
    {
        /// <summary>
        /// Создает запомненное значение ячейки
        /// </summary>
        /// <param name="column">Столбец таблицы</param>
        /// <param name="row">Индекс строки</param>
        /// <param name="value">Исходное значение</param>
        public CellValue(ICol column, int row, object value)
        {
            Column = column;
            Row = row;
            Value = value;
        }

        /// <summary>
        /// Столбец таблицы
        /// </summary>
        public ICol Column { get; }

        /// <summary>
        /// Индекс строки
        /// </summary>
        public int Row { get; }

        /// <summary>
        /// Исходное значение
        /// </summary>
        public object Value { get; }
    }
}
