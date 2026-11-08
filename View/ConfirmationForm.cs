using System;
using System.Windows.Forms;

using VrdoPdg.View.Theme;

namespace VrdoPdg.View
{
    /// <summary>
    /// Окно подтверждения действия в темном оформлении: остановка расчета,
    /// отказ от несохраненных изменений, восстановление таблицы эффективности по умолчанию.
    /// </summary>
    internal sealed partial class ConfirmationForm : BorderlessForm
    {
        /// <summary>
        /// Создает окно подтверждения.
        /// </summary>
        public ConfirmationForm()
        {
            InitializeComponent();
            EnableWindowDrag(_titleLabel, _messageLabel, _iconBox, _icon);
        }

        /// <summary>
        /// Создает окно подтверждения с заданным вопросом.
        /// </summary>
        /// <param name="title">Заголовок вопроса.</param>
        /// <param name="message">Текст вопроса.</param>
        /// <param name="confirmText">Текст кнопки подтверждения.</param>
        /// <param name="cancelText">Текст кнопки отказа.</param>
        /// <param name="isDangerous">Признак опасного действия (красная кнопка подтверждения).</param>
        public ConfirmationForm(string title, string message, string confirmText, string cancelText, bool isDangerous)
            : this()
        {
            _titleLabel.Text = title;
            _messageLabel.Text = message;
            _confirmButton.Text = confirmText;
            _cancelButton.Text = cancelText;
            _icon.Image = IconFactory.Create(IconGlyph.Warning, Palette.Warning, 24);
            UiStyle.ApplyOutlined(_cancelButton, null);
            if (isDangerous)
            {
                UiStyle.ApplyDanger(_confirmButton, null);
            }
            else
            {
                UiStyle.ApplyPrimary(_confirmButton, null);
            }

            AdjustButtons();
        }

        /// <summary>
        /// Запрашивает у пользователя подтверждение действия.
        /// </summary>
        /// <param name="owner">Окно-владелец.</param>
        /// <param name="title">Заголовок вопроса.</param>
        /// <param name="message">Текст вопроса.</param>
        /// <param name="confirmText">Текст кнопки подтверждения.</param>
        /// <param name="cancelText">Текст кнопки отказа.</param>
        /// <param name="isDangerous">Признак опасного действия.</param>
        /// <returns><c>true</c>, если действие подтверждено.</returns>
        public static bool Confirm(
            IWin32Window owner,
            string title,
            string message,
            string confirmText,
            string cancelText,
            bool isDangerous)
        {
            using (ConfirmationForm form = new ConfirmationForm(title, message, confirmText, cancelText, isDangerous))
            {
                return form.ShowDialog(owner) == DialogResult.OK;
            }
        }

        /// <summary>
        /// Подбирает ширину кнопок по тексту.
        /// </summary>
        private void AdjustButtons()
        {
            int confirmWidth = TextRenderer.MeasureText(_confirmButton.Text, _confirmButton.Font).Width + 40;
            int cancelWidth = TextRenderer.MeasureText(_cancelButton.Text, _cancelButton.Font).Width + 40;
            _confirmButton.Width = confirmWidth;
            _confirmButton.Left = ClientSize.Width - 24 - confirmWidth;
            _cancelButton.Width = cancelWidth;
            _cancelButton.Left = _confirmButton.Left - 12 - cancelWidth;
        }

        /// <summary>
        /// Подтверждает действие.
        /// </summary>
        /// <param name="sender">Кнопка.</param>
        /// <param name="e">Аргументы события.</param>
        private void ConfirmButton_Click(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        /// <summary>
        /// Отказывается от действия.
        /// </summary>
        /// <param name="sender">Кнопка.</param>
        /// <param name="e">Аргументы события.</param>
        private void CancelButton_Click(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        /// <summary>
        /// Закрывает окно клавишей Esc как отказ от действия.
        /// </summary>
        /// <param name="sender">Окно.</param>
        /// <param name="e">Аргументы события.</param>
        private void ConfirmationForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }
    }
}
