using System;
using System.Collections.Generic;
using System.Windows.Forms;

using VrdoPdg.Model.Entities;
using VrdoPdg.Model.Regime;
using VrdoPdg.Model.Services;
using VrdoPdg.View.Controls;
using VrdoPdg.View.Theme;

namespace VrdoPdg.View
{
    /// <summary>
    /// Форма «Напряжение по часам»: напряжение в КП, состояние и загрузка СКРМ по часам суток
    /// до и после расчета с выбором энергосистемы.
    /// </summary>
    internal sealed partial class HourlyVoltageForm : Form
    {
        /// <summary>
        /// Исходные данные сеанса.
        /// </summary>
        private readonly SessionData _session;

        /// <summary>
        /// Таблицы эффективности, определяющие состав столбцов СКРМ.
        /// </summary>
        private IReadOnlyDictionary<int, IReadOnlyList<EfficiencyItem>> _tables;

        /// <summary>
        /// Последний завершенный расчет.
        /// </summary>
        private CalculationResult? _result;

        /// <summary>
        /// Признак отображения режима после расчета.
        /// </summary>
        private bool _isAfter;

        /// <summary>
        /// Создает форму «Напряжение по часам».
        /// </summary>
        /// <param name="session">Исходные данные сеанса.</param>
        /// <param name="result">Последний завершенный расчет или <c>null</c>.</param>
        /// <param name="tables">Таблицы эффективности, определяющие состав столбцов СКРМ.</param>
        public HourlyVoltageForm(
            SessionData session,
            CalculationResult? result,
            IReadOnlyDictionary<int, IReadOnlyList<EfficiencyItem>> tables)
        {
            InitializeComponent();
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _result = result;
            _tables = tables ?? throw new ArgumentNullException(nameof(tables));

            UiStyle.ApplyOutlined(_backButton, IconGlyph.Back);
            StyleModeButton(_beforeButton);
            StyleModeButton(_afterButton);
            ShowData();
        }

        /// <summary>
        /// Устанавливает результаты завершенного расчета и делает доступным положение «После расчёта».
        /// </summary>
        /// <param name="result">Расчет.</param>
        public void SetResult(CalculationResult result)
        {
            _result = result;
            _tables = result.EfficiencyTables;
            ShowData();
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
        /// Оформляет кнопку переключателя «До расчёта» / «После расчёта».
        /// </summary>
        /// <param name="button">Кнопка.</param>
        private static void StyleModeButton(RoundedButton button)
        {
            UiStyle.ApplyFlat(button, null);
            button.CheckedState.FillColor = Palette.Accent;
            button.CheckedState.ForeColor = Palette.OnAccent;
        }

        /// <summary>
        /// Выводит в таблицу режим до или после расчета.
        /// </summary>
        private void ShowData()
        {
            if (_result == null)
            {
                _isAfter = false;
            }

            RegimeSnapshot regime = _isAfter && _result != null
                ? _result.After
                : _session.InitialRegime;
            HourlyTableModel model = HourlyTableModel.CreatePreview(
                _session,
                new HourlyValueSource(regime, _session.Schedule),
                _tables);
            _tableView.Display(model, null);

            _beforeButton.Checked = !_isAfter;
            _afterButton.Checked = _isAfter;
            _afterButton.Enabled = _result != null;
            Text = _result == null
                ? "ВРДО ПДГ — Напряжение по часам"
                : "ВРДО ПДГ — Напряжение по часам (расчёт №" + _result.Number + ")";
        }

        /// <summary>
        /// Показывает режим до расчета.
        /// </summary>
        /// <param name="sender">Кнопка «До расчёта».</param>
        /// <param name="e">Аргументы события.</param>
        private void BeforeButton_Click(object? sender, EventArgs e)
        {
            _isAfter = false;
            ShowData();
        }

        /// <summary>
        /// Показывает режим после расчета.
        /// </summary>
        /// <param name="sender">Кнопка «После расчёта».</param>
        /// <param name="e">Аргументы события.</param>
        private void AfterButton_Click(object? sender, EventArgs e)
        {
            _isAfter = _result != null;
            ShowData();
        }

        /// <summary>
        /// Возвращает к главному окну.
        /// </summary>
        /// <param name="sender">Кнопка «К расчёту».</param>
        /// <param name="e">Аргументы события.</param>
        private void BackButton_Click(object? sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Закрывает форму клавишей Esc.
        /// </summary>
        /// <param name="sender">Форма.</param>
        /// <param name="e">Аргументы события.</param>
        private void HourlyVoltageForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Close();
            }
        }
    }
}
