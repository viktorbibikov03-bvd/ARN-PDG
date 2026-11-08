using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

using Guna.UI2.WinForms;

using VrdoPdg.View.Theme;

namespace VrdoPdg.View.Controls
{
    /// <summary>
    /// Карточка исходного файла окна «Загрузка данных»: формат и имя файла, путь и статус загрузки.
    /// Незагруженный файл — серый фон и пунктирная рамка, загруженный — зеленые фон и рамка с флажком.
    /// </summary>
    internal sealed class FileCard : UserControl
    {
        /// <summary>
        /// Рамка карточки.
        /// </summary>
        private readonly Guna2Panel _frame;

        /// <summary>
        /// Значок карточки.
        /// </summary>
        private readonly Guna2Panel _iconBox;

        /// <summary>
        /// Изображение значка.
        /// </summary>
        private readonly PictureBox _icon;

        /// <summary>
        /// Наименование файла: «Мегаточка» или «Топология сети».
        /// </summary>
        private readonly Label _titleLabel;

        /// <summary>
        /// Расширение файла.
        /// </summary>
        private readonly Label _extensionLabel;

        /// <summary>
        /// Имя файла или «Файл не выбран».
        /// </summary>
        private readonly Label _nameLabel;

        /// <summary>
        /// Путь к файлу или статус загрузки.
        /// </summary>
        private readonly Label _statusLabel;

        /// <summary>
        /// Кнопка «Выбрать файл» / «Заменить».
        /// </summary>
        private readonly Guna2Button _chooseButton;

        /// <summary>
        /// Подсказка: предлагаемый путь к файлу.
        /// </summary>
        private string _hintPath = string.Empty;

        /// <summary>
        /// Создает карточку файла.
        /// </summary>
        public FileCard()
        {
            BackColor = Palette.Surface;
            Size = new Size(470, 96);

            _frame = new Guna2Panel
            {
                Dock = DockStyle.Fill,
                BorderRadius = 14,
                BorderThickness = 1,
                UseTransparentBackground = true
            };

            _iconBox = new Guna2Panel
            {
                Location = new Point(16, 24),
                Size = new Size(46, 46),
                BorderRadius = 10,
                UseTransparentBackground = true
            };
            _icon = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.CenterImage,
                BackColor = Color.Transparent
            };
            _iconBox.Controls.Add(_icon);

            _titleLabel = CreateLabel(new Point(78, 14), UiFonts.Semibold(11.5f), Palette.Text);
            _extensionLabel = CreateLabel(new Point(200, 16), UiFonts.Monospace(10f), Palette.TextSecondary);
            _nameLabel = CreateLabel(new Point(78, 40), UiFonts.Monospace(9.5f), Palette.TextSecondary);
            _statusLabel = CreateLabel(new Point(78, 62), UiFonts.Regular(9f), Palette.TextSecondary);
            _statusLabel.AutoSize = false;
            _statusLabel.AutoEllipsis = true;
            _statusLabel.Size = new Size(240, 20);
            _statusLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            _chooseButton = new Guna2Button
            {
                Size = new Size(140, 36),
                Location = new Point(316, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Text = "Выбрать файл"
            };
            UiStyle.ApplyOutlined(_chooseButton, IconGlyph.Upload);
            _chooseButton.Click += ChooseButton_Click;

            _frame.Controls.Add(_chooseButton);
            _frame.Controls.Add(_statusLabel);
            _frame.Controls.Add(_nameLabel);
            _frame.Controls.Add(_extensionLabel);
            _frame.Controls.Add(_titleLabel);
            _frame.Controls.Add(_iconBox);
            Controls.Add(_frame);
            UpdateView();
        }

        /// <summary>
        /// Возникает при нажатии кнопки «Выбрать файл» / «Заменить».
        /// </summary>
        public event EventHandler? ChooseRequested;

        /// <summary>
        /// Наименование файла: «Мегаточка» или «Топология сети».
        /// </summary>
        public string Title
        {
            get
            {
                return _titleLabel.Text;
            }

            set
            {
                _titleLabel.Text = value;
                _extensionLabel.Left = _titleLabel.Right + 12;
            }
        }

        /// <summary>
        /// Расширение файла, например «.mptsmz».
        /// </summary>
        public string Extension
        {
            get
            {
                return _extensionLabel.Text;
            }

            set
            {
                _extensionLabel.Text = value;
            }
        }

        /// <summary>
        /// Путь к загруженному файлу; <c>null</c>, если файл не выбран.
        /// </summary>
        public string? FilePath { get; private set; }

        /// <summary>
        /// Признак загруженного файла.
        /// </summary>
        public bool IsLoaded => FilePath != null;

        /// <summary>
        /// Предлагаемый путь к файлу.
        /// </summary>
        public string HintPath => _hintPath;

        /// <summary>
        /// Устанавливает предлагаемый путь к файлу, выводимый для незагруженного файла.
        /// </summary>
        /// <param name="path">Предлагаемый путь.</param>
        public void SetHint(string? path)
        {
            _hintPath = path ?? string.Empty;
            UpdateView();
        }

        /// <summary>
        /// Отмечает файл как загруженный.
        /// </summary>
        /// <param name="path">Путь к файлу.</param>
        public void SetLoaded(string path)
        {
            FilePath = path;
            UpdateView();
        }

        /// <summary>
        /// Создает надпись карточки.
        /// </summary>
        /// <param name="location">Положение.</param>
        /// <param name="font">Шрифт.</param>
        /// <param name="color">Цвет текста.</param>
        /// <returns>Надпись.</returns>
        private static Label CreateLabel(Point location, Font font, Color color)
        {
            return new Label
            {
                AutoSize = true,
                Location = location,
                Font = font,
                ForeColor = color,
                BackColor = Color.Transparent
            };
        }

        /// <summary>
        /// Обновляет оформление карточки по состоянию загрузки.
        /// </summary>
        private void UpdateView()
        {
            if (IsLoaded)
            {
                _frame.FillColor = Palette.AccentDark;
                _frame.BorderColor = Palette.Accent;
                _frame.BorderStyle = DashStyle.Solid;
                _iconBox.FillColor = Palette.Accent;
                _icon.Image = IconFactory.Create(IconGlyph.CheckMark, Palette.OnAccent, 24);
                _extensionLabel.ForeColor = Palette.Accent;
                _nameLabel.Text = Path.GetFileName(FilePath);
                _nameLabel.ForeColor = Palette.Text;
                _statusLabel.Text = "Загружено · " + FilePath;
                _chooseButton.Text = "Заменить";
            }
            else
            {
                _frame.FillColor = Palette.Surface;
                _frame.BorderColor = Palette.Border;
                _frame.BorderStyle = DashStyle.Dash;
                _iconBox.FillColor = Palette.SurfaceSunken;
                _icon.Image = IconFactory.Create(IconGlyph.Document, Palette.TextSecondary, 24);
                _extensionLabel.ForeColor = Palette.TextSecondary;
                _nameLabel.Text = "Файл не выбран";
                _nameLabel.ForeColor = Palette.TextSecondary;
                _statusLabel.Text = _hintPath;
                _chooseButton.Text = "Выбрать файл";
            }
        }

        /// <summary>
        /// Передает запрос выбора файла окну.
        /// </summary>
        /// <param name="sender">Кнопка.</param>
        /// <param name="e">Аргументы события.</param>
        private void ChooseButton_Click(object? sender, EventArgs e)
        {
            ChooseRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
