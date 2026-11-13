using System;
using System.Collections.Generic;
using System.Linq;

using VrdoPdg.Model.Entities;
using VrdoPdg.Model.Storage;

namespace VrdoPdg.Tests.Fakes
{
    /// <summary>
    /// Репозиторий БД в оперативной памяти для модульных тестов.
    /// </summary>
    internal sealed class FakeModelDataRepository : IModelDataRepository
    {
        /// <summary>
        /// Энергосистемы.
        /// </summary>
        public List<PowerSystem> PowerSystems { get; } = new List<PowerSystem>();

        /// <summary>
        /// КП.
        /// </summary>
        public List<ControlPoint> ControlPoints { get; } = new List<ControlPoint>();

        /// <summary>
        /// СКРМ.
        /// </summary>
        public List<Device> Devices { get; } = new List<Device>();

        /// <summary>
        /// Строки таблиц эффективности.
        /// </summary>
        public List<EfficiencyEntry> EfficiencyEntries { get; } = new List<EfficiencyEntry>();

        /// <summary>
        /// Границы графиков напряжений.
        /// </summary>
        public List<VoltageBounds> Schedule { get; } = new List<VoltageBounds>();

        /// <summary>
        /// Сохраненные напряжения в узлах.
        /// </summary>
        public List<NodeVoltage> SavedVoltages { get; } = new List<NodeVoltage>();

        /// <summary>
        /// Сохраненные режимы СКРМ.
        /// </summary>
        public List<DeviceMode> SavedModes { get; } = new List<DeviceMode>();

        /// <summary>
        /// Имена методов, которые завершаются ошибкой обращения к БД.
        /// </summary>
        public HashSet<string> FailingMethods { get; } = new HashSet<string>();

        /// <inheritdoc/>
        public void CheckDatabase()
        {
        }

        /// <inheritdoc/>
        public IReadOnlyList<PowerSystem> GetPowerSystems()
        {
            return PowerSystems.ToList();
        }

        /// <inheritdoc/>
        public IReadOnlyList<ControlPoint> GetControlPoints()
        {
            CheckFailure(nameof(GetControlPoints));
            return ControlPoints.ToList();
        }

        /// <inheritdoc/>
        public IReadOnlyList<Device> GetDevices()
        {
            return Devices.ToList();
        }

        /// <inheritdoc/>
        public IReadOnlyList<EfficiencyEntry> GetEfficiencyEntries()
        {
            CheckFailure(nameof(GetEfficiencyEntries));
            return EfficiencyEntries.ToList();
        }

        /// <inheritdoc/>
        public Device AddDevice(DeviceType type, string name, int number, SwitchedDeviceKind? switchedKind)
        {
            int id = Devices.Count == 0
                ? 1
                : Devices.Max(device => device.Id) + 1;
            Device device = new Device(id, type, name, number, switchedKind);
            Devices.Add(device);
            return device;
        }

        /// <inheritdoc/>
        public void SaveEfficiencyTable(int controlPointId, IReadOnlyList<DevicePriority> priorities)
        {
            List<EfficiencyEntry> others = EfficiencyEntries
                .Where(entry => entry.ControlPointId != controlPointId)
                .ToList();
            Dictionary<int, int?> defaults = EfficiencyEntries
                .Where(entry => entry.ControlPointId == controlPointId)
                .ToDictionary(entry => entry.DeviceId, entry => entry.DefaultPriority);
            EfficiencyEntries.Clear();
            EfficiencyEntries.AddRange(others);
            foreach (DevicePriority priority in priorities)
            {
                int? defaultPriority;
                defaults.TryGetValue(priority.DeviceId, out defaultPriority);
                EfficiencyEntries.Add(
                    new EfficiencyEntry(controlPointId, priority.DeviceId, priority.Priority, defaultPriority));
            }

            foreach (KeyValuePair<int, int?> pair in defaults.Where(pair => pair.Value.HasValue
                && priorities.All(priority => priority.DeviceId != pair.Key)))
            {
                EfficiencyEntries.Add(new EfficiencyEntry(controlPointId, pair.Key, null, pair.Value));
            }
        }

        /// <inheritdoc/>
        public IReadOnlyList<VoltageBounds> GetVoltageSchedule()
        {
            CheckFailure(nameof(GetVoltageSchedule));
            return Schedule.ToList();
        }

        /// <inheritdoc/>
        public void SaveVoltageSchedule(IReadOnlyList<VoltageBounds> bounds)
        {
            CheckFailure(nameof(SaveVoltageSchedule));
            Schedule.Clear();
            Schedule.AddRange(bounds);
        }

        /// <inheritdoc/>
        public void SaveRegime(IReadOnlyList<NodeVoltage> voltages, IReadOnlyList<DeviceMode> modes)
        {
            CheckFailure(nameof(SaveRegime));
            SavedVoltages.Clear();
            SavedVoltages.AddRange(voltages);
            SavedModes.Clear();
            SavedModes.AddRange(modes);
        }

        /// <summary>
        /// Завершает метод ошибкой обращения к БД, если метод задан в перечне <see cref="FailingMethods"/>.
        /// </summary>
        /// <param name="method">Имя метода.</param>
        private void CheckFailure(string method)
        {
            if (FailingMethods.Contains(method))
            {
                throw new InvalidOperationException("database is locked");
            }
        }
    }
}
