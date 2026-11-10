using System;
using System.Collections.Generic;
using System.Linq;

using VrdoPdg.Model.Entities;
using VrdoPdg.Model.Regime;

namespace VrdoPdg.Model.Services
{
    /// <summary>
    /// Сеанс и архив расчетов: хранит в оперативной памяти исходные данные текущего сеанса
    /// и завершенные расчеты до закрытия программы.
    /// </summary>
    public sealed class CalculationArchiveService
    {
        /// <summary>
        /// Объект синхронизации доступа к сеансу и архиву.
        /// </summary>
        private readonly object _syncRoot = new object();

        /// <summary>
        /// Завершенные расчеты в порядке выполнения.
        /// </summary>
        private readonly List<CalculationResult> _results = new List<CalculationResult>();

        /// <summary>
        /// Исходные данные текущего сеанса.
        /// </summary>
        private SessionData? _currentSession;

        /// <summary>
        /// Исходные данные текущего сеанса; <c>null</c>, если данные не загружены.
        /// </summary>
        public SessionData? CurrentSession
        {
            get
            {
                lock (_syncRoot)
                {
                    return _currentSession;
                }
            }
        }

        /// <summary>
        /// Устанавливает исходные данные текущего сеанса
        /// </summary>
        /// <param name="session">Исходные данные сеанса</param>
        public void SetSession(SessionData session)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            lock (_syncRoot)
            {
                _currentSession = session;
            }
        }

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
        /// Возвращает последний завершенный расчет, выполненный на заданных исходных данных.
        /// </summary>
        /// <param name="session">Исходные данные сеанса.</param>
        /// <returns>Расчет или <c>null</c>, если расчеты не выполнялись.</returns>
        public CalculationResult? GetLastResult(SessionData session)
        {
            lock (_syncRoot)
            {
                return _results.LastOrDefault(result => ReferenceEquals(result.Session, session));
            }
        }

        /// <summary>
        /// Помещает завершенный расчет в архив и присваивает ему порядковый номер.
        /// </summary>
        /// <param name="session">Исходные данные сеанса.</param>
        /// <param name="startedAt">Дата и время запуска.</param>
        /// <param name="finishedAt">Дата и время окончания.</param>
        /// <param name="efficiencyTables">Использованные таблицы эффективности.</param>
        /// <param name="after">Режим после расчета.</param>
        /// <param name="unresolvedViolations">КП и часы с невведенным режимом.</param>
        /// <returns>Расчет, помещенный в архив.</returns>
        public CalculationResult Add(
            SessionData session,
            DateTime startedAt,
            DateTime finishedAt,
            IReadOnlyDictionary<int, IReadOnlyList<EfficiencyItem>> efficiencyTables,
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
                    efficiencyTables,
                    after,
                    unresolvedViolations);
                _results.Add(result);
                return result;
            }
        }
    }
}
