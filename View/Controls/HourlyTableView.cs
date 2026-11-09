using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using VrdoPdg.View.Layout;
using VrdoPdg.View.Theme;

namespace VrdoPdg.View.Controls
{
    /// <summary>
    /// Таблицы по часам по схеме файла «Анализ.xlsx»: переключатель энергосистем (листы «U-500»),
    /// блоки КП выбранной энергосистемы один под другим и легенда цветовой индикации.
    /// Используется формами «Напряжение по часам» и «Сравнение расчетов».
    /// </summary>
    internal sealed class HourlyTableView : UserControl
    {
        /// <summary>
        /// Расстояние между блоками КП, пикселей.
        /// </summary>
        private const int BlockSpacing = 16;

        /// <summary>
        /// Панель переключателя энергосистем.
        /// </summary>
        private readonly FlowLayoutPanel _powerSystemPanel;

        /// <summary>
        /// Панель легенды.
        /// </summary>
        private readonly FlowLayoutPanel _legendPanel;

        /// <summary>
        /// Панель блоков КП с вертикальной прокруткой.
        /// </summary>
        private readonly StableScrollPanel _blocksPanel;

        /// <summary>
        /// Сообщение об отсутствии КП в энергосистеме.
        /// </summary>
        private readonly Label _emptyLabel;

        /// <summary>
        /// Кнопки энергосистем.
        /// </summary>
        private readonly List<RoundedButton> _powerSystemButtons = new List<RoundedButton>();

        /// <summary>
        /// Данные таблиц.
        /// </summary>
        private HourlyTableModel? _model;

        /// <summary>
        /// Индекс выбранной энергосистемы.
        /// </summary>
        private int _powerSystemIndex;

        /// <summary>
        /// Создает таблицы по часам.
        /// </summary>
        public HourlyTableView()
        {
            BackColor = Palette.Background;

            Panel toolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Palette.Background,
                Padding = new Padding(0, 8, 0, 8)
            };

            RoundedPanel switchFrame = new RoundedPanel
            {
                Dock = DockStyle.Left,
                Width = 420,
                BorderRadius = 12,
                FillColor = Palette.Surface,
                BorderColor = Palette.Border,
                BorderThickness = 1,
                Padding = new Padding(4)
            };
            _powerSystemPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                WrapContents = false
            };
            switchFrame.Controls.Add(_powerSystemPanel);

            _legendPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Palette.Background,
                Padding = new Padding(16, 6, 0, 0),
                WrapContents = false
            };

            toolbar.Controls.Add(_legendPanel);
            toolbar.Controls.Add(switchFrame);

            _blocksPanel = new StableScrollPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Palette.Background
            };

            _emptyLabel = new Label
            {
                Dock = DockStyle.Top,
                Height = 56,
                Text = "В энергосистеме нет контрольных пунктов",
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Palette.TextSecondary,
                Font = UiFonts.Regular(10.5f),
                Visible = false
            };

            Controls.Add(_blocksPanel);
            Controls.Add(_emptyLabel);
            Controls.Add(toolbar);
        }

        /// <summary>
        /// Выводит данные в таблицы.
        /// </summary>
        /// <param name="model">Данные таблиц.</param>
        /// <param name="runCaptions">Подписи сравниваемых расчетов для легенды; <c>null</c> — предпросмотр.</param>
        public void Display(HourlyTableModel model, IReadOnlyList<string>? runCaptions)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            BuildPowerSystemButtons(model.PowerSystems);
            BuildLegend(runCaptions);
            int index = _powerSystemIndex < model.PowerSystems.Count
                ? _powerSystemIndex
                : 0;
            SelectPowerSystem(index);
        }

        /// <summary>
        /// Освобождает ресурсы таблиц блоков.
        /// </summary>
        /// <param name="disposing">Признак освобождения управляемых ресурсов.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ClearBlocks();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Создает кнопку-метку легенды.
        /// </summary>
        /// <param name="text">Текст.</param>
        /// <param name="foreColor">Цвет текста.</param>
        /// <param name="fillColor">Цвет заливки.</param>
        /// <returns>Метка легенды.</returns>
        private static RoundedButton CreateLegendItem(string text, Color foreColor, Color fillColor)
        {
            RoundedButton item = new RoundedButton
            {
                Text = text,
                Margin = new Padding(0, 4, 8, 0),
                Height = 28
            };
            UiStyle.ApplyPill(item, foreColor, fillColor, null);
            item.Font = UiFonts.Regular(9f);
            item.Width = TextRenderer.MeasureText(text, item.Font).Width + 24;
            return item;
        }

        /// <summary>
        /// Создает переключатель энергосистем.
        /// </summary>
        /// <param name="powerSystems">Энергосистемы схемы таблиц.</param>
        private void BuildPowerSystemButtons(IReadOnlyList<HourlyLayoutPowerSystem> powerSystems)
        {
            _powerSystemPanel.Controls.Clear();
            foreach (RoundedButton button in _powerSystemButtons)
            {
                button.Dispose();
            }

            _powerSystemButtons.Clear();
            for (int index = 0; index < powerSystems.Count; index++)
            {
                RoundedButton button = new RoundedButton
                {
                    Text = powerSystems[index].Name,
                    Tag = index,
                    Height = 36,
                    Margin = new Padding(0, 0, 4, 0)
                };
                UiStyle.ApplyFlat(button, null);
                button.Font = UiFonts.Semibold(10f);
                button.Width = TextRenderer.MeasureText(button.Text, button.Font).Width + 32;
                button.CheckedState.FillColor = Palette.Accent;
                button.CheckedState.ForeColor = Palette.OnAccent;
                button.Click += PowerSystemButton_Click;
                _powerSystemButtons.Add(button);
                _powerSystemPanel.Controls.Add(button);
            }
        }

        /// <summary>
        /// Создает легенду цветовой индикации.
        /// </summary>
        /// <param name="runCaptions">Подписи сравниваемых расчетов.</param>
        private void BuildLegend(IReadOnlyList<string>? runCaptions)
        {
            _legendPanel.Controls.Clear();
            if (runCaptions != null && runCaptions.Count >= 2)
            {
                _legendPanel.Controls.Add(
                    CreateLegendItem("● сверху — " + runCaptions[0], Palette.FirstRun, Palette.Surface));
                _legendPanel.Controls.Add(
                    CreateLegendItem("● снизу — " + runCaptions[1], Palette.SecondRun, Palette.Surface));
            }

            _legendPanel.Controls.Add(CreateLegendItem("выше графика", Palette.Blue, Palette.BlueDark));
            _legendPanel.Controls.Add(CreateLegendItem("ниже графика", Color.FromArgb(255, 154, 162), Palette.RedDark));
            _legendPanel.Controls.Add(CreateLegendItem("Вкл", Palette.Accent, Palette.AccentDark));
            _legendPanel.Controls.Add(CreateLegendItem("Откл", Palette.TextSecondary, Palette.SurfaceRaised));
            _legendPanel.Controls.Add(CreateLegendItem("Q, Мвар", Palette.Purple, Palette.PurpleDark));
        }

        /// <summary>
        /// Выбирает энергосистему и выводит ее блоки КП один под другим.
        /// </summary>
        /// <param name="index">Индекс энергосистемы.</param>
        private void SelectPowerSystem(int index)
        {
            _powerSystemIndex = index;
            foreach (RoundedButton button in _powerSystemButtons)
            {
                button.Checked = (int)button.Tag! == index;
            }

            _blocksPanel.SuspendLayout();
            ClearBlocks();
            IReadOnlyList<HourlyLayoutBlock> blocks = _model != null && index < _model.PowerSystems.Count
                ? _model.PowerSystems[index].Blocks
                : new HourlyLayoutBlock[0];

            // Элементы с привязкой к верху размещаются в обратном порядке добавления.
            for (int blockIndex = blocks.Count - 1; blockIndex >= 0; blockIndex--)
            {
                _blocksPanel.Controls.Add(new Panel
                {
                    Dock = DockStyle.Top,
                    Height = BlockSpacing,
                    BackColor = Palette.Background
                });

                HourlyGrid grid = new HourlyGrid
                {
                    Dock = DockStyle.Top
                };
                grid.Bind(_model!, blocks[blockIndex]);
                grid.Height = grid.PreferredHeight;
                grid.MouseWheel += Grid_MouseWheel;
                _blocksPanel.Controls.Add(grid);
            }

            _blocksPanel.ResumeLayout(true);
            _blocksPanel.AutoScrollPosition = Point.Empty;
            _emptyLabel.Visible = blocks.Count == 0;
        }

        /// <summary>
        /// Удаляет таблицы блоков КП и освобождает их ресурсы.
        /// </summary>
        private void ClearBlocks()
        {
            List<Control> controls = new List<Control>();
            foreach (Control control in _blocksPanel.Controls)
            {
                controls.Add(control);
            }

            _blocksPanel.Controls.Clear();
            foreach (Control control in controls)
            {
                control.Dispose();
            }
        }

        /// <summary>
        /// Переключает энергосистему.
        /// </summary>
        /// <param name="sender">Кнопка энергосистемы.</param>
        /// <param name="e">Аргументы события.</param>
        private void PowerSystemButton_Click(object? sender, EventArgs e)
        {
            RoundedButton? button = sender as RoundedButton;
            if (button?.Tag is int index)
            {
                SelectPowerSystem(index);
            }
        }

        /// <summary>
        /// Прокручивает список блоков колесом мыши над таблицей: в таблице видны все 24 часа,
        /// поэтому прокрутка по вертикали передается панели блоков.
        /// </summary>
        /// <param name="sender">Таблица блока.</param>
        /// <param name="e">Аргументы события.</param>
        private void Grid_MouseWheel(object? sender, MouseEventArgs e)
        {
            Point position = _blocksPanel.AutoScrollPosition;
            _blocksPanel.AutoScrollPosition = new Point(-position.X, Math.Max(0, -position.Y - e.Delta));
            if (e is HandledMouseEventArgs handled)
            {
                handled.Handled = true;
            }
        }
    }
}
