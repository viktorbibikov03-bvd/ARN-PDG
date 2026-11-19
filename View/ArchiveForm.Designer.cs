#nullable disable

namespace VrdoPdg.View
{
    partial class ArchiveForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ArchiveForm));
            _headerPanel = new System.Windows.Forms.Panel();
            _titleLabel = new System.Windows.Forms.Label();
            _backButton = new VrdoPdg.View.Controls.RoundedButton();
            _headerLine = new System.Windows.Forms.Panel();
            _bodyPanel = new System.Windows.Forms.Panel();
            _frame = new VrdoPdg.View.Controls.RoundedPanel();
            _grid = new System.Windows.Forms.DataGridView();
            _selectColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            _numberColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            _startColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            _emptyLabel = new System.Windows.Forms.Label();
            _footerPanel = new System.Windows.Forms.Panel();
            _resetButton = new VrdoPdg.View.Controls.RoundedButton();
            _compareButton = new VrdoPdg.View.Controls.RoundedButton();
            _headerPanel.SuspendLayout();
            _bodyPanel.SuspendLayout();
            _frame.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_grid).BeginInit();
            _footerPanel.SuspendLayout();
            SuspendLayout();
            // 
            // _headerPanel
            // 
            _headerPanel.Controls.Add(_titleLabel);
            _headerPanel.Controls.Add(_backButton);
            _headerPanel.Dock = System.Windows.Forms.DockStyle.Top;
            _headerPanel.Location = new System.Drawing.Point(0, 0);
            _headerPanel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            _headerPanel.Name = "_headerPanel";
            _headerPanel.Padding = new System.Windows.Forms.Padding(36, 24, 36, 24);
            _headerPanel.Size = new System.Drawing.Size(1500, 114);
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
            _titleLabel.Size = new System.Drawing.Size(1203, 66);
            _titleLabel.TabIndex = 1;
            _titleLabel.Text = "Архив расчётов";
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
            // _headerLine
            // 
            _headerLine.BackColor = System.Drawing.Color.FromArgb(48, 48, 48);
            _headerLine.Dock = System.Windows.Forms.DockStyle.Top;
            _headerLine.Location = new System.Drawing.Point(0, 114);
            _headerLine.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            _headerLine.Name = "_headerLine";
            _headerLine.Size = new System.Drawing.Size(1500, 2);
            _headerLine.TabIndex = 1;
            // 
            // _bodyPanel
            // 
            _bodyPanel.Controls.Add(_frame);
            _bodyPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            _bodyPanel.Location = new System.Drawing.Point(0, 116);
            _bodyPanel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            _bodyPanel.Name = "_bodyPanel";
            _bodyPanel.Padding = new System.Windows.Forms.Padding(36, 24, 36, 24);
            _bodyPanel.Size = new System.Drawing.Size(1500, 400);
            _bodyPanel.TabIndex = 2;
            // 
            // _frame
            // 
            _frame.BorderColor = System.Drawing.Color.FromArgb(48, 48, 48);
            _frame.BorderRadius = 14;
            _frame.BorderThickness = 1;
            _frame.Controls.Add(_grid);
            _frame.Controls.Add(_emptyLabel);
            _frame.Dock = System.Windows.Forms.DockStyle.Fill;
            _frame.FillColor = System.Drawing.Color.FromArgb(17, 17, 17);
            _frame.Location = new System.Drawing.Point(36, 24);
            _frame.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            _frame.Name = "_frame";
            _frame.Padding = new System.Windows.Forms.Padding(18, 18, 18, 18);
            _frame.Size = new System.Drawing.Size(1428, 352);
            _frame.TabIndex = 0;
            // 
            // _grid
            // 
            _grid.ColumnHeadersHeight = 34;
            _grid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { _selectColumn, _numberColumn, _startColumn });
            _grid.Dock = System.Windows.Forms.DockStyle.Fill;
            _grid.Location = new System.Drawing.Point(18, 102);
            _grid.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            _grid.MultiSelect = false;
            _grid.Name = "_grid";
            _grid.ReadOnly = true;
            _grid.RowHeadersWidth = 62;
            _grid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            _grid.Size = new System.Drawing.Size(1392, 232);
            _grid.TabIndex = 0;
            _grid.CellClick += Grid_CellClick;
            _grid.CellPainting += Grid_CellPainting;
            // 
            // _selectColumn
            // 
            _selectColumn.HeaderText = "";
            _selectColumn.MinimumWidth = 8;
            _selectColumn.Name = "_selectColumn";
            _selectColumn.ReadOnly = true;
            _selectColumn.Width = 64;
            // 
            // _numberColumn
            // 
            _numberColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            _numberColumn.HeaderText = "№ РАСЧЁТА";
            _numberColumn.MinimumWidth = 8;
            _numberColumn.Name = "_numberColumn";
            _numberColumn.ReadOnly = true;
            // 
            // _startColumn
            // 
            _startColumn.HeaderText = "ЗАПУСК";
            _startColumn.MinimumWidth = 8;
            _startColumn.Name = "_startColumn";
            _startColumn.ReadOnly = true;
            _startColumn.Width = 200;
            // 
            // _emptyLabel
            // 
            _emptyLabel.Dock = System.Windows.Forms.DockStyle.Top;
            _emptyLabel.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            _emptyLabel.ForeColor = System.Drawing.Color.FromArgb(140, 140, 140);
            _emptyLabel.Location = new System.Drawing.Point(18, 18);
            _emptyLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            _emptyLabel.Name = "_emptyLabel";
            _emptyLabel.Size = new System.Drawing.Size(1392, 84);
            _emptyLabel.TabIndex = 1;
            _emptyLabel.Text = "Расчёты в текущей сессии не выполнялись";
            _emptyLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            _emptyLabel.Visible = false;
            // 
            // _footerPanel
            // 
            _footerPanel.BackColor = System.Drawing.Color.FromArgb(24, 24, 24);
            _footerPanel.Controls.Add(_resetButton);
            _footerPanel.Controls.Add(_compareButton);
            _footerPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            _footerPanel.Location = new System.Drawing.Point(0, 516);
            _footerPanel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            _footerPanel.Name = "_footerPanel";
            _footerPanel.Padding = new System.Windows.Forms.Padding(36, 24, 36, 24);
            _footerPanel.Size = new System.Drawing.Size(1500, 114);
            _footerPanel.TabIndex = 3;
            // 
            // _resetButton
            // 
            _resetButton.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            _resetButton.Dock = System.Windows.Forms.DockStyle.Right;
            _resetButton.Location = new System.Drawing.Point(969, 24);
            _resetButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            _resetButton.Name = "_resetButton";
            _resetButton.Padding = new System.Windows.Forms.Padding(21, 0, 21, 0);
            _resetButton.Size = new System.Drawing.Size(270, 66);
            _resetButton.TabIndex = 0;
            _resetButton.Text = "Сбросить выбор";
            _resetButton.Click += ResetButton_Click;
            // 
            // _compareButton
            // 
            _compareButton.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            _compareButton.Dock = System.Windows.Forms.DockStyle.Right;
            _compareButton.Location = new System.Drawing.Point(1239, 24);
            _compareButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            _compareButton.Name = "_compareButton";
            _compareButton.Padding = new System.Windows.Forms.Padding(21, 0, 21, 0);
            _compareButton.Size = new System.Drawing.Size(225, 66);
            _compareButton.TabIndex = 1;
            _compareButton.Text = "Сравнить";
            _compareButton.Click += CompareButton_Click;
            // 
            // ArchiveForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.FromArgb(17, 17, 17);
            ClientSize = new System.Drawing.Size(1500, 630);
            Controls.Add(_bodyPanel);
            Controls.Add(_footerPanel);
            Controls.Add(_headerLine);
            Controls.Add(_headerPanel);
            ForeColor = System.Drawing.Color.FromArgb(236, 236, 236);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            MinimumSize = new System.Drawing.Size(949, 512);
            Name = "ArchiveForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "ВРДО ПДГ — Архив расчётов";
            KeyDown += ArchiveForm_KeyDown;
            _headerPanel.ResumeLayout(false);
            _bodyPanel.ResumeLayout(false);
            _frame.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)_grid).EndInit();
            _footerPanel.ResumeLayout(false);
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
        /// Линия под заголовком.
        /// </summary>
        private System.Windows.Forms.Panel _headerLine;

        /// <summary>
        /// Область списка расчетов.
        /// </summary>
        private System.Windows.Forms.Panel _bodyPanel;

        /// <summary>
        /// Рамка списка расчетов.
        /// </summary>
        private VrdoPdg.View.Controls.RoundedPanel _frame;

        /// <summary>
        /// Список расчетов.
        /// </summary>
        private System.Windows.Forms.DataGridView _grid;

        /// <summary>
        /// Столбец отметки расчета.
        /// </summary>
        private System.Windows.Forms.DataGridViewTextBoxColumn _selectColumn;

        /// <summary>
        /// Столбец номера расчета.
        /// </summary>
        private System.Windows.Forms.DataGridViewTextBoxColumn _numberColumn;

        /// <summary>
        /// Столбец даты и времени запуска.
        /// </summary>
        private System.Windows.Forms.DataGridViewTextBoxColumn _startColumn;

        /// <summary>
        /// Сообщение о пустом архиве.
        /// </summary>
        private System.Windows.Forms.Label _emptyLabel;

        /// <summary>
        /// Нижняя панель с кнопками.
        /// </summary>
        private System.Windows.Forms.Panel _footerPanel;

        /// <summary>
        /// Кнопка «Сбросить выбор».
        /// </summary>
        private VrdoPdg.View.Controls.RoundedButton _resetButton;

        /// <summary>
        /// Кнопка «Сравнить».
        /// </summary>
        private VrdoPdg.View.Controls.RoundedButton _compareButton;
    }
}
