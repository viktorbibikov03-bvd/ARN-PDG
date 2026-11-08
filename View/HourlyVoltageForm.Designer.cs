#nullable disable

namespace VrdoPdg.View
{
    partial class HourlyVoltageForm
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
            _headerPanel = new System.Windows.Forms.Panel();
            _titleLabel = new System.Windows.Forms.Label();
            _backButton = new Guna.UI2.WinForms.Guna2Button();
            _modePanel = new Guna.UI2.WinForms.Guna2Panel();
            _afterButton = new Guna.UI2.WinForms.Guna2Button();
            _beforeButton = new Guna.UI2.WinForms.Guna2Button();
            _headerLine = new System.Windows.Forms.Panel();
            _bodyPanel = new System.Windows.Forms.Panel();
            _tableView = new VrdoPdg.View.Controls.HourlyTableView();
            _headerPanel.SuspendLayout();
            _modePanel.SuspendLayout();
            _bodyPanel.SuspendLayout();
            SuspendLayout();
            //
            // _headerPanel
            //
            _headerPanel.Controls.Add(_titleLabel);
            _headerPanel.Controls.Add(_backButton);
            _headerPanel.Controls.Add(_modePanel);
            _headerPanel.Dock = System.Windows.Forms.DockStyle.Top;
            _headerPanel.Name = "_headerPanel";
            _headerPanel.Padding = new System.Windows.Forms.Padding(24, 16, 24, 16);
            _headerPanel.Size = new System.Drawing.Size(1264, 76);
            _headerPanel.TabIndex = 0;
            //
            // _titleLabel
            //
            _titleLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            _titleLabel.Font = new System.Drawing.Font("Segoe UI Semibold", 18F);
            _titleLabel.ForeColor = System.Drawing.Color.FromArgb(236, 236, 236);
            _titleLabel.Name = "_titleLabel";
            _titleLabel.Padding = new System.Windows.Forms.Padding(16, 0, 0, 0);
            _titleLabel.TabIndex = 1;
            _titleLabel.Text = "Напряжение по часам";
            _titleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // _backButton
            //
            _backButton.Dock = System.Windows.Forms.DockStyle.Left;
            _backButton.Name = "_backButton";
            _backButton.Size = new System.Drawing.Size(150, 44);
            _backButton.TabIndex = 0;
            _backButton.Text = "К расчёту";
            _backButton.Click += BackButton_Click;
            //
            // _modePanel
            //
            _modePanel.BorderRadius = 12;
            _modePanel.Controls.Add(_afterButton);
            _modePanel.Controls.Add(_beforeButton);
            _modePanel.Dock = System.Windows.Forms.DockStyle.Right;
            _modePanel.FillColor = System.Drawing.Color.FromArgb(24, 24, 24);
            _modePanel.Name = "_modePanel";
            _modePanel.Padding = new System.Windows.Forms.Padding(4);
            _modePanel.Size = new System.Drawing.Size(300, 44);
            _modePanel.TabIndex = 2;
            //
            // _afterButton
            //
            _afterButton.Dock = System.Windows.Forms.DockStyle.Fill;
            _afterButton.Name = "_afterButton";
            _afterButton.TabIndex = 1;
            _afterButton.Text = "После расчёта";
            _afterButton.Click += AfterButton_Click;
            //
            // _beforeButton
            //
            _beforeButton.Dock = System.Windows.Forms.DockStyle.Left;
            _beforeButton.Name = "_beforeButton";
            _beforeButton.Size = new System.Drawing.Size(140, 36);
            _beforeButton.TabIndex = 0;
            _beforeButton.Text = "До расчёта";
            _beforeButton.Click += BeforeButton_Click;
            //
            // _headerLine
            //
            _headerLine.BackColor = System.Drawing.Color.FromArgb(48, 48, 48);
            _headerLine.Dock = System.Windows.Forms.DockStyle.Top;
            _headerLine.Name = "_headerLine";
            _headerLine.Size = new System.Drawing.Size(1264, 1);
            _headerLine.TabIndex = 1;
            //
            // _bodyPanel
            //
            _bodyPanel.Controls.Add(_tableView);
            _bodyPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            _bodyPanel.Name = "_bodyPanel";
            _bodyPanel.Padding = new System.Windows.Forms.Padding(24, 12, 24, 24);
            _bodyPanel.TabIndex = 2;
            //
            // _tableView
            //
            _tableView.Dock = System.Windows.Forms.DockStyle.Fill;
            _tableView.Name = "_tableView";
            _tableView.TabIndex = 0;
            //
            // HourlyVoltageForm
            //
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.FromArgb(17, 17, 17);
            ClientSize = new System.Drawing.Size(1264, 760);
            Controls.Add(_bodyPanel);
            Controls.Add(_headerLine);
            Controls.Add(_headerPanel);
            ForeColor = System.Drawing.Color.FromArgb(236, 236, 236);
            KeyPreview = true;
            MinimumSize = new System.Drawing.Size(900, 500);
            Name = "HourlyVoltageForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "ВРДО ПДГ — Напряжение по часам";
            KeyDown += HourlyVoltageForm_KeyDown;
            _headerPanel.ResumeLayout(false);
            _modePanel.ResumeLayout(false);
            _bodyPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        /// <summary>
        /// Заголовок формы.
        /// </summary>
        private System.Windows.Forms.Panel _headerPanel;

        /// <summary>
        /// Название формы.
        /// </summary>
        private System.Windows.Forms.Label _titleLabel;

        /// <summary>
        /// Кнопка «К расчёту».
        /// </summary>
        private Guna.UI2.WinForms.Guna2Button _backButton;

        /// <summary>
        /// Переключатель «До расчёта» / «После расчёта».
        /// </summary>
        private Guna.UI2.WinForms.Guna2Panel _modePanel;

        /// <summary>
        /// Кнопка «До расчёта».
        /// </summary>
        private Guna.UI2.WinForms.Guna2Button _beforeButton;

        /// <summary>
        /// Кнопка «После расчёта».
        /// </summary>
        private Guna.UI2.WinForms.Guna2Button _afterButton;

        /// <summary>
        /// Линия под заголовком.
        /// </summary>
        private System.Windows.Forms.Panel _headerLine;

        /// <summary>
        /// Область таблицы.
        /// </summary>
        private System.Windows.Forms.Panel _bodyPanel;

        /// <summary>
        /// Таблица по часам.
        /// </summary>
        private VrdoPdg.View.Controls.HourlyTableView _tableView;
    }
}
