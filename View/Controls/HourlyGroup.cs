using System.Collections.Generic;

using VrdoPdg.Model.Entities;

namespace VrdoPdg.View.Controls
{
    /// <summary>
    /// Группа столбцов таблицы по часам: КП с узлами (U сш) и СКРМ.
    /// </summary>
    internal sealed class HourlyGroup
    {
        /// <summary>
        /// Создает группу столбцов.
        /// </summary>
        /// <param name="controlPoint">КП.</param>
        /// <param name="nodeNumbers">Номера узлов КП.</param>
        /// <param name="devices">СКРМ таблицы эффективности КП.</param>
        public HourlyGroup(ControlPoint controlPoint, IReadOnlyList<int> nodeNumbers, IReadOnlyList<Device> devices)
        {
            ControlPoint = controlPoint;
            NodeNumbers = nodeNumbers;
            Devices = devices;
        }

        /// <summary>
        /// КП.
        /// </summary>
        public ControlPoint ControlPoint { get; }

        /// <summary>
        /// Номера узлов КП (столбцы «U сш»).
        /// </summary>
        public IReadOnlyList<int> NodeNumbers { get; }

        /// <summary>
        /// СКРМ таблицы эффективности КП (столбцы «СКРМ»).
        /// </summary>
        public IReadOnlyList<Device> Devices { get; }
    }
}
