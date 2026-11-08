using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using VrdoPdg.Model.Services;
using VrdoPdg.View.Theme;

namespace VrdoPdg.View
{
    /// <summary>
    /// Форма «Архив расчетов»: список расчетов текущего сеанса с выбором двух расчетов для сравнения.
    /// </summary>
    internal sealed partial class ArchiveForm : Form
    {
        /// <summary>
        /// Наибольшее количество расчетов, выбираемых для сравнения.
        /// </summary>
        private const int MaxSelection = 2;

        /// <summary>
        /// Сеанс и архив расчетов.
        /// </summary>
        private readonly CalculationArchiveService _archive;

        /// <summary>
        /// Номера выбранных расчетов в порядке выбора.
        /// </summary>
        private readonly List<int> _selection = new List<int>();

        /// <summary>
        /// Расчеты архива в порядке строк списка.
        /// </summary>
        private IReadOnlyList<CalculationResult> _results = new CalculationResult[0];

        /// <summary>
        /// Создает форму «Архив расчетов».
        /// </summary>
        /// <param name="archive">Сеанс и архив расчетов.</param>
        public ArchiveForm(CalculationArchiveService archive)
        {
            InitializeComponent();
            _archive = archive ?? throw new ArgumentNullException(nameof(archive));

            UiStyle.ApplyOutlined(_backButton, IconGlyph.Back);
            UiStyle.ApplyFlat(_resetButton, null);
            UiStyle.ApplyPrimary(_compareButton, IconGlyph.Compare);
            UiStyle.ApplyGrid(_grid);
            _grid.ColumnHeadersHeight = 44;
            _grid.RowTemplate.Height = 52;
            _grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            _grid.DefaultCellStyle.Font = UiFonts.MonospaceBold(11f);
            _grid.DefaultCellStyle.SelectionBackColor = Palette.Background;
            _startColumn.DefaultCellStyle.Font = UiFonts.Monospace(11f);
            _startColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            _startColumn.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            RefreshResults();
        }

        /// <summary>
        /// Обновляет список расчетов из архива.
        /// </summary>
        public void RefreshResults()
        {
            _results = _archive.GetResults();
            _selection.RemoveAll(number => _results.All(result => result.Number != number));
            _grid.Rows.Clear();
            foreach (CalculationResult result in _results)
            {
                _grid.Rows.Add(string.Empty, "№" + result.Number, result.StartedAt.ToString("dd.MM | HH:mm"));
            }

            _grid.ClearSelection();
            _emptyLabel.Visible = _results.Count == 0;
            _grid.Visible = _results.Count > 0;
            UpdateButtons();
        }

        /// <summary>
        /// Включает темный заголовок окна.
        /// </summary>
        /// <param name="e">Аргументы события.</param>
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            WindowTheme.ApplyDarkTitleBar(this);
        }

        /// <summary>
        /// Делает кнопку «Сравнить» доступной при двух выбранных расчетах.
        /// </summary>
        private void UpdateButtons()
        {
            _compareButton.Enabled = _selection.Count == MaxSelection;
            _resetButton.Enabled = _selection.Count > 0;
            _grid.Invalidate();
        }

        /// <summary>
        /// Отмечает расчет или снимает отметку. Одновременно может быть отмечено не более двух расчетов.
        /// </summary>
        /// <param name="sender">Список расчетов.</param>
        /// <param name="e">Аргументы события.</param>
        private void Grid_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _results.Count)
            {
                return;
            }

            int number = _results[e.RowIndex].Number;
            if (!_selection.Remove(number) && _selection.Count < MaxSelection)
            {
                _selection.Add(number);
            }

            _grid.ClearSelection();
            UpdateButtons();
        }

        /// <summary>
        /// Рисует отметку расчета: зеленую для первого, фиолетовую для второго выбранного расчета.
        /// </summary>
        /// <param name="sender">Список расчетов.</param>
        /// <param name="e">Аргументы события.</param>
        private void Grid_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != _selectColumn.Index || e.Graphics == null
                || e.RowIndex >= _results.Count)
            {
                return;
            }

            e.PaintBackground(e.CellBounds, false);
            int index = _selection.IndexOf(_results[e.RowIndex].Number);
            Rectangle box = new Rectangle(
                e.CellBounds.Left + 22,
                e.CellBounds.Top + (e.CellBounds.Height - 20) / 2,
                20,
                20);
            if (index >= 0)
            {
                UiDrawing.FillRoundedRectangle(e.Graphics, index == 0 ? Palette.Accent : Palette.Purple, box, 10);
            }
            else
            {
                Color border = _selection.Count >= MaxSelection
                    ? Palette.SurfaceRaised
                    : Palette.TextMuted;
                UiDrawing.DrawRoundedRectangle(e.Graphics, border, box, 6);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Снимает все отметки.
        /// </summary>
        /// <param name="sender">Кнопка «Сбросить выбор».</param>
        /// <param name="e">Аргументы события.</param>
        private void ResetButton_Click(object? sender, EventArgs e)
        {
            _selection.Clear();
            UpdateButtons();
        }

        /// <summary>
        /// Открывает форму «Сравнение расчетов»: сверху — расчет с меньшим номером.
        /// </summary>
        /// <param name="sender">Кнопка «Сравнить».</param>
        /// <param name="e">Аргументы события.</param>
        private void CompareButton_Click(object? sender, EventArgs e)
        {
            if (_selection.Count != MaxSelection)
            {
                return;
            }

            List<CalculationResult> selected = _results
                .Where(result => _selection.Contains(result.Number))
                .OrderBy(result => result.Number)
                .ToList();
            using (ComparisonForm form = new ComparisonForm(selected[0], selected[1]))
            {
                form.ShowDialog(this);
            }
        }

        /// <summary>
        /// Возвращает к главному окну.
        /// </summary>
        /// <param name="sender">Кнопка «К расчёту».</param>
        /// <param name="e">Аргументы события.</param>
        private void BackButton_Click(object? sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Закрывает форму клавишей Esc.
        /// </summary>
        /// <param name="sender">Форма.</param>
        /// <param name="e">Аргументы события.</param>
        private void ArchiveForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Close();
            }
        }
    }
}
