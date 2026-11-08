using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using VrdoPdg.Model;
using VrdoPdg.Model.Entities;
using VrdoPdg.Model.Services;
using VrdoPdg.View.Theme;

namespace VrdoPdg.View
{
    /// <summary>
    /// Модальное окно «Таблица эффективности»: просмотр и изменение порядка задействования СКРМ
    /// для выбранного КП, проверка и сохранение таблицы, восстановление состава СКРМ согласно ПУР.
    /// </summary>
    internal sealed partial class EfficiencyTableForm : Form
    {
        /// <summary>
        /// Сервис таблицы эффективности.
        /// </summary>
        private readonly EfficiencyTableService _service;

        /// <summary>
        /// Справочник СКРМ.
        /// </summary>
        private readonly IReadOnlyList<Device> _devices;

        /// <summary>
        /// Все КП.
        /// </summary>
        private readonly IReadOnlyList<ControlPoint> _controlPoints;

        /// <summary>
        /// Ошибки проверки по строкам таблицы.
        /// </summary>
        private readonly Dictionary<EfficiencyRow, string> _errors = new Dictionary<EfficiencyRow, string>();

        /// <summary>
        /// Количество СКРМ в действующих таблицах эффективности по коду КП.
        /// </summary>
        private IReadOnlyDictionary<int, int> _counts;

        /// <summary>
        /// Редактируемые строки таблицы выбранного КП.
        /// </summary>
        private List<EfficiencyRow> _rows = new List<EfficiencyRow>();

        /// <summary>
        /// Сохраненные строки таблицы выбранного КП.
        /// </summary>
        private List<EfficiencyRow> _original = new List<EfficiencyRow>();

        /// <summary>
        /// Выбранный КП.
        /// </summary>
        private ControlPoint? _controlPoint;

        /// <summary>
        /// Признак программного обновления таблицы или списка.
        /// </summary>
        private bool _isUpdating;

        /// <summary>
        /// Признак подтвержденного закрытия окна.
        /// </summary>
        private bool _isCloseConfirmed;

        /// <summary>
        /// Признак сообщения об успешном сохранении.
        /// </summary>
        private bool _isSaved;

        /// <summary>
        /// Индекс строки, которую начали перетаскивать.
        /// </summary>
        private int _dragRowIndex = -1;

        /// <summary>
        /// Область, за пределами которой начинается перетаскивание строки.
        /// </summary>
        private Rectangle _dragBox = Rectangle.Empty;

        /// <summary>
        /// Создает окно «Таблица эффективности».
        /// </summary>
        /// <param name="service">Сервис таблицы эффективности.</param>
        public EfficiencyTableForm(EfficiencyTableService service)
        {
            InitializeComponent();
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _devices = service.GetDevices();
            _controlPoints = service.GetControlPoints();
            _counts = service.GetActiveDeviceCounts();

            ApplyTheme();
            FillControlPointList();
        }

        /// <summary>
        /// Количество несохраненных изменений: измененные и добавленные строки
        /// и СКРМ, удаленные из таблицы.
        /// </summary>
        private int UnsavedChangeCount
        {
            get
            {
                int changed = _rows.Count(IsChanged);
                int removed = _original.Count(original => !_rows.Any(row => IsSameDevice(row, original)));
                return changed + removed;
            }
        }

        /// <summary>
        /// Проверяет, относятся ли строки к одному СКРМ.
        /// </summary>
        /// <param name="first">Первая строка.</param>
        /// <param name="second">Вторая строка.</param>
        /// <returns><c>true</c>, если тип и номер СКРМ совпадают.</returns>
        private static bool IsSameDevice(EfficiencyRow first, EfficiencyRow second)
        {
            return first.DeviceType == second.DeviceType && first.DeviceNumber == second.DeviceNumber;
        }

        /// <summary>
        /// Возвращает цвета метки типа СКРМ.
        /// </summary>
        /// <param name="type">Тип СКРМ.</param>
        /// <param name="foreColor">Цвет текста.</param>
        /// <param name="backColor">Цвет фона.</param>
        private static void GetTypeColors(DeviceType type, out Color foreColor, out Color backColor)
        {
            switch (type)
            {
                case DeviceType.Controlled:
                    foreColor = Palette.Accent;
                    backColor = Palette.AccentDark;
                    break;
                case DeviceType.Switched:
                    foreColor = Palette.Purple;
                    backColor = Palette.PurpleDark;
                    break;
                default:
                    foreColor = Palette.Warning;
                    backColor = Palette.WarningDark;
                    break;
            }
        }

        /// <summary>
        /// Разбирает целое число из текста ячейки.
        /// </summary>
        /// <param name="value">Значение ячейки.</param>
        /// <returns>Число или <c>null</c>, если ячейка пуста или содержит не число.</returns>
        private static int? ParseInteger(object? value)
        {
            int number;
            return value != null && int.TryParse(value.ToString()?.Trim(), out number)
                ? number
                : (int?)null;
        }

        /// <summary>
        /// Оформляет элементы окна.
        /// </summary>
        private void ApplyTheme()
        {
            _icon.Image = IconFactory.Create(IconGlyph.List, Palette.Purple, 24);
            _errorIcon.Image = IconFactory.Create(IconGlyph.Warning, Palette.Danger, 20);
            _closeButton.Text = string.Empty;
            UiStyle.ApplyFlat(_closeButton, IconGlyph.Close);
            _closeButton.ImageAlign = HorizontalAlignment.Center;
            UiStyle.ApplyPrimary(_defaultButton, null);
            UiStyle.ApplyOutlined(_cancelButton, null);
            UiStyle.ApplyPrimary(_saveButton, IconGlyph.Save);
            UiStyle.ApplyOutlined(_addButton, IconGlyph.Add);
            _borderlessForm.ShadowColor = Palette.Background;

            _searchBox.BorderRadius = 10;
            _searchBox.FillColor = Palette.Surface;
            _searchBox.BorderColor = Palette.Border;
            _searchBox.ForeColor = Palette.Text;
            _searchBox.PlaceholderForeColor = Palette.TextSecondary;
            _searchBox.Font = UiFonts.Regular(10.5f);
            _searchBox.IconLeft = IconFactory.Create(IconGlyph.Search, Palette.TextSecondary, 18);
            _searchBox.FocusedState.BorderColor = Palette.Accent;
            _searchBox.HoverState.BorderColor = Palette.TextMuted;

            UiStyle.ApplyGrid(_grid);
            _grid.ColumnHeadersHeight = 44;
            _grid.RowTemplate.Height = 58;
            _grid.CellBorderStyle = DataGridViewCellBorderStyle.None;
            _grid.DefaultCellStyle.SelectionBackColor = Palette.Background;
            _grid.ColumnHeadersDefaultCellStyle.BackColor = Palette.SurfaceRaised;
            _grid.ShowCellToolTips = true;
            _grid.EditMode = DataGridViewEditMode.EditOnEnter;
        }

        /// <summary>
        /// Заполняет список КП с учетом строки поиска.
        /// </summary>
        private void FillControlPointList()
        {
            string filter = _searchBox.Text.Trim();
            List<ControlPoint> items = _controlPoints
                .Where(controlPoint => filter.Length == 0
                    || controlPoint.Name.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) >= 0)
                .ToList();

            _isUpdating = true;
            _controlPointList.BeginUpdate();
            _controlPointList.Items.Clear();
            foreach (ControlPoint item in items)
            {
                _controlPointList.Items.Add(item);
            }

            _controlPointList.EndUpdate();
            if (_controlPoint != null && items.Contains(_controlPoint))
            {
                _controlPointList.SelectedItem = _controlPoint;
            }

            _isUpdating = false;
            if (_controlPoint == null && items.Count > 0)
            {
                _controlPointList.SelectedIndex = 0;
            }

            if (_controlPoint == null)
            {
                ShowTable(null);
            }
        }

        /// <summary>
        /// Выводит таблицу эффективности КП.
        /// </summary>
        /// <param name="controlPoint">КП или <c>null</c>.</param>
        private void ShowTable(ControlPoint? controlPoint)
        {
            _controlPoint = controlPoint;
            _errors.Clear();
            _isSaved = false;
            _errorBanner.Visible = false;
            if (controlPoint == null)
            {
                _original = new List<EfficiencyRow>();
                _rows = new List<EfficiencyRow>();
                _controlPointLabel.Text = "КП не выбран";
            }
            else
            {
                _original = _service.GetActiveTable(controlPoint.Id).ToList();
                _rows = _original.Select(row => row.Clone()).ToList();
                _controlPointLabel.Text = controlPoint.Name;
            }

            bool isEnabled = controlPoint != null;
            _addButton.Enabled = isEnabled;
            _defaultButton.Enabled = isEnabled;
            _saveButton.Enabled = isEnabled;
            RefreshGrid();
        }

        /// <summary>
        /// Перестраивает строки таблицы по редактируемым данным.
        /// </summary>
        private void RefreshGrid()
        {
            _grid.EndEdit();
            _isUpdating = true;
            _grid.Rows.Clear();
            foreach (EfficiencyRow row in _rows)
            {
                _grid.Rows.Add(
                    string.Empty,
                    row.Priority?.ToString() ?? string.Empty,
                    DeviceNames.GetTypeName(row.DeviceType),
                    row.DeviceNumber?.ToString() ?? string.Empty,
                    GetDeviceName(row),
                    string.Empty,
                    string.Empty,
                    string.Empty);
            }

            _isUpdating = false;
            UpdateStatus();
        }

        /// <summary>
        /// Возвращает наименование СКРМ строки.
        /// </summary>
        /// <param name="row">Строка таблицы.</param>
        /// <returns>Наименование или пустая строка.</returns>
        private string GetDeviceName(EfficiencyRow row)
        {
            Device? device = EfficiencyTableService.FindDevice(_devices, row.DeviceType, row.DeviceNumber);
            return device?.Name ?? string.Empty;
        }

        /// <summary>
        /// Проверяет, изменена ли строка относительно сохраненной таблицы.
        /// </summary>
        /// <param name="row">Строка таблицы.</param>
        /// <returns><c>true</c>, если строка добавлена или изменена.</returns>
        private bool IsChanged(EfficiencyRow row)
        {
            return !_original.Any(original => original.IsSameAs(row));
        }

        /// <summary>
        /// Обновляет счетчик несохраненных изменений или ошибок в заголовке окна.
        /// </summary>
        private void UpdateStatus()
        {
            int errorCount = _errors.Count;
            int changeCount = UnsavedChangeCount;
            if (errorCount > 0)
            {
                _statusPill.Text = TextFormat.Plural(errorCount, "ошибка", "ошибки", "ошибок");
                UiStyle.ApplyPill(_statusPill, Palette.Danger, Palette.DangerDark, IconGlyph.Warning);
                _statusPill.Visible = true;
            }
            else if (changeCount > 0)
            {
                _statusPill.Text = TextFormat.Plural(
                    changeCount,
                    "несохранённое изменение",
                    "несохранённых изменения",
                    "несохранённых изменений");
                UiStyle.ApplyPill(_statusPill, Palette.Warning, Palette.WarningDark, IconGlyph.Edit);
                _statusPill.Visible = true;
            }
            else if (_isSaved)
            {
                _statusPill.Text = "Изменения сохранены";
                UiStyle.ApplyPill(_statusPill, Palette.Accent, Palette.AccentDark, IconGlyph.CheckMark);
                _statusPill.Visible = true;
            }
            else
            {
                _statusPill.Visible = false;
            }

            _statusPill.Font = UiFonts.Semibold(10f);
            _grid.Invalidate();
        }

        /// <summary>
        /// Сообщает об изменении таблицы: сбрасывает результаты проверки и обновляет счетчики.
        /// </summary>
        private void OnTableChanged()
        {
            _errors.Clear();
            _isSaved = false;
            _errorBanner.Visible = false;
            UpdateStatus();
        }

        /// <summary>
        /// Перемещает строку на одну позицию вверх или вниз, меняя приоритеты местами.
        /// </summary>
        /// <param name="index">Индекс строки.</param>
        /// <param name="offset">Смещение: -1 — вверх, 1 — вниз.</param>
        private void MoveRow(int index, int offset)
        {
            int target = index + offset;
            if (index < 0 || target < 0 || target >= _rows.Count)
            {
                return;
            }

            EfficiencyRow row = _rows[index];
            EfficiencyRow other = _rows[target];
            int? priority = row.Priority;
            row.Priority = other.Priority;
            other.Priority = priority;
            _rows[index] = other;
            _rows[target] = row;
            RefreshGrid();
            OnTableChanged();
            SelectRow(target);
        }

        /// <summary>
        /// Перемещает строку перетаскиванием и нумерует приоритеты по порядку строк.
        /// </summary>
        /// <param name="source">Индекс перетаскиваемой строки.</param>
        /// <param name="target">Индекс строки, на место которой перемещается строка.</param>
        private void DragRow(int source, int target)
        {
            if (source < 0 || target < 0 || source >= _rows.Count || target >= _rows.Count || source == target)
            {
                return;
            }

            EfficiencyRow row = _rows[source];
            _rows.RemoveAt(source);
            _rows.Insert(target, row);
            for (int index = 0; index < _rows.Count; index++)
            {
                _rows[index].Priority = index + 1;
            }

            RefreshGrid();
            OnTableChanged();
            SelectRow(target);
        }

        /// <summary>
        /// Удаляет СКРМ из таблицы и сдвигает приоритеты следующих СКРМ.
        /// </summary>
        /// <param name="index">Индекс строки.</param>
        private void DeleteRow(int index)
        {
            if (index < 0 || index >= _rows.Count)
            {
                return;
            }

            int? priority = _rows[index].Priority;
            _rows.RemoveAt(index);
            if (priority.HasValue)
            {
                foreach (EfficiencyRow row in _rows.Where(row => row.Priority > priority.Value))
                {
                    row.Priority--;
                }
            }

            RefreshGrid();
            OnTableChanged();
        }

        /// <summary>
        /// Выделяет строку таблицы.
        /// </summary>
        /// <param name="index">Индекс строки.</param>
        private void SelectRow(int index)
        {
            if (index >= 0 && index < _grid.Rows.Count)
            {
                _grid.CurrentCell = _grid.Rows[index].Cells[_priorityColumn.Index];
            }
        }

        /// <summary>
        /// Запрашивает подтверждение отказа от несохраненных изменений.
        /// </summary>
        /// <returns><c>true</c>, если изменений нет или технолог отказался от них.</returns>
        private bool ConfirmDiscard()
        {
            int count = UnsavedChangeCount;
            if (count == 0 || _controlPoint == null)
            {
                return true;
            }

            string changes = TextFormat.Plural(count, "несохранённое изменение", "несохранённых изменения",
                "несохранённых изменений");
            return ConfirmationForm.Confirm(
                this,
                "Отменить изменения?",
                "В таблице эффективности КП «" + _controlPoint.Name + "» есть " + changes
                    + ". Если закрыть окно, они будут потеряны.",
                "Отменить",
                "Продолжить редактирование",
                true);
        }

        /// <summary>
        /// Показывает ошибки проверки: сообщение над таблицей и выделение строк с ошибками.
        /// </summary>
        /// <param name="errors">Ошибки проверки.</param>
        private void ShowErrors(IReadOnlyList<EfficiencyValidationError> errors)
        {
            _errors.Clear();
            List<string> tableErrors = new List<string>();
            foreach (EfficiencyValidationError error in errors)
            {
                if (error.RowIndex < 0 || error.RowIndex >= _rows.Count)
                {
                    tableErrors.Add(error.Message);
                    continue;
                }

                EfficiencyRow row = _rows[error.RowIndex];
                string? text;
                _errors[row] = _errors.TryGetValue(row, out text)
                    ? text + "; " + error.Message
                    : error.Message;
            }

            _errorLabel.Text = tableErrors.Count == 0
                ? "Изменения не сохранены: данные некорректны"
                : "Изменения не сохранены: " + string.Join("; ", tableErrors);
            _errorBanner.Visible = true;
            UpdateStatus();
            if (tableErrors.Count > 0 && _errors.Count == 0)
            {
                _statusPill.Text = TextFormat.Plural(tableErrors.Count, "ошибка", "ошибки", "ошибок");
                UiStyle.ApplyPill(_statusPill, Palette.Danger, Palette.DangerDark, IconGlyph.Warning);
                _statusPill.Font = UiFonts.Semibold(10f);
                _statusPill.Visible = true;
            }
        }

        /// <summary>
        /// Рисует ячейку таблицы эффективности.
        /// </summary>
        /// <param name="graphics">Поверхность рисования.</param>
        /// <param name="bounds">Границы ячейки.</param>
        /// <param name="row">Строка таблицы.</param>
        /// <param name="columnIndex">Индекс столбца.</param>
        private void PaintCell(Graphics graphics, Rectangle bounds, EfficiencyRow row, int columnIndex)
        {
            string? error;
            bool hasError = _errors.TryGetValue(row, out error);
            bool isChanged = IsChanged(row);
            Color background = hasError
                ? Palette.DangerDark
                : isChanged
                    ? Palette.AccentDark
                    : Palette.Background;
            using (SolidBrush brush = new SolidBrush(background))
            {
                graphics.FillRectangle(brush, bounds);
            }

            Color lineColor = hasError
                ? Palette.Danger
                : isChanged
                    ? Palette.Accent
                    : Palette.Border;
            using (Pen pen = new Pen(lineColor))
            {
                graphics.DrawLine(pen, bounds.Left, bounds.Bottom - 1, bounds.Right, bounds.Bottom - 1);
                if (hasError || isChanged)
                {
                    graphics.DrawLine(pen, bounds.Left, bounds.Top, bounds.Right, bounds.Top);
                }
            }

            TextFormatFlags centered = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
                | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix;
            if (columnIndex == _handleColumn.Index)
            {
                TextRenderer.DrawText(graphics, "⋮⋮", UiFonts.Bold(10f), bounds, Palette.TextMuted, centered);
            }
            else if (columnIndex == _priorityColumn.Index)
            {
                bool isPriorityError = hasError && error!.Contains("Приоритет");
                Rectangle box = new Rectangle(bounds.Left + 8, bounds.Top + (bounds.Height - 34) / 2, 42, 34);
                Color boxColor = isPriorityError
                    ? Palette.Danger
                    : Palette.SurfaceRaised;
                UiDrawing.FillRoundedRectangle(graphics, boxColor, box, 7);
                TextRenderer.DrawText(graphics, row.Priority?.ToString() ?? "—", UiFonts.MonospaceBold(11f), box,
                    isPriorityError ? Palette.OnAccent : Palette.Text, centered);
                if (isChanged && !hasError)
                {
                    UiDrawing.FillRoundedRectangle(graphics, Palette.Accent,
                        new Rectangle(box.Right + 8, bounds.Top + bounds.Height / 2 - 4, 8, 8), 4);
                }
            }
            else if (columnIndex == _typeColumn.Index)
            {
                Color foreColor;
                Color backColor;
                GetTypeColors(row.DeviceType, out foreColor, out backColor);
                string text = DeviceNames.GetTypeName(row.DeviceType);
                int width = TextRenderer.MeasureText(text, UiFonts.Semibold(10f)).Width + 22;
                Rectangle badge = new Rectangle(bounds.Left + 6, bounds.Top + (bounds.Height - 30) / 2,
                    Math.Min(width, bounds.Width - 12), 30);
                UiDrawing.FillRoundedRectangle(graphics, backColor, badge, 15);
                TextRenderer.DrawText(graphics, text, UiFonts.Semibold(10f), badge, foreColor, centered);
            }
            else if (columnIndex == _numberColumn.Index)
            {
                bool isNumberError = hasError && error!.Contains("не найден");
                Rectangle textBounds = new Rectangle(bounds.Left + 10, bounds.Top, bounds.Width - 10, bounds.Height);
                TextRenderer.DrawText(graphics, row.DeviceNumber?.ToString() ?? "—", UiFonts.Monospace(11f), textBounds,
                    isNumberError ? Palette.Danger : Palette.Text,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
            }
            else if (columnIndex == _nameColumn.Index)
            {
                string name = GetDeviceName(row);
                Rectangle nameBounds = new Rectangle(
                    bounds.Left + 8,
                    bounds.Top + (hasError ? 6 : 0),
                    bounds.Width - 12,
                    hasError ? bounds.Height / 2 - 2 : bounds.Height);
                TextFormatFlags flags = TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix
                    | (hasError ? TextFormatFlags.Bottom : TextFormatFlags.VerticalCenter) | TextFormatFlags.SingleLine;
                TextRenderer.DrawText(graphics, name.Length == 0 ? "—" : name, UiFonts.Regular(10.5f), nameBounds,
                    Palette.Text, flags);
                if (hasError)
                {
                    Rectangle errorBounds = new Rectangle(bounds.Left + 8, bounds.Top + bounds.Height / 2 + 2,
                        bounds.Width - 12, bounds.Height / 2 - 6);
                    TextRenderer.DrawText(graphics, error, UiFonts.Regular(9.5f), errorBounds, Palette.Danger,
                        TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.EndEllipsis
                        | TextFormatFlags.SingleLine);
                }
            }
            else
            {
                char glyph = columnIndex == _upColumn.Index
                    ? IconGlyph.Up
                    : columnIndex == _downColumn.Index
                        ? IconGlyph.Down
                        : IconGlyph.Delete;
                Color color = columnIndex == _deleteColumn.Index
                    ? Palette.Danger
                    : Palette.TextSecondary;
                Image? image = IconFactory.Create(glyph, color, 18);
                if (image != null)
                {
                    graphics.DrawImage(
                        image,
                        bounds.Left + (bounds.Width - 18) / 2,
                        bounds.Top + (bounds.Height - 18) / 2);
                }
                else
                {
                    string text = columnIndex == _upColumn.Index
                        ? "↑"
                        : columnIndex == _downColumn.Index
                            ? "↓"
                            : "×";
                    TextRenderer.DrawText(graphics, text, UiFonts.Bold(12f), bounds, color, centered);
                }
            }
        }

        /// <summary>
        /// Выбирает КП в списке. При наличии несохраненных изменений запрашивает подтверждение.
        /// </summary>
        /// <param name="sender">Список КП.</param>
        /// <param name="e">Аргументы события.</param>
        private void ControlPointList_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_isUpdating)
            {
                return;
            }

            ControlPoint? selected = _controlPointList.SelectedItem as ControlPoint;
            if (ReferenceEquals(selected, _controlPoint))
            {
                return;
            }

            if (!ConfirmDiscard())
            {
                _isUpdating = true;
                _controlPointList.SelectedItem = _controlPoint;
                _isUpdating = false;
                return;
            }

            ShowTable(selected);
            _controlPointList.Invalidate();
        }

        /// <summary>
        /// Рисует пункт списка КП: наименование, номер основного узла и количество СКРМ.
        /// </summary>
        /// <param name="sender">Список КП.</param>
        /// <param name="e">Аргументы события.</param>
        private void ControlPointList_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= _controlPointList.Items.Count)
            {
                return;
            }

            ControlPoint controlPoint = (ControlPoint)_controlPointList.Items[e.Index];
            bool isSelected = ReferenceEquals(controlPoint, _controlPoint);
            using (SolidBrush brush = new SolidBrush(_controlPointList.BackColor))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }

            Rectangle card = new Rectangle(e.Bounds.Left, e.Bounds.Top + 3, e.Bounds.Width - 2, e.Bounds.Height - 6);
            if (isSelected)
            {
                UiDrawing.FillRoundedRectangle(e.Graphics, Palette.AccentDark, card, 10);
                UiDrawing.DrawRoundedRectangle(e.Graphics, Palette.Accent, card, 10);
            }

            int count;
            _counts.TryGetValue(controlPoint.Id, out count);
            string countText = count.ToString();
            Rectangle badge = new Rectangle(card.Right - 38, card.Top + (card.Height - 26) / 2, 26, 26);
            Color badgeColor = isSelected
                ? Palette.AccentDark
                : Palette.SurfaceRaised;
            UiDrawing.FillRoundedRectangle(e.Graphics, badgeColor, badge, 13);
            TextRenderer.DrawText(e.Graphics, countText, UiFonts.MonospaceBold(9.5f), badge,
                isSelected ? Palette.Accent : Palette.TextSecondary,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            string nodeText = controlPoint.MainNode == null
                ? "узел —"
                : "узел " + controlPoint.MainNode.Number;
            Size nodeSize = TextRenderer.MeasureText(nodeText, UiFonts.Monospace(9.5f));
            Rectangle nodeBounds = new Rectangle(
                badge.Left - nodeSize.Width - 10,
                card.Top,
                nodeSize.Width + 4,
                card.Height);
            TextRenderer.DrawText(e.Graphics, nodeText, UiFonts.Monospace(9.5f), nodeBounds, Palette.TextMuted,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

            Rectangle nameBounds = new Rectangle(
                card.Left + 12,
                card.Top,
                nodeBounds.Left - card.Left - 16,
                card.Height);
            TextRenderer.DrawText(e.Graphics, controlPoint.Name, UiFonts.Semibold(10.5f), nameBounds,
                isSelected ? Palette.Text : Color.FromArgb(200, 200, 200),
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        /// <summary>
        /// Отбирает КП по вхождению строки поиска в наименование.
        /// </summary>
        /// <param name="sender">Поле поиска.</param>
        /// <param name="e">Аргументы события.</param>
        private void SearchBox_TextChanged(object? sender, EventArgs e)
        {
            FillControlPointList();
        }

        /// <summary>
        /// Рисует ячейки таблицы эффективности.
        /// </summary>
        /// <param name="sender">Таблица.</param>
        /// <param name="e">Аргументы события.</param>
        private void Grid_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.Graphics == null || e.RowIndex < 0 || e.ColumnIndex < 0 || e.RowIndex >= _rows.Count)
            {
                return;
            }

            if (_grid.IsCurrentCellInEditMode && _grid.CurrentCell != null
                && _grid.CurrentCell.RowIndex == e.RowIndex && _grid.CurrentCell.ColumnIndex == e.ColumnIndex)
            {
                return;
            }

            PaintCell(e.Graphics, e.CellBounds, _rows[e.RowIndex], e.ColumnIndex);
            e.Handled = true;
        }

        /// <summary>
        /// Выполняет команды «↑», «↓» и удаление СКРМ.
        /// </summary>
        /// <param name="sender">Таблица.</param>
        /// <param name="e">Аргументы события.</param>
        private void Grid_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _rows.Count)
            {
                return;
            }

            int rowIndex = e.RowIndex;
            if (e.ColumnIndex == _upColumn.Index)
            {
                BeginInvoke(new Action(() => MoveRow(rowIndex, -1)));
            }
            else if (e.ColumnIndex == _downColumn.Index)
            {
                BeginInvoke(new Action(() => MoveRow(rowIndex, 1)));
            }
            else if (e.ColumnIndex == _deleteColumn.Index)
            {
                BeginInvoke(new Action(() => DeleteRow(rowIndex)));
            }
        }

        /// <summary>
        /// Применяет значение, введенное в ячейку приоритета, типа или номера СКРМ.
        /// </summary>
        /// <param name="sender">Таблица.</param>
        /// <param name="e">Аргументы события.</param>
        private void Grid_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            if (_isUpdating || e.RowIndex < 0 || e.RowIndex >= _rows.Count)
            {
                return;
            }

            EfficiencyRow row = _rows[e.RowIndex];
            object? value = _grid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
            if (e.ColumnIndex == _priorityColumn.Index)
            {
                row.Priority = ParseInteger(value);
            }
            else if (e.ColumnIndex == _numberColumn.Index)
            {
                row.DeviceNumber = ParseInteger(value);
            }
            else if (e.ColumnIndex == _typeColumn.Index)
            {
                DeviceType type;
                if (DeviceNames.TryParseType(value?.ToString(), out type))
                {
                    row.DeviceType = type;
                }
            }
            else
            {
                return;
            }

            _isUpdating = true;
            _grid.Rows[e.RowIndex].Cells[_nameColumn.Index].Value = GetDeviceName(row);
            _isUpdating = false;
            OnTableChanged();
        }

        /// <summary>
        /// Сразу применяет значение, выбранное в списке типа СКРМ.
        /// </summary>
        /// <param name="sender">Таблица.</param>
        /// <param name="e">Аргументы события.</param>
        private void Grid_CurrentCellDirtyStateChanged(object? sender, EventArgs e)
        {
            if (_grid.IsCurrentCellDirty && _grid.CurrentCell?.ColumnIndex == _typeColumn.Index)
            {
                _grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        /// <summary>
        /// Подавляет стандартное окно ошибки таблицы при вводе недопустимого значения.
        /// </summary>
        /// <param name="sender">Таблица.</param>
        /// <param name="e">Аргументы события.</param>
        private void Grid_DataError(object? sender, DataGridViewDataErrorEventArgs e)
        {
            e.ThrowException = false;
        }

        /// <summary>
        /// Оформляет элемент редактирования ячейки в темной теме.
        /// </summary>
        /// <param name="sender">Таблица.</param>
        /// <param name="e">Аргументы события.</param>
        private void Grid_EditingControlShowing(object? sender, DataGridViewEditingControlShowingEventArgs e)
        {
            e.Control.BackColor = Palette.SurfaceRaised;
            e.Control.ForeColor = Palette.Text;
            e.Control.Font = UiFonts.Monospace(11f);
        }

        /// <summary>
        /// Запоминает строку, которую начали перетаскивать за маркер.
        /// </summary>
        /// <param name="sender">Таблица.</param>
        /// <param name="e">Аргументы события.</param>
        private void Grid_MouseDown(object? sender, MouseEventArgs e)
        {
            DataGridView.HitTestInfo hit = _grid.HitTest(e.X, e.Y);
            if (e.Button == MouseButtons.Left && hit.Type == DataGridViewHitTestType.Cell
                && hit.ColumnIndex == _handleColumn.Index)
            {
                _dragRowIndex = hit.RowIndex;
                Size dragSize = SystemInformation.DragSize;
                _dragBox = new Rectangle(new Point(e.X - dragSize.Width / 2, e.Y - dragSize.Height / 2), dragSize);
            }
            else
            {
                _dragRowIndex = -1;
                _dragBox = Rectangle.Empty;
            }
        }

        /// <summary>
        /// Начинает перетаскивание строки.
        /// </summary>
        /// <param name="sender">Таблица.</param>
        /// <param name="e">Аргументы события.</param>
        private void Grid_MouseMove(object? sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == 0 || _dragRowIndex < 0 || _dragBox == Rectangle.Empty
                || _dragBox.Contains(e.X, e.Y))
            {
                return;
            }

            _grid.DoDragDrop(_dragRowIndex, DragDropEffects.Move);
            _dragRowIndex = -1;
            _dragBox = Rectangle.Empty;
        }

        /// <summary>
        /// Разрешает перемещение строки.
        /// </summary>
        /// <param name="sender">Таблица.</param>
        /// <param name="e">Аргументы события.</param>
        private void Grid_DragOver(object? sender, DragEventArgs e)
        {
            e.Effect = e.Data != null && e.Data.GetDataPresent(typeof(int))
                ? DragDropEffects.Move
                : DragDropEffects.None;
        }

        /// <summary>
        /// Перемещает строку на место строки под указателем.
        /// </summary>
        /// <param name="sender">Таблица.</param>
        /// <param name="e">Аргументы события.</param>
        private void Grid_DragDrop(object? sender, DragEventArgs e)
        {
            if (e.Data == null || !e.Data.GetDataPresent(typeof(int)))
            {
                return;
            }

            int source = (int)e.Data.GetData(typeof(int))!;
            Point point = _grid.PointToClient(new Point(e.X, e.Y));
            int target = _grid.HitTest(point.X, point.Y).RowIndex;
            if (target < 0)
            {
                target = _rows.Count - 1;
            }

            DragRow(source, target);
        }

        /// <summary>
        /// Добавляет строку для нового СКРМ с приоритетом после последнего.
        /// </summary>
        /// <param name="sender">Кнопка «Добавить СКРМ».</param>
        /// <param name="e">Аргументы события.</param>
        private void AddButton_Click(object? sender, EventArgs e)
        {
            if (_controlPoint == null)
            {
                return;
            }

            int priority = _rows.Count == 0
                ? 1
                : _rows.Max(row => row.Priority ?? 0) + 1;
            _rows.Add(new EfficiencyRow(priority, DeviceType.Switched, null));
            RefreshGrid();
            OnTableChanged();
            int index = _rows.Count - 1;
            _grid.CurrentCell = _grid.Rows[index].Cells[_numberColumn.Index];
            _grid.BeginEdit(true);
        }

        /// <summary>
        /// Восстанавливает состав и приоритеты СКРМ выбранного КП согласно ПУР.
        /// </summary>
        /// <param name="sender">Кнопка «По умолчанию».</param>
        /// <param name="e">Аргументы события.</param>
        private void DefaultButton_Click(object? sender, EventArgs e)
        {
            if (_controlPoint == null)
            {
                return;
            }

            bool isConfirmed = ConfirmationForm.Confirm(
                this,
                "Восстановить по умолчанию?",
                "Таблица эффективности КП «" + _controlPoint.Name
                    + "» будет заменена составом и приоритетами СКРМ согласно ПУР. "
                    + "Изменения запишутся в БД после нажатия «Сохранить».",
                "Восстановить",
                "Отмена",
                false);
            if (!isConfirmed)
            {
                return;
            }

            _rows = _service.GetDefaultTable(_controlPoint.Id).ToList();
            RefreshGrid();
            OnTableChanged();
        }

        /// <summary>
        /// Проверяет таблицу и сохраняет ее в БД.
        /// </summary>
        /// <param name="sender">Кнопка «Сохранить».</param>
        /// <param name="e">Аргументы события.</param>
        private void SaveButton_Click(object? sender, EventArgs e)
        {
            if (_controlPoint == null)
            {
                return;
            }

            _grid.EndEdit();
            IReadOnlyList<EfficiencyValidationError> errors = EfficiencyTableService.Validate(_rows, _devices);
            if (errors.Count > 0)
            {
                ShowErrors(errors);
                return;
            }

            try
            {
                _service.Save(_controlPoint, _rows);
            }
            catch (VrdoException exception)
            {
                MessageBox.Show(this, exception.Message + Environment.NewLine + exception.Details,
                    "Таблица эффективности", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            catch (Exception exception)
            {
                MessageBox.Show(this, DataLoadService.DatabaseErrorMessage + Environment.NewLine + exception.Message,
                    "Таблица эффективности", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ControlPoint controlPoint = _controlPoint;
            _counts = _service.GetActiveDeviceCounts();
            ShowTable(controlPoint);
            _isSaved = true;
            UpdateStatus();
            _controlPointList.Invalidate();
        }

        /// <summary>
        /// Закрывает окно, запрашивая подтверждение при несохраненных изменениях.
        /// </summary>
        /// <param name="sender">Кнопка «Отмена» или кнопка закрытия.</param>
        /// <param name="e">Аргументы события.</param>
        private void CancelButton_Click(object? sender, EventArgs e)
        {
            if (ConfirmDiscard())
            {
                _isCloseConfirmed = true;
                Close();
            }
        }

        /// <summary>
        /// Запрашивает подтверждение при закрытии окна с несохраненными изменениями.
        /// </summary>
        /// <param name="sender">Окно.</param>
        /// <param name="e">Аргументы события.</param>
        private void EfficiencyTableForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (!_isCloseConfirmed && !ConfirmDiscard())
            {
                e.Cancel = true;
            }
        }

        /// <summary>
        /// Закрывает окно клавишей Esc.
        /// </summary>
        /// <param name="sender">Окно.</param>
        /// <param name="e">Аргументы события.</param>
        private void EfficiencyTableForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape && !_grid.IsCurrentCellInEditMode)
            {
                CancelButton_Click(sender, e);
            }
        }
    }
}
