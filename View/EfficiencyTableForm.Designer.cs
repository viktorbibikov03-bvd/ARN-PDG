#nullable disable

namespace VrdoPdg.View
{
    partial class EfficiencyTableForm
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
            _titlePanel = new System.Windows.Forms.Panel();
            _subtitleLabel = new System.Windows.Forms.Label();
            _titleLabel = new System.Windows.Forms.Label();
            _statusPill = new VrdoPdg.View.Controls.RoundedButton();
            _closeButton = new VrdoPdg.View.Controls.RoundedButton();
            _iconBox = new VrdoPdg.View.Controls.RoundedPanel();
            _icon = new System.Windows.Forms.PictureBox();
            _headerLine = new System.Windows.Forms.Panel();
            _footerPanel = new System.Windows.Forms.Panel();
            _defaultButton = new VrdoPdg.View.Controls.RoundedButton();
            _cancelButton = new VrdoPdg.View.Controls.RoundedButton();
            _saveButton = new VrdoPdg.View.Controls.RoundedButton();
            _footerLine = new System.Windows.Forms.Panel();
            _leftPanel = new System.Windows.Forms.Panel();
            _controlPointList = new System.Windows.Forms.ListBox();
            _searchSpacer = new System.Windows.Forms.Panel();
            _searchBox = new VrdoPdg.View.Controls.SearchBox();
            _leftLine = new System.Windows.Forms.Panel();
            _rightPanel = new System.Windows.Forms.Panel();
            _grid = new System.Windows.Forms.DataGridView();
            _handleColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            _priorityColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            _typeColumn = new System.Windows.Forms.DataGridViewComboBoxColumn();
            _numberColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            _nameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            _upColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            _downColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            _deleteColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            _errorBanner = new VrdoPdg.View.Controls.RoundedPanel();
            _errorLabel = new System.Windows.Forms.Label();
            _errorIcon = new System.Windows.Forms.PictureBox();
            _tableHeaderPanel = new System.Windows.Forms.Panel();
            _controlPointLabel = new System.Windows.Forms.Label();
            _addButton = new VrdoPdg.View.Controls.RoundedButton();
            _headerPanel.SuspendLayout();
            _titlePanel.SuspendLayout();
            _iconBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_icon).BeginInit();
            _footerPanel.SuspendLayout();
            _leftPanel.SuspendLayout();
            _rightPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_grid).BeginInit();
            _errorBanner.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_errorIcon).BeginInit();
            _tableHeaderPanel.SuspendLayout();
            SuspendLayout();
            //
            // _headerPanel
            //
            _headerPanel.Controls.Add(_titlePanel);
            _headerPanel.Controls.Add(_statusPill);
            _headerPanel.Controls.Add(_closeButton);
            _headerPanel.Controls.Add(_iconBox);
            _headerPanel.Dock = System.Windows.Forms.DockStyle.Top;
            _headerPanel.Name = "_headerPanel";
            _headerPanel.Padding = new System.Windows.Forms.Padding(20, 14, 16, 14);
            _headerPanel.Size = new System.Drawing.Size(1000, 74);
            _headerPanel.TabIndex = 0;
            //
            // _titlePanel
            //
            _titlePanel.Controls.Add(_subtitleLabel);
            _titlePanel.Controls.Add(_titleLabel);
            _titlePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            _titlePanel.Name = "_titlePanel";
            _titlePanel.Padding = new System.Windows.Forms.Padding(14, 0, 0, 0);
            _titlePanel.TabIndex = 1;
            //
            // _subtitleLabel
            //
            _subtitleLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            _subtitleLabel.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            _subtitleLabel.ForeColor = System.Drawing.Color.FromArgb(140, 140, 140);
            _subtitleLabel.Name = "_subtitleLabel";
            _subtitleLabel.TabIndex = 1;
            _subtitleLabel.Text = "Порядок задействования СКРМ при вводе режима по напряжению в КП";
            //
            // _titleLabel
            //
            _titleLabel.Dock = System.Windows.Forms.DockStyle.Top;
            _titleLabel.Font = new System.Drawing.Font("Segoe UI Semibold", 15F);
            _titleLabel.ForeColor = System.Drawing.Color.FromArgb(236, 236, 236);
            _titleLabel.Name = "_titleLabel";
            _titleLabel.Size = new System.Drawing.Size(500, 28);
            _titleLabel.TabIndex = 0;
            _titleLabel.Text = "Таблица эффективности";
            //
            // _statusPill
            //
            _statusPill.Dock = System.Windows.Forms.DockStyle.Right;
            _statusPill.Name = "_statusPill";
            _statusPill.Size = new System.Drawing.Size(270, 46);
            _statusPill.TabIndex = 2;
            _statusPill.TabStop = false;
            _statusPill.Visible = false;
            //
            // _closeButton
            //
            _closeButton.Dock = System.Windows.Forms.DockStyle.Right;
            _closeButton.Name = "_closeButton";
            _closeButton.Size = new System.Drawing.Size(46, 46);
            _closeButton.TabIndex = 3;
            _closeButton.Click += CancelButton_Click;
            //
            // _iconBox
            //
            _iconBox.BorderRadius = 10;
            _iconBox.Controls.Add(_icon);
            _iconBox.Dock = System.Windows.Forms.DockStyle.Left;
            _iconBox.FillColor = System.Drawing.Color.FromArgb(42, 32, 72);
            _iconBox.Name = "_iconBox";
            _iconBox.Size = new System.Drawing.Size(46, 46);
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
            // _headerLine
            //
            _headerLine.BackColor = System.Drawing.Color.FromArgb(48, 48, 48);
            _headerLine.Dock = System.Windows.Forms.DockStyle.Top;
            _headerLine.Name = "_headerLine";
            _headerLine.Size = new System.Drawing.Size(1000, 1);
            _headerLine.TabIndex = 1;
            //
            // _footerPanel
            //
            _footerPanel.Controls.Add(_defaultButton);
            _footerPanel.Controls.Add(_cancelButton);
            _footerPanel.Controls.Add(_saveButton);
            _footerPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            _footerPanel.Name = "_footerPanel";
            _footerPanel.Padding = new System.Windows.Forms.Padding(20, 14, 20, 14);
            _footerPanel.Size = new System.Drawing.Size(1000, 72);
            _footerPanel.TabIndex = 5;
            //
            // _defaultButton
            //
            _defaultButton.Dock = System.Windows.Forms.DockStyle.Left;
            _defaultButton.Name = "_defaultButton";
            _defaultButton.Size = new System.Drawing.Size(170, 44);
            _defaultButton.TabIndex = 0;
            _defaultButton.Text = "По умолчанию";
            _defaultButton.Click += DefaultButton_Click;
            //
            // _cancelButton
            //
            _cancelButton.Dock = System.Windows.Forms.DockStyle.Right;
            _cancelButton.Name = "_cancelButton";
            _cancelButton.Size = new System.Drawing.Size(130, 44);
            _cancelButton.TabIndex = 1;
            _cancelButton.Text = "Отмена";
            _cancelButton.Click += CancelButton_Click;
            //
            // _saveButton
            //
            _saveButton.Dock = System.Windows.Forms.DockStyle.Right;
            _saveButton.Name = "_saveButton";
            _saveButton.Size = new System.Drawing.Size(160, 44);
            _saveButton.TabIndex = 2;
            _saveButton.Text = "Сохранить";
            _saveButton.Click += SaveButton_Click;
            //
            // _footerLine
            //
            _footerLine.BackColor = System.Drawing.Color.FromArgb(48, 48, 48);
            _footerLine.Dock = System.Windows.Forms.DockStyle.Bottom;
            _footerLine.Name = "_footerLine";
            _footerLine.Size = new System.Drawing.Size(1000, 1);
            _footerLine.TabIndex = 4;
            //
            // _leftPanel
            //
            _leftPanel.BackColor = System.Drawing.Color.FromArgb(20, 20, 20);
            _leftPanel.Controls.Add(_controlPointList);
            _leftPanel.Controls.Add(_searchSpacer);
            _leftPanel.Controls.Add(_searchBox);
            _leftPanel.Dock = System.Windows.Forms.DockStyle.Left;
            _leftPanel.Name = "_leftPanel";
            _leftPanel.Padding = new System.Windows.Forms.Padding(16);
            _leftPanel.Size = new System.Drawing.Size(270, 508);
            _leftPanel.TabIndex = 2;
            //
            // _controlPointList
            //
            _controlPointList.BackColor = System.Drawing.Color.FromArgb(20, 20, 20);
            _controlPointList.BorderStyle = System.Windows.Forms.BorderStyle.None;
            _controlPointList.Dock = System.Windows.Forms.DockStyle.Fill;
            _controlPointList.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            _controlPointList.IntegralHeight = false;
            _controlPointList.ItemHeight = 54;
            _controlPointList.Name = "_controlPointList";
            _controlPointList.TabIndex = 1;
            _controlPointList.DrawItem += ControlPointList_DrawItem;
            _controlPointList.SelectedIndexChanged += ControlPointList_SelectedIndexChanged;
            //
            // _searchSpacer
            //
            _searchSpacer.Dock = System.Windows.Forms.DockStyle.Top;
            _searchSpacer.Name = "_searchSpacer";
            _searchSpacer.Size = new System.Drawing.Size(238, 12);
            _searchSpacer.TabIndex = 2;
            //
            // _searchBox
            //
            _searchBox.Dock = System.Windows.Forms.DockStyle.Top;
            _searchBox.Name = "_searchBox";
            _searchBox.PlaceholderText = "Поиск КП...";
            _searchBox.Size = new System.Drawing.Size(238, 44);
            _searchBox.TabIndex = 0;
            _searchBox.TextChanged += SearchBox_TextChanged;
            //
            // _leftLine
            //
            _leftLine.BackColor = System.Drawing.Color.FromArgb(48, 48, 48);
            _leftLine.Dock = System.Windows.Forms.DockStyle.Left;
            _leftLine.Name = "_leftLine";
            _leftLine.Size = new System.Drawing.Size(1, 508);
            _leftLine.TabIndex = 3;
            //
            // _rightPanel
            //
            _rightPanel.Controls.Add(_grid);
            _rightPanel.Controls.Add(_errorBanner);
            _rightPanel.Controls.Add(_tableHeaderPanel);
            _rightPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            _rightPanel.Name = "_rightPanel";
            _rightPanel.Padding = new System.Windows.Forms.Padding(24, 12, 24, 12);
            _rightPanel.TabIndex = 6;
            //
            // _grid
            //
            _grid.AllowDrop = true;
            _grid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[]
            {
                _handleColumn,
                _priorityColumn,
                _typeColumn,
                _numberColumn,
                _nameColumn,
                _upColumn,
                _downColumn,
                _deleteColumn
            });
            _grid.Dock = System.Windows.Forms.DockStyle.Fill;
            _grid.MultiSelect = false;
            _grid.Name = "_grid";
            _grid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            _grid.TabIndex = 2;
            _grid.CellClick += Grid_CellClick;
            _grid.CellPainting += Grid_CellPainting;
            _grid.CellValueChanged += Grid_CellValueChanged;
            _grid.CurrentCellDirtyStateChanged += Grid_CurrentCellDirtyStateChanged;
            _grid.DataError += Grid_DataError;
            _grid.EditingControlShowing += Grid_EditingControlShowing;
            _grid.DragDrop += Grid_DragDrop;
            _grid.DragOver += Grid_DragOver;
            _grid.MouseDown += Grid_MouseDown;
            _grid.MouseMove += Grid_MouseMove;
            //
            // _handleColumn
            //
            _handleColumn.HeaderText = "";
            _handleColumn.Name = "_handleColumn";
            _handleColumn.ReadOnly = true;
            _handleColumn.Width = 36;
            //
            // _priorityColumn
            //
            _priorityColumn.HeaderText = "ПРИОРИТЕТ";
            _priorityColumn.MaxInputLength = 3;
            _priorityColumn.Name = "_priorityColumn";
            _priorityColumn.Width = 110;
            //
            // _typeColumn
            //
            _typeColumn.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.Nothing;
            _typeColumn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            _typeColumn.HeaderText = "ТИП СКРМ";
            _typeColumn.Items.AddRange(new object[] { "Управляемые", "Коммутируемые", "Генераторы" });
            _typeColumn.Name = "_typeColumn";
            _typeColumn.Width = 176;
            //
            // _numberColumn
            //
            _numberColumn.HeaderText = "УЗЕЛ / РЕАКТОР";
            _numberColumn.MaxInputLength = 9;
            _numberColumn.Name = "_numberColumn";
            _numberColumn.Width = 150;
            //
            // _nameColumn
            //
            _nameColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            _nameColumn.HeaderText = "НАИМЕНОВАНИЕ";
            _nameColumn.Name = "_nameColumn";
            _nameColumn.ReadOnly = true;
            //
            // _upColumn
            //
            _upColumn.HeaderText = "";
            _upColumn.Name = "_upColumn";
            _upColumn.ReadOnly = true;
            _upColumn.ToolTipText = "Повысить приоритет";
            _upColumn.Width = 42;
            //
            // _downColumn
            //
            _downColumn.HeaderText = "";
            _downColumn.Name = "_downColumn";
            _downColumn.ReadOnly = true;
            _downColumn.ToolTipText = "Понизить приоритет";
            _downColumn.Width = 42;
            //
            // _deleteColumn
            //
            _deleteColumn.HeaderText = "";
            _deleteColumn.Name = "_deleteColumn";
            _deleteColumn.ReadOnly = true;
            _deleteColumn.ToolTipText = "Удалить СКРМ";
            _deleteColumn.Width = 42;
            //
            // _errorBanner
            //
            _errorBanner.BorderColor = System.Drawing.Color.FromArgb(255, 92, 102);
            _errorBanner.BorderRadius = 12;
            _errorBanner.BorderThickness = 1;
            _errorBanner.Controls.Add(_errorLabel);
            _errorBanner.Controls.Add(_errorIcon);
            _errorBanner.Dock = System.Windows.Forms.DockStyle.Top;
            _errorBanner.FillColor = System.Drawing.Color.FromArgb(64, 22, 28);
            _errorBanner.Name = "_errorBanner";
            _errorBanner.Padding = new System.Windows.Forms.Padding(14, 8, 14, 8);
            _errorBanner.Size = new System.Drawing.Size(682, 56);
            _errorBanner.TabIndex = 1;
            _errorBanner.Visible = false;
            //
            // _errorLabel
            //
            _errorLabel.BackColor = System.Drawing.Color.Transparent;
            _errorLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            _errorLabel.Font = new System.Drawing.Font("Segoe UI Semibold", 10.5F);
            _errorLabel.ForeColor = System.Drawing.Color.FromArgb(236, 236, 236);
            _errorLabel.Name = "_errorLabel";
            _errorLabel.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            _errorLabel.TabIndex = 1;
            _errorLabel.Text = "Изменения не сохранены: данные некорректны";
            _errorLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // _errorIcon
            //
            _errorIcon.BackColor = System.Drawing.Color.Transparent;
            _errorIcon.Dock = System.Windows.Forms.DockStyle.Left;
            _errorIcon.Name = "_errorIcon";
            _errorIcon.Size = new System.Drawing.Size(28, 40);
            _errorIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            _errorIcon.TabIndex = 0;
            _errorIcon.TabStop = false;
            //
            // _tableHeaderPanel
            //
            _tableHeaderPanel.Controls.Add(_controlPointLabel);
            _tableHeaderPanel.Controls.Add(_addButton);
            _tableHeaderPanel.Dock = System.Windows.Forms.DockStyle.Top;
            _tableHeaderPanel.Name = "_tableHeaderPanel";
            _tableHeaderPanel.Padding = new System.Windows.Forms.Padding(0, 6, 0, 10);
            _tableHeaderPanel.Size = new System.Drawing.Size(682, 62);
            _tableHeaderPanel.TabIndex = 0;
            //
            // _controlPointLabel
            //
            _controlPointLabel.AutoEllipsis = true;
            _controlPointLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            _controlPointLabel.Font = new System.Drawing.Font("Segoe UI Semibold", 18F);
            _controlPointLabel.ForeColor = System.Drawing.Color.FromArgb(236, 236, 236);
            _controlPointLabel.Name = "_controlPointLabel";
            _controlPointLabel.TabIndex = 0;
            _controlPointLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // _addButton
            //
            _addButton.Dock = System.Windows.Forms.DockStyle.Right;
            _addButton.Name = "_addButton";
            _addButton.Size = new System.Drawing.Size(186, 46);
            _addButton.TabIndex = 1;
            _addButton.Text = "Добавить СКРМ";
            _addButton.Click += AddButton_Click;
            //
            // EfficiencyTableForm
            //
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.FromArgb(24, 24, 24);
            ClientSize = new System.Drawing.Size(1040, 660);
            Controls.Add(_rightPanel);
            Controls.Add(_leftLine);
            Controls.Add(_leftPanel);
            Controls.Add(_footerLine);
            Controls.Add(_footerPanel);
            Controls.Add(_headerLine);
            Controls.Add(_headerPanel);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            KeyPreview = true;
            Name = "EfficiencyTableForm";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Таблица эффективности";
            FormClosing += EfficiencyTableForm_FormClosing;
            KeyDown += EfficiencyTableForm_KeyDown;
            _headerPanel.ResumeLayout(false);
            _titlePanel.ResumeLayout(false);
            _iconBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)_icon).EndInit();
            _footerPanel.ResumeLayout(false);
            _leftPanel.ResumeLayout(false);
            _rightPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)_grid).EndInit();
            _errorBanner.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)_errorIcon).EndInit();
            _tableHeaderPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        /// <summary>
        /// Заголовок окна.
        /// </summary>
        private System.Windows.Forms.Panel _headerPanel;

        /// <summary>
        /// Подложка значка окна.
        /// </summary>
        private VrdoPdg.View.Controls.RoundedPanel _iconBox;

        /// <summary>
        /// Значок окна.
        /// </summary>
        private System.Windows.Forms.PictureBox _icon;

        /// <summary>
        /// Область названия окна.
        /// </summary>
        private System.Windows.Forms.Panel _titlePanel;

        /// <summary>
        /// Название окна.
        /// </summary>
        private System.Windows.Forms.Label _titleLabel;

        /// <summary>
        /// Пояснение к названию окна.
        /// </summary>
        private System.Windows.Forms.Label _subtitleLabel;

        /// <summary>
        /// Счетчик несохраненных изменений или ошибок.
        /// </summary>
        private VrdoPdg.View.Controls.RoundedButton _statusPill;

        /// <summary>
        /// Кнопка закрытия окна.
        /// </summary>
        private VrdoPdg.View.Controls.RoundedButton _closeButton;

        /// <summary>
        /// Линия под заголовком.
        /// </summary>
        private System.Windows.Forms.Panel _headerLine;

        /// <summary>
        /// Нижняя панель с кнопками.
        /// </summary>
        private System.Windows.Forms.Panel _footerPanel;

        /// <summary>
        /// Кнопка «По умолчанию».
        /// </summary>
        private VrdoPdg.View.Controls.RoundedButton _defaultButton;

        /// <summary>
        /// Кнопка «Отмена».
        /// </summary>
        private VrdoPdg.View.Controls.RoundedButton _cancelButton;

        /// <summary>
        /// Кнопка «Сохранить».
        /// </summary>
        private VrdoPdg.View.Controls.RoundedButton _saveButton;

        /// <summary>
        /// Линия над нижней панелью.
        /// </summary>
        private System.Windows.Forms.Panel _footerLine;

        /// <summary>
        /// Панель списка КП.
        /// </summary>
        private System.Windows.Forms.Panel _leftPanel;

        /// <summary>
        /// Поле поиска КП.
        /// </summary>
        private VrdoPdg.View.Controls.SearchBox _searchBox;

        /// <summary>
        /// Отступ под полем поиска.
        /// </summary>
        private System.Windows.Forms.Panel _searchSpacer;

        /// <summary>
        /// Список КП.
        /// </summary>
        private System.Windows.Forms.ListBox _controlPointList;

        /// <summary>
        /// Линия справа от списка КП.
        /// </summary>
        private System.Windows.Forms.Panel _leftLine;

        /// <summary>
        /// Панель таблицы эффективности.
        /// </summary>
        private System.Windows.Forms.Panel _rightPanel;

        /// <summary>
        /// Заголовок таблицы эффективности.
        /// </summary>
        private System.Windows.Forms.Panel _tableHeaderPanel;

        /// <summary>
        /// Наименование выбранного КП.
        /// </summary>
        private System.Windows.Forms.Label _controlPointLabel;

        /// <summary>
        /// Кнопка «Добавить СКРМ».
        /// </summary>
        private VrdoPdg.View.Controls.RoundedButton _addButton;

        /// <summary>
        /// Сообщение об ошибках сохранения.
        /// </summary>
        private VrdoPdg.View.Controls.RoundedPanel _errorBanner;

        /// <summary>
        /// Значок сообщения об ошибках.
        /// </summary>
        private System.Windows.Forms.PictureBox _errorIcon;

        /// <summary>
        /// Текст сообщения об ошибках.
        /// </summary>
        private System.Windows.Forms.Label _errorLabel;

        /// <summary>
        /// Таблица эффективности КП.
        /// </summary>
        private System.Windows.Forms.DataGridView _grid;

        /// <summary>
        /// Столбец маркера перетаскивания.
        /// </summary>
        private System.Windows.Forms.DataGridViewTextBoxColumn _handleColumn;

        /// <summary>
        /// Столбец «Приоритет».
        /// </summary>
        private System.Windows.Forms.DataGridViewTextBoxColumn _priorityColumn;

        /// <summary>
        /// Столбец «Тип СКРМ».
        /// </summary>
        private System.Windows.Forms.DataGridViewComboBoxColumn _typeColumn;

        /// <summary>
        /// Столбец «Узел / реактор».
        /// </summary>
        private System.Windows.Forms.DataGridViewTextBoxColumn _numberColumn;

        /// <summary>
        /// Столбец «Наименование».
        /// </summary>
        private System.Windows.Forms.DataGridViewTextBoxColumn _nameColumn;

        /// <summary>
        /// Столбец кнопки «↑».
        /// </summary>
        private System.Windows.Forms.DataGridViewTextBoxColumn _upColumn;

        /// <summary>
        /// Столбец кнопки «↓».
        /// </summary>
        private System.Windows.Forms.DataGridViewTextBoxColumn _downColumn;

        /// <summary>
        /// Столбец кнопки удаления.
        /// </summary>
        private System.Windows.Forms.DataGridViewTextBoxColumn _deleteColumn;
    }
}
