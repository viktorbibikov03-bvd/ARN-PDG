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
        /// PQ-диаграммы генераторов.
        /// </summary>
        public Dictionary<int, PQDiagram> Diagrams { get; } = new Dictionary<int, PQDiagram>();

        /// <summary>
        /// Сохраненные напряжения в узлах.
        /// </summary>
        public List<NodeVoltage> SavedVoltages { get; } = new List<NodeVoltage>();

        /// <summary>
        /// Сохраненные режимы СКРМ.
        /// </summary>
        public List<DeviceMode> SavedModes { get; } = new List<DeviceMode>();

        /// <inheritdoc/>
        public void EnsureCreated()
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
            return EfficiencyEntries.ToList();
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
            return Schedule.ToList();
        }

        /// <inheritdoc/>
        public void SaveVoltageSchedule(IReadOnlyList<VoltageBounds> bounds)
        {
            Schedule.Clear();
            Schedule.AddRange(bounds);
        }

        /// <inheritdoc/>
        public IReadOnlyDictionary<int, PQDiagram> GetPQDiagrams()
        {
            return Diagrams.ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        /// <inheritdoc/>
        public void SavePQDiagram(int deviceId, IReadOnlyList<PQPoint> points)
        {
            Diagrams[deviceId] = new PQDiagram(points);
        }

        /// <inheritdoc/>
        public void SaveRegime(IReadOnlyList<NodeVoltage> voltages, IReadOnlyList<DeviceMode> modes)
        {
            SavedVoltages.Clear();
            SavedVoltages.AddRange(voltages);
            SavedModes.Clear();
            SavedModes.AddRange(modes);
        }
    }
}
