using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using VrdoPdg.Model.Entities;
using VrdoPdg.View.Theme;

namespace VrdoPdg.View.Controls
{
    /// <summary>
    /// Таблица по часам с переключателем энергосистем («ОЭС Сибири», «ЕЭС Казахстана»,
    /// «Внешние ЭС») и легендой цветовой индикации. Используется формами
    /// «Напряжение по часам» и «Сравнение расчетов».
    /// </summary>
    internal sealed class HourlyTableView : UserControl
    {
        /// <summary>
        /// Панель переключателя энергосистем.
        /// </summary>
        private readonly FlowLayoutPanel _powerSystemPanel;

        /// <summary>
        /// Панель легенды.
        /// </summary>
        private readonly FlowLayoutPanel _legendPanel;

        /// <summary>
        /// Таблица по часам.
        /// </summary>
        private readonly HourlyGrid _grid;

        /// <summary>
        /// Сообщение об отсутствии КП в энергосистеме.
        /// </summary>
        private readonly Label _emptyLabel;

        /// <summary>
        /// Кнопки энергосистем.
        /// </summary>
        private readonly List<RoundedButton> _powerSystemButtons = new List<RoundedButton>();

        /// <summary>
        /// Данные таблицы.
        /// </summary>
        private HourlyTableModel? _model;

        /// <summary>
        /// Код выбранной энергосистемы.
        /// </summary>
        private int? _powerSystemId;

        /// <summary>
        /// Создает таблицу по часам.
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

            _grid = new HourlyGrid
            {
                Dock = DockStyle.Fill
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

            Controls.Add(_grid);
            Controls.Add(_emptyLabel);
            Controls.Add(toolbar);
        }

        /// <summary>
        /// Выводит данные в таблицу.
        /// </summary>
        /// <param name="model">Данные таблицы.</param>
        /// <param name="runCaptions">Подписи сравниваемых расчетов для легенды; <c>null</c> — предпросмотр.</param>
        public void Display(HourlyTableModel model, IReadOnlyList<string>? runCaptions)
        {
            _model = model;
            BuildPowerSystemButtons(model.PowerSystems);
            BuildLegend(runCaptions);

            int? selected = _powerSystemId;
            if (!selected.HasValue || model.PowerSystems.All(item => item.Id != selected.Value))
            {
                selected = model.PowerSystems.Count > 0
                    ? model.PowerSystems[0].Id
                    : (int?)null;
            }

            SelectPowerSystem(selected);
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
        /// <param name="powerSystems">Энергосистемы.</param>
        private void BuildPowerSystemButtons(IReadOnlyList<PowerSystem> powerSystems)
        {
            _powerSystemPanel.Controls.Clear();
            _powerSystemButtons.Clear();
            foreach (PowerSystem powerSystem in powerSystems)
            {
                RoundedButton button = new RoundedButton
                {
                    Text = powerSystem.Name,
                    Tag = powerSystem.Id,
                    Height = 36,
                    Margin = new Padding(0, 0, 4, 0)
                };
                UiStyle.ApplyFlat(button, null);
                button.Font = UiFonts.Semibold(10f);
                button.Width = TextRenderer.MeasureText(powerSystem.Name, button.Font).Width + 32;
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
        /// Выбирает энергосистему и выводит ее КП.
        /// </summary>
        /// <param name="powerSystemId">Код энергосистемы.</param>
        private void SelectPowerSystem(int? powerSystemId)
        {
            _powerSystemId = powerSystemId;
            foreach (RoundedButton button in _powerSystemButtons)
            {
                button.Checked = powerSystemId.HasValue && (int)button.Tag! == powerSystemId.Value;
            }

            if (_model == null || !powerSystemId.HasValue)
            {
                _emptyLabel.Visible = true;
                return;
            }

            _grid.Bind(_model, powerSystemId.Value);
            _emptyLabel.Visible = _grid.IsEmpty;
        }

        /// <summary>
        /// Переключает энергосистему.
        /// </summary>
        /// <param name="sender">Кнопка энергосистемы.</param>
        /// <param name="e">Аргументы события.</param>
        private void PowerSystemButton_Click(object? sender, EventArgs e)
        {
            RoundedButton? button = sender as RoundedButton;
            if (button?.Tag is int powerSystemId)
            {
                SelectPowerSystem(powerSystemId);
            }
        }
    }
}
