using System.Collections.Generic;

namespace VrdoPdg.View.Layout
{
    /// <summary>
    /// КП в таблице по часам: наименование, разделы со столбцами и, при необходимости,
    /// свои границы напряжения на шинах, например для КП класса напряжения 220 кВ
    /// </summary>
    internal sealed class HourlyLayoutControlPoint
    {
        /// <summary>
        /// Создает КП
        /// </summary>
        /// <param name="name">Наименование КП</param>
        /// <param name="groups">Разделы КП</param>
        /// <param name="lowerVoltageBound">Нижняя граница напряжения на шинах КП, кВ; <c>null</c> — общая</param>
        /// <param name="upperVoltageBound">Верхняя граница напряжения на шинах КП, кВ; <c>null</c> — общая</param>
        public HourlyLayoutControlPoint(
            string name,
            IReadOnlyList<HourlyLayoutGroup> groups,
            double? lowerVoltageBound = null,
            double? upperVoltageBound = null)
        {
            Name = name;
            Groups = groups;
            LowerVoltageBound = lowerVoltageBound;
            UpperVoltageBound = upperVoltageBound;
        }

        /// <summary>
        /// Наименование КП
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Разделы КП
        /// </summary>
        public IReadOnlyList<HourlyLayoutGroup> Groups { get; }

        /// <summary>
        /// Нижняя граница напряжения на шинах КП, кВ; <c>null</c> — используется граница схемы таблиц
        /// </summary>
        public double? LowerVoltageBound { get; }

        /// <summary>
        /// Верхняя граница напряжения на шинах КП, кВ; <c>null</c> — используется граница схемы таблиц
        /// </summary>
        public double? UpperVoltageBound { get; }

        /// <summary>
        /// Признак КП со своими границами напряжения
        /// </summary>
        public bool HasOwnBounds => LowerVoltageBound.HasValue && UpperVoltageBound.HasValue;
    }
}
