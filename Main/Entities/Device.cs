using System;

namespace VrdoPdg.Model.Entities
{
    /// <summary>
    /// СКРМ: средство компенсации реактивной мощности или генератор,
    /// используемые алгоритмом для регулирования напряжения.
    /// </summary>
    public sealed class Device
    {
        /// <summary>
        /// Создает СКРМ.
        /// </summary>
        /// <param name="id">Код СКРМ в БД.</param>
        /// <param name="type">Тип СКРМ.</param>
        /// <param name="name">Наименование СКРМ.</param>
        /// <param name="number">Номер СКРМ в МТ.</param>
        /// <param name="switchedKind">Вид коммутируемого СКРМ.</param>
        public Device(int id, DeviceType type, string name, int number, SwitchedDeviceKind? switchedKind)
        {
            if (type == DeviceType.Switched && switchedKind == null)
            {
                throw new ArgumentException("Для коммутируемого СКРМ должен быть задан вид", nameof(switchedKind));
            }

            Id = id;
            Type = type;
            Name = name ?? string.Empty;
            Number = number;
            SwitchedKind = type == DeviceType.Switched
                ? switchedKind
                : null;
        }

        /// <summary>
        /// Код СКРМ в БД.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Тип СКРМ.
        /// </summary>
        public DeviceType Type { get; }

        /// <summary>
        /// Наименование СКРМ.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Номер СКРМ в МТ: номер узла для управляемых СКРМ, номер реактора
        /// для коммутируемых СКРМ, номер узла подключения для генераторов.
        /// </summary>
        public int Number { get; }

        /// <summary>
        /// Вид коммутируемого СКРМ (реактор или БСК); для остальных типов не задан.
        /// </summary>
        public SwitchedDeviceKind? SwitchedKind { get; }

        /// <summary>
        /// Признак БСК: коммутируемого СКРМ, выдающего реактивную мощность.
        /// </summary>
        public bool IsCapacitorBank => SwitchedKind == SwitchedDeviceKind.CapacitorBank;

        /// <summary>
        /// Наименование типа СКРМ.
        /// </summary>
        public string TypeName => DeviceNames.GetTypeName(Type);

        /// <summary>
        /// Подпись номера с номером, например «узел 6269» или «реактор 7».
        /// </summary>
        public string NumberText => DeviceNames.GetNumberCaption(Type) + " " + Number;

        /// <summary>
        /// Возвращает наименование СКРМ с номером в МТ.
        /// </summary>
        /// <returns>Строка вида «Р-1 ПС 500 кВ Барнаульская (реактор 9)».</returns>
        public override string ToString()
        {
            return Name + " (" + NumberText + ")";
        }
    }
}
