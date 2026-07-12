using Model;
using System.Data;

namespace View
{
    public partial class DatabaseViewForm : Form
    {
        private DataBaseRepository _repository;
        private TabControl _tabControl;

        public DatabaseViewForm()
        {
            InitializeComponent();
            _repository = new DataBaseRepository();
            InitializeCustomComponents();
            LoadAllData();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Просмотр данных из базы данных";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;

            _tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F)
            };

            this.Controls.Add(_tabControl);

            CreateTab("Adjustable RPCDs", "Управляемые реакторы");
            CreateTab("Generators", "Генераторы");
            CreateTab("PQ-Diagrams", "PQ-диаграммы");
            CreateTab("Switched RPCDs", "Коммутируемые реакторы");
            CreateTab("VoltageAtVSPs", "Напряжение в КП");
            CreateTab("EfficiencyTable", "Таблица эффективности");
        }

        private void CreateTab(string tableName, string displayName)
        {
            TabPage tabPage = new TabPage(displayName);
            tabPage.Name = $"tab_{tableName.Replace(" ", "_").Replace("-", "_")}";

            DataGridView dataGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                Font = new Font("Segoe UI", 9F),
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(240, 240, 240)
                }
            };

            dataGridView.Name = $"grid_{tableName.Replace(" ", "_").Replace("-", "_")}";
            tabPage.Controls.Add(dataGridView);
            _tabControl.TabPages.Add(tabPage);
        }

        private void LoadAllData()
        {
            try
            {
                LoadAdjustableRPCDs();
                LoadGenerators();
                LoadPQDiagrams();
                LoadSwitchedRPCDs();
                LoadVoltageAtVSPs();
                LoadEfficiencyTables();

                UpdateTabTitles();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadAdjustableRPCDs()
        {
            var data = _repository.GetAdjustableRPCDs();
            var grid = GetDataGridView("grid_Adjustable_RPCDs");

            if (grid != null)
            {
                grid.DataSource = null;
                grid.DataSource = data.Select(x => new
                {
                    Узел = x.ControlledRPCD_NodeNumber,
                    Час = x.Hour,
                    Имя = x.Name,
                    Реактивная_мощность = x.ReactivePower,
                    Мин_реактивная = x.MinReactivePower,
                    Макс_реактивная = x.MaxReactivePower,
                    Статус = x.OnOff 
                        ? "Вкл" 
                        : "Выкл",
                    Напряжение = x.VoltageAtNode
                }).ToList();

                FormatDataGridView(grid);
            }
        }

        private void LoadGenerators()
        {
            var data = _repository.GetGenerators();
            var grid = GetDataGridView("grid_Generators");

            if (grid != null)
            {
                grid.DataSource = null;
                grid.DataSource = data.Select(x => new
                {
                    Узел = x.GeneratorNodeNumber,
                    Час = x.Hour,
                    Имя = x.Name,
                    Напряжение_шины = x.BusVoltage,
                    PQ_диаграмма = x.PQDiagramNumber,
                    Статус = x.OnOff 
                        ? "Вкл" 
                        : "Выкл"
                }).ToList();

                FormatDataGridView(grid);
            }
        }

        private void LoadPQDiagrams()
        {
            var data = _repository.GetPQDiagrams();
            var grid = GetDataGridView("grid_PQ_Diagrams");

            if (grid != null)
            {
                grid.DataSource = null;
                grid.DataSource = data.Select(x => new
                {
                    Номер = x.PQDiagramNumber,
                    Активная_мощность = x.ActivePower,
                    Макс_реактивная = x.MaxReactivePower,
                    Мин_реактивная = x.MinReactivePower
                }).ToList();

                FormatDataGridView(grid);
            }
        }

        private void LoadSwitchedRPCDs()
        {
            var data = _repository.GetSwitchedRPCDs();
            var grid = GetDataGridView("grid_Switched_RPCDs");

            if (grid != null)
            {
                grid.DataSource = null;
                grid.DataSource = data.Select(x => new
                {
                    Номер = x.ReactorNumber,
                    Час = x.Hour,
                    Имя = x.Name,
                    Мощность = x.Capacity,
                    Статус = x.OnOff 
                        ? "Вкл" 
                        : "Выкл"
                }).ToList();

                FormatDataGridView(grid);
            }
        }

        private void LoadVoltageAtVSPs()
        {
            var data = _repository.GetVoltageAtVSPs();
            var grid = GetDataGridView("grid_VoltageAtVSPs");

            if (grid != null)
            {
                grid.DataSource = null;
                grid.DataSource = data.Select(x => new
                {
                    Узел = x.VCP_NodeNumber,
                    Час = x.Hour,
                    Напряжение = x.Voltage,
                    Мин_напряжение = x.MinVoltage,
                    Макс_напряжение = x.MaxVoltage
                }).ToList();

                FormatDataGridView(grid);
            }
        }

        private void LoadEfficiencyTables()
        {
            var data = _repository.GetEfficiencyTables();
            var grid = GetDataGridView("grid_EfficiencyTable");

            if (grid != null)
            {
                grid.DataSource = null;
                grid.DataSource = data.Select(x => new
                {
                    Узел_VCP = x.VCP_NodeNumber,
                    Приоритет_RPCD = x.PriorityRPCDNumber,
                    Узел_RPCD = x.ControlledRPCD_NodeNumber,
                    Номер_реактора = x.ReactorNumber,
                    Узел_генератора = x.GeneratorNodeNumber
                }).ToList();

                FormatDataGridView(grid);
            }
        }

        private void FormatDataGridView(DataGridView grid)
        {
            if (grid == null) return;

            grid.AutoResizeColumns();
            grid.ClearSelection();
        }

        private DataGridView GetDataGridView(string name)
        {
            foreach (TabPage tab in _tabControl.TabPages)
            {
                foreach (Control ctrl in tab.Controls)
                {
                    if (ctrl is DataGridView && ctrl.Name == name)
                    {
                        return (DataGridView)ctrl;
                    }
                }
            }
            return null;
        }

        private void UpdateTabTitles()
        {
            var stats = _repository.GetTablesStatistics();

            int tabIndex = 0;
            foreach (var stat in stats)
            {
                if (tabIndex < _tabControl.TabPages.Count)
                {
                    _tabControl.TabPages[tabIndex].Text = $"{stat.Key} ({stat.Value})";
                    tabIndex++;
                }
            }
        }

        private void DatabaseViewForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _repository?.Dispose();
        }
    }
}