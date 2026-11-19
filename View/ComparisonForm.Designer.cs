#nullable disable

namespace VrdoPdg.View
{
    partial class ComparisonForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ComparisonForm));
            _headerPanel = new System.Windows.Forms.Panel();
            _titlePanel = new System.Windows.Forms.Panel();
            _subtitleLabel = new System.Windows.Forms.Label();
            _titleLabel = new System.Windows.Forms.Label();
            _backButton = new VrdoPdg.View.Controls.RoundedButton();
            _headerLine = new System.Windows.Forms.Panel();
            _bodyPanel = new System.Windows.Forms.Panel();
            _tableView = new VrdoPdg.View.Controls.HourlyTableView();
            _headerPanel.SuspendLayout();
            _titlePanel.SuspendLayout();
            _bodyPanel.SuspendLayout();
            SuspendLayout();
            // 
            // _headerPanel
            // 
            _headerPanel.Controls.Add(_titlePanel);
            _headerPanel.Controls.Add(_backButton);
            _headerPanel.Dock = System.Windows.Forms.DockStyle.Top;
            _headerPanel.Location = new System.Drawing.Point(0, 0);
            _headerPanel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            _headerPanel.Name = "_headerPanel";
            _headerPanel.Padding = new System.Windows.Forms.Padding(36, 21, 36, 21);
            _headerPanel.Size = new System.Drawing.Size(1896, 120);
            _headerPanel.TabIndex = 0;
            // 
            // _titlePanel
            // 
            _titlePanel.Controls.Add(_subtitleLabel);
            _titlePanel.Controls.Add(_titleLabel);
            _titlePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            _titlePanel.Location = new System.Drawing.Point(261, 21);
            _titlePanel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            _titlePanel.Name = "_titlePanel";
            _titlePanel.Padding = new System.Windows.Forms.Padding(24, 0, 0, 0);
            _titlePanel.Size = new System.Drawing.Size(1599, 78);
            _titlePanel.TabIndex = 1;
            // 
            // _subtitleLabel
            // 
            _subtitleLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            _subtitleLabel.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            _subtitleLabel.ForeColor = System.Drawing.Color.FromArgb(140, 140, 140);
            _subtitleLabel.Location = new System.Drawing.Point(24, 51);
            _subtitleLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            _subtitleLabel.Name = "_subtitleLabel";
            _subtitleLabel.Size = new System.Drawing.Size(1575, 27);
            _subtitleLabel.TabIndex = 1;
            // 
            // _titleLabel
            // 
            _titleLabel.Dock = System.Windows.Forms.DockStyle.Top;
            _titleLabel.Font = new System.Drawing.Font("Segoe UI Semibold", 18F);
            _titleLabel.ForeColor = System.Drawing.Color.FromArgb(236, 236, 236);
            _titleLabel.Location = new System.Drawing.Point(24, 0);
            _titleLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            _titleLabel.Name = "_titleLabel";
            _titleLabel.Size = new System.Drawing.Size(1575, 51);
            _titleLabel.TabIndex = 0;
            _titleLabel.Text = "Сравнение расчётов";
            // 
            // _backButton
            // 
            _backButton.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            _backButton.Dock = System.Windows.Forms.DockStyle.Left;
            _backButton.Location = new System.Drawing.Point(36, 21);
            _backButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            _backButton.Name = "_backButton";
            _backButton.Padding = new System.Windows.Forms.Padding(21, 0, 21, 0);
            _backButton.Size = new System.Drawing.Size(225, 78);
            _backButton.TabIndex = 0;
            _backButton.Text = "К архиву";
            _backButton.Click += BackButton_Click;
            // 
            // _headerLine
            // 
            _headerLine.BackColor = System.Drawing.Color.FromArgb(48, 48, 48);
            _headerLine.Dock = System.Windows.Forms.DockStyle.Top;
            _headerLine.Location = new System.Drawing.Point(0, 120);
            _headerLine.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            _headerLine.Name = "_headerLine";
            _headerLine.Size = new System.Drawing.Size(1896, 2);
            _headerLine.TabIndex = 1;
            // 
            // _bodyPanel
            // 
            _bodyPanel.Controls.Add(_tableView);
            _bodyPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            _bodyPanel.Location = new System.Drawing.Point(0, 122);
            _bodyPanel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            _bodyPanel.Name = "_bodyPanel";
            _bodyPanel.Padding = new System.Windows.Forms.Padding(36, 18, 36, 36);
            _bodyPanel.Size = new System.Drawing.Size(1896, 1078);
            _bodyPanel.TabIndex = 2;
            // 
            // _tableView
            // 
            _tableView.BackColor = System.Drawing.Color.FromArgb(17, 17, 17);
            _tableView.Dock = System.Windows.Forms.DockStyle.Fill;
            _tableView.Location = new System.Drawing.Point(36, 18);
            _tableView.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            _tableView.Name = "_tableView";
            _tableView.Size = new System.Drawing.Size(1824, 1024);
            _tableView.TabIndex = 0;
            // 
            // ComparisonForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.FromArgb(17, 17, 17);
            ClientSize = new System.Drawing.Size(1896, 1200);
            Controls.Add(_bodyPanel);
            Controls.Add(_headerLine);
            Controls.Add(_headerPanel);
            ForeColor = System.Drawing.Color.FromArgb(236, 236, 236);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            MinimumSize = new System.Drawing.Size(1339, 722);
            Name = "ComparisonForm";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "ВРДО ПДГ — Сравнение расчётов";
            KeyDown += ComparisonForm_KeyDown;
            _headerPanel.ResumeLayout(false);
            _titlePanel.ResumeLayout(false);
            _bodyPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        /// <summary>
        /// Заголовок формы.
        /// </summary>
        private System.Windows.Forms.Panel _headerPanel;

        /// <summary>
        /// Область названия формы.
        /// </summary>
        private System.Windows.Forms.Panel _titlePanel;

        /// <summary>
        /// Название формы.
        /// </summary>
        private System.Windows.Forms.Label _titleLabel;

        /// <summary>
        /// Сутки и сравниваемые расчеты.
        /// </summary>
        private System.Windows.Forms.Label _subtitleLabel;

        /// <summary>
        /// Кнопка «К архиву».
        /// </summary>
        private VrdoPdg.View.Controls.RoundedButton _backButton;

        /// <summary>
        /// Линия под заголовком.
        /// </summary>
        private System.Windows.Forms.Panel _headerLine;

        /// <summary>
        /// Область таблицы.
        /// </summary>
        private System.Windows.Forms.Panel _bodyPanel;

        /// <summary>
        /// Таблица сравнения по часам.
        /// </summary>
        private VrdoPdg.View.Controls.HourlyTableView _tableView;
    }
}
