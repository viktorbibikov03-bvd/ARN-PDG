using System.Collections.Generic;

using VrdoPdg.Model.Entities;

namespace VrdoPdg.Model.Services
{
    /// <summary>
    /// Сведения для формирования режимов СКРМ при загрузке исходных данных.
    /// </summary>
    internal sealed class DeviceModeContext : IDeviceModeContext
    {
        /// <summary>
        /// Часы, в которых состояние СКРМ задано в файле smp_smzu.csv.
        /// </summary>
        private readonly HashSet<(int DeviceId, int Hour)> _fixedDeviceHours;

        /// <summary>
        /// PQ-диаграммы генераторов по коду СКРМ.
        /// </summary>
        private readonly IReadOnlyDictionary<int, PQDiagram> _pqDiagrams;

        /// <summary>
        /// Создает сведения для формирования режимов СКРМ.
        /// </summary>
        /// <param name="fixedDeviceHours">Часы, в которых состояние СКРМ задано в файле smp_smzu.csv.</param>
        /// <param name="pqDiagrams">PQ-диаграммы генераторов по коду СКРМ.</param>
        public DeviceModeContext(
            HashSet<(int DeviceId, int Hour)> fixedDeviceHours,
            IReadOnlyDictionary<int, PQDiagram> pqDiagrams)
        {
            _fixedDeviceHours = fixedDeviceHours;
            _pqDiagrams = pqDiagrams;
        }

        /// <inheritdoc/>
        public bool IsFixed(int deviceId, int hour)
        {
            return _fixedDeviceHours.Contains((deviceId, hour));
        }

        /// <inheritdoc/>
        public PQDiagram GetPQDiagram(int deviceId)
        {
            PQDiagram? diagram;
            return _pqDiagrams.TryGetValue(deviceId, out diagram)
                ? diagram
                : PQDiagram.Empty;
        }
    }
}
