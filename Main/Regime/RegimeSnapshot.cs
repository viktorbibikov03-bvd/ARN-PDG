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
        }
    }
}
