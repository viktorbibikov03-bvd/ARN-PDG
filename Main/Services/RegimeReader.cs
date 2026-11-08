using System.Collections.Generic;

using VrdoPdg.Model.Entities;
using VrdoPdg.Model.Regime;

namespace VrdoPdg.Model.Services
{
    /// <summary>
    /// Чтение напряжений в узлах КП и режимов СКРМ текущего часа МТ.
    /// </summary>
    internal static class RegimeReader
    {
        /// <summary>
        /// Считывает режим текущего часа МТ в снимок.
        /// </summary>
        /// <param name="rastrAdapter">Модуль взаимодействия с ПО «Барс».</param>
        /// <param name="hour">Час суток (1–24).</param>
        /// <param name="controlPoints">КП, напряжения в узлах которых считываются.</param>
        /// <param name="devices">СКРМ, режимы которых считываются.</param>
        /// <param name="session">Исходные данные: PQ-диаграммы и признаки заданных состояний.</param>
        /// <param name="snapshot">Снимок, в который записываются значения.</param>
        public static void ReadHour(
            IRastrAdapter rastrAdapter,
            int hour,
            IEnumerable<ControlPoint> controlPoints,
            IEnumerable<Device> devices,
            IDeviceModeContext session,
            RegimeSnapshot snapshot)
        {
            foreach (ControlPoint controlPoint in controlPoints)
            {
                foreach (ControlPointNode node in controlPoint.Nodes)
                {
                    double? voltage = rastrAdapter.GetNodeVoltage(node.Number);
                    if (voltage.HasValue)
                    {
                        snapshot.SetVoltage(new NodeVoltage(node.Number, hour, voltage.Value));
                    }
                }
            }

            foreach (Device device in devices)
            {
                DeviceMode? mode = ReadMode(rastrAdapter, device, hour, session);
                if (mode != null)
                {
                    snapshot.SetMode(mode);
                }
            }
        }

        /// <summary>
        /// Считывает режим СКРМ текущего часа МТ.
        /// </summary>
        /// <param name="rastrAdapter">Модуль взаимодействия с ПО «Барс».</param>
        /// <param name="device">СКРМ.</param>
        /// <param name="hour">Час суток (1–24).</param>
        /// <param name="session">Исходные данные: PQ-диаграммы и признаки заданных состояний.</param>
        /// <returns>Режим СКРМ или <c>null</c>, если СКРМ не найдено в МТ.</returns>
        public static DeviceMode? ReadMode(
            IRastrAdapter rastrAdapter,
            Device device,
            int hour,
            IDeviceModeContext session)
        {
            bool isFixed = session.IsFixed(device.Id, hour);
            switch (device.Type)
            {
                case DeviceType.Controlled:
                    ControlledDeviceState? controlled = rastrAdapter.GetControlledDevice(device.Number);
                    return controlled == null
                        ? null
                        : new DeviceMode(
                            device.Id,
                            hour,
                            controlled.IsOn,
                            controlled.ReactivePower,
                            controlled.MinReactivePower,
                            controlled.MaxReactivePower,
                            controlled.SetVoltage,
                            null,
                            isFixed);

                case DeviceType.Switched:
                    SwitchedDeviceState? switched = rastrAdapter.GetSwitchedDevice(device.Number);
                    return switched == null
                        ? null
                        : new DeviceMode(
                            device.Id,
                            hour,
                            switched.IsOn,
                            switched.ReactivePower,
                            null,
                            null,
                            null,
                            null,
                            isFixed);

                default:
                    GeneratorState? generator = rastrAdapter.GetGenerator(device.Number);
                    if (generator == null)
                    {
                        return null;
                    }

                    ReactivePowerLimits limits = GetGeneratorLimits(generator, session.GetPQDiagram(device.Id));
                    return new DeviceMode(
                        device.Id,
                        hour,
                        generator.IsOn,
                        generator.ReactivePower,
                        limits.Min,
                        limits.Max,
                        generator.SetVoltage,
                        generator.ActivePower,
                        isFixed);
            }
        }

        /// <summary>
        /// Определяет пределы реактивной мощности генератора: по PQ-диаграмме при текущей
        /// активной мощности, а если диаграмма не задана — по таблице генераторов МТ.
        /// </summary>
        /// <param name="generator">Состояние генератора.</param>
        /// <param name="diagram">PQ-диаграмма генератора.</param>
        /// <returns>Пределы реактивной мощности.</returns>
        public static ReactivePowerLimits GetGeneratorLimits(GeneratorState generator, PQDiagram diagram)
        {
            return diagram.IsEmpty
                ? new ReactivePowerLimits(generator.MinReactivePower, generator.MaxReactivePower)
                : diagram.GetLimits(generator.ActivePower);
        }
    }
}
