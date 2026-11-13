using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using VrdoPdg.Model;
using VrdoPdg.Model.Entities;
using VrdoPdg.Model.Regime;
using VrdoPdg.View.Layout;
using VrdoPdg.View.Theme;

namespace VrdoPdg.View.Controls
{
    /// <summary>
    /// Таблица одного блока КП по часам суток, как блок листа «U-500» файла «Анализ.xlsx»: строки — часы,
    /// заголовок из трех строк — наименования КП, разделы («U сш», «СКРМ», «U ген») и номера узлов
    /// или реакторов. В режиме сравнения в ячейке выводятся значения двух расчетов.
    /// </summary>
    internal sealed class HourlyGrid : DataGridView
    {
        /// <summary>
        /// Высота строки заголовка, пикселей.
        /// </summary>
        private const int BandHeight = 24;

        /// <summary>
        /// Ширина столбца «Час», пикселей.
        /// </summary>
        private const int HourColumnWidth = 54;

        /// <summary>
        /// Ширина столбца значений, пикселей.
        /// </summary>
        private const int ValueColumnWidth = 64;

        /// <summary>
        /// Отступ наименования КП от границ заголовка, пикселей.
        /// </summary>
        private const int TitlePadding = 16;

        /// <summary>
        /// Высота строки с одним значением, пикселей.
        /// </summary>
        private const int SingleRowHeight = 28;

        /// <summary>
        /// Высота строки с двумя значениями сравниваемых расчетов, пикселей.
        /// </summary>
        private const int DoubleRowHeight = 42;

        /// <summary>
        /// Цвет напряжения выше верхней границы графика.
        /// </summary>
        private static readonly Color _aboveText = Color.FromArgb(156, 203, 255);

        /// <summary>
        /// Цвет напряжения ниже нижней границы графика.
        /// </summary>
        private static readonly Color _belowText = Color.FromArgb(255, 154, 162);

        /// <summary>
        /// Фон четных строк.
        /// </summary>
        private static readonly Color _alternateBack = Color.FromArgb(21, 21, 21);

        /// <summary>
        /// Диапазоны столбцов КП.
        /// </summary>
        private readonly List<GroupRange> _ranges = new List<GroupRange>();

        /// <summary>
        /// Смещения столбцов значений от правой границы столбца «Час».
        /// </summary>
        private readonly List<int> _offsets = new List<int>();

        /// <summary>
        /// Данные таблицы.
        /// </summary>
        private HourlyTableModel? _model;

        /// <summary>
        /// Создает таблицу.
        /// </summary>
        public HourlyGrid()
        {
            DoubleBuffered = true;
            UiStyle.ApplyGrid(this);
            VirtualMode = true;
            ReadOnly = true;
            AllowUserToResizeColumns = false;
            AllowUserToOrderColumns = false;
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            CellBorderStyle = DataGridViewCellBorderStyle.None;
            SelectionMode = DataGridViewSelectionMode.CellSelect;
            MultiSelect = false;
            ShowCellToolTips = true;
            ColumnHeadersHeight = BandHeight * 3;
            ScrollBars = ScrollBars.Horizontal;
            StandardTab = true;
        }

        /// <summary>
        /// Признак таблицы без столбцов КП.
        /// </summary>
        public bool IsEmpty => _ranges.Count == 0;

        /// <summary>
        /// Высота таблицы, при которой видны все 24 часа и полоса горизонтальной прокрутки.
        /// </summary>
        public int PreferredHeight => ColumnHeadersHeight + (RowCount * RowTemplate.Height)
            + SystemInformation.HorizontalScrollBarHeight + 2;

        /// <summary>
        /// Шрифт наименования КП.
        /// </summary>
        private static Font TitleFont => UiFonts.Semibold(9.5f);

        /// <summary>
        /// Выводит в таблицу блок КП.
        /// </summary>
        /// <param name="model">Данные таблиц.</param>
        /// <param name="block">Блок КП схемы таблиц.</param>
        public void Bind(HourlyTableModel model, HourlyLayoutBlock block)
        {
            _model = model;
            _ranges.Clear();
            _offsets.Clear();
            Columns.Clear();

            AddColumn(new HourlyColumn(null, null, null), HourColumnWidth, "ЧАС", true);
            int offset = 0;
            _offsets.Add(0);
            foreach (HourlyLayoutControlPoint controlPoint in block.ControlPoints)
            {
                int columnWidth = GetValueColumnWidth(controlPoint);
                int first = Columns.Count;
                List<SectionRange> sections = new List<SectionRange>();
                foreach (HourlyLayoutGroup group in controlPoint.Groups)
                {
                    int sectionFirst = Columns.Count;
                    foreach (HourlyLayoutColumn source in group.Columns)
                    {
                        HourlyColumn column = new HourlyColumn(controlPoint, group, source);
                        AddColumn(column, columnWidth, column.NumberText, false);
                        _offsets.Add(offset);
                        offset += columnWidth;
                    }

                    if (Columns.Count > sectionFirst)
                    {
                        sections.Add(new SectionRange(group.Caption, sectionFirst, Columns.Count - 1));
                    }
                }

                if (Columns.Count > first)
                {
                    _ranges.Add(new GroupRange(controlPoint, first, Columns.Count - 1, sections));
                }
            }

            RowTemplate.Height = model.Sources.Count > 1
                ? DoubleRowHeight
                : SingleRowHeight;
            RowCount = Columns.Count > 1
                ? 24
                : 0;
            Invalidate();
        }

        /// <summary>
        /// Рисует ячейки заголовка и значений.
        /// </summary>
        /// <param name="e">Аргументы события.</param>
        protected override void OnCellPainting(DataGridViewCellPaintingEventArgs e)
        {
            base.OnCellPainting(e);
            if (e.Handled || e.Graphics == null || e.ColumnIndex < 0 || _model == null)
            {
                return;
            }

            HourlyColumn? column = Columns[e.ColumnIndex].Tag as HourlyColumn;
            if (column == null)
            {
                return;
            }

            if (e.RowIndex < 0)
            {
                PaintHeaderCell(e.Graphics, e.CellBounds, column, e.ColumnIndex);
            }
            else
            {
                PaintValueCell(e.Graphics, e.CellBounds, column, e.ColumnIndex, e.RowIndex + 1);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Дорисовывает объединенные строки заголовка: наименования КП и подписи разделов.
        /// </summary>
        /// <param name="e">Аргументы события.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (_model == null || Columns.Count == 0)
            {
                return;
            }

            int frozenWidth = Columns[0].Width;
            Rectangle area = new Rectangle(frozenWidth, 0, Math.Max(0, ClientSize.Width - frozenWidth), BandHeight * 2);
            Graphics graphics = e.Graphics;
            graphics.SetClip(area);
            foreach (GroupRange range in _ranges)
            {
                int left = GetColumnLeft(range.First);
                int right = GetColumnRight(range.Last);
                if (right <= frozenWidth || left >= ClientSize.Width)
                {
                    continue;
                }

                Rectangle title = new Rectangle(left, 0, right - left, BandHeight);
                FillBand(graphics, title);
                Rectangle visibleTitle = Rectangle.Intersect(title, area);
                TextRenderer.DrawText(graphics, range.ControlPoint.Name, TitleFont, visibleTitle,
                    Palette.Text, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
                    | TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);

                foreach (SectionRange section in range.Sections)
                {
                    int sectionLeft = GetColumnLeft(section.First);
                    Rectangle bounds = new Rectangle(
                        sectionLeft,
                        BandHeight,
                        GetColumnRight(section.Last) - sectionLeft,
                        BandHeight);
                    DrawSection(graphics, bounds, area, section.Caption, GetSectionColor(section.Caption));
                }

                using (Pen pen = new Pen(Palette.Border))
                {
                    graphics.DrawLine(pen, left, BandHeight - 1, right, BandHeight - 1);
                    graphics.DrawLine(pen, left, 0, left, ColumnHeadersHeight);
                }
            }

            graphics.ResetClip();
        }

        /// <summary>
        /// Перерисовывает заголовок при прокрутке.
        /// </summary>
        /// <param name="e">Аргументы события.</param>
        protected override void OnScroll(ScrollEventArgs e)
        {
            base.OnScroll(e);
            Invalidate(new Rectangle(0, 0, ClientSize.Width, ColumnHeadersHeight));
        }

        /// <summary>
        /// Выводит подсказку с КП, разделом и объектом МТ.
        /// </summary>
        /// <param name="e">Аргументы события.</param>
        protected override void OnCellToolTipTextNeeded(DataGridViewCellToolTipTextNeededEventArgs e)
        {
            base.OnCellToolTipTextNeeded(e);
            if (e.ColumnIndex >= 0 && Columns[e.ColumnIndex].Tag is HourlyColumn column && !column.IsHour)
            {
                e.ToolTipText = column.Description;
            }
        }

        /// <summary>
        /// Возвращает ширину столбцов значений КП: наименование КП должно поместиться
        /// в заголовок над его столбцами.
        /// </summary>
        /// <param name="controlPoint">КП схемы таблиц.</param>
        /// <returns>Ширина столбца, пикселей.</returns>
        private static int GetValueColumnWidth(HourlyLayoutControlPoint controlPoint)
        {
            int count = 0;
            foreach (HourlyLayoutGroup group in controlPoint.Groups)
            {
                count += group.Columns.Count;
            }

            if (count == 0)
            {
                return ValueColumnWidth;
            }

            int titleWidth = TextRenderer.MeasureText(controlPoint.Name, TitleFont).Width + TitlePadding;
            return Math.Max(ValueColumnWidth, (titleWidth + count - 1) / count);
        }

        /// <summary>
        /// Заливает фон строки заголовка.
        /// </summary>
        /// <param name="graphics">Поверхность рисования.</param>
        /// <param name="bounds">Область.</param>
        private static void FillBand(Graphics graphics, Rectangle bounds)
        {
            using (SolidBrush brush = new SolidBrush(Palette.SurfaceRaised))
            {
                graphics.FillRectangle(brush, bounds);
            }
        }

        /// <summary>
        /// Рисует подпись раздела КП.
        /// </summary>
        /// <param name="graphics">Поверхность рисования.</param>
        /// <param name="bounds">Область раздела.</param>
        /// <param name="area">Видимая область заголовка.</param>
        /// <param name="text">Подпись.</param>
        /// <param name="color">Цвет подписи.</param>
        private static void DrawSection(Graphics graphics, Rectangle bounds, Rectangle area, string text, Color color)
        {
            FillBand(graphics, bounds);
            Rectangle visible = Rectangle.Intersect(bounds, area);
            if (visible.Width > 0)
            {
                TextRenderer.DrawText(graphics, text, UiFonts.Regular(9f), visible, color,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine
                    | TextFormatFlags.NoPrefix);
            }
        }

        /// <summary>
        /// Возвращает цвет подписи раздела: «U сш» — синий, «СКРМ» — фиолетовый, «U ген» — желтый.
        /// </summary>
        /// <param name="caption">Подпись раздела.</param>
        /// <returns>Цвет подписи.</returns>
        private static Color GetSectionColor(string caption)
        {
            switch (caption)
            {
                case "U сш":
                    return Palette.Blue;
                case "СКРМ":
                    return Palette.Purple;
                case "U ген":
                    return Palette.Warning;
                default:
                    return Palette.TextSecondary;
            }
        }

        /// <summary>
        /// Добавляет столбец таблицы.
        /// </summary>
        /// <param name="column">Описание столбца.</param>
        /// <param name="width">Ширина столбца.</param>
        /// <param name="headerText">Текст заголовка.</param>
        /// <param name="isFrozen">Признак закрепленного столбца.</param>
        private void AddColumn(HourlyColumn column, int width, string headerText, bool isFrozen)
        {
            DataGridViewTextBoxColumn gridColumn = new DataGridViewTextBoxColumn
            {
                HeaderText = headerText,
                Width = width,
                MinimumWidth = width,
                FillWeight = 1,
                Frozen = isFrozen,
                ReadOnly = true,
                Resizable = DataGridViewTriState.False,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                Tag = column
            };
            Columns.Add(gridColumn);
        }

        /// <summary>
        /// Возвращает левую границу столбца с учетом прокрутки.
        /// </summary>
        /// <param name="columnIndex">Индекс столбца.</param>
        /// <returns>Координата левой границы.</returns>
        private int GetColumnLeft(int columnIndex)
        {
            return Columns[0].Width + _offsets[columnIndex] - HorizontalScrollingOffset;
        }

        /// <summary>
        /// Возвращает правую границу столбца с учетом прокрутки.
        /// </summary>
        /// <param name="columnIndex">Индекс столбца.</param>
        /// <returns>Координата правой границы.</returns>
        private int GetColumnRight(int columnIndex)
        {
            return GetColumnLeft(columnIndex) + Columns[columnIndex].Width;
        }

        /// <summary>
        /// Проверяет, начинает ли столбец КП.
        /// </summary>
        /// <param name="columnIndex">Индекс столбца.</param>
        /// <returns><c>true</c>, если столбец первый в КП.</returns>
        private bool IsGroupStart(int columnIndex)
        {
            foreach (GroupRange range in _ranges)
            {
                if (range.First == columnIndex)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Рисует ячейку заголовка: «ЧАС» или номер узла, реактора в нижней строке.
        /// </summary>
        /// <param name="graphics">Поверхность рисования.</param>
        /// <param name="bounds">Границы ячейки.</param>
        /// <param name="column">Описание столбца.</param>
        /// <param name="columnIndex">Индекс столбца.</param>
        private void PaintHeaderCell(Graphics graphics, Rectangle bounds, HourlyColumn column, int columnIndex)
        {
            FillBand(graphics, bounds);
            if (column.IsHour)
            {
                TextRenderer.DrawText(graphics, "ЧАС", UiFonts.Monospace(9f), bounds, Palette.TextSecondary,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
            else
            {
                Rectangle numberBand = new Rectangle(bounds.X, bounds.Bottom - BandHeight, bounds.Width, BandHeight);
                TextRenderer.DrawText(graphics, column.NumberText, UiFonts.Monospace(9f), numberBand,
                    Palette.TextSecondary, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }

            DrawCellLines(graphics, bounds, column, columnIndex);
        }

        /// <summary>
        /// Рисует ячейку значений: одно значение или два значения сравниваемых расчетов.
        /// </summary>
        /// <param name="graphics">Поверхность рисования.</param>
        /// <param name="bounds">Границы ячейки.</param>
        /// <param name="column">Описание столбца.</param>
        /// <param name="columnIndex">Индекс столбца.</param>
        /// <param name="hour">Час суток.</param>
        private void PaintValueCell(Graphics graphics, Rectangle bounds, HourlyColumn column, int columnIndex, int hour)
        {
            HourlyTableModel model = _model!;
            using (SolidBrush brush = new SolidBrush(hour % 2 == 0
                ? _alternateBack
                : Palette.Background))
            {
                graphics.FillRectangle(brush, bounds);
            }

            TextFormatFlags flags = TextFormatFlags.HorizontalCenter
                | TextFormatFlags.VerticalCenter
                | TextFormatFlags.SingleLine
                | TextFormatFlags.NoPrefix;
            if (column.IsHour)
            {
                TextRenderer.DrawText(graphics, hour.ToString(), UiFonts.Monospace(9.5f), bounds,
                    Palette.TextSecondary, flags);
            }
            else
            {
                int count = model.Sources.Count;
                int partHeight = bounds.Height / count;
                string? firstText = null;
                for (int index = 0; index < count; index++)
                {
                    int top = bounds.Top + (index * partHeight);
                    int height = index == count - 1
                        ? bounds.Bottom - top
                        : partHeight;
                    Rectangle part = new Rectangle(bounds.Left, top, bounds.Width, height);
                    CellText value = GetCellText(column, model.Sources[index], hour, count > 1 ? index : -1);
                    if (value.BackColor.HasValue)
                    {
                        using (SolidBrush brush = new SolidBrush(value.BackColor.Value))
                        {
                            graphics.FillRectangle(brush, Rectangle.Inflate(part, -1, -1));
                        }
                    }

                    bool isDifferent = index > 0 && firstText != value.Text;
                    Font font = isDifferent
                        ? UiFonts.MonospaceBold(9.5f)
                        : UiFonts.Monospace(9.5f);
                    TextRenderer.DrawText(graphics, value.Text, font, part, value.ForeColor, flags);
                    if (index == 0)
                    {
                        firstText = value.Text;
                    }
                }
            }

            DrawCellLines(graphics, bounds, column, columnIndex);
        }

        /// <summary>
        /// Рисует нижнюю линию ячейки и вертикальные линии на границе столбца «Час» и в начале КП.
        /// </summary>
        /// <param name="graphics">Поверхность рисования.</param>
        /// <param name="bounds">Границы ячейки.</param>
        /// <param name="column">Описание столбца.</param>
        /// <param name="columnIndex">Индекс столбца.</param>
        private void DrawCellLines(Graphics graphics, Rectangle bounds, HourlyColumn column, int columnIndex)
        {
            using (Pen pen = new Pen(Palette.Border))
            {
                graphics.DrawLine(pen, bounds.Left, bounds.Bottom - 1, bounds.Right, bounds.Bottom - 1);
                if (column.IsHour)
                {
                    graphics.DrawLine(pen, bounds.Right - 1, bounds.Top, bounds.Right - 1, bounds.Bottom);
                }
                else if (IsGroupStart(columnIndex))
                {
                    graphics.DrawLine(pen, bounds.Left, bounds.Top, bounds.Left, bounds.Bottom);
                }
            }
        }

        /// <summary>
        /// Формирует текст и цвета значения ячейки по виду столбца.
        /// </summary>
        /// <param name="gridColumn">Описание столбца таблицы</param>
        /// <param name="source">Источник значений.</param>
        /// <param name="hour">Час суток.</param>
        /// <param name="runIndex">Индекс сравниваемого расчета; -1 — предпросмотр.</param>
        /// <returns>Текст и цвета.</returns>
        private CellText GetCellText(HourlyColumn gridColumn, HourlyValueSource source, int hour, int runIndex)
        {
            HourlyLayoutColumn column = gridColumn.Source!;
            Color numberColor = runIndex < 0
                ? Palette.Text
                : runIndex == 0
                    ? Palette.FirstRun
                    : Palette.SecondRun;
            RegimeSnapshot regime = source.Regime;
            if (column.Kind == HourlyLayoutColumnKind.Reactor)
            {
                ObservedReactorState? reactor = regime.GetObservedReactor(column.Number, hour);
                if (reactor == null)
                {
                    return CellText.Missing;
                }

                return reactor.IsOn
                    ? new CellText("Вкл", Palette.Accent, null)
                    : CellText.Off;
            }

            ObservedNodeState? node = regime.GetObservedNode(column.Number, hour);
            if (node == null)
            {
                return CellText.Missing;
            }

            switch (column.Kind)
            {
                case HourlyLayoutColumnKind.BusVoltage:
                    return GetBusVoltageText(gridColumn.ControlPoint!, node, source, hour, numberColor);
                case HourlyLayoutColumnKind.SetVoltage:
                    return new CellText(ValueFormat.ShortVoltage(node.SetVoltage), numberColor, null);
                case HourlyLayoutColumnKind.ReactivePower:
                    Color powerColor = runIndex < 0
                        ? Palette.Purple
                        : numberColor;
                    return new CellText(ValueFormat.ShortPower(node.MinReactivePower), powerColor, null);
                default:
                    return node.IsOn && node.Voltage != 0
                        ? new CellText(ValueFormat.Voltage(node.SetVoltage), numberColor, null)
                        : CellText.Off;
            }
        }

        /// <summary>
        /// Формирует значение напряжения на шинах с выделением выхода за границы графика.
        /// Напряжения КП без своих границ вне диапазона выделения схемы таблиц (узлы другого
        /// класса напряжения) не выделяются
        /// </summary>
        /// <param name="controlPoint">КП схемы таблиц</param>
        /// <param name="node">Значения узла.</param>
        /// <param name="source">Источник значений.</param>
        /// <param name="hour">Час суток.</param>
        /// <param name="numberColor">Цвет значения в границах графика.</param>
        /// <returns>Текст и цвета.</returns>
        private CellText GetBusVoltageText(
            HourlyLayoutControlPoint controlPoint,
            ObservedNodeState node,
            HourlyValueSource source,
            int hour,
            Color numberColor)
        {
            if (!node.IsOn || node.Voltage == 0)
            {
                return CellText.Off;
            }

            string text = ValueFormat.ShortVoltage(node.Voltage);
            if (!_model!.Layout.IsHighlighted(controlPoint, node.Voltage))
            {
                return new CellText(text, numberColor, null);
            }

            VoltageBounds bounds = _model!.GetBounds(source, controlPoint, node.NodeNumber, hour);
            if (bounds.IsAbove(node.Voltage))
            {
                return new CellText(text, _aboveText, Palette.BlueDark);
            }

            if (bounds.IsBelow(node.Voltage))
            {
                return new CellText(text, _belowText, Palette.RedDark);
            }

            return new CellText(text, numberColor, null);
        }

        /// <summary>
        /// Диапазон столбцов КП.
        /// </summary>
        private sealed class GroupRange
        {
            /// <summary>
            /// Создает диапазон столбцов.
            /// </summary>
            /// <param name="controlPoint">КП.</param>
            /// <param name="first">Индекс первого столбца КП.</param>
            /// <param name="last">Индекс последнего столбца КП.</param>
            /// <param name="sections">Диапазоны разделов КП.</param>
            public GroupRange(
                HourlyLayoutControlPoint controlPoint,
                int first,
                int last,
                IReadOnlyList<SectionRange> sections)
            {
                ControlPoint = controlPoint;
                First = first;
                Last = last;
                Sections = sections;
            }

            /// <summary>
            /// КП.
            /// </summary>
            public HourlyLayoutControlPoint ControlPoint { get; }

            /// <summary>
            /// Индекс первого столбца КП.
            /// </summary>
            public int First { get; }

            /// <summary>
            /// Индекс последнего столбца КП.
            /// </summary>
            public int Last { get; }

            /// <summary>
            /// Диапазоны разделов КП.
            /// </summary>
            public IReadOnlyList<SectionRange> Sections { get; }
        }

        /// <summary>
        /// Диапазон столбцов раздела КП.
        /// </summary>
        private sealed class SectionRange
        {
            /// <summary>
            /// Создает диапазон столбцов раздела.
            /// </summary>
            /// <param name="caption">Подпись раздела.</param>
            /// <param name="first">Индекс первого столбца раздела.</param>
            /// <param name="last">Индекс последнего столбца раздела.</param>
            public SectionRange(string caption, int first, int last)
            {
                Caption = caption;
                First = first;
                Last = last;
            }

            /// <summary>
            /// Подпись раздела.
            /// </summary>
            public string Caption { get; }

            /// <summary>
            /// Индекс первого столбца раздела.
            /// </summary>
            public int First { get; }

            /// <summary>
            /// Индекс последнего столбца раздела.
            /// </summary>
            public int Last { get; }
        }

        /// <summary>
        /// Текст и цвета значения ячейки.
        /// </summary>
        private readonly struct CellText
        {
            /// <summary>
            /// Создает значение ячейки.
            /// </summary>
            /// <param name="text">Текст.</param>
            /// <param name="foreColor">Цвет текста.</param>
            /// <param name="backColor">Цвет выделения фона.</param>
            public CellText(string text, Color foreColor, Color? backColor)
            {
                Text = text;
                ForeColor = foreColor;
                BackColor = backColor;
            }

            /// <summary>
            /// Значение объекта, не найденного в МТ.
            /// </summary>
            public static CellText Missing => new CellText("—", Palette.TextMuted, null);

            /// <summary>
            /// Значение отключенного узла или реактора.
            /// </summary>
            public static CellText Off => new CellText("Откл", Palette.TextMuted, null);

            /// <summary>
            /// Текст.
            /// </summary>
            public string Text { get; }

            /// <summary>
            /// Цвет текста.
            /// </summary>
            public Color ForeColor { get; }

            /// <summary>
            /// Цвет выделения фона; <c>null</c> — без выделения.
            /// </summary>
            public Color? BackColor { get; }
        }
    }
}
