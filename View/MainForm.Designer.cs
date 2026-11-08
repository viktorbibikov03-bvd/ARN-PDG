#nullable disable

namespace VrdoPdg.View
{
    partial class MainForm
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
            _toolbarPanel = new System.Windows.Forms.Panel();
            _rightToolbar = new System.Windows.Forms.FlowLayoutPanel();
            _megapointIndicator = new Guna.UI2.WinForms.Guna2Button();
            _smpSmzuIndicator = new Guna.UI2.WinForms.Guna2Button();
            _settingsButton = new Guna.UI2.WinForms.Guna2Button();
            _leftToolbar = new System.Windows.Forms.FlowLayoutPanel();
            _loadButton = new Guna.UI2.WinForms.Guna2Button();
            _saveButton = new Guna.UI2.WinForms.Guna2Button();
            _helpButton = new Guna.UI2.WinForms.Guna2Button();
            _separatorPanel = new System.Windows.Forms.Panel();
            _runButton = new Guna.UI2.WinForms.Guna2Button();
            _previewButton = new Guna.UI2.WinForms.Guna2Button();
            _archiveButton = new Guna.UI2.WinForms.Guna2Button();
            _toolbarLine = new System.Windows.Forms.Panel();
            _progressPanel = new System.Windows.Forms.Panel();
            _progressBar = new Guna.UI2.WinForms.Guna2ProgressBar();
            _progressPercentLabel = new System.Windows.Forms.Label();
            _progressLabel = new System.Windows.Forms.Label();
            _progressIcon = new System.Windows.Forms.PictureBox();
            _contentPanel = new System.Windows.Forms.Panel();
            _protocolPanel = new VrdoPdg.View.Controls.ProtocolPanel();
            _toolbarPanel.SuspendLayout();
            _rightToolbar.SuspendLayout();
            _leftToolbar.SuspendLayout();
            _progressPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_progressIcon).BeginInit();
            _contentPanel.SuspendLayout();
            SuspendLayout();
            //
            // _toolbarPanel
            //
            _toolbarPanel.BackColor = System.Drawing.Color.FromArgb(17, 17, 17);
            _toolbarPanel.Controls.Add(_rightToolbar);
            _toolbarPanel.Controls.Add(_leftToolbar);
            _toolbarPanel.Dock = System.Windows.Forms.DockStyle.Top;
            _toolbarPanel.Location = new System.Drawing.Point(0, 0);
            _toolbarPanel.Name = "_toolbarPanel";
            _toolbarPanel.Padding = new System.Windows.Forms.Padding(16, 12, 16, 12);
            _toolbarPanel.Size = new System.Drawing.Size(1264, 66);
            _toolbarPanel.TabIndex = 0;
            //
            // _rightToolbar
            //
            _rightToolbar.AutoSize = true;
            _rightToolbar.Controls.Add(_megapointIndicator);
            _rightToolbar.Controls.Add(_smpSmzuIndicator);
            _rightToolbar.Controls.Add(_settingsButton);
            _rightToolbar.Dock = System.Windows.Forms.DockStyle.Right;
            _rightToolbar.Location = new System.Drawing.Point(894, 12);
            _rightToolbar.Name = "_rightToolbar";
            _rightToolbar.Size = new System.Drawing.Size(354, 42);
            _rightToolbar.TabIndex = 1;
            _rightToolbar.WrapContents = false;
            //
            // _megapointIndicator
            //
            _megapointIndicator.Margin = new System.Windows.Forms.Padding(0, 6, 8, 0);
            _megapointIndicator.Name = "_megapointIndicator";
            _megapointIndicator.Size = new System.Drawing.Size(100, 30);
            _megapointIndicator.TabIndex = 0;
            _megapointIndicator.TabStop = false;
            _megapointIndicator.Text = "mptsmz";
            //
            // _smpSmzuIndicator
            //
            _smpSmzuIndicator.Margin = new System.Windows.Forms.Padding(0, 6, 16, 0);
            _smpSmzuIndicator.Name = "_smpSmzuIndicator";
            _smpSmzuIndicator.Size = new System.Drawing.Size(76, 30);
            _smpSmzuIndicator.TabIndex = 1;
            _smpSmzuIndicator.TabStop = false;
            _smpSmzuIndicator.Text = "csv";
            //
            // _settingsButton
            //
            _settingsButton.Margin = new System.Windows.Forms.Padding(0);
            _settingsButton.Name = "_settingsButton";
            _settingsButton.Size = new System.Drawing.Size(146, 40);
            _settingsButton.TabIndex = 2;
            _settingsButton.Text = "Настройка";
            _settingsButton.Click += SettingsButton_Click;
            //
            // _leftToolbar
            //
            _leftToolbar.AutoSize = true;
            _leftToolbar.Controls.Add(_loadButton);
            _leftToolbar.Controls.Add(_saveButton);
            _leftToolbar.Controls.Add(_helpButton);
            _leftToolbar.Controls.Add(_separatorPanel);
            _leftToolbar.Controls.Add(_runButton);
            _leftToolbar.Controls.Add(_previewButton);
            _leftToolbar.Controls.Add(_archiveButton);
            _leftToolbar.Dock = System.Windows.Forms.DockStyle.Left;
            _leftToolbar.Location = new System.Drawing.Point(16, 12);
            _leftToolbar.Name = "_leftToolbar";
            _leftToolbar.Size = new System.Drawing.Size(860, 42);
            _leftToolbar.TabIndex = 0;
            _leftToolbar.WrapContents = false;
            //
            // _loadButton
            //
            _loadButton.Margin = new System.Windows.Forms.Padding(0, 0, 4, 0);
            _loadButton.Name = "_loadButton";
            _loadButton.Size = new System.Drawing.Size(140, 40);
            _loadButton.TabIndex = 0;
            _loadButton.Text = "Загрузить";
            _loadButton.Click += LoadButton_Click;
            //
            // _saveButton
            //
            _saveButton.Margin = new System.Windows.Forms.Padding(0, 0, 4, 0);
            _saveButton.Name = "_saveButton";
            _saveButton.Size = new System.Drawing.Size(144, 40);
            _saveButton.TabIndex = 1;
            _saveButton.Text = "Сохранить";
            _saveButton.Click += SaveButton_Click;
            //
            // _helpButton
            //
            _helpButton.Margin = new System.Windows.Forms.Padding(0, 0, 4, 0);
            _helpButton.Name = "_helpButton";
            _helpButton.Size = new System.Drawing.Size(124, 40);
            _helpButton.TabIndex = 2;
            _helpButton.Text = "Справка";
            _helpButton.Click += HelpButton_Click;
            //
            // _separatorPanel
            //
            _separatorPanel.BackColor = System.Drawing.Color.FromArgb(48, 48, 48);
            _separatorPanel.Margin = new System.Windows.Forms.Padding(12, 4, 16, 4);
            _separatorPanel.Name = "_separatorPanel";
            _separatorPanel.Size = new System.Drawing.Size(1, 32);
            _separatorPanel.TabIndex = 3;
            //
            // _runButton
            //
            _runButton.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            _runButton.Name = "_runButton";
            _runButton.Size = new System.Drawing.Size(160, 40);
            _runButton.TabIndex = 4;
            _runButton.Text = "Запустить";
            _runButton.Click += RunButton_Click;
            //
            // _previewButton
            //
            _previewButton.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            _previewButton.Name = "_previewButton";
            _previewButton.Size = new System.Drawing.Size(176, 40);
            _previewButton.TabIndex = 5;
            _previewButton.Text = "Предпросмотр";
            _previewButton.Click += PreviewButton_Click;
            //
            // _archiveButton
            //
            _archiveButton.Margin = new System.Windows.Forms.Padding(0);
            _archiveButton.Name = "_archiveButton";
            _archiveButton.Size = new System.Drawing.Size(116, 40);
            _archiveButton.TabIndex = 6;
            _archiveButton.Text = "Архив";
            _archiveButton.Click += ArchiveButton_Click;
            //
            // _toolbarLine
            //
            _toolbarLine.BackColor = System.Drawing.Color.FromArgb(48, 48, 48);
            _toolbarLine.Dock = System.Windows.Forms.DockStyle.Top;
            _toolbarLine.Location = new System.Drawing.Point(0, 66);
            _toolbarLine.Name = "_toolbarLine";
            _toolbarLine.Size = new System.Drawing.Size(1264, 1);
            _toolbarLine.TabIndex = 1;
            //
            // _progressPanel
            //
            _progressPanel.BackColor = System.Drawing.Color.FromArgb(24, 24, 24);
            _progressPanel.Controls.Add(_progressBar);
            _progressPanel.Controls.Add(_progressPercentLabel);
            _progressPanel.Controls.Add(_progressLabel);
            _progressPanel.Controls.Add(_progressIcon);
            _progressPanel.Dock = System.Windows.Forms.DockStyle.Top;
            _progressPanel.Location = new System.Drawing.Point(0, 67);
            _progressPanel.Name = "_progressPanel";
            _progressPanel.Padding = new System.Windows.Forms.Padding(20, 12, 20, 12);
            _progressPanel.Size = new System.Drawing.Size(1264, 46);
            _progressPanel.TabIndex = 2;
            _progressPanel.Visible = false;
            //
            // _progressBar
            //
            _progressBar.BorderRadius = 4;
            _progressBar.Dock = System.Windows.Forms.DockStyle.Fill;
            _progressBar.FillColor = System.Drawing.Color.FromArgb(40, 40, 40);
            _progressBar.Name = "_progressBar";
            _progressBar.ProgressColor = System.Drawing.Color.FromArgb(176, 150, 255);
            _progressBar.ProgressColor2 = System.Drawing.Color.FromArgb(176, 150, 255);
            _progressBar.TabIndex = 0;
            //
            // _progressPercentLabel
            //
            _progressPercentLabel.Dock = System.Windows.Forms.DockStyle.Right;
            _progressPercentLabel.Font = new System.Drawing.Font("Consolas", 10F);
            _progressPercentLabel.ForeColor = System.Drawing.Color.FromArgb(176, 150, 255);
            _progressPercentLabel.Name = "_progressPercentLabel";
            _progressPercentLabel.Size = new System.Drawing.Size(60, 22);
            _progressPercentLabel.TabIndex = 1;
            _progressPercentLabel.Text = "0%";
            _progressPercentLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // _progressLabel
            //
            _progressLabel.Dock = System.Windows.Forms.DockStyle.Left;
            _progressLabel.Font = new System.Drawing.Font("Segoe UI Semibold", 10.5F);
            _progressLabel.ForeColor = System.Drawing.Color.FromArgb(236, 236, 236);
            _progressLabel.Name = "_progressLabel";
            _progressLabel.Size = new System.Drawing.Size(150, 22);
            _progressLabel.TabIndex = 2;
            _progressLabel.Text = "Идёт расчёт…";
            _progressLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // _progressIcon
            //
            _progressIcon.Dock = System.Windows.Forms.DockStyle.Left;
            _progressIcon.Name = "_progressIcon";
            _progressIcon.Size = new System.Drawing.Size(30, 22);
            _progressIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            _progressIcon.TabIndex = 3;
            _progressIcon.TabStop = false;
            //
            // _contentPanel
            //
            _contentPanel.BackColor = System.Drawing.Color.FromArgb(17, 17, 17);
            _contentPanel.Controls.Add(_protocolPanel);
            _contentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            _contentPanel.Location = new System.Drawing.Point(0, 113);
            _contentPanel.Name = "_contentPanel";
            _contentPanel.Padding = new System.Windows.Forms.Padding(20);
            _contentPanel.Size = new System.Drawing.Size(1264, 568);
            _contentPanel.TabIndex = 3;
            //
            // _protocolPanel
            //
            _protocolPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            _protocolPanel.Name = "_protocolPanel";
            _protocolPanel.TabIndex = 0;
            //
            // MainForm
            //
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.FromArgb(17, 17, 17);
            ClientSize = new System.Drawing.Size(1264, 681);
            Controls.Add(_contentPanel);
            Controls.Add(_progressPanel);
            Controls.Add(_toolbarLine);
            Controls.Add(_toolbarPanel);
            ForeColor = System.Drawing.Color.FromArgb(236, 236, 236);
            MinimumSize = new System.Drawing.Size(1120, 560);
            Name = "MainForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "ВРДО ПДГ — Ввод режима по напряжению в допустимую область";
            FormClosing += MainForm_FormClosing;
            _toolbarPanel.ResumeLayout(false);
            _toolbarPanel.PerformLayout();
            _rightToolbar.ResumeLayout(false);
            _leftToolbar.ResumeLayout(false);
            _progressPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)_progressIcon).EndInit();
            _contentPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        /// <summary>
        /// Панель инструментов.
        /// </summary>
        private System.Windows.Forms.Panel _toolbarPanel;

        /// <summary>
        /// Левая группа панели инструментов: файловые операции, справка, расчет и просмотр.
        /// </summary>
        private System.Windows.Forms.FlowLayoutPanel _leftToolbar;

        /// <summary>
        /// Правая группа панели инструментов: индикаторы файлов и настройка.
        /// </summary>
        private System.Windows.Forms.FlowLayoutPanel _rightToolbar;

        /// <summary>
        /// Кнопка «Загрузить».
        /// </summary>
        private Guna.UI2.WinForms.Guna2Button _loadButton;

        /// <summary>
        /// Кнопка «Сохранить».
        /// </summary>
        private Guna.UI2.WinForms.Guna2Button _saveButton;

        /// <summary>
        /// Кнопка «Справка».
        /// </summary>
        private Guna.UI2.WinForms.Guna2Button _helpButton;

        /// <summary>
        /// Вертикальный разделитель панели инструментов.
        /// </summary>
        private System.Windows.Forms.Panel _separatorPanel;

        /// <summary>
        /// Кнопка «Запустить» / «Остановить».
        /// </summary>
        private Guna.UI2.WinForms.Guna2Button _runButton;

        /// <summary>
        /// Кнопка «Предпросмотр».
        /// </summary>
        private Guna.UI2.WinForms.Guna2Button _previewButton;

        /// <summary>
        /// Кнопка «Архив».
        /// </summary>
        private Guna.UI2.WinForms.Guna2Button _archiveButton;

        /// <summary>
        /// Индикатор загрузки файла МТ.
        /// </summary>
        private Guna.UI2.WinForms.Guna2Button _megapointIndicator;

        /// <summary>
        /// Индикатор загрузки файла smp_smzu.csv.
        /// </summary>
        private Guna.UI2.WinForms.Guna2Button _smpSmzuIndicator;

        /// <summary>
        /// Кнопка «Настройка».
        /// </summary>
        private Guna.UI2.WinForms.Guna2Button _settingsButton;

        /// <summary>
        /// Линия под панелью инструментов.
        /// </summary>
        private System.Windows.Forms.Panel _toolbarLine;

        /// <summary>
        /// Строка хода расчета.
        /// </summary>
        private System.Windows.Forms.Panel _progressPanel;

        /// <summary>
        /// Значок строки хода расчета.
        /// </summary>
        private System.Windows.Forms.PictureBox _progressIcon;

        /// <summary>
        /// Надпись «Идёт расчёт…».
        /// </summary>
        private System.Windows.Forms.Label _progressLabel;

        /// <summary>
        /// Процент выполнения расчета.
        /// </summary>
        private System.Windows.Forms.Label _progressPercentLabel;

        /// <summary>
        /// Полоса хода расчета.
        /// </summary>
        private Guna.UI2.WinForms.Guna2ProgressBar _progressBar;

        /// <summary>
        /// Область панели протокола.
        /// </summary>
        private System.Windows.Forms.Panel _contentPanel;

        /// <summary>
        /// Панель «Протокол расчета».
        /// </summary>
        private VrdoPdg.View.Controls.ProtocolPanel _protocolPanel;
    }
}
