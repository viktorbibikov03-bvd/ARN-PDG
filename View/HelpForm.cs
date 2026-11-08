using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

using VrdoPdg.View.Theme;

namespace VrdoPdg.View
{
    /// <summary>
    /// Окно справки: оглавление руководства пользователя и содержимое выбранного раздела.
    /// </summary>
    internal sealed partial class HelpForm : Form
    {
        /// <summary>
        /// Имя ресурса руководства пользователя.
        /// </summary>
        private const string GuideResourceName = "VrdoPdg.View.UserGuide.md";

        /// <summary>
        /// Признак заголовка раздела в тексте руководства.
        /// </summary>
        private const string SectionPrefix = "# ";

        /// <summary>
        /// Признак пункта списка в тексте руководства.
        /// </summary>
        private const string BulletPrefix = "- ";

        /// <summary>
        /// Разделы руководства: заголовок и строки текста.
        /// </summary>
        private readonly List<KeyValuePair<string, List<string>>> _sections =
            new List<KeyValuePair<string, List<string>>>();

        /// <summary>
        /// Создает окно справки.
        /// </summary>
        public HelpForm()
        {
            InitializeComponent();
            LoadGuide();
            foreach (KeyValuePair<string, List<string>> section in _sections)
            {
                _sectionList.Items.Add(section.Key);
            }

            if (_sectionList.Items.Count > 0)
            {
                _sectionList.SelectedIndex = 0;
            }
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
        /// Загружает руководство пользователя из ресурсов и делит его на разделы.
        /// </summary>
        private void LoadGuide()
        {
            using (Stream? stream = typeof(HelpForm).Assembly.GetManifestResourceStream(GuideResourceName))
            {
                if (stream == null)
                {
                    _sections.Add(new KeyValuePair<string, List<string>>(
                        "Справка",
                        new List<string> { "Руководство пользователя не найдено." }));
                    return;
                }

                using (StreamReader reader = new StreamReader(stream))
                {
                    List<string>? lines = null;
                    string? line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.StartsWith(SectionPrefix, StringComparison.Ordinal))
                        {
                            lines = new List<string>();
                            string title = line.Substring(SectionPrefix.Length);
                            _sections.Add(new KeyValuePair<string, List<string>>(title, lines));
                        }
                        else if (lines != null && line.Trim().Length > 0)
                        {
                            lines.Add(line);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Выводит содержимое раздела.
        /// </summary>
        /// <param name="index">Индекс раздела.</param>
        private void ShowSection(int index)
        {
            if (index < 0 || index >= _sections.Count)
            {
                return;
            }

            KeyValuePair<string, List<string>> section = _sections[index];
            _contentBox.Clear();
            _contentBox.SelectionFont = UiFonts.Semibold(16f);
            _contentBox.SelectionColor = Palette.Text;
            _contentBox.AppendText(section.Key + Environment.NewLine + Environment.NewLine);
            foreach (string line in section.Value)
            {
                bool isBullet = line.StartsWith(BulletPrefix, StringComparison.Ordinal);
                _contentBox.SelectionFont = UiFonts.Regular(10.5f);
                _contentBox.SelectionColor = Color.FromArgb(220, 220, 220);
                _contentBox.SelectionIndent = isBullet ? 18 : 0;
                _contentBox.SelectionHangingIndent = isBullet ? 14 : 0;
                string text = isBullet
                    ? "•  " + line.Substring(BulletPrefix.Length)
                    : line;
                _contentBox.AppendText(text + Environment.NewLine + (isBullet ? string.Empty : Environment.NewLine));
            }

            _contentBox.SelectionStart = 0;
            _contentBox.ScrollToCaret();
        }

        /// <summary>
        /// Рисует пункт оглавления.
        /// </summary>
        /// <param name="sender">Оглавление.</param>
        /// <param name="e">Аргументы события.</param>
        private void SectionList_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0)
            {
                return;
            }

            bool isSelected = (e.State & DrawItemState.Selected) != 0;
            using (SolidBrush brush = new SolidBrush(isSelected ? Palette.AccentDark : Palette.Surface))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }

            if (isSelected)
            {
                using (SolidBrush marker = new SolidBrush(Palette.Accent))
                {
                    Rectangle markerBounds = new Rectangle(e.Bounds.Left, e.Bounds.Top + 8, 3, e.Bounds.Height - 16);
                    e.Graphics.FillRectangle(marker, markerBounds);
                }
            }

            Rectangle textBounds = new Rectangle(
                e.Bounds.Left + 18,
                e.Bounds.Top,
                e.Bounds.Width - 24,
                e.Bounds.Height);
            string? title = _sectionList.Items[e.Index].ToString();
            TextRenderer.DrawText(e.Graphics, title, UiFonts.Regular(10.5f), textBounds,
                isSelected ? Palette.Accent : Palette.Text,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        /// <summary>
        /// Выводит выбранный раздел.
        /// </summary>
        /// <param name="sender">Оглавление.</param>
        /// <param name="e">Аргументы события.</param>
        private void SectionList_SelectedIndexChanged(object? sender, EventArgs e)
        {
            ShowSection(_sectionList.SelectedIndex);
            _sectionList.Invalidate();
        }

        /// <summary>
        /// Закрывает окно клавишей Esc.
        /// </summary>
        /// <param name="sender">Окно.</param>
        /// <param name="e">Аргументы события.</param>
        private void HelpForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Close();
            }
        }
    }
}
