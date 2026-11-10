using System;
using System.IO;
using System.Windows.Forms;

using VrdoPdg.Infrastructure;
using VrdoPdg.Infrastructure.Bars;
using VrdoPdg.Model;
using VrdoPdg.Model.Protocol;
using VrdoPdg.Model.Regime;
using VrdoPdg.Model.Services;
using VrdoPdg.View.Layout;
using VrdoPdg.View.Threading;

namespace VrdoPdg.View
{
    /// <summary>
    /// Точка входа АС «ВРДО ПДГ»: создание модулей инфраструктуры, сервисов прикладного уровня
    /// и главной формы
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// Время ожидания освобождения ASTRALib при завершении программы
        /// </summary>
        private static readonly TimeSpan _shutdownTimeout = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Запускает приложение
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            AppSettings settings = AppSettings.Load(Path.Combine(AppContext.BaseDirectory, AppSettings.FileName));
            ModelDataRepository repository = new ModelDataRepository(settings.GetDatabasePath());
            try
            {
                repository.CheckDatabase();
            }
            catch (Exception exception)
            {
                string details = exception is VrdoException vrdoException
                    ? exception.Message + Environment.NewLine + vrdoException.Details
                    : exception.Message;
                MessageBox.Show(
                    "Не удалось открыть базу данных " + repository.DatabasePath
                        + Environment.NewLine + details,
                    "АС «ВРДО ПДГ»",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            HourlyLayout hourlyLayout = LoadHourlyLayout(settings.GetHourlyLayoutPath());
            ObservedObjects observedObjects = hourlyLayout.GetObservedObjects();
            using (StaWorker worker = new StaWorker())
            {
                RastrAdapter rastrAdapter = new RastrAdapter();
                try
                {
                    CalculationProtocol protocol = new CalculationProtocol();
                    CalculationArchiveService archive = new CalculationArchiveService();
                    AppServices services = new AppServices(
                        settings,
                        worker,
                        protocol,
                        archive,
                        new DataLoadService(
                            rastrAdapter,
                            repository,
                            new CkModel(settings.LowerVoltageBound, settings.UpperVoltageBound),
                            new SmpSmzuCsvReader(),
                            protocol,
                            observedObjects),
                        new VoltageRegulationService(rastrAdapter, repository, archive, protocol, observedObjects),
                        new ResultExportService(new SmpSmzuCsvWriter(settings.GetCsvDecimalSeparator()), protocol),
                        new EfficiencyTableService(repository, protocol),
                        hourlyLayout);
                    Application.Run(new MainForm(services));
                }
                finally
                {
                    worker.Run(rastrAdapter.Dispose).Wait(_shutdownTimeout);
                }
            }
        }

        /// <summary>
        /// Загружает схему таблиц по часам. Если файл не найден или заполнен неправильно,
        /// выводится предупреждение и таблицы форм просмотра режима остаются пустыми
        /// </summary>
        /// <param name="path">Путь к файлу схемы таблиц.</param>
        /// <returns>Схема таблиц</returns>
        private static HourlyLayout LoadHourlyLayout(string path)
        {
            try
            {
                return HourlyLayout.Load(path);
            }
            catch (InvalidDataException exception)
            {
                MessageBox.Show(
                    "Не удалось загрузить схему таблиц по часам " + path
                        + Environment.NewLine + exception.Message
                        + Environment.NewLine + Environment.NewLine
                        + "Таблицы форм «Напряжение по часам» и «Сравнение расчётов» будут пустыми.",
                    "АС «ВРДО ПДГ»",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return HourlyLayout.Empty;
            }
        }
    }
}
