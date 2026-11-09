using VrdoPdg.View.Layout;

namespace VrdoPdg.View.Controls
{
    /// <summary>
    /// Столбец таблицы по часам: столбец «Час» или столбец значения КП из схемы таблиц.
    /// </summary>
    internal sealed class HourlyColumn
    {
        /// <summary>
        /// Создает столбец.
        /// </summary>
        /// <param name="controlPoint">КП; <c>null</c> для столбца «Час».</param>
        /// <param name="group">Раздел КП; <c>null</c> для столбца «Час».</param>
        /// <param name="source">Столбец схемы таблиц; <c>null</c> для столбца «Час».</param>
        public HourlyColumn(
            HourlyLayoutControlPoint? controlPoint,
            HourlyLayoutGroup? group,
            HourlyLayoutColumn? source)
        {
            ControlPoint = controlPoint;
            Group = group;
            Source = source;
        }

        /// <summary>
        /// КП; <c>null</c> для столбца «Час».
        /// </summary>
        public HourlyLayoutControlPoint? ControlPoint { get; }

        /// <summary>
        /// Раздел КП; <c>null</c> для столбца «Час».
        /// </summary>
        public HourlyLayoutGroup? Group { get; }

        /// <summary>
        /// Столбец схемы таблиц; <c>null</c> для столбца «Час».
        /// </summary>
        public HourlyLayoutColumn? Source { get; }

        /// <summary>
        /// Признак столбца «Час».
        /// </summary>
        public bool IsHour => Source == null;

        /// <summary>
        /// Номер в нижней строке заголовка. Для столбца загрузки управляемого СКРМ номер не выводится:
        /// он относится к столбцу заданного напряжения слева, как в файле «Анализ.xlsx».
        /// </summary>
        public string NumberText => Source == null || Source.Kind == HourlyLayoutColumnKind.ReactivePower
            ? string.Empty
            : Source.Number.ToString();

        /// <summary>
        /// Подсказка к столбцу: КП, раздел и объект МТ.
        /// </summary>
        public string Description
        {
            get
            {
                if (Source == null || ControlPoint == null)
                {
                    return string.Empty;
                }

                string prefix = ControlPoint.Name + " · " + Group?.Caption + " · ";
                switch (Source.Kind)
                {
                    case HourlyLayoutColumnKind.BusVoltage:
                        return prefix + "напряжение узла " + Source.Number + " (vras)";
                    case HourlyLayoutColumnKind.SetVoltage:
                        return prefix + "заданное напряжение узла " + Source.Number + " (vzd)";
                    case HourlyLayoutColumnKind.ReactivePower:
                        return prefix + "загрузка узла " + Source.Number + " (qmin), Мвар";
                    case HourlyLayoutColumnKind.Reactor:
                        return prefix + "реактор " + Source.Number;
                    default:
                        return prefix + "заданное напряжение генератора, узел " + Source.Number + " (vzd)";
                }
            }
        }
    }
}
