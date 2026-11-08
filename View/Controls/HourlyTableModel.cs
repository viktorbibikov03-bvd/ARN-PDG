using System.Collections.Generic;
using System.Linq;

using VrdoPdg.Model.Entities;
using VrdoPdg.Model.Services;

namespace VrdoPdg.View.Controls
{
    /// <summary>
    /// Данные таблицы по часам: энергосистемы, группы столбцов по КП
    /// и один (предпросмотр) или два (сравнение) источника значений.
    /// </summary>
    internal sealed class HourlyTableModel
    {
        /// <summary>
        /// Группы столбцов по КП.
        /// </summary>
        private readonly IReadOnlyList<HourlyGroup> _groups;

        /// <summary>
        /// Создает данные таблицы.
        /// </summary>
        /// <param name="powerSystems">Энергосистемы.</param>
        /// <param name="groups">Группы столбцов по КП.</param>
        /// <param name="sources">Источники значений.</param>
        public HourlyTableModel(
            IReadOnlyList<PowerSystem> powerSystems,
            IReadOnlyList<HourlyGroup> groups,
            IReadOnlyList<HourlyValueSource> sources)
        {
            PowerSystems = powerSystems;
            _groups = groups;
            Sources = sources;
        }

        /// <summary>
        /// Энергосистемы (вкладки таблицы).
        /// </summary>
        public IReadOnlyList<PowerSystem> PowerSystems { get; }

        /// <summary>
        /// Источники значений: один для предпросмотра, два для сравнения расчетов.
        /// </summary>
        public IReadOnlyList<HourlyValueSource> Sources { get; }

        /// <summary>
        /// Создает данные для формы «Напряжение по часам».
        /// </summary>
        /// <param name="session">Исходные данные сеанса.</param>
        /// <param name="source">Источник значений: режим до или после расчета.</param>
        /// <param name="tables">Таблицы эффективности по коду КП.</param>
        /// <returns>Данные таблицы.</returns>
        public static HourlyTableModel CreatePreview(
            SessionData session,
            HourlyValueSource source,
            IReadOnlyDictionary<int, IReadOnlyList<EfficiencyItem>> tables)
        {
            List<HourlyGroup> groups = session.ControlPoints
                .Select(controlPoint => CreateGroup(controlPoint, GetItems(tables, controlPoint.Id)))
                .ToList();
            return new HourlyTableModel(session.PowerSystems, groups, new[] { source });
        }

        /// <summary>
        /// Создает данные для формы «Сравнение расчетов».
        /// </summary>
        /// <param name="first">Расчет с меньшим номером (верхнее значение).</param>
        /// <param name="second">Второй расчет (нижнее значение).</param>
        /// <returns>Данные таблицы.</returns>
        public static HourlyTableModel CreateComparison(CalculationResult first, CalculationResult second)
        {
            List<HourlyGroup> groups = first.Session.ControlPoints
                .Select(controlPoint => CreateGroup(
                    controlPoint,
                    first.GetEfficiencyTable(controlPoint.Id).Concat(second.GetEfficiencyTable(controlPoint.Id))))
                .ToList();
            HourlyValueSource[] sources =
            {
                new HourlyValueSource(first.After, first.Session.Schedule),
                new HourlyValueSource(second.After, second.Session.Schedule)
            };
            return new HourlyTableModel(first.Session.PowerSystems, groups, sources);
        }

        /// <summary>
        /// Возвращает группы столбцов КП энергосистемы.
        /// </summary>
        /// <param name="powerSystemId">Код энергосистемы.</param>
        /// <returns>Группы столбцов.</returns>
        public IReadOnlyList<HourlyGroup> GetGroups(int powerSystemId)
        {
            return _groups
                .Where(group => group.ControlPoint.PowerSystemId == powerSystemId)
                .ToList();
        }

        /// <summary>
        /// Возвращает таблицу эффективности КП.
        /// </summary>
        /// <param name="tables">Таблицы эффективности.</param>
        /// <param name="controlPointId">Код КП.</param>
        /// <returns>СКРМ таблицы эффективности.</returns>
        private static IEnumerable<EfficiencyItem> GetItems(
            IReadOnlyDictionary<int, IReadOnlyList<EfficiencyItem>> tables,
            int controlPointId)
        {
            IReadOnlyList<EfficiencyItem>? items;
            return tables.TryGetValue(controlPointId, out items)
                ? items
                : Enumerable.Empty<EfficiencyItem>();
        }

        /// <summary>
        /// Создает группу столбцов КП: узлы КП и СКРМ таблицы эффективности
        /// в порядке перебора алгоритмом.
        /// </summary>
        /// <param name="controlPoint">КП.</param>
        /// <param name="items">СКРМ таблиц эффективности.</param>
        /// <returns>Группа столбцов.</returns>
        private static HourlyGroup CreateGroup(ControlPoint controlPoint, IEnumerable<EfficiencyItem> items)
        {
            List<Device> devices = new List<Device>();
            HashSet<int> added = new HashSet<int>();
            foreach (EfficiencyItem item in items.OrderBy(item => item.Device.Type).ThenBy(item => item.Priority))
            {
                if (added.Add(item.Device.Id))
                {
                    devices.Add(item.Device);
                }
            }

            List<int> nodeNumbers = controlPoint.Nodes
                .Select(node => node.Number)
                .ToList();
            return new HourlyGroup(controlPoint, nodeNumbers, devices);
        }
    }
}
