using System.Collections.Generic;

namespace VrdoPdg.View.Layout
{
    /// <summary>
    /// Энергосистема в таблице по часам: блоки КП, выводимые один под другим.
    /// </summary>
    internal sealed class HourlyLayoutPowerSystem
    {
        /// <summary>
        /// Создает энергосистему.
        /// </summary>
        /// <param name="name">Наименование энергосистемы.</param>
        /// <param name="blocks">Блоки КП.</param>
        public HourlyLayoutPowerSystem(string name, IReadOnlyList<HourlyLayoutBlock> blocks)
        {
            Name = name;
            Blocks = blocks;
        }

        /// <summary>
        /// Наименование энергосистемы.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Блоки КП.
        /// </summary>
        public IReadOnlyList<HourlyLayoutBlock> Blocks { get; }
    }
}
