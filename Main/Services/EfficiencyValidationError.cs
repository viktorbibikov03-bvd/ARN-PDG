namespace VrdoPdg.Model.Services
{
    /// <summary>
    /// Ошибка проверки таблицы эффективности.
    /// </summary>
    public sealed class EfficiencyValidationError
    {
        /// <summary>
        /// Индекс строки для ошибок, относящихся ко всей таблице.
        /// </summary>
        public const int TableRowIndex = -1;

        /// <summary>
        /// Создает ошибку проверки.
        /// </summary>
        /// <param name="rowIndex">Индекс строки с ошибкой или <see cref="TableRowIndex"/>.</param>
        /// <param name="message">Описание ошибки.</param>
        public EfficiencyValidationError(int rowIndex, string message)
        {
            RowIndex = rowIndex;
            Message = message;
        }

        /// <summary>
        /// Индекс строки с ошибкой; <see cref="TableRowIndex"/> — ошибка всей таблицы.
        /// </summary>
        public int RowIndex { get; }

        /// <summary>
        /// Описание ошибки, например «Приоритет 3 повторяется».
        /// </summary>
        public string Message { get; }
    }
}
