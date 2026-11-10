namespace VrdoPdg.Model.Entities
{
    /// <summary>
    /// Тип СКРМ. Определяет подтип СКРМ, порядок перебора алгоритмом
    /// и таблицу МТ, в которой хранятся параметры СКРМ
    /// </summary>
    public enum DeviceType
    {
        /// <summary>
        /// Управляемые СКРМ. Параметры хранятся в таблице «Узлы» МТ
        /// </summary>
        Controlled,

        /// <summary>
        /// Коммутируемые СКРМ. Параметры хранятся в таблице «Реакторы» МТ
        /// </summary>
        Switched,

        /// <summary>
        /// Генераторы. Таблицы «Генераторы УР» и «Узлы» МТ
        /// </summary>
        Generator
    }
}
