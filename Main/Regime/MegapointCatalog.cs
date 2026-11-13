using System.Collections.Generic;
using System.Linq;

namespace VrdoPdg.Model.Regime
{
    /// <summary>
    /// Перечень объектов загруженной МТ: узлы таблицы «Узлы», реакторы таблицы «Реакторы»
    /// и генераторы таблицы «Генераторы УР» с наименованиями. Используется для проверки СКРМ,
    /// добавляемых в таблицу эффективности, и для вывода узлов подключения генераторов
    /// </summary>
    public sealed class MegapointCatalog
    {
        /// <summary>
        /// Наименования узлов по номеру узла (ny)
        /// </summary>
        private readonly Dictionary<int, string> _nodes;

        /// <summary>
        /// Наименования реакторов по номеру реактора (Id)
        /// </summary>
        private readonly Dictionary<int, string> _reactors;

        /// <summary>
        /// Генераторы по номеру генератора (Num)
        /// </summary>
        private readonly Dictionary<int, MegapointGenerator> _generators;

        /// <summary>
        /// Пустой перечень: МТ не загружена
        /// </summary>
        public static MegapointCatalog Empty { get; } = new MegapointCatalog(
            new Dictionary<int, string>(),
            new Dictionary<int, string>(),
            new MegapointGenerator[0]);

        /// <summary>
        /// Создает перечень объектов МТ
        /// </summary>
        /// <param name="nodes">Наименования узлов по номеру узла (ny)</param>
        /// <param name="reactors">Наименования реакторов по номеру реактора (Id)</param>
        /// <param name="generators">Генераторы</param>
        public MegapointCatalog(
            IReadOnlyDictionary<int, string> nodes,
            IReadOnlyDictionary<int, string> reactors,
            IEnumerable<MegapointGenerator> generators)
        {
            _nodes = nodes.ToDictionary(pair => pair.Key, pair => pair.Value ?? string.Empty);
            _reactors = reactors.ToDictionary(pair => pair.Key, pair => pair.Value ?? string.Empty);
            _generators = new Dictionary<int, MegapointGenerator>();
            foreach (MegapointGenerator generator in generators)
            {
                _generators[generator.Number] = generator;
            }
        }

        /// <summary>
        /// Возвращает наименование узла
        /// </summary>
        /// <param name="nodeNumber">Номер узла (ny)</param>
        /// <returns>Наименование или <c>null</c>, если узла нет в МТ</returns>
        public string? GetNodeName(int nodeNumber)
        {
            string? name;
            return _nodes.TryGetValue(nodeNumber, out name)
                ? name
                : null;
        }

        /// <summary>
        /// Возвращает наименование реактора
        /// </summary>
        /// <param name="reactorNumber">Номер реактора (Id)</param>
        /// <returns>Наименование или <c>null</c>, если реактора нет в МТ</returns>
        public string? GetReactorName(int reactorNumber)
        {
            string? name;
            return _reactors.TryGetValue(reactorNumber, out name)
                ? name
                : null;
        }

        /// <summary>
        /// Возвращает генератор по номеру
        /// </summary>
        /// <param name="generatorNumber">Номер генератора (Num)</param>
        /// <returns>Генератор или <c>null</c>, если генератора нет в МТ</returns>
        public MegapointGenerator? GetGenerator(int generatorNumber)
        {
            MegapointGenerator? generator;
            return _generators.TryGetValue(generatorNumber, out generator)
                ? generator
                : null;
        }

        /// <summary>
        /// Возвращает генераторы, подключенные к узлу, в порядке номеров
        /// </summary>
        /// <param name="nodeNumber">Номер узла (ny)</param>
        /// <returns>Генераторы узла</returns>
        public IReadOnlyList<MegapointGenerator> GetGeneratorsAtNode(int nodeNumber)
        {
            return _generators.Values
                .Where(generator => generator.NodeNumber == nodeNumber)
                .OrderBy(generator => generator.Number)
                .ToList();
        }
    }
}
