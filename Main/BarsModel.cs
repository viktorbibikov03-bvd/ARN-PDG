using ASTRALib;
using Model.Models;
using System.Data;
using System.Diagnostics;

namespace Model
{
    public class BarsModel
    {
        private IRastr _rastr;
        private DataTable _voltageTable;
        private bool _isLoaded;
        private bool _isCalculated;

        public BarsModel()
        {
            _rastr = new Rastr();
            _voltageTable = new DataTable("NodeVoltages");
            _isLoaded = false;
            _isCalculated = false;
        }

        public bool IsLoaded => _isLoaded;
        public bool IsCalculated => _isCalculated;

        public void LoadFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Файл не найден: {filePath}");
            }

            _rastr.Load(RG_KOD.RG_REPL, filePath, "");
            _isLoaded = true;
            _isCalculated = false;
        }

        public List<int> GetNodeNumbers()
        {
            if (!_isLoaded)
            {
                throw new InvalidOperationException("Сначала загрузите файл мегаточки");
            }

            List<int> nodeNumbers = new List<int>();
            ITable nodeTable = (ITable)_rastr.Tables.Item("node");
            ICol nodeNumber = (ICol)nodeTable.Cols.Item("ny");
            int nodeCount = nodeTable.Count;

            for (int i = 0; i < nodeCount; i++)
            {
                int num = Convert.ToInt32(nodeNumber.get_ZN(i));
                nodeNumbers.Add(num);
            }

            return nodeNumbers;
        }

        public void CalculateHourlyRegime()
        {
            if (!_isLoaded)
            {
                throw new InvalidOperationException("Сначала загрузите файл мегаточки");
            }

            int hoursCount = 24;

            ITable nodeTable = (ITable)_rastr.Tables.Item("node");
            ICol nodeNumber = (ICol)nodeTable.Cols.Item("ny");
            ICol nodeU = (ICol)nodeTable.Cols.Item("vras");
            int nodeCount = nodeTable.Count;

            _voltageTable.Clear();
            _voltageTable.Columns.Clear();

            _voltageTable.Columns.Add("Hour", typeof(int));

            for (int i = 0; i < nodeCount; i++)
            {
                int num = Convert.ToInt32(nodeNumber.get_ZN(i));
                string columnName = $"Node_{num}";
                _voltageTable.Columns.Add(columnName, typeof(double));
            }

            for (int hour = 1; hour <= hoursCount; hour++)
            {
                RastrRetCode retCode = RastrRetCode.AST_OK;
                _rastr.RgmMpt(hour, hour, ref retCode);

                DataRow row = _voltageTable.NewRow();
                row["Hour"] = hour;

                for (int i = 0; i < nodeCount; i++)
                {
                    double vras = Convert.ToDouble(nodeU.get_ZN(i));
                    int num = Convert.ToInt32(nodeNumber.get_ZN(i));
                    string columnName = $"Node_{num}";
                    row[columnName] = vras;
                }

                _voltageTable.Rows.Add(row);
            }

            _isCalculated = true;
        }

        public Dictionary<int, double> GetNodeVoltages(int nodeNumber)
        {
            if (!_isCalculated)
            {
                throw new InvalidOperationException("Сначала выполните расчет режима");
            }

            Dictionary<int, double> voltages = new Dictionary<int, double>();
            string columnName = $"Node_{nodeNumber}";

            if (!_voltageTable.Columns.Contains(columnName))
            {
                throw new ArgumentException($"Узел {nodeNumber} не найден в результатах");
            }

            foreach (DataRow row in _voltageTable.Rows)
            {
                int hour = Convert.ToInt32(row["Hour"]);
                if (row[columnName] != DBNull.Value)
                {
                    double voltage = Convert.ToDouble(row[columnName]);
                    voltages[hour] = voltage;
                }
            }

            return voltages;
        }

        public void ExportToCsv(string filePath)
        {
            if (!_isCalculated)
            {
                throw new InvalidOperationException("Сначала выполните расчет режима");
            }

            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            ITable nodeTable = (ITable)_rastr.Tables.Item("node");
            ICol nodeNumber = (ICol)nodeTable.Cols.Item("ny");

            sb.Append("Час;");
            for (int i = 0; i < nodeTable.Count; i++)
            {
                int num = Convert.ToInt32(nodeNumber.get_ZN(i));
                sb.Append($"U{num}");
                if (i < nodeTable.Count - 1)
                {
                    sb.Append(";");
                }
            }
            sb.AppendLine();

            foreach (DataRow row in _voltageTable.Rows)
            {
                sb.Append(row["Hour"].ToString());

                for (int i = 0; i < nodeTable.Count; i++)
                {
                    int num = Convert.ToInt32(nodeNumber.get_ZN(i));
                    string columnName = $"Node_{num}";

                    sb.Append(";");
                    if (row[columnName] != DBNull.Value)
                    {
                        double voltage = Convert.ToDouble(row[columnName]);
                        sb.Append(voltage.ToString("F3", System.Globalization.CultureInfo.InvariantCulture));
                    }
                }
                sb.AppendLine();
            }

            File.WriteAllText(filePath, sb.ToString(), new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
        }

        public List<AdjustableRPCD> GetAdjustableRPCDsData()
        {
            List<AdjustableRPCD> rpcds = new List<AdjustableRPCD>();

            if (!_isLoaded)
            {
                return rpcds;
            }

            try
            {
                ITable nodeTable = (ITable)_rastr.Tables.Item("node");

                ICol nodeNumber = (ICol)nodeTable.Cols.Item("ny");
                ICol nodeName = (ICol)nodeTable.Cols.Item("name");
                ICol nodeQg = (ICol)nodeTable.Cols.Item("qg");
                ICol nodeQmin = (ICol)nodeTable.Cols.Item("qmin");
                ICol nodeQmax = (ICol)nodeTable.Cols.Item("qmax");
                ICol nodeVras = (ICol)nodeTable.Cols.Item("vras");

                int nodeCount = nodeTable.Count;

                for (int hour = 1; hour <= 24; hour++)
                {
                    RastrRetCode retCode = RastrRetCode.AST_OK;
                    _rastr.RgmMpt(hour, hour, ref retCode);

                    for (int i = 0; i < nodeCount; i++)
                    {
                        try
                        {
                            int nodeNum = Convert.ToInt32(nodeNumber.get_ZN(i));
                            string name = nodeName.get_ZN(i)?.ToString() ?? "";
                            double qg = 0.0;
                            double qmin = 0.0;
                            double qmax = 0.0;
                            double vras = 0.0;

                            try 
                            { 
                                qg = Convert.ToDouble(nodeQg.get_ZN(i)); 
                            } 
                            catch { }
                            try 
                            { 
                                qmin = Convert.ToDouble(nodeQmin.get_ZN(i)); 
                            } 
                            catch { }
                            try 
                            { 
                                qmax = Convert.ToDouble(nodeQmax.get_ZN(i)); 
                            } 
                            catch { }
                            try 
                            { 
                                vras = Convert.ToDouble(nodeVras.get_ZN(i)); 
                            } 
                            catch { }

                            bool isOn = (vras != 0.0);

                            rpcds.Add(new AdjustableRPCD
                            {
                                ControlledRPCD_NodeNumber = nodeNum,
                                ReactivePower = qg,
                                Name = name,
                                MinReactivePower = qmin,
                                MaxReactivePower = qmax,
                                OnOff = isOn,
                                Hour = hour,
                                VoltageAtNode = vras
                            });
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Ошибка при обработке узла {i}, час {hour}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при получении Adjustable RPCDs: {ex.Message}");
            }

            return rpcds;
        }

        public List<Generator> GetGeneratorsData()
        {
            List<Generator> generators = new List<Generator>();

            if (!_isLoaded)
            {
                return generators;
            }

            try
            {
                ITable generatorTable = (ITable)_rastr.Tables.Item("Generator");
                ICol genNode = (ICol)generatorTable.Cols.Item("Node");
                ICol genNumPQ = (ICol)generatorTable.Cols.Item("NumPQ");
                ICol genName = (ICol)generatorTable.Cols.Item("Name");

                ITable nodeTable = (ITable)_rastr.Tables.Item("node");
                ICol nodeNy = (ICol)nodeTable.Cols.Item("ny");
                ICol nodeVras = (ICol)nodeTable.Cols.Item("vras");

                int genCount = generatorTable.Count;

                for (int hour = 1; hour <= 24; hour++)
                {
                    RastrRetCode retCode = RastrRetCode.AST_OK;
                    _rastr.RgmMpt(hour, hour, ref retCode);

                    HashSet<int> addedNodes = new HashSet<int>();

                    for (int i = 0; i < genCount; i++)
                    {
                        try
                        {
                            int nodeNumber = Convert.ToInt32(genNode.get_ZN(i));

                            if (addedNodes.Contains(nodeNumber))
                            {
                                continue;
                            }
                            addedNodes.Add(nodeNumber);

                            int pqDiagramNumber = Convert.ToInt32(genNumPQ.get_ZN(i));
                            string name = genName.get_ZN(i)?.ToString() ?? "";

                            double busVoltage = 0.0;
                            for (int j = 0; j < nodeTable.Count; j++)
                            {
                                int nodeNum = Convert.ToInt32(nodeNy.get_ZN(j));
                                if (nodeNum == nodeNumber)
                                {
                                    busVoltage = Convert.ToDouble(nodeVras.get_ZN(j));
                                    break;
                                }
                            }

                            bool isOn = (busVoltage != 0.0);

                            generators.Add(new Generator
                            {
                                GeneratorNodeNumber = nodeNumber,
                                BusVoltage = busVoltage,
                                PQDiagramNumber = pqDiagramNumber,
                                Name = name,
                                Hour = hour,
                                OnOff = isOn
                            });
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Ошибка при обработке генератора {i}, час {hour}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при получении данных генераторов: {ex.Message}");
            }

            return generators;
        }

        public List<PQDiagram> GetPQDiagramsData()
        {
            List<PQDiagram> diagrams = new List<PQDiagram>();

            if (!_isLoaded)
            {
                return diagrams;
            }

            try
            {
                ITable graphik2Table = (ITable)_rastr.Tables.Item("graphik2");
                ICol g2Num = (ICol)graphik2Table.Cols.Item("Num");
                ICol g2P = (ICol)graphik2Table.Cols.Item("P");
                ICol g2Qmax = (ICol)graphik2Table.Cols.Item("Qmax");
                ICol g2Qmin = (ICol)graphik2Table.Cols.Item("Qmin");

                int count = graphik2Table.Count;

                for (int i = 0; i < count; i++)
                {
                    try
                    {
                        int pqDiagramNumber = Convert.ToInt32(g2Num.get_ZN(i));
                        double activePower = Convert.ToDouble(g2P.get_ZN(i));
                        double maxReactivePower = Convert.ToDouble(g2Qmax.get_ZN(i));
                        double minReactivePower = 0.0;

                        try
                        {
                            minReactivePower = Convert.ToDouble(g2Qmin.get_ZN(i));
                        }
                        catch
                        {
                            try
                            {
                                minReactivePower = Convert.ToDouble(g2Qmax.get_ZN(i));
                            }
                            catch { }
                        }

                        diagrams.Add(new PQDiagram
                        {
                            PQDiagramNumber = pqDiagramNumber,
                            ActivePower = activePower,
                            MaxReactivePower = maxReactivePower,
                            MinReactivePower = minReactivePower
                        });
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Ошибка при обработке PQ-диаграммы {i}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при получении данных PQ-диаграмм: {ex.Message}");
            }

            return diagrams;
        }

        public List<SwitchedRPCD> GetSwitchedRPCDsData()
        {
            List<SwitchedRPCD> rpcds = new List<SwitchedRPCD>();

            if (!_isLoaded)
            {
                return rpcds;
            }

            try
            {
                ITable reactorsTable = (ITable)_rastr.Tables.Item("Reactors");
                ICol rId = (ICol)reactorsTable.Cols.Item("Id");
                ICol rName = (ICol)reactorsTable.Cols.Item("Name");
                ICol rQr = (ICol)reactorsTable.Cols.Item("Qr");

                int reactorCount = reactorsTable.Count;

                for (int hour = 1; hour <= 24; hour++)
                {
                    RastrRetCode retCode = RastrRetCode.AST_OK;
                    _rastr.RgmMpt(hour, hour, ref retCode);

                    for (int i = 0; i < reactorCount; i++)
                    {
                        try
                        {
                            int reactorNumber = Convert.ToInt32(rId.get_ZN(i));
                            string name = rName.get_ZN(i)?.ToString() ?? "";
                            double capacity = Convert.ToDouble(rQr.get_ZN(i));
                            bool isOn = (capacity != 0.0);

                            rpcds.Add(new SwitchedRPCD
                            {
                                ReactorNumber = reactorNumber,
                                Name = name,
                                Capacity = capacity,
                                Hour = hour,
                                OnOff = isOn
                            });
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Ошибка при обработке реактора {i}, час {hour}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при получении данных реакторов: {ex.Message}");
            }

            return rpcds;
        }

        public void SaveToDatabase(DataBaseRepository repository)
        {
            if (!_isLoaded)
            {
                throw new InvalidOperationException("Сначала загрузите файл мегаточки");
            }

            try
            {
                //repository.ClearAllTables();

                var pqDiagrams = GetPQDiagramsData();
                Debug.WriteLine($"PQ-диаграмм найдено: {pqDiagrams.Count}");
                if (pqDiagrams.Count > 0)
                {
                    repository.SavePQDiagrams(pqDiagrams);
                    Debug.WriteLine($"✓ Сохранено PQ-диаграмм: {pqDiagrams.Count}");
                }

                var generators = GetGeneratorsData();
                Debug.WriteLine($"Генераторов найдено: {generators.Count}");
                if (generators.Count > 0)
                {
                    repository.SaveGenerators(generators);
                    Debug.WriteLine($"✓ Сохранено записей генераторов: {generators.Count}");
                }

                var switchedRpcs = GetSwitchedRPCDsData();
                Debug.WriteLine($"Реакторов найдено: {switchedRpcs.Count}");
                if (switchedRpcs.Count > 0)
                {
                    repository.SaveSwitchedRPCDs(switchedRpcs);
                    Debug.WriteLine($"✓ Сохранено записей реакторов: {switchedRpcs.Count}");
                }

                var adjustableRPCDs = GetAdjustableRPCDsData();
                Debug.WriteLine($"Adjustable RPCDs найдено: {adjustableRPCDs.Count}");
                if (adjustableRPCDs.Count > 0)
                {
                    repository.SaveAdjustableRPCDs(adjustableRPCDs);
                    Debug.WriteLine($"✓ Сохранено записей Adjustable RPCDs: {adjustableRPCDs.Count}");
                }

                UpdateVoltageInDatabase(repository);

                Debug.WriteLine("=== ВСЕ ДАННЫЕ УСПЕШНО СОХРАНЕНЫ ===");
            }
            catch (Exception ex)
            {
                string errorMsg = $"Ошибка при сохранении в БД: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMsg += $"\n\nВнутренняя ошибка: {ex.InnerException.Message}";
                    if (ex.InnerException.InnerException != null)
                    {
                        errorMsg += $"\nДетали: {ex.InnerException.InnerException.Message}";
                    }
                }

                Debug.WriteLine(errorMsg);
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");

                throw new Exception(errorMsg, ex);
            }
        }


        public List<VoltageAtVSP> GetVoltageAtVSPsData()
        {
            List<VoltageAtVSP> voltages = new List<VoltageAtVSP>();
        
            if (!_isLoaded)
            {
                return voltages;
            }
        
            try
            {
                ITable nodeTable = (ITable)_rastr.Tables.Item("node");
                ICol nodeNy = (ICol)nodeTable.Cols.Item("ny");
                ICol nodeVras = (ICol)nodeTable.Cols.Item("vras");
        
                int nodeCount = nodeTable.Count;
        
                for (int hour = 1; hour <= 24; hour++)
                {
                    RastrRetCode retCode = RastrRetCode.AST_OK;
                    _rastr.RgmMpt(hour, hour, ref retCode);
        
                    for (int i = 0; i < nodeCount; i++)
                    {
                        try
                        {
                            int nodeNumber = Convert.ToInt32(nodeNy.get_ZN(i));
                            double voltage = 0.0;
        
                            try
                            {
                                voltage = Convert.ToDouble(nodeVras.get_ZN(i));
                            }
                            catch { }
        
        
                            int minVoltage = 500;
                            int maxVoltage = 525;
        
                            voltages.Add(new VoltageAtVSP
                            {
                                VCP_NodeNumber = nodeNumber,
                                Voltage = voltage,
                                MinVoltage = minVoltage,
                                MaxVoltage = maxVoltage,
                                Hour = hour
                            });
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Ошибка при обработке узла {i}, час {hour}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при получении данных VoltageAtVSPs: {ex.Message}");
            }
        
            return voltages;
        }

        public void UpdateVoltageInDatabase(DataBaseRepository repository)
        {
            if (!_isLoaded)
            {
                throw new InvalidOperationException("Сначала загрузите файл мегаточки");
            }

            try
            {
                Debug.WriteLine("=== Обновление напряжений в VoltageAtVSPs ===");

                ITable nodeTable = (ITable)_rastr.Tables.Item("node");
                ICol nodeNy = (ICol)nodeTable.Cols.Item("ny");
                ICol nodeVras = (ICol)nodeTable.Cols.Item("vras");

                int nodeCount = nodeTable.Count;
                int updatedCount = 0;

                for (int hour = 1; hour <= 24; hour++)
                {
                    RastrRetCode retCode = RastrRetCode.AST_OK;
                    _rastr.RgmMpt(hour, hour, ref retCode);

                    for (int i = 0; i < nodeCount; i++)
                    {
                        try
                        {
                            int nodeNumber = Convert.ToInt32(nodeNy.get_ZN(i));
                            double voltage = 0.0;

                            try
                            {
                                voltage = Convert.ToDouble(nodeVras.get_ZN(i));
                            }
                            catch { }

                            repository.UpdateVoltageAtVSP(nodeNumber, voltage, hour);
                            updatedCount++;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Ошибка при обновлении узла {i}, час {hour}: {ex.Message}");
                        }
                    }
                }

                Debug.WriteLine($"✓ Обновлено напряжений: {updatedCount} записей");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при обновлении напряжений: {ex.Message}");
                throw;
            }
        }

        public void Dispose()
        {
            if (_rastr != null)
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(_rastr);
                _rastr = null;
            }
        }
    }
}