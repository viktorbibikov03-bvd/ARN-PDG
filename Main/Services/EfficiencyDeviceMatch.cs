using VrdoPdg.Model.Entities;

namespace VrdoPdg.Model.Services
{
    /// <summary>
    /// СКРМ строки таблицы эффективности: СКРМ из БД или СКРМ МТ, которого еще нет в БД
    /// и которое будет добавлено в БД при сохранении таблицы
    /// </summary>
    public sealed class EfficiencyDeviceMatch
    {
        /// <summary>
        /// Создает СКРМ, найденное в БД
        /// </summary>
        /// <param name="device">СКРМ из БД</param>
        public EfficiencyDeviceMatch(Device device)
        {
            Device = device;
            Type = device.Type;
            Number = device.Number;
            Name = device.Name;
            SwitchedKind = device.SwitchedKind;
        }

        /// <summary>
        /// Создает СКРМ МТ, которого нет в БД
        /// </summary>
        /// <param name="type">Тип СКРМ</param>
        /// <param name="number">Номер СКРМ в БД: номер узла, реактора или генератора (Num)</param>
        /// <param name="name">Наименование СКРМ из МТ</param>
        /// <param name="switchedKind">Вид коммутируемого СКРМ</param>
        public EfficiencyDeviceMatch(DeviceType type, int number, string name, SwitchedDeviceKind? switchedKind)
        {
            Type = type;
            Number = number;
            Name = name;
            SwitchedKind = switchedKind;
        }

        /// <summary>
        /// СКРМ из БД; <c>null</c> — СКРМ будет добавлено в БД
        /// </summary>
        public Device? Device { get; }

        /// <summary>
        /// Тип СКРМ
        /// </summary>
        public DeviceType Type { get; }

        /// <summary>
        /// Номер СКРМ в БД: номер узла, реактора или генератора (Num)
        /// </summary>
        public int Number { get; }

        /// <summary>
        /// Наименование СКРМ
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Вид коммутируемого СКРМ
        /// </summary>
        public SwitchedDeviceKind? SwitchedKind { get; }

        /// <summary>
        /// Признак СКРМ, которого нет в БД
        /// </summary>
        public bool IsNew => Device == null;
    }
}
