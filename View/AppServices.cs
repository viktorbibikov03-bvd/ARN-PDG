using System;

using VrdoPdg.Model.Protocol;
using VrdoPdg.Model.Services;
using VrdoPdg.View.Threading;

namespace VrdoPdg.View
{
    /// <summary>
    /// Сервисы прикладного уровня и общие объекты, передаваемые формам.
    /// </summary>
    internal sealed class AppServices
    {
        /// <summary>
        /// Создает набор сервисов.
        /// </summary>
        /// <param name="settings">Настройки системы.</param>
        /// <param name="worker">Рабочий поток обращений к ASTRALib.</param>
        /// <param name="protocol">Протокол расчета.</param>
        /// <param name="archive">Сеанс и архив расчетов.</param>
        /// <param name="dataLoad">Сервис загрузки данных.</param>
        /// <param name="regulation">Сервис ввода режима по напряжению в допустимую область.</param>
        /// <param name="resultExport">Сервис сохранения результатов.</param>
        /// <param name="efficiencyTable">Сервис таблицы эффективности.</param>
        public AppServices(
            AppSettings settings,
            StaWorker worker,
            CalculationProtocol protocol,
            CalculationArchiveService archive,
            DataLoadService dataLoad,
            VoltageRegulationService regulation,
            ResultExportService resultExport,
            EfficiencyTableService efficiencyTable)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            Worker = worker ?? throw new ArgumentNullException(nameof(worker));
            Protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
            Archive = archive ?? throw new ArgumentNullException(nameof(archive));
            DataLoad = dataLoad ?? throw new ArgumentNullException(nameof(dataLoad));
            Regulation = regulation ?? throw new ArgumentNullException(nameof(regulation));
            ResultExport = resultExport ?? throw new ArgumentNullException(nameof(resultExport));
            EfficiencyTable = efficiencyTable ?? throw new ArgumentNullException(nameof(efficiencyTable));
        }

        /// <summary>
        /// Настройки системы.
        /// </summary>
        public AppSettings Settings { get; }

        /// <summary>
        /// Рабочий поток обращений к ASTRALib.
        /// </summary>
        public StaWorker Worker { get; }

        /// <summary>
        /// Протокол расчета.
        /// </summary>
        public CalculationProtocol Protocol { get; }

        /// <summary>
        /// Сеанс и архив расчетов.
        /// </summary>
        public CalculationArchiveService Archive { get; }

        /// <summary>
        /// Сервис загрузки данных.
        /// </summary>
        public DataLoadService DataLoad { get; }

        /// <summary>
        /// Сервис ввода режима по напряжению в допустимую область.
        /// </summary>
        public VoltageRegulationService Regulation { get; }

        /// <summary>
        /// Сервис сохранения результатов.
        /// </summary>
        public ResultExportService ResultExport { get; }

        /// <summary>
        /// Сервис таблицы эффективности.
        /// </summary>
        public EfficiencyTableService EfficiencyTable { get; }
    }
}
