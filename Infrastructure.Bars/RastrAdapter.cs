using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

using ASTRALib;

using VrdoPdg.Model.Regime;

namespace VrdoPdg.Infrastructure.Bars
{
    /// <summary>
    /// Модуль взаимодействия с ПО «Барс» через интерфейс IRastr библиотеки ASTRALib.dll:
    /// открывает МТ, считывает таблицы «Узлы», «Реакторы» и «Генераторы УР»
    /// по часам, изменяет состояние и загрузку СКРМ и выполняет расчет установившегося режима.
    /// </summary>
    /// <remarks>
    /// Объект ASTRALib создается при открытии МТ. Все методы модуля должны вызываться
    /// из одного потока с однопоточным подразделением (STA), в котором открыта МТ.
    /// </remarks>
    [SupportedOSPlatform("windows")]
    public sealed class RastrAdapter : IRastrAdapter
    {
        /// <summary>
        /// Таблица «Узлы».
        /// </summary>
        private const string NodeTableName = "node";

        /// <summary>
        /// Таблица «Реакторы».
        /// </summary>
        private const string ReactorTableName = "Reactors";

        /// <summary>
        /// Таблица «Генераторы для УР, динамики, оптимизации».
        /// </summary>
        private const string GeneratorTableName = "Generator";

        /// <summary>
        /// Изменения параметров МТ после выбора часа; каждое изменение — набор ячеек.
        /// </summary>
        private readonly Stack<List<CellValue>> _changes = new Stack<List<CellValue>>();

        /// <summary>
        /// Объект ASTRALib с открытой МТ.
        /// </summary>
        private IRastr? _rastr;

        /// <summary>
        /// Таблица «Узлы» текущего часа.
        /// </summary>
        private RastrTable? _nodes;

        /// <summary>
        /// Таблица «Реакторы» текущего часа.
        /// </summary>
        private RastrTable? _reactors;

        /// <summary>
        /// Таблица «Генераторы УР» текущего часа.
        /// </summary>
        private RastrTable? _generators;

        /// <inheritdoc/>
        public void Open(string megapointPath)
        {
            Close();
            IRastr rastr = new Rastr();
            try
            {
                rastr.Load(RG_KOD.RG_REPL, megapointPath, string.Empty);
            }
            catch
            {
                ReleaseComObject(rastr);
                throw;
            }

            _rastr = rastr;
            RefreshTables();
        }

        /// <inheritdoc/>
        public MegapointCatalog ReadCatalog()
        {
            RastrTable nodes = GetNodes();
            Dictionary<int, string> nodeNames = new Dictionary<int, string>();
            foreach (KeyValuePair<int, int> pair in nodes.Rows)
            {
                nodeNames[pair.Key] = nodes.ReadString("name", pair.Value);
            }

            Dictionary<int, string> reactorNames = new Dictionary<int, string>();
            if (_reactors != null)
            {
                foreach (KeyValuePair<int, int> pair in _reactors.Rows)
                {
                    reactorNames[pair.Key] = _reactors.ReadString("Name", pair.Value);
                }
            }

            List<MegapointGenerator> generators = new List<MegapointGenerator>();
            if (_generators != null)
            {
                foreach (KeyValuePair<int, int> pair in _generators.Rows)
                {
                    generators.Add(new MegapointGenerator(
                        pair.Key,
                        _generators.ReadInteger("Node", pair.Value),
                        _generators.ReadString("Name", pair.Value)));
                }
            }

            return new MegapointCatalog(nodeNames, reactorNames, generators);
        }

        /// <inheritdoc/>
        public bool SelectHour(int hour)
        {
            IRastr rastr = GetRastr();
            _changes.Clear();
            RastrRetCode retCode = RastrRetCode.AST_OK;
            rastr.RgmMpt(hour, hour, ref retCode);
            RefreshTables();
            return retCode == RastrRetCode.AST_OK;
        }

        /// <inheritdoc/>
        public bool CalculateRegime()
        {
            RastrRetCode retCode = GetRastr().rgm(string.Empty);
            return retCode == RastrRetCode.AST_OK;
        }

        /// <inheritdoc/>
        public double? GetNodeVoltage(int nodeNumber)
        {
            RastrTable nodes = GetNodes();
            int row;
            if (!nodes.TryFindRow(nodeNumber, out row) || nodes.ReadBoolean("sta", row))
            {
                return null;
            }

            return nodes.ReadDouble("vras", row);
        }

        /// <inheritdoc/>
        public NodeState? GetNodeState(int nodeNumber)
        {
            RastrTable nodes = GetNodes();
            int row;
            if (!nodes.TryFindRow(nodeNumber, out row))
            {
                return null;
            }

            bool isOn = !nodes.ReadBoolean("sta", row);
            return new NodeState(
                isOn,
                isOn ? nodes.ReadDouble("vras", row) : 0,
                nodes.ReadDouble("vzd", row),
                nodes.ReadDouble("qmin", row));
        }

        /// <inheritdoc/>
        public ControlledDeviceState? GetControlledDevice(int nodeNumber)
        {
            RastrTable nodes = GetNodes();
            int row;
            if (!nodes.TryFindRow(nodeNumber, out row))
            {
                return null;
            }

            double minReactivePower = nodes.ReadDouble("qmin", row);
            double maxReactivePower = nodes.ReadDouble("qmax", row);
            bool isOff = nodes.ReadBoolean("sta", row) || (minReactivePower == 0 && maxReactivePower == 0);
            return new ControlledDeviceState(
                !isOff,
                nodes.ReadDouble("qg", row),
                minReactivePower,
                maxReactivePower,
                nodes.ReadDouble("vzd", row));
        }

        /// <inheritdoc/>
        public SwitchedDeviceState? GetSwitchedDevice(int reactorNumber)
        {
            int row;
            if (_reactors == null || !_reactors.TryFindRow(reactorNumber, out row))
            {
                return null;
            }

            return new SwitchedDeviceState(!_reactors.ReadBoolean("sta", row), _reactors.ReadDouble("Qr", row));
        }

        /// <inheritdoc/>
        public GeneratorState? GetGenerator(int generatorNumber)
        {
            RastrTable nodes = GetNodes();
            int row;
            if (_generators == null || !_generators.TryFindRow(generatorNumber, out row))
            {
                return null;
            }

            int nodeNumber = _generators.ReadInteger("Node", row);
            int nodeRow;
            if (!nodes.TryFindRow(nodeNumber, out nodeRow))
            {
                return null;
            }

            bool isOn = !_generators.ReadBoolean("sta", row) && !nodes.ReadBoolean("sta", nodeRow);
            return new GeneratorState(
                isOn,
                nodeNumber,
                _generators.ReadDouble("P", row),
                nodes.ReadDouble("qg", nodeRow),
                nodes.ReadDouble("vzd", nodeRow),
                nodes.ReadDouble("qmin", nodeRow),
                nodes.ReadDouble("qmax", nodeRow));
        }

        /// <inheritdoc/>
        public void SetControlledDeviceReactivePower(int nodeNumber, double reactivePower)
        {
            RastrTable nodes = GetNodes();
            int row = FindRow(nodes, nodeNumber, "Узел");
            List<CellValue> change = new List<CellValue>();
            SetValue(change, nodes.GetColumn("qmin"), row, reactivePower);
            SetValue(change, nodes.GetColumn("qmax"), row, reactivePower);
            _changes.Push(change);
        }

        /// <inheritdoc/>
        public void SetSwitchedDeviceState(int reactorNumber, bool isOn)
        {
            if (_reactors == null)
            {
                throw new InvalidOperationException("В МТ нет таблицы «Реакторы»");
            }

            int row = FindRow(_reactors, reactorNumber, "Реактор");
            List<CellValue> change = new List<CellValue>();
            SetValue(change, _reactors.GetColumn("sta"), row, !isOn);
            _changes.Push(change);
        }

        /// <inheritdoc/>
        public void SetNodeSetVoltage(int nodeNumber, double voltage)
        {
            RastrTable nodes = GetNodes();
            int row = FindRow(nodes, nodeNumber, "Узел");
            List<CellValue> change = new List<CellValue>();
            SetValue(change, nodes.GetColumn("vzd"), row, voltage);
            _changes.Push(change);
        }

        /// <inheritdoc/>
        public void UndoLastChange()
        {
            if (_changes.Count > 0)
            {
                Restore(_changes.Pop());
            }
        }

        /// <inheritdoc/>
        public void UndoAllChanges()
        {
            while (_changes.Count > 0)
            {
                Restore(_changes.Pop());
            }
        }

        /// <summary>
        /// Закрывает МТ и освобождает объект ASTRALib.
        /// </summary>
        public void Dispose()
        {
            Close();
        }

        /// <summary>
        /// Освобождает объект ASTRALib
        /// </summary>
        /// <param name="rastr">Объект ASTRALib</param>
        private static void ReleaseComObject(IRastr rastr)
        {
            if (Marshal.IsComObject(rastr))
            {
                Marshal.FinalReleaseComObject(rastr);
            }
        }

        /// <summary>
        /// Ищет строку таблицы по ключу
        /// </summary>
        /// <param name="table">Таблица</param>
        /// <param name="key">Значение ключа</param>
        /// <param name="caption">Наименование объекта для сообщения об ошибке</param>
        /// <returns>Индекс строки</returns>
        private static int FindRow(RastrTable table, int key, string caption)
        {
            int row;
            if (!table.TryFindRow(key, out row))
            {
                throw new InvalidOperationException(caption + " " + key + " не найден в МТ");
            }

            return row;
        }

        /// <summary>
        /// Запоминает исходное значение ячейки и записывает новое
        /// </summary>
        /// <param name="change">Изменение, в которое добавляется ячейка.</param>
        /// <param name="column">Столбец.</param>
        /// <param name="row">Индекс строки.</param>
        /// <param name="value">Новое значение.</param>
        private static void SetValue(List<CellValue> change, ICol column, int row, object value)
        {
            object? original = RastrTable.Read(column, row);
            change.Add(new CellValue(column, row, original ?? 0));
            RastrTable.Write(column, row, value);
        }

        /// <summary>
        /// Восстанавливает исходные значения ячеек изменения
        /// </summary>
        /// <param name="change">Изменение.</param>
        private static void Restore(List<CellValue> change)
        {
            for (int index = change.Count - 1; index >= 0; index--)
            {
                CellValue cell = change[index];
                RastrTable.Write(cell.Column, cell.Row, cell.Value);
            }
        }

        /// <summary>
        /// Возвращает объект ASTRALib, проверяя, что МТ открыта
        /// </summary>
        /// <returns>Объект ASTRALib</returns>
        private IRastr GetRastr()
        {
            if (_rastr == null)
            {
                throw new InvalidOperationException("МТ не открыта");
            }

            return _rastr;
        }

        /// <summary>
        /// Возвращает таблицу «Узлы», проверяя, что МТ открыта.
        /// </summary>
        /// <returns>Таблица «Узлы».</returns>
        private RastrTable GetNodes()
        {
            if (_nodes == null)
            {
                throw new InvalidOperationException("МТ не открыта или в ней нет таблицы «Узлы»");
            }

            return _nodes;
        }

        /// <summary>
        /// Открывает таблицы МТ и строит индексы строк.
        /// </summary>
        private void RefreshTables()
        {
            IRastr rastr = GetRastr();
            _nodes = RastrTable.TryOpen(rastr, NodeTableName, "ny");
            _reactors = RastrTable.TryOpen(rastr, ReactorTableName, "Id");
            _generators = RastrTable.TryOpen(rastr, GeneratorTableName, "Num");
        }

        /// <summary>
        /// Закрывает МТ и освобождает объект ASTRALib.
        /// </summary>
        private void Close()
        {
            _changes.Clear();
            _nodes = null;
            _reactors = null;
            _generators = null;
            if (_rastr != null)
            {
                ReleaseComObject(_rastr);
                _rastr = null;
            }
        }
    }
}
