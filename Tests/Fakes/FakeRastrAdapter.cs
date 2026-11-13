using System;
using System.Collections.Generic;
using System.Linq;

using VrdoPdg.Model.Entities;
using VrdoPdg.Model.Regime;

namespace VrdoPdg.Tests.Fakes
{
    /// <summary>
    /// Тестовая реализация модуля взаимодействия с ПО «Барс» с линейной моделью режима:
    /// напряжение в узле равно базовому напряжению часа плюс вклад изменений СКРМ.
    /// Параметры СКРМ, как и таблицы МТ, сохраняются при смене часа, поэтому
    /// сервис должен сам отменять изменения после обработки часа.
    /// </summary>
    internal sealed class FakeRastrAdapter : IRastrAdapter
    {
        /// <summary>
        /// Действия отмены изменений.
        /// </summary>
        private readonly Stack<Action> _undo = new Stack<Action>();

        /// <summary>
        /// Текущий час.
        /// </summary>
        private int _hour = 1;

        /// <summary>
        /// Базовые напряжения узлов по часам (индекс массива — час).
        /// </summary>
        public Dictionary<int, double[]> BaseVoltages { get; } = new Dictionary<int, double[]>();

        /// <summary>
        /// Управляемые СКРМ по номеру узла.
        /// </summary>
        public Dictionary<int, FakeControlledDevice> ControlledDevices { get; } =
            new Dictionary<int, FakeControlledDevice>();

        /// <summary>
        /// Коммутируемые СКРМ по номеру реактора.
        /// </summary>
        public Dictionary<int, FakeSwitchedDevice> SwitchedDevices { get; } = new Dictionary<int, FakeSwitchedDevice>();

        /// <summary>
        /// Генераторы по номеру генератора.
        /// </summary>
        public Dictionary<int, FakeGenerator> Generators { get; } = new Dictionary<int, FakeGenerator>();

        /// <summary>
        /// Условие, при котором расчет режима не сходится.
        /// </summary>
        public Func<bool>? DivergenceCondition { get; set; }

        /// <summary>
        /// Условие отключения узла по его номеру: напряжение в отключенном узле не рассчитывается
        /// </summary>
        public Func<int, bool>? NodeLossCondition { get; set; }

        /// <summary>
        /// Ошибка ПО «Барс» при открытии МТ.
        /// </summary>
        public Exception? OpenException { get; set; }

        /// <summary>
        /// Ошибка ПО «Барс» при выборе часа.
        /// </summary>
        public Exception? SelectHourException { get; set; }

        /// <summary>
        /// Часы, расчет режима которых при выборе часа не сходится.
        /// </summary>
        public HashSet<int> DivergentHours { get; } = new HashSet<int>();

        /// <summary>
        /// Количество открытий МТ.
        /// </summary>
        public int OpenCount { get; private set; }

        /// <summary>
        /// Количество расчетов установившегося режима.
        /// </summary>
        public int RegimeCalculationCount { get; private set; }

        /// <summary>
        /// Устанавливает базовое напряжение узла на все часы.
        /// </summary>
        /// <param name="nodeNumber">Номер узла.</param>
        /// <param name="voltage">Напряжение, кВ.</param>
        public void SetBaseVoltage(int nodeNumber, double voltage)
        {
            double[] values = new double[RegimeSnapshot.HourCount + 1];
            for (int hour = 1; hour <= RegimeSnapshot.HourCount; hour++)
            {
                values[hour] = voltage;
            }

            BaseVoltages[nodeNumber] = values;
        }

        /// <inheritdoc/>
        public void Open(string megapointPath)
        {
            if (OpenException != null)
            {
                throw OpenException;
            }

            OpenCount++;
            _undo.Clear();
        }

        /// <summary>
        /// Наименования узлов МТ, не входящих в КП и не являющихся узлами СКРМ
        /// </summary>
        public Dictionary<int, string> ExtraNodes { get; } = new Dictionary<int, string>();

        /// <inheritdoc/>
        public MegapointCatalog ReadCatalog()
        {
            Dictionary<int, string> nodes = new Dictionary<int, string>(ExtraNodes);
            foreach (int node in BaseVoltages.Keys.Concat(ControlledDevices.Keys))
            {
                nodes[node] = "Узел " + node;
            }

            foreach (FakeGenerator generator in Generators.Values)
            {
                nodes[generator.NodeNumber] = "Узел " + generator.NodeNumber;
            }

            return new MegapointCatalog(
                nodes,
                SwitchedDevices.Keys.ToDictionary(number => number, number => "Реактор " + number),
                Generators.Select(pair => new MegapointGenerator(pair.Key, pair.Value.NodeNumber, "ТГ " + pair.Key)));
        }

        /// <inheritdoc/>
        public bool SelectHour(int hour)
        {
            if (SelectHourException != null)
            {
                throw SelectHourException;
            }

            _hour = hour;
            _undo.Clear();
            return !DivergentHours.Contains(hour);
        }

        /// <inheritdoc/>
        public bool CalculateRegime()
        {
            RegimeCalculationCount++;
            return DivergenceCondition == null || !DivergenceCondition();
        }

        /// <inheritdoc/>
        public double? GetNodeVoltage(int nodeNumber)
        {
            double[]? values;
            if (!BaseVoltages.TryGetValue(nodeNumber, out values)
                || (NodeLossCondition != null && NodeLossCondition(nodeNumber)))
            {
                return null;
            }

            double voltage = values[_hour];
            foreach (FakeControlledDevice device in ControlledDevices.Values)
            {
                voltage += device.GetEffect(nodeNumber);
            }

            foreach (FakeSwitchedDevice device in SwitchedDevices.Values)
            {
                voltage += device.GetEffect(nodeNumber);
            }

            foreach (FakeGenerator generator in Generators.Values)
            {
                voltage += generator.GetEffect(nodeNumber);
            }

            return voltage;
        }

        /// <inheritdoc/>
        public NodeState? GetNodeState(int nodeNumber)
        {
            double? voltage = GetNodeVoltage(nodeNumber);
            FakeControlledDevice? device;
            if (ControlledDevices.TryGetValue(nodeNumber, out device))
            {
                return new NodeState(voltage.HasValue, voltage ?? 0, device.SetVoltage, device.MinReactivePower);
            }

            return voltage.HasValue
                ? new NodeState(true, voltage.Value, 0, 0)
                : null;
        }

        /// <inheritdoc/>
        public ControlledDeviceState? GetControlledDevice(int nodeNumber)
        {
            FakeControlledDevice? device;
            return ControlledDevices.TryGetValue(nodeNumber, out device)
                ? new ControlledDeviceState(device.IsOn, device.ReactivePower, device.MinReactivePower,
                    device.MaxReactivePower, device.SetVoltage)
                : null;
        }

        /// <inheritdoc/>
        public SwitchedDeviceState? GetSwitchedDevice(int reactorNumber)
        {
            FakeSwitchedDevice? device;
            return SwitchedDevices.TryGetValue(reactorNumber, out device)
                ? new SwitchedDeviceState(device.IsOn, device.IsOn ? device.NominalPower : 0)
                : null;
        }

        /// <inheritdoc/>
        public GeneratorState? GetGenerator(int generatorNumber)
        {
            FakeGenerator? generator;
            return Generators.TryGetValue(generatorNumber, out generator)
                ? new GeneratorState(generator.IsOn, generator.NodeNumber, generator.ActivePower,
                    generator.ReactivePower, generator.SetVoltage, generator.MinReactivePower,
                    generator.MaxReactivePower)
                : null;
        }

        /// <inheritdoc/>
        public void SetControlledDeviceReactivePower(int nodeNumber, double reactivePower)
        {
            FakeControlledDevice device = ControlledDevices[nodeNumber];
            double min = device.MinReactivePower;
            double max = device.MaxReactivePower;
            _undo.Push(() =>
            {
                device.MinReactivePower = min;
                device.MaxReactivePower = max;
            });
            device.MinReactivePower = reactivePower;
            device.MaxReactivePower = reactivePower;
        }

        /// <inheritdoc/>
        public void SetSwitchedDeviceState(int reactorNumber, bool isOn)
        {
            FakeSwitchedDevice device = SwitchedDevices[reactorNumber];
            bool previous = device.IsOn;
            _undo.Push(() => device.IsOn = previous);
            device.IsOn = isOn;
        }

        /// <inheritdoc/>
        public void SetNodeSetVoltage(int nodeNumber, double voltage)
        {
            List<FakeGenerator> generators = Generators.Values
                .Where(generator => generator.NodeNumber == nodeNumber)
                .ToList();
            List<double> previous = generators
                .Select(generator => generator.SetVoltage)
                .ToList();
            _undo.Push(() =>
            {
                for (int index = 0; index < generators.Count; index++)
                {
                    generators[index].SetVoltage = previous[index];
                }
            });
            foreach (FakeGenerator generator in generators)
            {
                generator.SetVoltage = voltage;
            }
        }

        /// <inheritdoc/>
        public void UndoLastChange()
        {
            if (_undo.Count > 0)
            {
                _undo.Pop()();
            }
        }

        /// <inheritdoc/>
        public void UndoAllChanges()
        {
            while (_undo.Count > 0)
            {
                _undo.Pop()();
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
        }
    }
}
