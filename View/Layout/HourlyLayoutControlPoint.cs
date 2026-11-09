using System.Collections.Generic;

namespace VrdoPdg.View.Layout
{
    /// <summary>
    /// КП в таблице по часам: наименование и разделы со столбцами.
    /// </summary>
    internal sealed class HourlyLayoutControlPoint
    {
        /// <summary>
        /// Создает КП.
        /// </summary>
        /// <param name="name">Наименование КП.</param>
        /// <param name="groups">Разделы КП.</param>
        public HourlyLayoutControlPoint(string name, IReadOnlyList<HourlyLayoutGroup> groups)
        {
            Name = name;
            Groups = groups;
        }

        /// <summary>
        /// Наименование КП.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Разделы КП.
        /// </summary>
        public IReadOnlyList<HourlyLayoutGroup> Groups { get; }
    }
}
