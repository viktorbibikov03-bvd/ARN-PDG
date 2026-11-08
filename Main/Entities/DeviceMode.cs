namespace VrdoPdg.Model.Entities
{
    /// <summary>
    /// Режим СКРМ: состояние и загрузка СКРМ в определенном часе суток.
    /// </summary>
    public sealed class DeviceMode
    {
        /// <summary>
        /// Создает режим СКРМ.
        /// </summary>
        /// <param name="deviceId">Код СКРМ.</param>
        /// <param name="hour">Час суток (1–24).</param>
        /// <param name="isOn">Признак состояния «Включено».</param>
        /// <param name="reactivePower">Загрузка по реактивной мощности, Мвар.</param>
        /// <param name="minReactivePower">Нижний предел реактивной мощности, Мвар.</param>
        /// <param name="maxReactivePower">Верхний предел реактивной мощности, Мвар.</param>
        /// <param name="setVoltage">Заданное напряжение, кВ.</param>
        /// <param name="activePower">Активная мощность генератора, МВт.</param>
        /// <param name="isFixed">Признак состояния, заданного в файле smp_smzu.csv.</param>
        public DeviceMode(
            int deviceId,
            int hour,
            bool isOn,
            double? reactivePower,
            double? minReactivePower,
            double? maxReactivePower,
            double? setVoltage,
            double? activePower,
            bool isFixed)
        {
            DeviceId = deviceId;
            Hour = hour;
            IsOn = isOn;
            ReactivePower = reactivePower;
            MinReactivePower = minReactivePower;
            MaxReactivePower = maxReactivePower;
            SetVoltage = setVoltage;
            ActivePower = activePower;
            IsFixed = isFixed;
        }

        /// <summary>
        /// Код СКРМ.
        /// </summary>
        public int DeviceId { get; }

        /// <summary>
        /// Час суток (1–24).
        /// </summary>
        public int Hour { get; }

        /// <summary>
        /// Признак состояния «Включено».
        /// </summary>
        public bool IsOn { get; }

        /// <summary>
        /// Загрузка по реактивной мощности, Мвар.
        /// </summary>
        public double? ReactivePower { get; }

        /// <summary>
        /// Нижний предел реактивной мощности, Мвар.
        /// </summary>
        public double? MinReactivePower { get; }

        /// <summary>
        /// Верхний предел реактивной мощности, Мвар.
        /// </summary>
        public double? MaxReactivePower { get; }

        /// <summary>
        /// Заданное напряжение в узле СКРМ или на шинах генератора, кВ.
        /// </summary>
        public double? SetVoltage { get; }

        /// <summary>
        /// Активная мощность генератора для определения пределов по PQ-диаграмме, МВт.
        /// </summary>
        public double? ActivePower { get; }

        /// <summary>
        /// Признак состояния, заданного в файле smp_smzu.csv: алгоритм его не изменяет.
        /// </summary>
        public bool IsFixed { get; }
    }
}
