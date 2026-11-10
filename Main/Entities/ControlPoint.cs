using System.Collections.Generic;
using System.Linq;

namespace VrdoPdg.Model.Entities
{
    /// <summary>
    /// Контрольный пункт по напряжению (КП), в котором диспетчерский центр
    /// задает допустимый диапазон напряжений
    /// </summary>
    public sealed class ControlPoint
    {
        /// <summary>
        /// Создает КП
        /// </summary>
        /// <param name="id">Код КП.</param>
        /// <param name="powerSystemId">Код энергосистемы.</param>
        /// <param name="name">Наименование КП.</param>
        /// <param name="nodes">Узлы КП.</param>
        public ControlPoint(int id, int powerSystemId, string name, IEnumerable<ControlPointNode> nodes)
        {
            Id = id;
            PowerSystemId = powerSystemId;
            Name = name ?? string.Empty;
            Nodes = nodes
                .OrderBy(node => node.Number)
                .ToList();
            MainNode = Nodes.FirstOrDefault(node => node.IsMain);
        }

        /// <summary>
        /// Код КП. Задает порядок вывода КП в формах
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Код энергосистемы, к которой относится КП
        /// </summary>
        public int PowerSystemId { get; }

        /// <summary>
        /// Наименование КП.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Узлы КП, упорядоченные по номеру.
        /// </summary>
        public IReadOnlyList<ControlPointNode> Nodes { get; }

        /// <summary>
        /// Основной узел КП, в котором осуществляется ввод режима;
        /// <c>null</c>, если основной узел не задан.
        /// </summary>
        public ControlPointNode? MainNode { get; }
    }
}
