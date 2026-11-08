using VrdoPdg.Model.Entities;

namespace VrdoPdg.View.Controls
{
    /// <summary>
    /// Описание столбца таблицы по часам.
    /// </summary>
    internal sealed class HourlyColumn
    {
        /// <summary>
        /// Создает описание столбца.
        /// </summary>
        /// <param name="kind">Вид столбца.</param>
        /// <param name="group">Группа столбцов КП.</param>
        /// <param name="nodeNumber">Номер узла для столбца напряжения.</param>
        /// <param name="device">СКРМ для столбца СКРМ.</param>
        public HourlyColumn(HourlyColumnKind kind, HourlyGroup? group, int nodeNumber, Device? device)
        {
            Kind = kind;
            Group = group;
            NodeNumber = nodeNumber;
            Device = device;
        }

        /// <summary>
        /// Вид столбца.
        /// </summary>
        public HourlyColumnKind Kind { get; }

        /// <summary>
        /// Группа столбцов КП; не задана для столбца часа.
        /// </summary>
        public HourlyGroup? Group { get; }

        /// <summary>
        /// Номер узла для столбца напряжения.
        /// </summary>
        public int NodeNumber { get; }

        /// <summary>
        /// СКРМ для столбца СКРМ.
        /// </summary>
        public Device? Device { get; }

        /// <summary>
        /// Номер, выводимый в нижней строке заголовка.
        /// </summary>
        public string NumberText => Kind == HourlyColumnKind.Voltage
            ? NodeNumber.ToString()
            : Device?.Number.ToString() ?? string.Empty;
    }
}
