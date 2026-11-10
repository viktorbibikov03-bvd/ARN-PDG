namespace VrdoPdg.Model.Entities
{
    /// <summary>
    /// Энергосистема. По энергосистемам КП распределяются по вкладкам
    /// форм «Напряжение по часам» и «Сравнение расчетов»
    /// </summary>
    public sealed class PowerSystem
    {
        /// <summary>
        /// Создает энергосистему
        /// </summary>
        /// <param name="id">Код энергосистемы.</param>
        /// <param name="name">Наименование энергосистемы.</param>
        public PowerSystem(int id, string name)
        {
            Id = id;
            Name = name ?? string.Empty;
        }

        /// <summary>
        /// Код энергосистемы. Задает порядок вкладок
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Наименование энергосистемы
        /// </summary>
        public string Name { get; }
    }
}
