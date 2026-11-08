#nullable disable

namespace VrdoPdg.View
{
    partial class LoadDataForm
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
            _titleLabel = new System.Windows.Forms.Label();
            _closeButton = new VrdoPdg.View.Controls.RoundedButton();
            _megapointCard = new VrdoPdg.View.Controls.FileCard();
            _smpSmzuCard = new VrdoPdg.View.Controls.FileCard();
            _hintPanel = new VrdoPdg.View.Controls.RoundedPanel();
            _hintIcon = new System.Windows.Forms.PictureBox();
            _hintLabel = new System.Windows.Forms.Label();
            _cancelButton = new VrdoPdg.View.Controls.RoundedButton();
            _doneButton = new VrdoPdg.View.Controls.RoundedButton();
            _hintPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_hintIcon).BeginInit();
            SuspendLayout();
            //
            // _titleLabel
            //
            _titleLabel.AutoSize = true;
            _titleLabel.Font = new System.Drawing.Font("Segoe UI Semibold", 18F);
            _titleLabel.ForeColor = System.Drawing.Color.FromArgb(236, 236, 236);
            _titleLabel.Location = new System.Drawing.Point(22, 20);
            _titleLabel.Name = "_titleLabel";
            _titleLabel.Size = new System.Drawing.Size(220, 41);
            _titleLabel.TabIndex = 0;
            _titleLabel.Text = "Загрузка данных";
            //
            // _closeButton
            //
            _closeButton.Location = new System.Drawing.Point(486, 20);
            _closeButton.Name = "_closeButton";
            _closeButton.Size = new System.Drawing.Size(36, 36);
            _closeButton.TabIndex = 6;
            _closeButton.Click += CancelButton_Click;
            //
            // _megapointCard
            //
            _megapointCard.Location = new System.Drawing.Point(24, 80);
            _megapointCard.Name = "_megapointCard";
            _megapointCard.Size = new System.Drawing.Size(498, 96);
            _megapointCard.TabIndex = 1;
            _megapointCard.ChooseRequested += MegapointCard_ChooseRequested;
            //
            // _smpSmzuCard
            //
            _smpSmzuCard.Location = new System.Drawing.Point(24, 190);
            _smpSmzuCard.Name = "_smpSmzuCard";
            _smpSmzuCard.Size = new System.Drawing.Size(498, 96);
            _smpSmzuCard.TabIndex = 2;
            _smpSmzuCard.ChooseRequested += SmpSmzuCard_ChooseRequested;
            //
            // _hintPanel
            //
            _hintPanel.BorderRadius = 12;
            _hintPanel.Controls.Add(_hintLabel);
            _hintPanel.Controls.Add(_hintIcon);
            _hintPanel.FillColor = System.Drawing.Color.FromArgb(32, 32, 32);
            _hintPanel.Location = new System.Drawing.Point(24, 302);
            _hintPanel.Name = "_hintPanel";
            _hintPanel.Size = new System.Drawing.Size(498, 62);
            _hintPanel.TabIndex = 3;
            //
            // _hintIcon
            //
            _hintIcon.BackColor = System.Drawing.Color.Transparent;
            _hintIcon.Location = new System.Drawing.Point(14, 17);
            _hintIcon.Name = "_hintIcon";
            _hintIcon.Size = new System.Drawing.Size(28, 28);
            _hintIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            _hintIcon.TabIndex = 0;
            _hintIcon.TabStop = false;
            //
            // _hintLabel
            //
            _hintLabel.BackColor = System.Drawing.Color.Transparent;
            _hintLabel.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            _hintLabel.ForeColor = System.Drawing.Color.FromArgb(140, 140, 140);
            _hintLabel.Location = new System.Drawing.Point(50, 8);
            _hintLabel.Name = "_hintLabel";
            _hintLabel.Size = new System.Drawing.Size(436, 46);
            _hintLabel.TabIndex = 1;
            _hintLabel.Text = "Серый — файл ещё не загружен, зелёный — загружен. "
                + "Сообщения о загрузке дублируются в протоколе.";
            _hintLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // _cancelButton
            //
            _cancelButton.Location = new System.Drawing.Point(286, 392);
            _cancelButton.Name = "_cancelButton";
            _cancelButton.Size = new System.Drawing.Size(112, 42);
            _cancelButton.TabIndex = 5;
            _cancelButton.Text = "Отмена";
            _cancelButton.Click += CancelButton_Click;
            //
            // _doneButton
            //
            _doneButton.Location = new System.Drawing.Point(410, 392);
            _doneButton.Name = "_doneButton";
            _doneButton.Size = new System.Drawing.Size(112, 42);
            _doneButton.TabIndex = 4;
            _doneButton.Text = "Готово";
            _doneButton.Click += DoneButton_Click;
            //
            // LoadDataForm
            //
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.FromArgb(24, 24, 24);
            ClientSize = new System.Drawing.Size(546, 458);
            Controls.Add(_doneButton);
            Controls.Add(_cancelButton);
            Controls.Add(_hintPanel);
            Controls.Add(_smpSmzuCard);
            Controls.Add(_megapointCard);
            Controls.Add(_closeButton);
            Controls.Add(_titleLabel);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            KeyPreview = true;
            Name = "LoadDataForm";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Загрузка данных";
            KeyDown += LoadDataForm_KeyDown;
            _hintPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)_hintIcon).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        /// <summary>
        /// Заголовок окна.
        /// </summary>
        private System.Windows.Forms.Label _titleLabel;

        /// <summary>
        /// Кнопка закрытия окна.
        /// </summary>
        private VrdoPdg.View.Controls.RoundedButton _closeButton;

        /// <summary>
        /// Карточка файла МТ.
        /// </summary>
        private VrdoPdg.View.Controls.FileCard _megapointCard;

        /// <summary>
        /// Карточка файла smp_smzu.csv.
        /// </summary>
        private VrdoPdg.View.Controls.FileCard _smpSmzuCard;

        /// <summary>
        /// Подсказка о цветовой индикации.
        /// </summary>
        private VrdoPdg.View.Controls.RoundedPanel _hintPanel;

        /// <summary>
        /// Значок подсказки.
        /// </summary>
        private System.Windows.Forms.PictureBox _hintIcon;

        /// <summary>
        /// Текст подсказки.
        /// </summary>
        private System.Windows.Forms.Label _hintLabel;

        /// <summary>
        /// Кнопка «Отмена».
        /// </summary>
        private VrdoPdg.View.Controls.RoundedButton _cancelButton;

        /// <summary>
        /// Кнопка «Готово».
        /// </summary>
        private VrdoPdg.View.Controls.RoundedButton _doneButton;
    }
}
