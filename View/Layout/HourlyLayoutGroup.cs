using System.Collections.Generic;

namespace VrdoPdg.View.Layout
{
    /// <summary>
    /// Раздел КП в таблице по часам: «U сш», «СКРМ» или «U ген» со своими столбцами.
    /// </summary>
    internal sealed class HourlyLayoutGroup
    {
        /// <summary>
        /// Создает раздел.
        /// </summary>
        /// <param name="caption">Подпись раздела.</param>
        /// <param name="columns">Столбцы раздела.</param>
        public HourlyLayoutGroup(string caption, IReadOnlyList<HourlyLayoutColumn> columns)
        {
            Caption = caption;
            Columns = columns;
        }

        /// <summary>
        /// Подпись раздела.
        /// </summary>
        public string Caption { get; }

        /// <summary>
        /// Столбцы раздела.
        /// </summary>
        public IReadOnlyList<HourlyLayoutColumn> Columns { get; }
    }
}
