using Model.Models;
using System.Data.Entity.Validation;
using System.Diagnostics;

namespace Model
{
    public class DataBaseRepository : IDisposable
    {
        private readonly AppDbContext _context;
        private bool _disposed = false;

        public DataBaseRepository()
        {
            _context = new AppDbContext();
        }

        public void ClearAllTables()
        {
            try
            {
                Debug.WriteLine("Очистка таблиц...");

                // Очищаем таблицы в правильном порядке (сначала зависимые)
                _context.Database.ExecuteSqlCommand("DELETE FROM \"VoltageAtVSPs\"");
                _context.Database.ExecuteSqlCommand("DELETE FROM \"EfficiencyTable\"");
                _context.Database.ExecuteSqlCommand("DELETE FROM \"SwitchedRPCDs\"");
                _context.Database.ExecuteSqlCommand("DELETE FROM \"Generators\"");
                _context.Database.ExecuteSqlCommand("DELETE FROM \"PQ-Diagrams\"");
                _context.Database.ExecuteSqlCommand("DELETE FROM \"Adjustable RPCDs\"");

                _context.SaveChanges();
                Debug.WriteLine("Таблицы очищены");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при очистке таблиц: {ex.Message}");
            }
        }

        public void SaveAdjustableRPCDs(List<AdjustableRPCD> rpcds)
        {
            if (rpcds == null || rpcds.Count == 0)
            {
                Debug.WriteLine("SaveAdjustableRPCDs: список пуст");
                return;
            }

            try
            {
                Debug.WriteLine($"SaveAdjustableRPCDs: начинаем добавление {rpcds.Count} записей...");

                // Проверяем первую запись
                var first = rpcds.First();
                Debug.WriteLine($"Пример первой записи: Node={first.ControlledRPCD_NodeNumber}, " +
                               $"Hour={first.Hour}, Voltage={first.VoltageAtNode}");

                foreach (var rpcd in rpcds)
                {
                    var adjustableRpcd = new AdjustableRPCD
                    {
                        ControlledRPCD_NodeNumber = rpcd.ControlledRPCD_NodeNumber,
                        Hour = rpcd.Hour,
                        ReactivePower = rpcd.ReactivePower,
                        Name = rpcd.Name ?? "",
                        MinReactivePower = rpcd.MinReactivePower,
                        MaxReactivePower = rpcd.MaxReactivePower,
                        OnOff = rpcd.OnOff,
                        VoltageAtNode = rpcd.VoltageAtNode
                    };

                    _context.AdjustableRPCDs.Add(adjustableRpcd);
                }

                Debug.WriteLine("SaveAdjustableRPCDs: вызываем SaveChanges()...");
                _context.SaveChanges();
                Debug.WriteLine($"✓ SaveAdjustableRPCDs: успешно сохранено {rpcds.Count} записей");
            }
            catch (DbEntityValidationException dbEx)
            {
                // Специальная обработка ошибок валидации EF
                string validationErrors = "";
                foreach (var validationErrorsPerEntity in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrorsPerEntity.ValidationErrors)
                    {
                        validationErrors += $"Property: {validationError.PropertyName} " +
                                          $"Error: {validationError.ErrorMessage}\n";
                    }
                }

                Debug.WriteLine($"SaveAdjustableRPCDs: ошибка валидации EF:\n{validationErrors}");
                throw new Exception($"Ошибка валидации данных:\n{validationErrors}", dbEx);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SaveAdjustableRPCDs: общая ошибка - {ex.Message}");
                if (ex.InnerException != null)
                    Debug.WriteLine($"  Inner: {ex.InnerException.Message}");
                throw;
            }
        }

        public void SaveGenerators(List<Generator> generators)
        {
            if (generators == null || generators.Count == 0)
            {
                Debug.WriteLine("SaveGenerators: список пуст");
                return;
            }

            try
            {
                Debug.WriteLine($"SaveGenerators: добавление {generators.Count} записей");

                foreach (var gen in generators)
                {
                    // Создаем объект
                    var generator = new Generator
                    {
                        GeneratorNodeNumber = gen.GeneratorNodeNumber,
                        Hour = gen.Hour,
                        BusVoltage = gen.BusVoltage,
                        PQDiagramNumber = gen.PQDiagramNumber,
                        Name = gen.Name ?? "",
                        OnOff = gen.OnOff
                    };

                    // Добавляем объект через контекст
                    _context.Generators.Add(generator);
                }

                // Сохраняем изменения в БД
                _context.SaveChanges();
                Debug.WriteLine($"SaveGenerators: успешно сохранено {generators.Count} записей");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SaveGenerators: ошибка - {ex.Message}");
                throw;
            }
        }

        public void SavePQDiagrams(List<PQDiagram> diagrams)
        {
            if (diagrams == null || diagrams.Count == 0)
            {
                Debug.WriteLine("SavePQDiagrams: список пуст");
                return;
            }

            try
            {
                Debug.WriteLine($"SavePQDiagrams: добавление {diagrams.Count} записей");

                foreach (var diag in diagrams)
                {
                    // Создаем объект
                    var pqDiagram = new PQDiagram
                    {
                        PQDiagramNumber = diag.PQDiagramNumber,
                        ActivePower = diag.ActivePower,
                        MaxReactivePower = diag.MaxReactivePower,
                        MinReactivePower = diag.MinReactivePower
                    };

                    // Добавляем объект через контекст
                    _context.PQDiagrams.Add(pqDiagram);
                }

                // Сохраняем изменения в БД
                _context.SaveChanges();
                Debug.WriteLine($"SavePQDiagrams: успешно сохранено {diagrams.Count} записей");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SavePQDiagrams: ошибка - {ex.Message}");
                throw;
            }
        }

        public void SaveSwitchedRPCDs(List<SwitchedRPCD> rpcds)
        {
            if (rpcds == null || rpcds.Count == 0)
            {
                Debug.WriteLine("SaveSwitchedRPCDs: список пуст");
                return;
            }

            try
            {
                Debug.WriteLine($"SaveSwitchedRPCDs: добавление {rpcds.Count} записей");

                foreach (var rpcd in rpcds)
                {
                    // Создаем объект
                    var switchedRpcd = new SwitchedRPCD
                    {
                        ReactorNumber = rpcd.ReactorNumber,
                        Hour = rpcd.Hour,
                        Name = rpcd.Name ?? "",
                        Capacity = rpcd.Capacity,
                        OnOff = rpcd.OnOff
                    };

                    // Добавляем объект через контекст
                    _context.SwitchedRPCDs.Add(switchedRpcd);
                }

                // Сохраняем изменения в БД
                _context.SaveChanges();
                Debug.WriteLine($"SaveSwitchedRPCDs: успешно сохранено {rpcds.Count} записей");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SaveSwitchedRPCDs: ошибка - {ex.Message}");
                throw;
            }
        }

        public void SaveVoltageAtVSPs(List<VoltageAtVSP> voltages)
        {
            if (voltages == null || voltages.Count == 0)
            {
                Debug.WriteLine("SaveVoltageAtVSPs: список пуст");
                return;
            }

            try
            {
                Debug.WriteLine($"SaveVoltageAtVSPs: добавление {voltages.Count} записей");

                foreach (var voltage in voltages)
                {
                    // Создаем объект
                    var voltageAtVsp = new VoltageAtVSP
                    {
                        VCP_NodeNumber = voltage.VCP_NodeNumber,
                        Hour = voltage.Hour,
                        Voltage = voltage.Voltage,
                        MinVoltage = voltage.MinVoltage,
                        MaxVoltage = voltage.MaxVoltage
                    };

                    // Добавляем объект через контекст
                    _context.VoltageAtVSPs.Add(voltageAtVsp);
                }

                // Сохраняем изменения в БД
                _context.SaveChanges();
                Debug.WriteLine($"SaveVoltageAtVSPs: успешно сохранено {voltages.Count} записей");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SaveVoltageAtVSPs: ошибка - {ex.Message}");
                throw;
            }
        }

        public void SaveEfficiencyTables(List<EfficiencyTable> tables)
        {
            if (tables == null || tables.Count == 0)
            {
                Debug.WriteLine("SaveEfficiencyTables: список пуст");
                return;
            }

            try
            {
                Debug.WriteLine($"SaveEfficiencyTables: добавление {tables.Count} записей");

                foreach (var table in tables)
                {
                    // Создаем объект
                    var efficiencyTable = new EfficiencyTable
                    {
                        VCP_NodeNumber = table.VCP_NodeNumber,
                        PriorityRPCDNumber = table.PriorityRPCDNumber,
                        ControlledRPCD_NodeNumber = table.ControlledRPCD_NodeNumber,
                        ReactorNumber = table.ReactorNumber,
                        GeneratorNodeNumber = table.GeneratorNodeNumber
                    };

                    // Добавляем объект через контекст
                    _context.EfficiencyTables.Add(efficiencyTable);
                }

                // Сохраняем изменения в БД
                _context.SaveChanges();
                Debug.WriteLine($"SaveEfficiencyTables: успешно сохранено {tables.Count} записей");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SaveEfficiencyTables: ошибка - {ex.Message}");
                throw;
            }
        }

        // Методы чтения данных
        public List<AdjustableRPCD> GetAdjustableRPCDs()
        {
            try
            {
                return _context.AdjustableRPCDs.ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetAdjustableRPCDs: ошибка - {ex.Message}");
                return new List<AdjustableRPCD>();
            }
        }

        public List<Generator> GetGenerators()
        {
            try
            {
                return _context.Generators.ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetGenerators: ошибка - {ex.Message}");
                return new List<Generator>();
            }
        }

        public List<PQDiagram> GetPQDiagrams()
        {
            try
            {
                return _context.PQDiagrams.ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetPQDiagrams: ошибка - {ex.Message}");
                return new List<PQDiagram>();
            }
        }

        public List<SwitchedRPCD> GetSwitchedRPCDs()
        {
            try
            {
                return _context.SwitchedRPCDs.ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetSwitchedRPCDs: ошибка - {ex.Message}");
                return new List<SwitchedRPCD>();
            }
        }

        public List<VoltageAtVSP> GetVoltageAtVSPs()
        {
            try
            {
                return _context.VoltageAtVSPs.ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetVoltageAtVSPs: ошибка - {ex.Message}");
                return new List<VoltageAtVSP>();
            }
        }

        public List<EfficiencyTable> GetEfficiencyTables()
        {
            try
            {
                return _context.EfficiencyTables.ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetEfficiencyTables: ошибка - {ex.Message}");
                return new List<EfficiencyTable>();
            }
        }

        public Dictionary<string, int> GetTablesStatistics()
        {
            var stats = new Dictionary<string, int>();
            try
            {
                stats["Adjustable RPCDs"] = _context.AdjustableRPCDs.Count();
                stats["Generators"] = _context.Generators.Count();
                stats["PQ-Diagrams"] = _context.PQDiagrams.Count();
                stats["SwitchedRPCDs"] = _context.SwitchedRPCDs.Count();
                stats["VoltageAtVSPs"] = _context.VoltageAtVSPs.Count();
                stats["EfficiencyTable"] = _context.EfficiencyTables.Count();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetTablesStatistics: ошибка - {ex.Message}");
            }
            return stats;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context?.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}