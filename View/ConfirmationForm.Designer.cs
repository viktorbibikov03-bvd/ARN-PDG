#nullable disable

namespace VrdoPdg.View
{
    partial class ConfirmationForm
    {
        /// <summary>
        /// Контейнер компонентов формы.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освобождает ресурсы формы.
        /// </summary>
        /// <param name="disposing">Признак освобождения управляемых ресурсов.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Создает элементы управления формы.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            _borderlessForm = new Guna.UI2.WinForms.Guna2BorderlessForm(components);
            _iconBox = new Guna.UI2.WinForms.Guna2Panel();
            _icon = new System.Windows.Forms.PictureBox();
            _titleLabel = new System.Windows.Forms.Label();
            _messageLabel = new System.Windows.Forms.Label();
            _cancelButton = new Guna.UI2.WinForms.Guna2Button();
            _confirmButton = new Guna.UI2.WinForms.Guna2Button();
            _iconBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_icon).BeginInit();
            SuspendLayout();
            //
            // _borderlessForm
            //
            _borderlessForm.BorderRadius = 18;
            _borderlessForm.ContainerControl = this;
            _borderlessForm.DragForm = true;
            _borderlessForm.ResizeForm = false;
            _borderlessForm.TransparentWhileDrag = true;
            //
            // _iconBox
            //
            _iconBox.BorderRadius = 10;
            _iconBox.Controls.Add(_icon);
            _iconBox.FillColor = System.Drawing.Color.FromArgb(58, 42, 14);
            _iconBox.Location = new System.Drawing.Point(24, 24);
            _iconBox.Name = "_iconBox";
            _iconBox.Size = new System.Drawing.Size(44, 44);
            _iconBox.TabIndex = 0;
            //
            // _icon
            //
            _icon.BackColor = System.Drawing.Color.Transparent;
            _icon.Dock = System.Windows.Forms.DockStyle.Fill;
            _icon.Name = "_icon";
            _icon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            _icon.TabIndex = 0;
            _icon.TabStop = false;
            //
            // _titleLabel
            //
            _titleLabel.AutoEllipsis = true;
            _titleLabel.Font = new System.Drawing.Font("Segoe UI Semibold", 16F);
            _titleLabel.ForeColor = System.Drawing.Color.FromArgb(236, 236, 236);
            _titleLabel.Location = new System.Drawing.Point(22, 82);
            _titleLabel.Name = "_titleLabel";
            _titleLabel.Size = new System.Drawing.Size(376, 36);
            _titleLabel.TabIndex = 1;
            _titleLabel.Text = "Заголовок";
            //
            // _messageLabel
            //
            _messageLabel.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            _messageLabel.ForeColor = System.Drawing.Color.FromArgb(140, 140, 140);
            _messageLabel.Location = new System.Drawing.Point(24, 122);
            _messageLabel.Name = "_messageLabel";
            _messageLabel.Size = new System.Drawing.Size(372, 66);
            _messageLabel.TabIndex = 2;
            _messageLabel.Text = "Сообщение";
            //
            // _cancelButton
            //
            _cancelButton.Location = new System.Drawing.Point(46, 200);
            _cancelButton.Name = "_cancelButton";
            _cancelButton.Size = new System.Drawing.Size(232, 40);
            _cancelButton.TabIndex = 4;
            _cancelButton.Text = "Отмена";
            _cancelButton.Click += CancelButton_Click;
            //
            // _confirmButton
            //
            _confirmButton.Location = new System.Drawing.Point(290, 200);
            _confirmButton.Name = "_confirmButton";
            _confirmButton.Size = new System.Drawing.Size(106, 40);
            _confirmButton.TabIndex = 3;
            _confirmButton.Text = "ОК";
            _confirmButton.Click += ConfirmButton_Click;
            //
            // ConfirmationForm
            //
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.FromArgb(24, 24, 24);
            ClientSize = new System.Drawing.Size(420, 260);
            Controls.Add(_confirmButton);
            Controls.Add(_cancelButton);
            Controls.Add(_messageLabel);
            Controls.Add(_titleLabel);
            Controls.Add(_iconBox);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            KeyPreview = true;
            Name = "ConfirmationForm";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Подтверждение";
            KeyDown += ConfirmationForm_KeyDown;
            _iconBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)_icon).EndInit();
            ResumeLayout(false);
        }

        /// <summary>
        /// Оформление окна без рамки.
        /// </summary>
        private Guna.UI2.WinForms.Guna2BorderlessForm _borderlessForm;

        /// <summary>
        /// Подложка значка.
        /// </summary>
        private Guna.UI2.WinForms.Guna2Panel _iconBox;

        /// <summary>
        /// Значок предупреждения.
        /// </summary>
        private System.Windows.Forms.PictureBox _icon;

        /// <summary>
        /// Заголовок вопроса.
        /// </summary>
        private System.Windows.Forms.Label _titleLabel;

        /// <summary>
        /// Текст вопроса.
        /// </summary>
        private System.Windows.Forms.Label _messageLabel;

        /// <summary>
        /// Кнопка отказа от действия.
        /// </summary>
        private Guna.UI2.WinForms.Guna2Button _cancelButton;

        /// <summary>
        /// Кнопка подтверждения действия.
        /// </summary>
        private Guna.UI2.WinForms.Guna2Button _confirmButton;
    }
}
