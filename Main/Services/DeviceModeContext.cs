using System.Collections.Generic;

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
        /// Создает сведения для формирования режимов СКРМ.
        /// </summary>
        /// <param name="fixedDeviceHours">Часы, в которых состояние СКРМ задано в файле smp_smzu.csv.</param>
        public DeviceModeContext(HashSet<(int DeviceId, int Hour)> fixedDeviceHours)
        {
            _fixedDeviceHours = fixedDeviceHours;
        }

        /// <inheritdoc/>
        public bool IsFixed(int deviceId, int hour)
        {
            return _fixedDeviceHours.Contains((deviceId, hour));
        }
    }
}
