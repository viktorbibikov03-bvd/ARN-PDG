using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

using VrdoPdg.Model;
using VrdoPdg.Model.Services;
using VrdoPdg.View.Theme;

namespace VrdoPdg.View
{
    /// <summary>
    /// Главная форма: панель инструментов, строка хода расчета и панель протокола расчета.
    /// Загрузка данных и расчет выполняются асинхронно в рабочем потоке ASTRALib.
    /// </summary>
    internal sealed partial class MainForm : Form
    {
        /// <summary>
        /// Заголовок окон сообщений.
        /// </summary>
        private const string MessageCaption = "АС «ВРДО ПДГ»";

        /// <summary>
        /// Сервисы прикладного уровня.
        /// </summary>
        private readonly AppServices _services;

        /// <summary>
        /// Исходные данные текущего сеанса.
        /// </summary>
        private SessionData? _session;

        /// <summary>
        /// Последний завершенный расчет на текущих исходных данных.
        /// </summary>
        private CalculationResult? _lastResult;

        /// <summary>
        /// Источник токена остановки расчета.
        /// </summary>
        private CancellationTokenSource? _cancellation;

        /// <summary>
        /// Признак выполнения загрузки данных.
        /// </summary>
        private bool _isLoading;

        /// <summary>
        /// Признак выполнения расчета.
        /// </summary>
        private bool _isCalculating;

        /// <summary>
        /// Признак запроса закрытия программы во время расчета.
        /// </summary>
        private bool _isCloseRequested;

        /// <summary>
        /// Открытая форма «Напряжение по часам».
        /// </summary>
        private HourlyVoltageForm? _previewForm;

        /// <summary>
        /// Открытая форма «Архив расчетов».
        /// </summary>
        private ArchiveForm? _archiveForm;

        /// <summary>
        /// Открытое окно справки.
        /// </summary>
        private HelpForm? _helpForm;

        /// <summary>
        /// Создает главную форму.
        /// </summary>
        /// <param name="services">Сервисы прикладного уровня.</param>
        public MainForm(AppServices services)
        {
            InitializeComponent();
            _services = services ?? throw new ArgumentNullException(nameof(services));
            ApplyTheme();
            _protocolPanel.Attach(services.Protocol);
            UpdateCommands();
        }

        /// <summary>
        /// Включает темный заголовок окна.
        /// </summary>
        /// <param name="e">Аргументы события.</param>
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            WindowTheme.ApplyDarkTitleBar(this);
        }

        /// <summary>
        /// Оформляет элементы панели инструментов.
        /// </summary>
        private void ApplyTheme()
        {
            UiStyle.ApplyFlat(_loadButton, IconGlyph.Upload);
            UiStyle.ApplyFlat(_saveButton, IconGlyph.Save);
            UiStyle.ApplyFlat(_helpButton, IconGlyph.Help);
            UiStyle.ApplyOutlined(_previewButton, IconGlyph.View);
            UiStyle.ApplyOutlined(_archiveButton, IconGlyph.Archive);
            UiStyle.ApplyFlat(_settingsButton, IconGlyph.Settings);
            _progressIcon.Image = IconFactory.Create(IconGlyph.Refresh, Palette.Purple, 18);
            SetRunButtonState(false);
            UpdateIndicators();
        }

        /// <summary>
        /// Заменяет кнопку «Запустить» кнопкой «Остановить» и обратно.
        /// </summary>
        /// <param name="isCalculating">Признак выполнения расчета.</param>
        private void SetRunButtonState(bool isCalculating)
        {
            if (isCalculating)
            {
                _runButton.Text = "Остановить";
                UiStyle.ApplyDanger(_runButton, IconGlyph.Stop);
            }
            else
            {
                _runButton.Text = "Запустить";
                UiStyle.ApplyPrimary(_runButton, IconGlyph.Play);
            }
        }

        /// <summary>
        /// Обновляет индикаторы загруженных файлов: зеленый с флажком — файл загружен, серый — не загружен.
        /// </summary>
        private void UpdateIndicators()
        {
            bool isLoaded = _session != null;
            Color foreColor = isLoaded
                ? Palette.Accent
                : Palette.TextMuted;
            Color fillColor = isLoaded
                ? Palette.AccentDark
                : Palette.SurfaceRaised;
            char? glyph = isLoaded
                ? IconGlyph.CheckMark
                : (char?)null;
            UiStyle.ApplyPill(_megapointIndicator, foreColor, fillColor, glyph);
            UiStyle.ApplyPill(_smpSmzuIndicator, foreColor, fillColor, glyph);
        }

        /// <summary>
        /// Устанавливает доступность команд по состоянию системы (таблица 2.2.4.2).
        /// </summary>
        private void UpdateCommands()
        {
            bool isBusy = _isLoading || _isCalculating;
            bool isLoaded = _session != null;
            _loadButton.Enabled = !isBusy;
            _saveButton.Enabled = !isBusy && _lastResult != null;
            _runButton.Enabled = _isCalculating
                ? _cancellation != null && !_cancellation.IsCancellationRequested
                : !_isLoading && isLoaded;
            _previewButton.Enabled = isLoaded && !_isLoading;
            _archiveButton.Enabled = true;
            _settingsButton.Enabled = !isBusy;
        }

        /// <summary>
        /// Показывает или скрывает строку хода расчета.
        /// </summary>
        /// <param name="isVisible">Признак отображения.</param>
        private void ShowProgress(bool isVisible)
        {
            UpdateProgress(0);
            _progressPanel.Visible = isVisible;
        }

        /// <summary>
        /// Обновляет процент выполнения расчета.
        /// </summary>
        /// <param name="percent">Процент выполнения.</param>
        private void UpdateProgress(int percent)
        {
            int value = Math.Max(0, Math.Min(100, percent));
            _progressBar.Value = value;
            _progressPercentLabel.Text = value + "%";
        }

        /// <summary>
        /// Выводит сообщение об ошибке.
        /// </summary>
        /// <param name="message">Сообщение.</param>
        /// <param name="details">Пояснение.</param>
        private void ShowError(string message, string? details)
        {
            string text = string.IsNullOrEmpty(details)
                ? message
                : message + Environment.NewLine + Environment.NewLine + details;
            MessageBox.Show(this, text, MessageCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// Запрашивает подтверждение и останавливает расчет после текущего расчета УР.
        /// </summary>
        private void RequestStop()
        {
            if (_cancellation == null || _cancellation.IsCancellationRequested)
            {
                return;
            }

            bool isConfirmed = ConfirmationForm.Confirm(
                this,
                "Остановить расчёт?",
                "Расчёт будет остановлен после завершения текущего расчёта установившегося режима. "
                    + "Результаты незавершённого расчёта в архив не сохраняются.",
                "Остановить",
                "Продолжить расчёт",
                true);
            if (isConfirmed && _cancellation != null)
            {
                _cancellation.Cancel();
                _runButton.Text = "Остановка…";
                UpdateCommands();
            }
        }

        /// <summary>
        /// Загружает исходные данные: открывает окно «Загрузка данных» и передает пути
        /// к файлам сервису загрузки данных.
        /// </summary>
        /// <param name="sender">Кнопка «Загрузить».</param>
        /// <param name="e">Аргументы события.</param>
        private async void LoadButton_Click(object? sender, EventArgs e)
        {
            if (_isLoading || _isCalculating)
            {
                return;
            }

            string? megapointPath;
            string? smpSmzuPath;
            using (LoadDataForm form = new LoadDataForm(
                _session?.MegapointPath,
                _session?.SmpSmzuPath,
                _services.Settings.ResultsDirectory))
            {
                if (form.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                megapointPath = form.MegapointPath;
                smpSmzuPath = form.SmpSmzuPath;
            }

            if (megapointPath == null || smpSmzuPath == null)
            {
                return;
            }

            _isLoading = true;
            UpdateCommands();
            UseWaitCursor = true;
            try
            {
                SessionData session = await _services.Worker.Run(
                    () => _services.DataLoad.Load(megapointPath, smpSmzuPath));
                _session = session;
                _lastResult = null;
                _previewForm?.Close();
            }
            catch (VrdoException exception)
            {
                ShowError(exception.Message, exception.Details);
            }
            catch (Exception exception)
            {
                ShowError("Ошибка загрузки исходных данных", exception.Message);
            }
            finally
            {
                _isLoading = false;
                UseWaitCursor = false;
                UpdateIndicators();
                UpdateCommands();
            }
        }

        /// <summary>
        /// Запускает расчет или останавливает выполняющийся расчет.
        /// </summary>
        /// <param name="sender">Кнопка «Запустить» / «Остановить».</param>
        /// <param name="e">Аргументы события.</param>
        private async void RunButton_Click(object? sender, EventArgs e)
        {
            if (_isCalculating)
            {
                RequestStop();
                return;
            }

            SessionData? session = _session;
            if (session == null)
            {
                MessageBox.Show(
                    this,
                    "Необходимо загрузить данные форматов .csv и .mptsmz.",
                    MessageCaption,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                LoadButton_Click(sender, e);
                return;
            }

            CancellationTokenSource cancellation = new CancellationTokenSource();
            CancellationToken token = cancellation.Token;
            _cancellation = cancellation;
            _isCalculating = true;
            SetRunButtonState(true);
            ShowProgress(true);
            UpdateCommands();
            Progress<int> progress = new Progress<int>(UpdateProgress);
            try
            {
                CalculationResult result = await _services.Worker.Run(
                    () => _services.Regulation.Run(session, progress, token));
                _lastResult = result;
                if (_previewForm != null && !_previewForm.IsDisposed)
                {
                    _previewForm.SetResult(result);
                }

                if (_archiveForm != null && !_archiveForm.IsDisposed)
                {
                    _archiveForm.RefreshResults();
                }
            }
            catch (OperationCanceledException)
            {
                // Остановка расчета записана в протокол сервисом ввода режима.
            }
            catch (VrdoException exception)
            {
                ShowError(exception.Message, exception.Details);
            }
            catch (Exception exception)
            {
                ShowError("Ошибка расчета", exception.Message);
            }
            finally
            {
                _isCalculating = false;
                _cancellation = null;
                cancellation.Dispose();
                SetRunButtonState(false);
                ShowProgress(false);
                UpdateCommands();
            }

            if (_isCloseRequested)
            {
                Close();
            }
        }

        /// <summary>
        /// Сохраняет результаты последнего завершенного расчета в файл smp_smzu.csv.
        /// </summary>
        /// <param name="sender">Кнопка «Сохранить».</param>
        /// <param name="e">Аргументы события.</param>
        private void SaveButton_Click(object? sender, EventArgs e)
        {
            CalculationResult? result = _lastResult;
            if (result == null)
            {
                MessageBox.Show(
                    this,
                    "Результаты для сохранения отсутствуют: расчёт не выполнялся.",
                    MessageCaption,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            string defaultPath = ResultExportService.GetDefaultPath(result);
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Title = "Сохранение результатов расчёта №" + result.Number;
                dialog.Filter = "Топология сети (*.csv)|*.csv";
                dialog.DefaultExt = "csv";
                dialog.AddExtension = true;
                dialog.OverwritePrompt = true;
                string? directory = Path.GetDirectoryName(defaultPath);
                if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory))
                {
                    dialog.InitialDirectory = directory;
                }

                dialog.FileName = Path.GetFileName(defaultPath);
                while (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        int count = _services.ResultExport.Export(result, dialog.FileName);
                        MessageBox.Show(
                            this,
                            "Результаты расчёта №" + result.Number + " сохранены."
                                + Environment.NewLine + dialog.FileName
                                + Environment.NewLine + "Добавлено записей: " + count,
                            MessageCaption,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                        return;
                    }
                    catch (DataFileException exception)
                    {
                        ShowError(exception.Message, exception.Details);
                    }
                }
            }
        }

        /// <summary>
        /// Открывает окно справки.
        /// </summary>
        /// <param name="sender">Кнопка «Справка».</param>
        /// <param name="e">Аргументы события.</param>
        private void HelpButton_Click(object? sender, EventArgs e)
        {
            if (_helpForm == null || _helpForm.IsDisposed)
            {
                _helpForm = new HelpForm();
                _helpForm.Show(this);
            }
            else
            {
                _helpForm.Activate();
            }
        }

        /// <summary>
        /// Открывает форму «Напряжение по часам».
        /// </summary>
        /// <param name="sender">Кнопка «Предпросмотр».</param>
        /// <param name="e">Аргументы события.</param>
        private void PreviewButton_Click(object? sender, EventArgs e)
        {
            SessionData? session = _session;
            if (session == null)
            {
                return;
            }

            if (_previewForm != null && !_previewForm.IsDisposed)
            {
                _previewForm.Activate();
                return;
            }

            try
            {
                _previewForm = new HourlyVoltageForm(session, _lastResult, _services.HourlyLayout);
                _previewForm.Show(this);
            }
            catch (Exception exception)
            {
                ShowError(DataLoadService.DatabaseErrorMessage, exception.Message);
            }
        }

        /// <summary>
        /// Открывает форму «Архив расчетов».
        /// </summary>
        /// <param name="sender">Кнопка «Архив».</param>
        /// <param name="e">Аргументы события.</param>
        private void ArchiveButton_Click(object? sender, EventArgs e)
        {
            if (_archiveForm != null && !_archiveForm.IsDisposed)
            {
                _archiveForm.RefreshResults();
                _archiveForm.Activate();
                return;
            }

            _archiveForm = new ArchiveForm(_services.Archive, _services.HourlyLayout);
            _archiveForm.Show(this);
        }

        /// <summary>
        /// Открывает окно «Таблица эффективности».
        /// </summary>
        /// <param name="sender">Кнопка «Настройка».</param>
        /// <param name="e">Аргументы события.</param>
        private void SettingsButton_Click(object? sender, EventArgs e)
        {
            if (_isCalculating || _isLoading)
            {
                return;
            }

            try
            {
                using (EfficiencyTableForm form = new EfficiencyTableForm(_services.EfficiencyTable))
                {
                    form.ShowDialog(this);
                }
            }
            catch (Exception exception)
            {
                ShowError(DataLoadService.DatabaseErrorMessage, exception.Message);
            }
        }

        /// <summary>
        /// При закрытии во время расчета запрашивает подтверждение и останавливает расчет.
        /// </summary>
        /// <param name="sender">Форма.</param>
        /// <param name="e">Аргументы события.</param>
        private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (!_isCalculating && !_isLoading)
            {
                return;
            }

            e.Cancel = true;
            if (_isCalculating && !_isCloseRequested)
            {
                bool isConfirmed = ConfirmationForm.Confirm(
                    this,
                    "Закрыть программу?",
                    "Идёт расчёт. Расчёт будет остановлен, результаты незавершённого расчёта не сохранятся.",
                    "Закрыть",
                    "Продолжить расчёт",
                    true);
                if (isConfirmed)
                {
                    _isCloseRequested = true;
                    _cancellation?.Cancel();
                    UpdateCommands();
                }
            }
        }
    }
}
