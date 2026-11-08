using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using VrdoPdg.Model;
using VrdoPdg.Model.Services;
using VrdoPdg.View.Controls;
using VrdoPdg.View.Theme;

namespace VrdoPdg.View
{
    /// <summary>
    /// Модальное окно «Загрузка данных»: выбор файла МТ (*.mptsmz) и файла топологии (*.csv).
    /// </summary>
    internal sealed partial class LoadDataForm : Form
    {
        /// <summary>
        /// Имя файла МТ по умолчанию.
        /// </summary>
        private const string MegapointFileName = "smzu_mega_act.mptsmz";

        /// <summary>
        /// Имя файла топологии по умолчанию.
        /// </summary>
        private const string SmpSmzuFileName = "smp_smzu.csv";

        /// <summary>
        /// Каталог файла топологии внутри каталога результатов суток.
        /// </summary>
        private const string SmpSmzuDirectory = "BarsCSV";

        /// <summary>
        /// Создает окно для конструктора форм.
        /// </summary>
        public LoadDataForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Создает окно загрузки данных.
        /// </summary>
        /// <param name="megapointPath">Путь к ранее загруженному файлу МТ.</param>
        /// <param name="smpSmzuPath">Путь к ранее загруженному файлу smp_smzu.csv.</param>
        /// <param name="resultsDirectory">Каталог результатов ПО Барс-МДП.</param>
        public LoadDataForm(string? megapointPath, string? smpSmzuPath, string resultsDirectory)
            : this()
        {
            _megapointCard.Title = "Мегаточка";
            _megapointCard.Extension = SourceFileValidator.MegapointExtension;
            _smpSmzuCard.Title = "Топология сети";
            _smpSmzuCard.Extension = SourceFileValidator.SmpSmzuExtension;

            string? dayDirectory = FindLatestDayDirectory(resultsDirectory);
            if (dayDirectory != null)
            {
                _megapointCard.SetHint(Path.Combine(dayDirectory, MegapointFileName));
                _smpSmzuCard.SetHint(Path.Combine(dayDirectory, SmpSmzuDirectory, SmpSmzuFileName));
            }

            if (megapointPath != null)
            {
                _megapointCard.SetLoaded(megapointPath);
            }

            if (smpSmzuPath != null)
            {
                _smpSmzuCard.SetLoaded(smpSmzuPath);
            }

            _closeButton.Text = string.Empty;
            UiStyle.ApplyFlat(_closeButton, IconGlyph.Close);
            _closeButton.ImageAlign = HorizontalAlignment.Center;
            UiStyle.ApplyOutlined(_cancelButton, null);
            UiStyle.ApplyPrimary(_doneButton, null);
            _hintIcon.Image = IconFactory.Create(IconGlyph.Info, Palette.TextSecondary, 20);
            _borderlessForm.ShadowColor = Palette.Background;
            UpdateDoneButton();
        }

        /// <summary>
        /// Путь к выбранному файлу МТ.
        /// </summary>
        public string? MegapointPath => _megapointCard.FilePath;

        /// <summary>
        /// Путь к выбранному файлу smp_smzu.csv.
        /// </summary>
        public string? SmpSmzuPath => _smpSmzuCard.FilePath;

        /// <summary>
        /// Ищет последний по времени изменения каталог результатов суток.
        /// </summary>
        /// <param name="resultsDirectory">Каталог результатов ПО Барс-МДП.</param>
        /// <returns>Каталог суток или <c>null</c>, если каталог результатов недоступен.</returns>
        private static string? FindLatestDayDirectory(string resultsDirectory)
        {
            try
            {
                return Directory.Exists(resultsDirectory)
                    ? Directory.GetDirectories(resultsDirectory)
                        .OrderByDescending(Directory.GetLastWriteTime)
                        .FirstOrDefault()
                    : null;
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
            {
                return null;
            }
        }

        /// <summary>
        /// Открывает диалог выбора файла и проверяет выбранный файл.
        /// Если файл не прошел проверку, карточка сохраняет прежнее состояние.
        /// </summary>
        /// <param name="card">Карточка файла.</param>
        /// <param name="filter">Фильтр диалога по расширению.</param>
        /// <param name="extension">Ожидаемое расширение.</param>
        /// <param name="hintPath">Предлагаемый путь к файлу.</param>
        private void ChooseFile(FileCard card, string filter, string extension, string hintPath)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Выбор файла: " + card.Title;
                dialog.Filter = filter;
                dialog.CheckFileExists = false;
                string currentPath = hintPath;
                string? directory = string.IsNullOrEmpty(currentPath)
                    ? null
                    : Path.GetDirectoryName(currentPath);
                if (directory != null && Directory.Exists(directory))
                {
                    dialog.InitialDirectory = directory;
                    dialog.FileName = Path.GetFileName(currentPath);
                }

                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    SourceFileValidator.Validate(dialog.FileName, extension);
                    card.SetLoaded(dialog.FileName);
                }
                catch (DataFileException exception)
                {
                    MessageBox.Show(
                        this,
                        exception.Message + Environment.NewLine + exception.Details,
                        "Загрузка данных",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }

            UpdateDoneButton();
        }

        /// <summary>
        /// Делает кнопку «Готово» доступной, когда выбраны оба файла.
        /// </summary>
        private void UpdateDoneButton()
        {
            _doneButton.Enabled = _megapointCard.IsLoaded && _smpSmzuCard.IsLoaded;
        }

        /// <summary>
        /// Выбирает файл МТ.
        /// </summary>
        /// <param name="sender">Карточка.</param>
        /// <param name="e">Аргументы события.</param>
        private void MegapointCard_ChooseRequested(object? sender, EventArgs e)
        {
            ChooseFile(
                _megapointCard,
                "Мегаточка СМЗУ (*.mptsmz)|*.mptsmz",
                SourceFileValidator.MegapointExtension,
                _megapointCard.FilePath ?? _megapointCard.HintPath);
        }

        /// <summary>
        /// Выбирает файл smp_smzu.csv.
        /// </summary>
        /// <param name="sender">Карточка.</param>
        /// <param name="e">Аргументы события.</param>
        private void SmpSmzuCard_ChooseRequested(object? sender, EventArgs e)
        {
            ChooseFile(
                _smpSmzuCard,
                "Топология сети (*.csv)|*.csv",
                SourceFileValidator.SmpSmzuExtension,
                _smpSmzuCard.FilePath ?? _smpSmzuCard.HintPath);
        }

        /// <summary>
        /// Подтверждает выбор файлов.
        /// </summary>
        /// <param name="sender">Кнопка «Готово».</param>
        /// <param name="e">Аргументы события.</param>
        private void DoneButton_Click(object? sender, EventArgs e)
        {
            if (_megapointCard.IsLoaded && _smpSmzuCard.IsLoaded)
            {
                DialogResult = DialogResult.OK;
            }
        }

        /// <summary>
        /// Закрывает окно без изменения ранее загруженных данных.
        /// </summary>
        /// <param name="sender">Кнопка «Отмена» или кнопка закрытия.</param>
        /// <param name="e">Аргументы события.</param>
        private void CancelButton_Click(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        /// <summary>
        /// Закрывает окно клавишей Esc.
        /// </summary>
        /// <param name="sender">Окно.</param>
        /// <param name="e">Аргументы события.</param>
        private void LoadDataForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }
    }
}
