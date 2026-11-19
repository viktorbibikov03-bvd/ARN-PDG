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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HourlyVoltageForm));
            _headerPanel = new System.Windows.Forms.Panel();
            _titleLabel = new System.Windows.Forms.Label();
            _backButton = new VrdoPdg.View.Controls.RoundedButton();
            _modePanel = new VrdoPdg.View.Controls.RoundedPanel();
            _afterButton = new VrdoPdg.View.Controls.RoundedButton();
            _beforeButton = new VrdoPdg.View.Controls.RoundedButton();
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
            _headerPanel.Location = new System.Drawing.Point(0, 0);
            _headerPanel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            _headerPanel.Name = "_headerPanel";
            _headerPanel.Padding = new System.Windows.Forms.Padding(36, 24, 36, 24);
            _headerPanel.Size = new System.Drawing.Size(1896, 114);
            _headerPanel.TabIndex = 0;
            // 
            // _titleLabel
            // 
            _titleLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            _titleLabel.Font = new System.Drawing.Font("Segoe UI Semibold", 18F);
            _titleLabel.ForeColor = System.Drawing.Color.FromArgb(236, 236, 236);
            _titleLabel.Location = new System.Drawing.Point(261, 24);
            _titleLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            _titleLabel.Name = "_titleLabel";
            _titleLabel.Padding = new System.Windows.Forms.Padding(24, 0, 0, 0);
            _titleLabel.Size = new System.Drawing.Size(1149, 66);
            _titleLabel.TabIndex = 1;
            _titleLabel.Text = "Напряжение по часам";
            _titleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _backButton
            // 
            _backButton.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            _backButton.Dock = System.Windows.Forms.DockStyle.Left;
            _backButton.Location = new System.Drawing.Point(36, 24);
            _backButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            _backButton.Name = "_backButton";
            _backButton.Padding = new System.Windows.Forms.Padding(21, 0, 21, 0);
            _backButton.Size = new System.Drawing.Size(225, 66);
            _backButton.TabIndex = 0;
            _backButton.Text = "К расчёту";
            _backButton.Click += BackButton_Click;
            // 
            // _modePanel
            // 
            _modePanel.Controls.Add(_afterButton);
            _modePanel.Controls.Add(_beforeButton);
            _modePanel.Dock = System.Windows.Forms.DockStyle.Right;
            _modePanel.FillColor = System.Drawing.Color.FromArgb(24, 24, 24);
            _modePanel.Location = new System.Drawing.Point(1410, 24);
            _modePanel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            _modePanel.Name = "_modePanel";
            _modePanel.Padding = new System.Windows.Forms.Padding(6, 6, 6, 6);
            _modePanel.Size = new System.Drawing.Size(450, 66);
            _modePanel.TabIndex = 2;
            // 
            // _afterButton
            // 
            _afterButton.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            _afterButton.Dock = System.Windows.Forms.DockStyle.Fill;
            _afterButton.Location = new System.Drawing.Point(216, 6);
            _afterButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            _afterButton.Name = "_afterButton";
            _afterButton.Padding = new System.Windows.Forms.Padding(21, 0, 21, 0);
            _afterButton.Size = new System.Drawing.Size(228, 54);
            _afterButton.TabIndex = 1;
            _afterButton.Text = "После расчёта";
            _afterButton.Click += AfterButton_Click;
            // 
            // _beforeButton
            // 
            _beforeButton.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            _beforeButton.Dock = System.Windows.Forms.DockStyle.Left;
            _beforeButton.Location = new System.Drawing.Point(6, 6);
            _beforeButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            _beforeButton.Name = "_beforeButton";
            _beforeButton.Padding = new System.Windows.Forms.Padding(21, 0, 21, 0);
            _beforeButton.Size = new System.Drawing.Size(210, 54);
            _beforeButton.TabIndex = 0;
            _beforeButton.Text = "До расчёта";
            _beforeButton.Click += BeforeButton_Click;
            // 
            // _headerLine
            // 
            _headerLine.BackColor = System.Drawing.Color.FromArgb(48, 48, 48);
            _headerLine.Dock = System.Windows.Forms.DockStyle.Top;
            _headerLine.Location = new System.Drawing.Point(0, 114);
            _headerLine.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            _headerLine.Name = "_headerLine";
            _headerLine.Size = new System.Drawing.Size(1896, 2);
            _headerLine.TabIndex = 1;
            // 
            // _bodyPanel
            // 
            _bodyPanel.Controls.Add(_tableView);
            _bodyPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            _bodyPanel.Location = new System.Drawing.Point(0, 116);
            _bodyPanel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            _bodyPanel.Name = "_bodyPanel";
            _bodyPanel.Padding = new System.Windows.Forms.Padding(36, 18, 36, 36);
            _bodyPanel.Size = new System.Drawing.Size(1896, 1024);
            _bodyPanel.TabIndex = 2;
            // 
            // _tableView
            // 
            _tableView.BackColor = System.Drawing.Color.FromArgb(17, 17, 17);
            _tableView.Dock = System.Windows.Forms.DockStyle.Fill;
            _tableView.Location = new System.Drawing.Point(36, 18);
            _tableView.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            _tableView.Name = "_tableView";
            _tableView.Size = new System.Drawing.Size(1824, 970);
            _tableView.TabIndex = 0;
            // 
            // HourlyVoltageForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.FromArgb(17, 17, 17);
            ClientSize = new System.Drawing.Size(1896, 1140);
            Controls.Add(_bodyPanel);
            Controls.Add(_headerLine);
            Controls.Add(_headerPanel);
            ForeColor = System.Drawing.Color.FromArgb(236, 236, 236);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            MinimumSize = new System.Drawing.Size(1339, 722);
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
        private VrdoPdg.View.Controls.RoundedButton _backButton;

        /// <summary>
        /// Переключатель «До расчёта» / «После расчёта».
        /// </summary>
        private VrdoPdg.View.Controls.RoundedPanel _modePanel;

        /// <summary>
        /// Кнопка «До расчёта».
        /// </summary>
        private VrdoPdg.View.Controls.RoundedButton _beforeButton;

        /// <summary>
        /// Кнопка «После расчёта».
        /// </summary>
        private VrdoPdg.View.Controls.RoundedButton _afterButton;

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
