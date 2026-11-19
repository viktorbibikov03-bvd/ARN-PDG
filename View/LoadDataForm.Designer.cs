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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoadDataForm));
            _titleLabel = new System.Windows.Forms.Label();
            _closeButton = new VrdoPdg.View.Controls.RoundedButton();
            _megapointCard = new VrdoPdg.View.Controls.FileCard();
            _smpSmzuCard = new VrdoPdg.View.Controls.FileCard();
            _hintPanel = new VrdoPdg.View.Controls.RoundedPanel();
            _hintLabel = new System.Windows.Forms.Label();
            _hintIcon = new System.Windows.Forms.PictureBox();
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
            _titleLabel.Location = new System.Drawing.Point(33, 30);
            _titleLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            _titleLabel.Name = "_titleLabel";
            _titleLabel.Size = new System.Drawing.Size(305, 48);
            _titleLabel.TabIndex = 0;
            _titleLabel.Text = "Загрузка данных";
            // 
            // _closeButton
            // 
            _closeButton.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            _closeButton.Location = new System.Drawing.Point(729, 30);
            _closeButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            _closeButton.Name = "_closeButton";
            _closeButton.Padding = new System.Windows.Forms.Padding(21, 0, 21, 0);
            _closeButton.Size = new System.Drawing.Size(54, 54);
            _closeButton.TabIndex = 6;
            _closeButton.Click += CancelButton_Click;
            // 
            // _megapointCard
            // 
            _megapointCard.BackColor = System.Drawing.Color.FromArgb(24, 24, 24);
            _megapointCard.Extension = "";
            _megapointCard.Location = new System.Drawing.Point(36, 120);
            _megapointCard.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            _megapointCard.Name = "_megapointCard";
            _megapointCard.Size = new System.Drawing.Size(747, 144);
            _megapointCard.TabIndex = 1;
            _megapointCard.Title = "";
            _megapointCard.ChooseRequested += MegapointCard_ChooseRequested;
            // 
            // _smpSmzuCard
            // 
            _smpSmzuCard.BackColor = System.Drawing.Color.FromArgb(24, 24, 24);
            _smpSmzuCard.Extension = "";
            _smpSmzuCard.Location = new System.Drawing.Point(36, 285);
            _smpSmzuCard.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            _smpSmzuCard.Name = "_smpSmzuCard";
            _smpSmzuCard.Size = new System.Drawing.Size(747, 144);
            _smpSmzuCard.TabIndex = 2;
            _smpSmzuCard.Title = "";
            _smpSmzuCard.ChooseRequested += SmpSmzuCard_ChooseRequested;
            // 
            // _hintPanel
            // 
            _hintPanel.Controls.Add(_hintLabel);
            _hintPanel.Controls.Add(_hintIcon);
            _hintPanel.FillColor = System.Drawing.Color.FromArgb(32, 32, 32);
            _hintPanel.Location = new System.Drawing.Point(36, 453);
            _hintPanel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            _hintPanel.Name = "_hintPanel";
            _hintPanel.Size = new System.Drawing.Size(747, 93);
            _hintPanel.TabIndex = 3;
            // 
            // _hintLabel
            // 
            _hintLabel.BackColor = System.Drawing.Color.Transparent;
            _hintLabel.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            _hintLabel.ForeColor = System.Drawing.Color.FromArgb(140, 140, 140);
            _hintLabel.Location = new System.Drawing.Point(75, 12);
            _hintLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            _hintLabel.Name = "_hintLabel";
            _hintLabel.Size = new System.Drawing.Size(654, 69);
            _hintLabel.TabIndex = 1;
            _hintLabel.Text = "Серый — файл ещё не загружен, зелёный — загружен.";
            _hintLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _hintIcon
            // 
            _hintIcon.BackColor = System.Drawing.Color.Transparent;
            _hintIcon.Location = new System.Drawing.Point(21, 26);
            _hintIcon.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            _hintIcon.Name = "_hintIcon";
            _hintIcon.Size = new System.Drawing.Size(42, 42);
            _hintIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            _hintIcon.TabIndex = 0;
            _hintIcon.TabStop = false;
            // 
            // _cancelButton
            // 
            _cancelButton.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            _cancelButton.Location = new System.Drawing.Point(429, 588);
            _cancelButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            _cancelButton.Name = "_cancelButton";
            _cancelButton.Padding = new System.Windows.Forms.Padding(21, 0, 21, 0);
            _cancelButton.Size = new System.Drawing.Size(168, 63);
            _cancelButton.TabIndex = 5;
            _cancelButton.Text = "Отмена";
            _cancelButton.Click += CancelButton_Click;
            // 
            // _doneButton
            // 
            _doneButton.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            _doneButton.Location = new System.Drawing.Point(615, 588);
            _doneButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            _doneButton.Name = "_doneButton";
            _doneButton.Padding = new System.Windows.Forms.Padding(21, 0, 21, 0);
            _doneButton.Size = new System.Drawing.Size(168, 63);
            _doneButton.TabIndex = 4;
            _doneButton.Text = "Готово";
            _doneButton.Click += DoneButton_Click;
            // 
            // LoadDataForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.FromArgb(24, 24, 24);
            ClientSize = new System.Drawing.Size(819, 687);
            Controls.Add(_doneButton);
            Controls.Add(_cancelButton);
            Controls.Add(_hintPanel);
            Controls.Add(_smpSmzuCard);
            Controls.Add(_megapointCard);
            Controls.Add(_closeButton);
            Controls.Add(_titleLabel);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
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
