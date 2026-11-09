using System.Collections.Generic;
using System.Linq;

using VrdoPdg.Model.Entities;

namespace VrdoPdg.Model.Regime
{
    /// <summary>
    /// Снимок режима: почасовые напряжения в узлах КП и режимы СКРМ.
    /// </summary>
    public sealed class RegimeSnapshot
    {
        /// <summary>
        /// Количество часов в сутках МТ.
        /// </summary>
        public const int HourCount = 24;

        /// <summary>
        /// Напряжения по номеру узла и часу.
        /// </summary>
        private readonly Dictionary<(int NodeNumber, int Hour), NodeVoltage> _voltages =
            new Dictionary<(int NodeNumber, int Hour), NodeVoltage>();

        /// <summary>
        /// Режимы СКРМ по коду СКРМ и часу.
        /// </summary>
        private readonly Dictionary<(int DeviceId, int Hour), DeviceMode> _modes =
            new Dictionary<(int DeviceId, int Hour), DeviceMode>();

        /// <summary>
        /// Значения наблюдаемых узлов по номеру узла и часу.
        /// </summary>
        private readonly Dictionary<(int NodeNumber, int Hour), ObservedNodeState> _observedNodes =
            new Dictionary<(int NodeNumber, int Hour), ObservedNodeState>();

        /// <summary>
        /// Состояния наблюдаемых реакторов по номеру реактора и часу.
        /// </summary>
        private readonly Dictionary<(int ReactorNumber, int Hour), ObservedReactorState> _observedReactors =
            new Dictionary<(int ReactorNumber, int Hour), ObservedReactorState>();

        /// <summary>
        /// Напряжения в узлах, упорядоченные по номеру узла и часу.
        /// </summary>
        public IReadOnlyList<NodeVoltage> Voltages => _voltages.Values
            .OrderBy(item => item.NodeNumber)
            .ThenBy(item => item.Hour)
            .ToList();

        /// <summary>
        /// Режимы СКРМ, упорядоченные по коду СКРМ и часу.
        /// </summary>
        public IReadOnlyList<DeviceMode> Modes => _modes.Values
            .OrderBy(item => item.DeviceId)
            .ThenBy(item => item.Hour)
            .ToList();

        /// <summary>
        /// Добавляет или заменяет напряжение в узле.
        /// </summary>
        /// <param name="voltage">Напряжение в узле.</param>
        public void SetVoltage(NodeVoltage voltage)
        {
            _voltages[(voltage.NodeNumber, voltage.Hour)] = voltage;
        }

        /// <summary>
        /// Добавляет или заменяет режим СКРМ.
        /// </summary>
        /// <param name="mode">Режим СКРМ.</param>
        public void SetMode(DeviceMode mode)
        {
            _modes[(mode.DeviceId, mode.Hour)] = mode;
        }

        /// <summary>
        /// Возвращает напряжение в узле.
        /// </summary>
        /// <param name="nodeNumber">Номер узла.</param>
        /// <param name="hour">Час суток (1–24).</param>
        /// <returns>Напряжение, кВ, или <c>null</c>, если значение отсутствует.</returns>
        public double? GetVoltage(int nodeNumber, int hour)
        {
            NodeVoltage? voltage;
            return _voltages.TryGetValue((nodeNumber, hour), out voltage)
                ? voltage.Voltage
                : (double?)null;
        }

        /// <summary>
        /// Возвращает режим СКРМ.
        /// </summary>
        /// <param name="deviceId">Код СКРМ.</param>
        /// <param name="hour">Час суток (1–24).</param>
        /// <returns>Режим СКРМ или <c>null</c>, если значение отсутствует.</returns>
        public DeviceMode? GetMode(int deviceId, int hour)
        {
            DeviceMode? mode;
            return _modes.TryGetValue((deviceId, hour), out mode)
                ? mode
                : null;
        }

        /// <summary>
        /// Сохраняет значения наблюдаемого узла.
        /// </summary>
        /// <param name="state">Значения узла.</param>
        public void SetObservedNode(ObservedNodeState state)
        {
            _observedNodes[(state.NodeNumber, state.Hour)] = state;
        }

        /// <summary>
        /// Сохраняет состояние наблюдаемого реактора.
        /// </summary>
        /// <param name="state">Состояние реактора.</param>
        public void SetObservedReactor(ObservedReactorState state)
        {
            _observedReactors[(state.ReactorNumber, state.Hour)] = state;
        }

        /// <summary>
        /// Возвращает значения наблюдаемого узла.
        /// </summary>
        /// <param name="nodeNumber">Номер узла (ny).</param>
        /// <param name="hour">Час суток (1–24).</param>
        /// <returns>Значения узла или <c>null</c>, если узел не найден в МТ.</returns>
        public ObservedNodeState? GetObservedNode(int nodeNumber, int hour)
        {
            ObservedNodeState? state;
            return _observedNodes.TryGetValue((nodeNumber, hour), out state)
                ? state
                : null;
        }

        /// <summary>
        /// Возвращает состояние наблюдаемого реактора.
        /// </summary>
        /// <param name="reactorNumber">Номер реактора (Id).</param>
        /// <param name="hour">Час суток (1–24).</param>
        /// <returns>Состояние реактора или <c>null</c>, если реактор не найден в МТ.</returns>
        public ObservedReactorState? GetObservedReactor(int reactorNumber, int hour)
        {
            ObservedReactorState? state;
            return _observedReactors.TryGetValue((reactorNumber, hour), out state)
                ? state
                : null;
        }

        /// <summary>
        /// Копирует значения заданного часа из другого снимка.
        /// </summary>
        /// <param name="source">Снимок-источник.</param>
        /// <param name="hour">Час суток (1–24).</param>
        public void CopyHour(RegimeSnapshot source, int hour)
        {
            foreach (NodeVoltage voltage in source._voltages.Values.Where(item => item.Hour == hour))
            {
                SetVoltage(voltage);
            }

            foreach (DeviceMode mode in source._modes.Values.Where(item => item.Hour == hour))
            {
                SetMode(mode);
            }

            foreach (ObservedNodeState state in source._observedNodes.Values.Where(item => item.Hour == hour))
            {
                SetObservedNode(state);
            }

            foreach (ObservedReactorState state in source._observedReactors.Values.Where(item => item.Hour == hour))
            {
                SetObservedReactor(state);
            }
        }
    }
}
