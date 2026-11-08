using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using VrdoPdg.Model;
using VrdoPdg.Model.Entities;
using VrdoPdg.View.Theme;

namespace VrdoPdg.View.Controls
{
    /// <summary>
    /// Таблица почасовых значений в привычной технологу форме (файл Анализ.xlsx):
    /// строки — часы суток, группы столбцов — КП с напряжениями на шинах (U сш) и СКРМ.
    /// Заголовок состоит из трех строк: наименование КП, «U сш» / «СКРМ», номера узлов и реакторов.
    /// В режиме сравнения в каждой ячейке выводятся два значения: сверху — первого расчета, снизу — второго.
    /// </summary>
    internal sealed class HourlyGrid : DataGridView
    {
        /// <summary>
        /// Высота одной строки заголовка, пикселей.
        /// </summary>
        private const int BandHeight = 24;

        /// <summary>
        /// Ширина столбца часа, пикселей.
        /// </summary>
        private const int HourColumnWidth = 54;

        /// <summary>
        /// Ширина столбца значений, пикселей.
        /// </summary>
        private const int ValueColumnWidth = 64;

        /// <summary>
        /// Высота строки с одним значением, пикселей.
        /// </summary>
        private const int SingleRowHeight = 28;

        /// <summary>
        /// Высота строки с двумя значениями, пикселей.
        /// </summary>
        private const int DoubleRowHeight = 42;

        /// <summary>
        /// Цвет текста напряжения выше верхней границы графика.
        /// </summary>
        private static readonly Color _aboveText = Color.FromArgb(156, 203, 255);

        /// <summary>
        /// Цвет текста напряжения ниже нижней границы графика.
        /// </summary>
        private static readonly Color _belowText = Color.FromArgb(255, 154, 162);

        /// <summary>
        /// Цвет фона нечетных строк.
        /// </summary>
        private static readonly Color _alternateBack = Color.FromArgb(21, 21, 21);

        /// <summary>
        /// Диапазоны столбцов групп КП.
        /// </summary>
        private readonly List<GroupRange> _ranges = new List<GroupRange>();

        /// <summary>
        /// Смещения столбцов относительно правой границы столбца часа без учета прокрутки.
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
            StandardTab = true;
        }

        /// <summary>
        /// Признак таблицы без групп КП.
        /// </summary>
        public bool IsEmpty => _ranges.Count == 0;

        /// <summary>
        /// Выводит в таблицу КП энергосистемы.
        /// </summary>
        /// <param name="model">Данные таблицы.</param>
        /// <param name="powerSystemId">Код энергосистемы.</param>
        public void Bind(HourlyTableModel model, int powerSystemId)
        {
            _model = model;
            _ranges.Clear();
            _offsets.Clear();
            Columns.Clear();

            AddColumn(new HourlyColumn(HourlyColumnKind.Hour, null, 0, null), HourColumnWidth, "ЧАС", true);
            int offset = 0;
            _offsets.Add(0);
            foreach (HourlyGroup group in model.GetGroups(powerSystemId))
            {
                int first = Columns.Count;
                foreach (int nodeNumber in group.NodeNumbers)
                {
                    AddColumn(new HourlyColumn(HourlyColumnKind.Voltage, group, nodeNumber, null), ValueColumnWidth,
                        nodeNumber.ToString(), false);
                    _offsets.Add(offset);
                    offset += ValueColumnWidth;
                }

                int firstDevice = Columns.Count;
                foreach (Device device in group.Devices)
                {
                    AddColumn(new HourlyColumn(HourlyColumnKind.Device, group, 0, device), ValueColumnWidth,
                        device.Number.ToString(), false);
                    _offsets.Add(offset);
                    offset += ValueColumnWidth;
                }

                if (Columns.Count > first)
                {
                    _ranges.Add(new GroupRange(group, first, firstDevice, Columns.Count - 1));
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
        /// Дорисовывает объединенные строки заголовка: наименования КП и подписи «U сш» / «СКРМ».
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
                int right = GetColumnLeft(range.Last) + Columns[range.Last].Width;
                if (right <= frozenWidth || left >= ClientSize.Width)
                {
                    continue;
                }

                Rectangle title = new Rectangle(left, 0, right - left, BandHeight);
                FillBand(graphics, title);
                Rectangle visibleTitle = Rectangle.Intersect(title, area);
                TextRenderer.DrawText(graphics, range.Group.ControlPoint.Name, UiFonts.Semibold(9.5f), visibleTitle,
                    Palette.Text, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
                    | TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine);

                if (range.FirstDevice > range.First)
                {
                    int voltageRight = GetColumnLeft(range.FirstDevice - 1) + Columns[range.FirstDevice - 1].Width;
                    DrawSection(graphics, new Rectangle(left, BandHeight, voltageRight - left, BandHeight), area,
                        "U сш", Palette.Blue);
                }

                if (range.FirstDevice <= range.Last)
                {
                    int deviceLeft = GetColumnLeft(range.FirstDevice);
                    DrawSection(graphics, new Rectangle(deviceLeft, BandHeight, right - deviceLeft, BandHeight), area,
                        "СКРМ", Palette.Purple);
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
        /// Выводит подсказку с наименованием КП, узла или СКРМ.
        /// </summary>
        /// <param name="e">Аргументы события.</param>
        protected override void OnCellToolTipTextNeeded(DataGridViewCellToolTipTextNeededEventArgs e)
        {
            base.OnCellToolTipTextNeeded(e);
            if (e.ColumnIndex < 0)
            {
                return;
            }

            HourlyColumn? column = Columns[e.ColumnIndex].Tag as HourlyColumn;
            if (column?.Group == null)
            {
                return;
            }

            e.ToolTipText = column.Kind == HourlyColumnKind.Voltage
                ? column.Group.ControlPoint.Name + " · узел " + column.NodeNumber
                : column.Device + " · " + column.Device?.TypeName;
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
        /// Рисует подпись раздела группы («U сш» или «СКРМ»).
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
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
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
        /// Проверяет, начинает ли столбец группу КП.
        /// </summary>
        /// <param name="columnIndex">Индекс столбца.</param>
        /// <returns><c>true</c>, если столбец первый в группе.</returns>
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
        /// Рисует ячейку заголовка: «ЧАС» или номер узла, реактора, генератора в нижней строке.
        /// </summary>
        /// <param name="graphics">Поверхность рисования.</param>
        /// <param name="bounds">Границы ячейки.</param>
        /// <param name="column">Описание столбца.</param>
        /// <param name="columnIndex">Индекс столбца.</param>
        private void PaintHeaderCell(Graphics graphics, Rectangle bounds, HourlyColumn column, int columnIndex)
        {
            FillBand(graphics, bounds);
            if (column.Kind == HourlyColumnKind.Hour)
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

            using (Pen pen = new Pen(Palette.Border))
            {
                graphics.DrawLine(pen, bounds.Left, bounds.Bottom - 1, bounds.Right, bounds.Bottom - 1);
                if (column.Kind == HourlyColumnKind.Hour)
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
            using (SolidBrush brush = new SolidBrush(hour % 2 == 0 ? _alternateBack : Palette.Background))
            {
                graphics.FillRectangle(brush, bounds);
            }

            TextFormatFlags flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
                | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix;
            if (column.Kind == HourlyColumnKind.Hour)
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
                    int top = bounds.Top + index * partHeight;
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

            using (Pen pen = new Pen(Palette.Border))
            {
                graphics.DrawLine(pen, bounds.Left, bounds.Bottom - 1, bounds.Right, bounds.Bottom - 1);
                if (column.Kind == HourlyColumnKind.Hour)
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
        /// Формирует текст и цвета значения ячейки.
        /// </summary>
        /// <param name="column">Описание столбца.</param>
        /// <param name="source">Источник значений.</param>
        /// <param name="hour">Час суток.</param>
        /// <param name="runIndex">Индекс сравниваемого расчета; -1 — предпросмотр.</param>
        /// <returns>Текст и цвета.</returns>
        private CellText GetCellText(HourlyColumn column, HourlyValueSource source, int hour, int runIndex)
        {
            Color numberColor = runIndex < 0
                ? Palette.Text
                : runIndex == 0
                    ? Palette.FirstRun
                    : Palette.SecondRun;
            if (column.Kind == HourlyColumnKind.Voltage)
            {
                double? voltage = source.Regime.GetVoltage(column.NodeNumber, hour);
                if (!voltage.HasValue)
                {
                    return new CellText("—", Palette.TextMuted, null);
                }

                string text = ValueFormat.ShortVoltage(voltage.Value);
                VoltageBounds? bounds = column.Group == null
                    ? null
                    : source.Schedule.GetBounds(column.Group.ControlPoint.Id, hour);
                if (bounds != null && bounds.IsAbove(voltage.Value))
                {
                    return new CellText(text, _aboveText, Palette.BlueDark);
                }

                if (bounds != null && bounds.IsBelow(voltage.Value))
                {
                    return new CellText(text, _belowText, Palette.RedDark);
                }

                return new CellText(text, numberColor, null);
            }

            Device? device = column.Device;
            DeviceMode? mode = device == null
                ? null
                : source.Regime.GetMode(device.Id, hour);
            if (device == null || mode == null)
            {
                return new CellText("—", Palette.TextMuted, null);
            }

            if (device.Type == DeviceType.Switched || !mode.IsOn || !mode.ReactivePower.HasValue)
            {
                return mode.IsOn
                    ? new CellText("Вкл", Palette.Accent, null)
                    : new CellText("Откл", Palette.TextMuted, null);
            }

            Color powerColor = runIndex < 0
                ? Palette.Purple
                : numberColor;
            return new CellText(ValueFormat.ShortPower(mode.ReactivePower.Value), powerColor, null);
        }

        /// <summary>
        /// Диапазон столбцов группы КП.
        /// </summary>
        private sealed class GroupRange
        {
            /// <summary>
            /// Создает диапазон столбцов.
            /// </summary>
            /// <param name="group">Группа КП.</param>
            /// <param name="first">Индекс первого столбца группы.</param>
            /// <param name="firstDevice">Индекс первого столбца СКРМ.</param>
            /// <param name="last">Индекс последнего столбца группы.</param>
            public GroupRange(HourlyGroup group, int first, int firstDevice, int last)
            {
                Group = group;
                First = first;
                FirstDevice = firstDevice;
                Last = last;
            }

            /// <summary>
            /// Группа КП.
            /// </summary>
            public HourlyGroup Group { get; }

            /// <summary>
            /// Индекс первого столбца группы.
            /// </summary>
            public int First { get; }

            /// <summary>
            /// Индекс первого столбца СКРМ.
            /// </summary>
            public int FirstDevice { get; }

            /// <summary>
            /// Индекс последнего столбца группы.
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
