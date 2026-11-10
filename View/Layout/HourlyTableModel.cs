using System;
using System.Collections.Generic;

using VrdoPdg.Model.Entities;
using VrdoPdg.Model.Regime;
using VrdoPdg.Model.Services;

namespace VrdoPdg.View.Layout
{
    /// <summary>
    /// Данные таблиц по часам: схема таблиц из файла «Анализ.xlsx» и источники значений —
    /// один для предпросмотра или два для сравнения расчетов.
    /// </summary>
    internal sealed class HourlyTableModel
    {
        /// <summary>
        /// Создает данные таблиц.
        /// </summary>
        /// <param name="layout">Схема таблиц.</param>
        /// <param name="sources">Источники значений.</param>
        public HourlyTableModel(HourlyLayout layout, IReadOnlyList<HourlyValueSource> sources)
        {
            Layout = layout ?? throw new ArgumentNullException(nameof(layout));
            Sources = sources ?? throw new ArgumentNullException(nameof(sources));
        }

        /// <summary>
        /// Схема таблиц.
        /// </summary>
        public HourlyLayout Layout { get; }

        /// <summary>
        /// Источники значений.
        /// </summary>
        public IReadOnlyList<HourlyValueSource> Sources { get; }

        /// <summary>
        /// Энергосистемы схемы таблиц.
        /// </summary>
        public IReadOnlyList<HourlyLayoutPowerSystem> PowerSystems => Layout.PowerSystems;

        /// <summary>
        /// Создает данные предпросмотра режима до или после расчета.
        /// </summary>
        /// <param name="layout">Схема таблиц.</param>
        /// <param name="session">Исходные данные сеанса.</param>
        /// <param name="regime">Режим по часам.</param>
        /// <returns>Данные таблиц.</returns>
        public static HourlyTableModel CreatePreview(HourlyLayout layout, SessionData session, RegimeSnapshot regime)
        {
            return new HourlyTableModel(layout, new[] { new HourlyValueSource(regime, session) });
        }

        /// <summary>
        /// Создает данные сравнения режимов после двух расчетов.
        /// </summary>
        /// <param name="layout">Схема таблиц.</param>
        /// <param name="first">Первый расчет.</param>
        /// <param name="second">Второй расчет.</param>
        /// <returns>Данные таблиц.</returns>
        public static HourlyTableModel CreateComparison(
            HourlyLayout layout,
            CalculationResult first,
            CalculationResult second)
        {
            HourlyValueSource[] sources =
            {
                new HourlyValueSource(first.After, first.Session),
                new HourlyValueSource(second.After, second.Session)
            };
            return new HourlyTableModel(layout, sources);
        }

        /// <summary>
        /// Возвращает границы напряжения на шинах: график КП БД, в который входит узел,
        /// или границы схемы таблиц для остальных узлов.
        /// </summary>
        /// <param name="source">Источник значений.</param>
        /// <param name="nodeNumber">Номер узла.</param>
        /// <param name="hour">Час суток (1–24).</param>
        /// <returns>Границы напряжения.</returns>
        public VoltageBounds GetBounds(HourlyValueSource source, int nodeNumber, int hour)
        {
            return source.GetScheduleBounds(nodeNumber, hour)
                ?? new VoltageBounds(0, hour, Layout.LowerVoltageBound, Layout.UpperVoltageBound, DateTime.MinValue);
        }
    }
}
