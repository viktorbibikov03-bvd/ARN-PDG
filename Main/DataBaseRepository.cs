using Microsoft.Data.Sqlite;
using Model.Models;
using System.Diagnostics;


namespace Model
{
    public class DataBaseRepository : IDisposable
    {
        private readonly string _connectionString;
        private readonly string _dbPath;
        private bool _disposed = false;

        public DataBaseRepository()
        {
            _dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ARN-DB.db");
            _connectionString = $"Data Source={_dbPath};";

            Debug.WriteLine($"=== Подключение к БД ===");
            Debug.WriteLine($"Путь к БД: {_dbPath}");
            Debug.WriteLine($"БД существует: {File.Exists(_dbPath)}");

            CheckTablesExist();
        }

        /// <summary>
        /// Метод для проверки существования таблиц в БД
        /// </summary>
        private void CheckTablesExist()
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                var tables = new[] {
                    "Adjustable RPCDs",
                    "Generators",
                    "PQ-Diagrams",
                    "SwitchedRPCDs",
                    "VoltageAtVSPs",
                    "EfficiencyTable"
                };

                foreach (var tableName in tables)
                {
                    var command = new SqliteCommand(
                        $"SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='{tableName}'",
                        connection);

                    int count = Convert.ToInt32(command.ExecuteScalar());

                    if (count == 0)
                    {
                        Debug.WriteLine($"️ ВНИМАНИЕ: Таблица '{tableName}' не найдена!");
                    }
                    else
                    {
                        Debug.WriteLine($"✓ Таблица '{tableName}' существует");
                    }
                }
            }
        }

        /// <summary>
        /// Метод для очистки таблиц
        /// </summary>
        public void ClearAllTables()
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
        
                var tables = new[] {
                    "\"Adjustable RPCDs\"",
                    "\"Generators\"",
                    "\"PQ-Diagrams\"",
                    "\"SwitchedRPCDs\"",
                    "\"VoltageAtVSPs\"",
                    "\"EfficiencyTable\""
                };
        
                using (var transaction = connection.BeginTransaction())
                {
                    foreach (var table in tables)
                    {
                        try
                        {
                            using (var command = new SqliteCommand($"DELETE FROM {table}", connection))
                            {
                                command.Transaction = transaction;
                                int rows = command.ExecuteNonQuery();
                                Debug.WriteLine($"Очищено {rows} записей из {table}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Ошибка при очистке {table}: {ex.Message}");
                        }
                    }
                    transaction.Commit();
                }
                
                using (var command = new SqliteCommand("PRAGMA wal_checkpoint(TRUNCATE)", connection))
                {
                    command.ExecuteNonQuery();
                }
        
                Debug.WriteLine("✓ Все таблицы очищены");
            }
        }

        public void SaveAdjustableRPCDs(List<AdjustableRPCD> rpcds)
        {
            if (rpcds == null || rpcds.Count == 0)
            {
                Debug.WriteLine("SaveAdjustableRPCDs: список пуст");
                return;
            }

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    var command = new SqliteCommand();
                    command.Connection = connection;
                    command.Transaction = transaction;

                    command.CommandText = @"INSERT OR REPLACE INTO ""Adjustable RPCDs"" 
                        (ControlledRPCD_NodeNumber, ReactivePower, Name, MinReactivePower, MaxReactivePower, ""On/Off"", Hour, VoltageAtNode)
                        VALUES (@NodeNum, @Reactive, @Name, @MinReactive, @MaxReactive, @OnOff, @Hour, @Voltage)";

                    command.Parameters.Add(new SqliteParameter("@NodeNum", SqliteType.Integer));
                    command.Parameters.Add(new SqliteParameter("@Reactive", SqliteType.Real));
                    command.Parameters.Add(new SqliteParameter("@Name", SqliteType.Text));
                    command.Parameters.Add(new SqliteParameter("@MinReactive", SqliteType.Real));
                    command.Parameters.Add(new SqliteParameter("@MaxReactive", SqliteType.Real));
                    command.Parameters.Add(new SqliteParameter("@OnOff", SqliteType.Integer));
                    command.Parameters.Add(new SqliteParameter("@Hour", SqliteType.Integer));
                    command.Parameters.Add(new SqliteParameter("@Voltage", SqliteType.Real));

                    foreach (var rpcd in rpcds)
                    {
                        command.Parameters["@NodeNum"].Value = rpcd.ControlledRPCD_NodeNumber;
                        command.Parameters["@Reactive"].Value = rpcd.ReactivePower;
                        command.Parameters["@Name"].Value = rpcd.Name ?? "";
                        command.Parameters["@MinReactive"].Value = rpcd.MinReactivePower;
                        command.Parameters["@MaxReactive"].Value = rpcd.MaxReactivePower;
                        command.Parameters["@OnOff"].Value = rpcd.OnOff 
                            ? 1 
                            : 0;
                        command.Parameters["@Hour"].Value = rpcd.Hour;
                        command.Parameters["@Voltage"].Value = rpcd.VoltageAtNode;
                        command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }

                using (var command = new SqliteCommand("PRAGMA wal_checkpoint(TRUNCATE)", connection))
                {
                    command.ExecuteNonQuery();
                }
            }

            Debug.WriteLine($"✓ Сохранено Adjustable RPCDs: {rpcds.Count} записей");
        }

        public void SaveGenerators(List<Generator> generators)
        {
            if (generators == null || generators.Count == 0)
            {
                Debug.WriteLine("SaveGenerators: список пуст");
                return;
            }

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    var command = new SqliteCommand();
                    command.Connection = connection;
                    command.Transaction = transaction;

                    command.CommandText = @"INSERT OR REPLACE INTO ""Generators"" 
                        (GeneratorNodeNumber, BusVoltage, PQDiagramNumber, Name, Hour, ""On/Off"")
                        VALUES (@NodeNum, @Voltage, @PQNum, @Name, @Hour, @OnOff)";

                    command.Parameters.Add(new SqliteParameter("@NodeNum", SqliteType.Integer));
                    command.Parameters.Add(new SqliteParameter("@Voltage", SqliteType.Real));
                    command.Parameters.Add(new SqliteParameter("@PQNum", SqliteType.Integer));
                    command.Parameters.Add(new SqliteParameter("@Name", SqliteType.Text));
                    command.Parameters.Add(new SqliteParameter("@Hour", SqliteType.Integer));
                    command.Parameters.Add(new SqliteParameter("@OnOff", SqliteType.Integer));

                    foreach (var gen in generators)
                    {
                        command.Parameters["@NodeNum"].Value = gen.GeneratorNodeNumber;
                        command.Parameters["@Voltage"].Value = gen.BusVoltage;
                        command.Parameters["@PQNum"].Value = gen.PQDiagramNumber;
                        command.Parameters["@Name"].Value = gen.Name ?? "";
                        command.Parameters["@Hour"].Value = gen.Hour;
                        command.Parameters["@OnOff"].Value = gen.OnOff ? 1 : 0;
                        command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }

                using (var command = new SqliteCommand("PRAGMA wal_checkpoint(TRUNCATE)", connection))
                {
                    command.ExecuteNonQuery();
                }
            }

            Debug.WriteLine($"✓ Сохранено Generators: {generators.Count} записей");
        }

        public void SavePQDiagrams(List<PQDiagram> diagrams)
        {
            if (diagrams == null || diagrams.Count == 0)
            {
                Debug.WriteLine("SavePQDiagrams: список пуст");
                return;
            }

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    var command = new SqliteCommand();
                    command.Connection = connection;
                    command.Transaction = transaction;

                    command.CommandText = @"INSERT OR REPLACE INTO ""PQ-Diagrams"" 
                        (PQDiagramNumber, ActivePower, MaxReactivePower, MinReactivePower)
                        VALUES (@Num, @Active, @Max, @Min)";

                    command.Parameters.Add(new SqliteParameter("@Num", SqliteType.Integer));
                    command.Parameters.Add(new SqliteParameter("@Active", SqliteType.Real));
                    command.Parameters.Add(new SqliteParameter("@Max", SqliteType.Real));
                    command.Parameters.Add(new SqliteParameter("@Min", SqliteType.Real));

                    foreach (var diag in diagrams)
                    {
                        command.Parameters["@Num"].Value = diag.PQDiagramNumber;
                        command.Parameters["@Active"].Value = diag.ActivePower;
                        command.Parameters["@Max"].Value = diag.MaxReactivePower;
                        command.Parameters["@Min"].Value = diag.MinReactivePower;
                        command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }

                using (var command = new SqliteCommand("PRAGMA wal_checkpoint(TRUNCATE)", connection))
                {
                    command.ExecuteNonQuery();
                }
            }

            Debug.WriteLine($"✓ Сохранено PQ-Diagrams: {diagrams.Count} записей");
        }

        public void SaveSwitchedRPCDs(List<SwitchedRPCD> rpcds)
        {
            if (rpcds == null || rpcds.Count == 0)
            {
                Debug.WriteLine("SaveSwitchedRPCDs: список пуст");
                return;
            }

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    var command = new SqliteCommand();
                    command.Connection = connection;
                    command.Transaction = transaction;

                    command.CommandText = @"INSERT OR REPLACE INTO ""SwitchedRPCDs"" 
                        (ReactorNumber, Name, Capacity, ""On/Off"", Hour)
                        VALUES (@Num, @Name, @Capacity, @OnOff, @Hour)";

                    command.Parameters.Add(new SqliteParameter("@Num", SqliteType.Integer));
                    command.Parameters.Add(new SqliteParameter("@Name", SqliteType.Text));
                    command.Parameters.Add(new SqliteParameter("@Capacity", SqliteType.Real));
                    command.Parameters.Add(new SqliteParameter("@OnOff", SqliteType.Integer));
                    command.Parameters.Add(new SqliteParameter("@Hour", SqliteType.Integer));

                    foreach (var rpcd in rpcds)
                    {
                        command.Parameters["@Num"].Value = rpcd.ReactorNumber;
                        command.Parameters["@Name"].Value = rpcd.Name ?? "";
                        command.Parameters["@Capacity"].Value = rpcd.Capacity;
                        command.Parameters["@OnOff"].Value = rpcd.OnOff 
                            ? 1 
                            : 0;
                        command.Parameters["@Hour"].Value = rpcd.Hour;
                        command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }

                using (var command = new SqliteCommand("PRAGMA wal_checkpoint(TRUNCATE)", connection))
                {
                    command.ExecuteNonQuery();
                }
            }

            Debug.WriteLine($"✓ Сохранено SwitchedRPCDs: {rpcds.Count} записей");
        }

        public void SaveVoltageAtVSPs(List<VoltageAtVSP> voltages)
        {
            if (voltages == null || voltages.Count == 0)
            {
                Debug.WriteLine("SaveVoltageAtVSPs: список пуст");
                return;
            }
        
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
        
                using (var transaction = connection.BeginTransaction())
                {
                    var command = new SqliteCommand();
                    command.Connection = connection;
                    command.Transaction = transaction;
        
                    command.CommandText = @"INSERT OR REPLACE INTO ""VoltageAtVSPs"" 
                        (VCP_NodeNumber, Voltage, MinVoltage, MaxVoltage, Hour)
                        VALUES (@NodeNum, @Voltage, @MinVoltage, @MaxVoltage, @Hour)";
        
                    command.Parameters.Add(new SqliteParameter("@NodeNum", SqliteType.Integer));
                    command.Parameters.Add(new SqliteParameter("@Voltage", SqliteType.Real));
                    command.Parameters.Add(new SqliteParameter("@MinVoltage", SqliteType.Integer));
                    command.Parameters.Add(new SqliteParameter("@MaxVoltage", SqliteType.Integer));
                    command.Parameters.Add(new SqliteParameter("@Hour", SqliteType.Integer));
        
                    foreach (var voltage in voltages)
                    {
                        command.Parameters["@NodeNum"].Value = voltage.VCP_NodeNumber;
                        command.Parameters["@Voltage"].Value = voltage.Voltage;
                        command.Parameters["@MinVoltage"].Value = voltage.MinVoltage;
                        command.Parameters["@MaxVoltage"].Value = voltage.MaxVoltage;
                        command.Parameters["@Hour"].Value = voltage.Hour;
                        command.ExecuteNonQuery();
                    }
        
                    transaction.Commit();
                }
        
                using (var command = new SqliteCommand("PRAGMA wal_checkpoint(TRUNCATE)", connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        
            Debug.WriteLine($"✓ Сохранено VoltageAtVSPs: {voltages.Count} записей");
        }
        
        public void SaveEfficiencyTables(List<EfficiencyTable> tables)
        {
            if (tables == null || tables.Count == 0)
            {
                Debug.WriteLine("SaveEfficiencyTables: список пуст");
                return;
            }
        
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
        
                using (var transaction = connection.BeginTransaction())
                {
                    var command = new SqliteCommand();
                    command.Connection = connection;
                    command.Transaction = transaction;
        
                    command.CommandText = @"INSERT OR REPLACE INTO ""EfficiencyTable"" 
                        (VCP_NodeNumber, PriorityRPCDNumber, ControlledRPCD_NodeNumber, ReactorNumber, GeneratorNodeNumber)
                        VALUES (@VCPNode, @Priority, @ControlledNode, @Reactor, @Generator)";
        
                    command.Parameters.Add(new SqliteParameter("@VCPNode", SqliteType.Integer));
                    command.Parameters.Add(new SqliteParameter("@Priority", SqliteType.Integer));
                    command.Parameters.Add(new SqliteParameter("@ControlledNode", SqliteType.Integer));
                    command.Parameters.Add(new SqliteParameter("@Reactor", SqliteType.Integer));
                    command.Parameters.Add(new SqliteParameter("@Generator", SqliteType.Integer));
        
                    foreach (var table in tables)
                    {
                        command.Parameters["@VCPNode"].Value = table.VCP_NodeNumber;
                        command.Parameters["@Priority"].Value = table.PriorityRPCDNumber;
                        command.Parameters["@ControlledNode"].Value = table.ControlledRPCD_NodeNumber;
                        command.Parameters["@Reactor"].Value = table.ReactorNumber;
                        command.Parameters["@Generator"].Value = table.GeneratorNodeNumber;
                        command.ExecuteNonQuery();
                    }
        
                    transaction.Commit();
                }
        
                using (var command = new SqliteCommand("PRAGMA wal_checkpoint(TRUNCATE)", connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        
            Debug.WriteLine($"✓ Сохранено EfficiencyTables: {tables.Count} записей");
        }

        public List<AdjustableRPCD> GetAdjustableRPCDs()
        {
            var list = new List<AdjustableRPCD>();
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqliteCommand("SELECT * FROM \"Adjustable RPCDs\"", connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new AdjustableRPCD
                        {
                            ControlledRPCD_NodeNumber = reader.GetInt32(0),
                            ReactivePower = reader.GetDouble(1),
                            Name = reader.IsDBNull(2) 
                                ? "" 
                                : reader.GetString(2),
                            MinReactivePower = reader.GetDouble(3),
                            MaxReactivePower = reader.GetDouble(4),
                            OnOff = reader.GetInt32(5) != 0,
                            Hour = reader.GetInt32(6),
                            VoltageAtNode = reader.GetDouble(7)
                        });
                    }
                }
            }
            return list;
        }

        public List<Generator> GetGenerators()
        {
            var list = new List<Generator>();
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqliteCommand("SELECT * FROM \"Generators\"", connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Generator
                        {
                            GeneratorNodeNumber = reader.GetInt32(0),
                            BusVoltage = reader.GetDouble(1),
                            PQDiagramNumber = reader.GetInt32(2),
                            Name = reader.IsDBNull(3) 
                                ? "" 
                                : reader.GetString(3),
                            Hour = reader.GetInt32(4),
                            OnOff = reader.GetInt32(5) != 0
                        });
                    }
                }
            }
            return list;
        }

        public List<PQDiagram> GetPQDiagrams()
        {
            var list = new List<PQDiagram>();
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqliteCommand("SELECT * FROM \"PQ-Diagrams\"", connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new PQDiagram
                        {
                            PQDiagramNumber = reader.GetInt32(0),
                            ActivePower = reader.GetDouble(1),
                            MaxReactivePower = reader.GetDouble(2),
                            MinReactivePower = reader.GetDouble(3)
                        });
                    }
                }
            }
            return list;
        }

        public List<SwitchedRPCD> GetSwitchedRPCDs()
        {
            var list = new List<SwitchedRPCD>();
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqliteCommand("SELECT * FROM \"SwitchedRPCDs\"", connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new SwitchedRPCD
                        {
                            ReactorNumber = reader.GetInt32(0),
                            Name = reader.IsDBNull(1) 
                                ? "" 
                                : reader.GetString(1),
                            Capacity = reader.GetDouble(2),
                            OnOff = reader.GetInt32(3) != 0,
                            Hour = reader.GetInt32(4)
                        });
                    }
                }
            }
            return list;
        }

        public List<VoltageAtVSP> GetVoltageAtVSPs()
        {
            var list = new List<VoltageAtVSP>();
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqliteCommand("SELECT * FROM \"VoltageAtVSPs\"", connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new VoltageAtVSP
                        {
                            VCP_NodeNumber = reader.GetInt32(0),
                            Voltage = reader.GetDouble(1),
                            MinVoltage = reader.GetInt32(2),
                            MaxVoltage = reader.GetInt32(3),
                            Hour = reader.GetInt32(4)
                        });
                    }
                }
            }
            return list;
        }

        public List<EfficiencyTable> GetEfficiencyTables()
        {
            var list = new List<EfficiencyTable>();
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqliteCommand("SELECT * FROM \"EfficiencyTable\"", connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new EfficiencyTable
                        {
                            VCP_NodeNumber = reader.GetInt32(0),
                            PriorityRPCDNumber = reader.GetInt32(1),
                            ControlledRPCD_NodeNumber = reader.GetInt32(2),
                            ReactorNumber = reader.GetInt32(3),
                            GeneratorNodeNumber = reader.GetInt32(4)
                        });
                    }
                }
            }
            return list;
        }

        public Dictionary<string, int> GetTablesStatistics()
        {
            var stats = new Dictionary<string, int>();
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                var tables = new[] {
                    "\"Adjustable RPCDs\"",
                    "\"Generators\"",
                    "\"PQ-Diagrams\"",
                    "\"SwitchedRPCDs\"",
                    "\"VoltageAtVSPs\"",
                    "\"EfficiencyTable\""
                };

                foreach (var table in tables)
                {
                    try
                    {
                        using (var command = new SqliteCommand($"SELECT COUNT(*) FROM {table}", connection))
                        {
                            string tableName = table.Replace("\"", "");
                            int count = Convert.ToInt32(command.ExecuteScalar());
                            stats[tableName] = count;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Ошибка при получении статистики {table}: {ex.Message}");
                        stats[table.Replace("\"", "")] = 0;
                    }
                }
            }
            return stats;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
                _disposed = true;
            }
        }


        public void UpdateVoltageAtVSP(int nodeNumber, double voltage, int hour)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                var command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = @"UPDATE ""VoltageAtVSPs"" 
                    SET Voltage = @Voltage 
                    WHERE VCP_NodeNumber = @NodeNum AND Hour = @Hour";

                command.Parameters.Add(new SqliteParameter("@NodeNum", SqliteType.Integer));
                command.Parameters.Add(new SqliteParameter("@Voltage", SqliteType.Real));
                command.Parameters.Add(new SqliteParameter("@Hour", SqliteType.Integer));

                command.Parameters["@NodeNum"].Value = nodeNumber;
                command.Parameters["@Voltage"].Value = voltage;
                command.Parameters["@Hour"].Value = hour;

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    Debug.WriteLine($"⚠️ Не найдена запись для обновления: Node={nodeNumber}, Hour={hour}");
                }
            }
        }

        public void UpdateVoltages(List<VoltageAtVSP> voltages)
        {
            if (voltages == null || voltages.Count == 0)
            {
                Debug.WriteLine("UpdateVoltages: список пуст");
                return;
            }
        
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
        
                using (var transaction = connection.BeginTransaction())
                {
                    var command = new SqliteCommand();
                    command.Connection = connection;
                    command.Transaction = transaction;
                    command.CommandText = @"UPDATE ""VoltageAtVSPs"" 
                SET Voltage = @Voltage 
                WHERE VCP_NodeNumber = @NodeNum AND Hour = @Hour";
        
                    command.Parameters.Add(new SqliteParameter("@NodeNum", SqliteType.Integer));
                    command.Parameters.Add(new SqliteParameter("@Voltage", SqliteType.Real));
                    command.Parameters.Add(new SqliteParameter("@Hour", SqliteType.Integer));
        
                    foreach (var voltage in voltages)
                    {
                        command.Parameters["@NodeNum"].Value = voltage.VCP_NodeNumber;
                        command.Parameters["@Voltage"].Value = voltage.Voltage;
                        command.Parameters["@Hour"].Value = voltage.Hour;
                        command.ExecuteNonQuery();
                    }
        
                    transaction.Commit();
                }
        
                using (var command = new SqliteCommand("PRAGMA wal_checkpoint(TRUNCATE)", connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        
            Debug.WriteLine($"✓ Обновлено VoltageAtVSPs: {voltages.Count} записей");
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}