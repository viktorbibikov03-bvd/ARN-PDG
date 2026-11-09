using System;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

using VrdoPdg.Model.Services;
using VrdoPdg.View.Controls;
using VrdoPdg.View.Layout;
using VrdoPdg.View.Theme;

namespace VrdoPdg.View
{
    /// <summary>
    /// Форма «Сравнение расчетов»: в каждой ячейке два значения — сверху значение расчета
    /// с меньшим номером, снизу — второго расчета.
    /// </summary>
    internal sealed partial class ComparisonForm : Form
    {
        /// <summary>
        /// Создает форму сравнения расчетов.
        /// </summary>
        /// <param name="first">Расчет с меньшим номером.</param>
        /// <param name="second">Второй расчет.</param>
        /// <param name="layout">Схема таблиц по часам.</param>
        public ComparisonForm(CalculationResult first, CalculationResult second, HourlyLayout layout)
        {
            InitializeComponent();
            if (first == null)
            {
                throw new ArgumentNullException(nameof(first));
            }

            if (second == null)
            {
                throw new ArgumentNullException(nameof(second));
            }

            UiStyle.ApplyOutlined(_backButton, IconGlyph.Back);
            _subtitleLabel.Text = GetDayText(first) + "Расчёт №" + first.Number + " ("
                + first.StartedAt.ToString("dd.MM HH:mm") + ") и расчёт №" + second.Number + " ("
                + second.StartedAt.ToString("dd.MM HH:mm") + ")";
            _tableView.Display(
                HourlyTableModel.CreateComparison(layout, first, second),
                new[] { "расчёт №" + first.Number, "расчёт №" + second.Number });
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
        /// Определяет сутки расчета по каталогу результатов МТ (D:\BarsMDP\Results\ДДММГГ).
        /// </summary>
        /// <param name="result">Расчет.</param>
        /// <returns>Строка вида «Сутки 13.10.2026 · » или пустая строка.</returns>
        private static string GetDayText(CalculationResult result)
        {
            string? directory = Path.GetFileName(Path.GetDirectoryName(result.Session.MegapointPath));
            DateTime day;
            return directory != null
                && DateTime.TryParseExact(
                    directory,
                    "ddMMyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out day)
                ? "Сутки " + day.ToString("dd.MM.yyyy") + " · "
                : string.Empty;
        }

        /// <summary>
        /// Возвращает к архиву расчетов.
        /// </summary>
        /// <param name="sender">Кнопка «К архиву».</param>
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
        private void ComparisonForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Close();
            }
        }
    }
}
