using System;
using System.Collections.Generic;

using VrdoPdg.Model.Entities;
using VrdoPdg.Model.Regime;

namespace VrdoPdg.Model.Services
{
    /// <summary>
    /// Завершенный расчет, хранящийся в архиве расчетов текущей сессии.
    /// </summary>
    public sealed class CalculationResult
    {
        /// <summary>
        /// Создает результат расчета.
        /// </summary>
        /// <param name="number">Порядковый номер расчета в сессии.</param>
        /// <param name="startedAt">Дата и время запуска.</param>
        /// <param name="finishedAt">Дата и время окончания.</param>
        /// <param name="session">Исходные данные сеанса.</param>
        /// <param name="efficiencyTables">Использованные таблицы эффективности по коду КП.</param>
        /// <param name="after">Режим после расчета.</param>
        /// <param name="unresolvedViolations">КП и часы, в которых напряжение не введено в допустимую область.</param>
        public CalculationResult(
            int number,
            DateTime startedAt,
            DateTime finishedAt,
            SessionData session,
            IReadOnlyDictionary<int, IReadOnlyList<EfficiencyItem>> efficiencyTables,
            RegimeSnapshot after,
            IReadOnlyList<UnresolvedViolation> unresolvedViolations)
        {
            Number = number;
            StartedAt = startedAt;
            FinishedAt = finishedAt;
            Session = session;
            EfficiencyTables = efficiencyTables;
            After = after;
            UnresolvedViolations = unresolvedViolations;
        }

        /// <summary>
        /// Порядковый номер расчета в сессии (№1, №2, ...).
        /// </summary>
        public int Number { get; }

        /// <summary>
        /// Дата и время запуска расчета.
        /// </summary>
        public DateTime StartedAt { get; }

        /// <summary>
        /// Дата и время окончания расчета.
        /// </summary>
        public DateTime FinishedAt { get; }

        /// <summary>
        /// Исходные данные сеанса, на которых выполнен расчет.
        /// </summary>
        public SessionData Session { get; }

        /// <summary>
        /// Таблицы эффективности, с которыми выполнен расчет, по коду КП.
        /// </summary>
        public IReadOnlyDictionary<int, IReadOnlyList<EfficiencyItem>> EfficiencyTables { get; }

        /// <summary>
        /// Режим до расчета.
        /// </summary>
        public RegimeSnapshot Before => Session.InitialRegime;

        /// <summary>
        /// Режим после расчета.
        /// </summary>
        public RegimeSnapshot After { get; }

        /// <summary>
        /// КП и часы, в которых напряжение не введено в допустимую область.
        /// </summary>
        public IReadOnlyList<UnresolvedViolation> UnresolvedViolations { get; }
    }
}
