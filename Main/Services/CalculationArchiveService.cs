using System;
using System.Collections.Generic;
using System.Linq;

using VrdoPdg.Model.Regime;

namespace VrdoPdg.Model.Services
{
    /// <summary>
    /// Архив расчетов: хранит в оперативной памяти завершенные расчеты до закрытия программы.
    /// </summary>
    public sealed class CalculationArchiveService
    {
        /// <summary>
        /// Объект синхронизации доступа к архиву.
        /// </summary>
        private readonly object _syncRoot = new object();

        /// <summary>
        /// Завершенные расчеты в порядке выполнения.
        /// </summary>
        private readonly List<CalculationResult> _results = new List<CalculationResult>();

        /// <summary>
        /// Возвращает завершенные расчеты в порядке выполнения.
        /// </summary>
        /// <returns>Копия списка расчетов.</returns>
        public IReadOnlyList<CalculationResult> GetResults()
        {
            lock (_syncRoot)
            {
                return _results.ToList();
            }
        }

        /// <summary>
        /// Помещает завершенный расчет в архив и присваивает ему порядковый номер.
        /// </summary>
        /// <param name="session">Исходные данные сеанса.</param>
        /// <param name="startedAt">Дата и время запуска.</param>
        /// <param name="finishedAt">Дата и время окончания.</param>
        /// <param name="after">Режим после расчета.</param>
        /// <param name="unresolvedViolations">КП и часы с невведенным режимом.</param>
        /// <returns>Расчет, помещенный в архив.</returns>
        public CalculationResult Add(
            SessionData session,
            DateTime startedAt,
            DateTime finishedAt,
            RegimeSnapshot after,
            IReadOnlyList<UnresolvedViolation> unresolvedViolations)
        {
            lock (_syncRoot)
            {
                CalculationResult result = new CalculationResult(
                    _results.Count + 1,
                    startedAt,
                    finishedAt,
                    session,
                    after,
                    unresolvedViolations);
                _results.Add(result);
                return result;
            }
        }
    }
}
