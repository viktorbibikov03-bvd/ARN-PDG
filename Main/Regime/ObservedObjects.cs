using System;
using System.Collections.Generic;
using System.Linq;

namespace VrdoPdg.Model.Regime
{
    /// <summary>
    /// Объекты МТ, значения которых считываются по часам для просмотра в формах
    /// «Напряжение по часам» и «Сравнение расчетов»: узлы таблицы «Узлы» и реакторы
    /// таблицы «Реакторы».
    /// </summary>
    public sealed class ObservedObjects
    {
        /// <summary>
        /// Создает перечень наблюдаемых объектов.
        /// </summary>
        /// <param name="nodeNumbers">Номера узлов (ny).</param>
        /// <param name="reactorNumbers">Номера реакторов (Id).</param>
        public ObservedObjects(IEnumerable<int> nodeNumbers, IEnumerable<int> reactorNumbers)
        {
            if (nodeNumbers == null)
            {
                throw new ArgumentNullException(nameof(nodeNumbers));
            }

            if (reactorNumbers == null)
            {
                throw new ArgumentNullException(nameof(reactorNumbers));
            }

            NodeNumbers = nodeNumbers.Distinct().OrderBy(number => number).ToList();
            ReactorNumbers = reactorNumbers.Distinct().OrderBy(number => number).ToList();
        }

        /// <summary>
        /// Пустой перечень: дополнительные объекты не считываются.
        /// </summary>
        public static ObservedObjects Empty { get; } = new ObservedObjects(new int[0], new int[0]);

        /// <summary>
        /// Номера узлов (ny).
        /// </summary>
        public IReadOnlyList<int> NodeNumbers { get; }

        /// <summary>
        /// Номера реакторов (Id).
        /// </summary>
        public IReadOnlyList<int> ReactorNumbers { get; }
    }
}
