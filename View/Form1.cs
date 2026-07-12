using Model;
using System.Windows.Forms.DataVisualization.Charting;

namespace View
{
    public partial class MainForm : Form
    {
        private BarsModel _model;
        private DataBaseRepository _repository;

        public MainForm()
        {
            InitializeComponent();
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            _model = new BarsModel();
            _repository = new DataBaseRepository();

            voltageDataGridView.CellFormatting += VoltageDataGridView_CellFormatting;

            SetupChart();
        }

        private void SetupChart()
        {
            voltageChart.Series.Clear();
            voltageChart.Titles.Clear();
            voltageChart.ChartAreas.Clear();

            ChartArea chartArea = new ChartArea("MainArea");
            voltageChart.ChartAreas.Add(chartArea);

            chartArea.AxisX.Title = "Час";
            chartArea.AxisY.Title = "Напряжение, кВ";
            chartArea.AxisX.Minimum = 1;
            chartArea.AxisX.Maximum = 24;
            chartArea.AxisX.Interval = 1;
            chartArea.AxisX.LabelStyle.Format = "0";

            chartArea.AxisX.MajorGrid.Enabled = false;
            chartArea.AxisY.MajorGrid.Enabled = true;
            chartArea.AxisY.MajorGrid.LineColor = Color.LightGray;
            chartArea.AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

            Title title = new Title("График напряжения для объекта",
                Docking.Top,
                new Font("Segoe UI", 12F, FontStyle.Bold),
                Color.Black);
            voltageChart.Titles.Add(title);

            Series voltageSeries = new Series("Voltage")
            {
                ChartType = SeriesChartType.Line,
                Color = Color.Black,
                BorderWidth = 2,
                XValueType = ChartValueType.Int32,
                YValueType = ChartValueType.Double,
                IsVisibleInLegend = false
            };
            voltageChart.Series.Add(voltageSeries);

            Series upperLimitSeries = new Series("UpperLimit")
            {
                ChartType = SeriesChartType.Line,
                Color = Color.DodgerBlue,
                BorderWidth = 2,
                BorderDashStyle = ChartDashStyle.Solid,
                XValueType = ChartValueType.Int32,
                YValueType = ChartValueType.Double,
                IsVisibleInLegend = false
            };
            upperLimitSeries.Points.AddXY(1, 525);
            upperLimitSeries.Points.AddXY(24, 525);
            voltageChart.Series.Add(upperLimitSeries);

            Series lowerLimitSeries = new Series("LowerLimit")
            {
                ChartType = SeriesChartType.Line,
                Color = Color.Red,
                BorderWidth = 2,
                BorderDashStyle = ChartDashStyle.Solid,
                XValueType = ChartValueType.Int32,
                YValueType = ChartValueType.Double,
                IsVisibleInLegend = false
            };
            lowerLimitSeries.Points.AddXY(1, 500);
            lowerLimitSeries.Points.AddXY(24, 500);
            voltageChart.Series.Add(lowerLimitSeries);
        }

        private void LoadButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = "Файлы мегаточки (*.mptsmz)|*.mptsmz|Все файлы (*.*)|*.*";
                    openFileDialog.Title = "Выберите файл мегаточки";
                    openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        _model.LoadFile(openFileDialog.FileName);

                        List<int> nodeNumbers = _model.GetNodeNumbers();

                        nodeComboBox.Items.Clear();
                        nodeComboBox.Items.Add("Название объекта");

                        foreach (int nodeNum in nodeNumbers)
                        {
                            nodeComboBox.Items.Add($"Узел {nodeNum}");
                        }

                        nodeComboBox.SelectedIndex = 0;

                        MessageBox.Show($"Файл успешно загружен. Количество узлов: {nodeNumbers.Count}",
                            "Загрузка", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке файла: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            if (!_model.IsLoaded)
            {
                MessageBox.Show("Сначала загрузите файл мегаточки!", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                _model.CalculateHourlyRegime();

                MessageBox.Show("Расчет успешно завершен!", "Расчет",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                if (nodeComboBox.SelectedIndex > 0)
                {
                    ShowVoltageTable();
                    ShowVoltageChart();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при расчете: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void nodeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_model.IsCalculated || nodeComboBox.SelectedIndex == 0)
            {
                return;
            }

            ShowVoltageTable();
            ShowVoltageChart();
        }

        private void ShowVoltageTable()
        {
            try
            {
                string selectedNode = nodeComboBox.SelectedItem.ToString();
                int nodeNumber = Convert.ToInt32(selectedNode.Replace("Узел ", ""));

                Dictionary<int, double> voltages = _model.GetNodeVoltages(nodeNumber);

                voltageDataGridView.Columns.Clear();
                voltageDataGridView.Rows.Clear();

                voltageDataGridView.Columns.Add("Hour", "Час");
                voltageDataGridView.Columns.Add("Voltage", $"Напряжение");

                voltageDataGridView.Columns["Hour"].Width = 80;
                voltageDataGridView.Columns["Voltage"].Width = 150;

                foreach (var kvp in voltages)
                {
                    int rowIndex = voltageDataGridView.Rows.Add();
                    voltageDataGridView.Rows[rowIndex].Cells["Hour"].Value = kvp.Key;
                    voltageDataGridView.Rows[rowIndex].Cells["Voltage"].Value = kvp.Value.ToString("F4");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отображении данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowVoltageChart()
        {
            try
            {
                string selectedNode = nodeComboBox.SelectedItem.ToString();
                int nodeNumber = Convert.ToInt32(selectedNode.Replace("Узел ", ""));

                Dictionary<int, double> voltages = _model.GetNodeVoltages(nodeNumber);

                voltageChart.Series["Voltage"].Points.Clear();

                foreach (var kvp in voltages)
                {
                    voltageChart.Series["Voltage"].Points.AddXY(kvp.Key, kvp.Value);
                }

                voltageChart.Titles[0].Text = $"График напряжения для узла {nodeNumber}";

                if (voltages.Count > 0)
                {
                    double minVoltage = voltages.Values.Min();
                    double maxVoltage = voltages.Values.Max();

                    double range = maxVoltage - minVoltage;
                    double padding = range * 0.1;

                    voltageChart.ChartAreas[0].AxisY.Minimum = Math.Floor(minVoltage - padding);
                    voltageChart.ChartAreas[0].AxisY.Maximum = Math.Ceiling(maxVoltage + padding);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при построении графика: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void VoltageDataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != 1)
            {
                return;
            }

            if (e.Value != null && double.TryParse(e.Value.ToString(), out double voltage))
            {
                if (voltage > 525)
                {
                    voltageDataGridView.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightBlue;
                    voltageDataGridView.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.DarkBlue;
                }
                else if (voltage < 500)
                {
                    voltageDataGridView.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightCoral;
                    voltageDataGridView.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.DarkRed;
                }
                else
                {
                    voltageDataGridView.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.White;
                    voltageDataGridView.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.Black;
                }
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (!_model.IsCalculated)
            {
                MessageBox.Show("Сначала выполните расчет!", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "CSV файлы (*.csv)|*.csv|Все файлы (*.*)|*.*";
                    saveFileDialog.Title = "Сохранить результаты";
                    saveFileDialog.FileName = "voltages_results.csv";
                    saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        _model.ExportToCsv(saveFileDialog.FileName);

                        MessageBox.Show($"Результаты успешно сохранены в файл:\n{saveFileDialog.FileName}",
                            "Сохранение", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении файла: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveToDatabaseButton_Click(object sender, EventArgs e)
        {
            if (!_model.IsLoaded)
            {
                MessageBox.Show("Сначала загрузите файл мегаточки!", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                _model.SaveToDatabase(_repository);

                MessageBox.Show("Данные успешно сохранены в базу данных ARN-DB.db!",
                    "Сохранение в БД", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении в БД: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ViewDatabaseButton_Click(object sender, EventArgs e)
        {
            try
            {
                DatabaseViewForm viewForm = new DatabaseViewForm();
                viewForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии просмотра БД: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _repository?.Dispose();
            _model?.Dispose();
        }
    }
}