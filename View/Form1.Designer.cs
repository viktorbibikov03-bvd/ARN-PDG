namespace View
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            loadButton = new Button();
            saveButton = new Button();
            saveToDbButton = new Button();
            viewDbButton = new Button();
            startButton = new Button();
            nodeComboBox = new ComboBox();
            objectLabel = new Label();
            voltageDataGridView = new DataGridView();
            tableLabel = new Label();
            voltageChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            ((System.ComponentModel.ISupportInitialize)voltageDataGridView).BeginInit();
            ((System.ComponentModel.ISupportInitialize)voltageChart).BeginInit();
            SuspendLayout();
            // 
            // loadButton
            // 
            loadButton.Location = new Point(17, 20);
            loadButton.Margin = new Padding(4, 5, 4, 5);
            loadButton.Name = "loadButton";
            loadButton.Size = new Size(143, 50);
            loadButton.TabIndex = 0;
            loadButton.Text = "Загрузить";
            loadButton.UseVisualStyleBackColor = true;
            loadButton.Click += LoadButton_Click;
            // 
            // saveButton
            // 
            saveButton.Location = new Point(186, 20);
            saveButton.Margin = new Padding(4, 5, 4, 5);
            saveButton.Name = "saveButton";
            saveButton.Size = new Size(143, 50);
            saveButton.TabIndex = 1;
            saveButton.Text = "Сохранить CSV";
            saveButton.UseVisualStyleBackColor = true;
            saveButton.Click += SaveButton_Click;
            // 
            // saveToDbButton
            // 
            saveToDbButton.BackColor = Color.LightBlue;
            saveToDbButton.Location = new Point(355, 20);
            saveToDbButton.Margin = new Padding(4, 5, 4, 5);
            saveToDbButton.Name = "saveToDbButton";
            saveToDbButton.Size = new Size(143, 50);
            saveToDbButton.TabIndex = 8;
            saveToDbButton.Text = "Сохранить в БД";
            saveToDbButton.UseVisualStyleBackColor = false;
            saveToDbButton.Click += SaveToDatabaseButton_Click;
            // 
            // viewDbButton
            // 
            viewDbButton.BackColor = Color.LightGreen;
            viewDbButton.Location = new Point(524, 20);
            viewDbButton.Margin = new Padding(4, 5, 4, 5);
            viewDbButton.Name = "viewDbButton";
            viewDbButton.Size = new Size(143, 50);
            viewDbButton.TabIndex = 9;
            viewDbButton.Text = "Просмотр БД";
            viewDbButton.UseVisualStyleBackColor = false;
            viewDbButton.Click += ViewDatabaseButton_Click;
            // 
            // startButton
            // 
            startButton.BackColor = Color.FromArgb(0, 255, 0);
            startButton.Location = new Point(693, 20);
            startButton.Margin = new Padding(4, 5, 4, 5);
            startButton.Name = "startButton";
            startButton.Size = new Size(143, 50);
            startButton.TabIndex = 2;
            startButton.Text = "Запустить";
            startButton.UseVisualStyleBackColor = false;
            startButton.Click += StartButton_Click;
            // 
            // nodeComboBox
            // 
            nodeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            nodeComboBox.FormattingEnabled = true;
            nodeComboBox.Location = new Point(17, 100);
            nodeComboBox.Margin = new Padding(4, 5, 4, 5);
            nodeComboBox.Name = "nodeComboBox";
            nodeComboBox.Size = new Size(312, 33);
            nodeComboBox.TabIndex = 3;
            nodeComboBox.SelectedIndexChanged += nodeComboBox_SelectedIndexChanged;
            // 
            // objectLabel
            // 
            objectLabel.AutoSize = true;
            objectLabel.Location = new Point(17, 70);
            objectLabel.Margin = new Padding(4, 0, 4, 0);
            objectLabel.Name = "objectLabel";
            objectLabel.Size = new Size(135, 25);
            objectLabel.TabIndex = 4;
            objectLabel.Text = "Выберите узел:";
            // 
            // voltageDataGridView
            // 
            voltageDataGridView.AllowUserToAddRows = false;
            voltageDataGridView.AllowUserToResizeColumns = false;
            voltageDataGridView.AllowUserToResizeRows = false;
            voltageDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            voltageDataGridView.BackgroundColor = Color.White;
            voltageDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            voltageDataGridView.Location = new Point(17, 158);
            voltageDataGridView.MultiSelect = false;
            voltageDataGridView.Name = "voltageDataGridView";
            voltageDataGridView.ReadOnly = true;
            voltageDataGridView.RowHeadersVisible = false;
            voltageDataGridView.RowHeadersWidth = 62;
            voltageDataGridView.ScrollBars = ScrollBars.Vertical;
            voltageDataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            voltageDataGridView.Size = new Size(312, 460);
            voltageDataGridView.TabIndex = 5;
            // 
            // tableLabel
            // 
            tableLabel.AutoSize = true;
            tableLabel.Font = new Font("Segoe UI Black", 11F, FontStyle.Bold, GraphicsUnit.Point, 204);
            tableLabel.Location = new Point(500, 99);
            tableLabel.Name = "tableLabel";
            tableLabel.Size = new Size(391, 30);
            tableLabel.TabIndex = 6;
            tableLabel.Text = "График напряжения для объекта";
            // 
            // voltageChart
            // 
            voltageChart.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            voltageChart.Location = new Point(350, 158);
            voltageChart.Name = "voltageChart";
            voltageChart.Size = new Size(610, 460);
            voltageChart.TabIndex = 7;
            voltageChart.Text = "chart1";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoScroll = true;
            ClientSize = new Size(978, 644);
            Controls.Add(viewDbButton);
            Controls.Add(saveToDbButton);
            Controls.Add(voltageChart);
            Controls.Add(tableLabel);
            Controls.Add(voltageDataGridView);
            Controls.Add(objectLabel);
            Controls.Add(nodeComboBox);
            Controls.Add(startButton);
            Controls.Add(saveButton);
            Controls.Add(loadButton);
            Margin = new Padding(4, 5, 4, 5);
            MinimumSize = new Size(1000, 700);
            Name = "MainForm";
            Text = "АРН-ПДГ";
            FormClosing += MainForm_FormClosing;
            ((System.ComponentModel.ISupportInitialize)voltageDataGridView).EndInit();
            ((System.ComponentModel.ISupportInitialize)voltageChart).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private System.Windows.Forms.Button loadButton;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.Button saveToDbButton;
        private System.Windows.Forms.Button viewDbButton;
        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.ComboBox nodeComboBox;
        private System.Windows.Forms.Label objectLabel;
        private System.Windows.Forms.Label tableLabel;
        private System.Windows.Forms.DataGridView voltageDataGridView;
        private System.Windows.Forms.DataVisualization.Charting.Chart voltageChart;
    }
}