using System;
using System.Collections.Generic;

using VrdoPdg.Model.Entities;

namespace VrdoPdg.Model.Regime
{
    /// <summary>
    /// Модуль взаимодействия с ПО «Барс»: открывает МТ, считывает таблицы по часам,
    /// изменяет состояние и загрузку СКРМ и выполняет расчет установившегося режима.
    /// </summary>
    /// <remarks>
    /// Все изменения параметров МТ, выполненные после выбора часа, запоминаются
    /// и могут быть отменены методами <see cref="UndoLastChange"/> и <see cref="UndoAllChanges"/>.
    /// </remarks>
    public interface IRastrAdapter : IDisposable
    {
        /// <summary>
        /// Открывает файл МТ. Ранее открытая МТ закрывается.
        /// </summary>
        /// <param name="megapointPath">Путь к файлу МТ (*.mptsmz).</param>
        void Open(string megapointPath);

        /// <summary>
        /// Делает текущим заданный час МТ и рассчитывает его установившийся режим.
        /// </summary>
        /// <param name="hour">Час суток (1–24).</param>
        /// <returns><c>true</c>, если расчет режима сошелся.</returns>
        bool SelectHour(int hour);

        /// <summary>
        /// Рассчитывает установившийся режим текущего часа.
        /// </summary>
        /// <returns><c>true</c>, если расчет режима сошелся.</returns>
        bool CalculateRegime();

        /// <summary>
        /// Возвращает расчетное напряжение в узле.
        /// </summary>
        /// <param name="nodeNumber">Номер узла.</param>
        /// <returns>Напряжение, кВ, или <c>null</c>, если узел не найден или отключен.</returns>
        double? GetNodeVoltage(int nodeNumber);

        /// <summary>
        /// Возвращает состояние управляемого СКРМ.
        /// </summary>
        /// <param name="nodeNumber">Номер узла СКРМ.</param>
        /// <returns>Состояние СКРМ или <c>null</c>, если узел не найден.</returns>
        ControlledDeviceState? GetControlledDevice(int nodeNumber);

        /// <summary>
        /// Возвращает состояние коммутируемого СКРМ.
        /// </summary>
        /// <param name="reactorNumber">Номер в таблице «Реакторы».</param>
        /// <returns>Состояние СКРМ или <c>null</c>, если реактор не найден.</returns>
        SwitchedDeviceState? GetSwitchedDevice(int reactorNumber);

        /// <summary>
        /// Возвращает состояние генератора: генератор находится в таблице «Генераторы УР» по номеру,
        /// его узел (столбец Node) — в таблице «Узлы» по столбцу ny; заданное напряжение (vzd),
        /// загрузка (qg) и пределы реактивной мощности (qmin, qmax) берутся из строки узла.
        /// </summary>
        /// <param name="generatorNumber">Номер генератора в таблице «Генераторы УР».</param>
        /// <returns>Состояние генератора или <c>null</c>, если генератор или его узел не найден.</returns>
        GeneratorState? GetGenerator(int generatorNumber);

        /// <summary>
        /// Фиксирует загрузку управляемого СКРМ по реактивной мощности, задавая
        /// одинаковые верхний и нижний пределы реактивной мощности узла.
        /// </summary>
        /// <param name="nodeNumber">Номер узла СКРМ.</param>
        /// <param name="reactivePower">Загрузка по реактивной мощности, Мвар.</param>
        void SetControlledDeviceReactivePower(int nodeNumber, double reactivePower);

        /// <summary>
        /// Изменяет состояние коммутируемого СКРМ.
        /// </summary>
        /// <param name="reactorNumber">Номер в таблице «Реакторы».</param>
        /// <param name="isOn">Новое состояние: <c>true</c> — «Включено».</param>
        void SetSwitchedDeviceState(int reactorNumber, bool isOn);

        /// <summary>
        /// Изменяет заданное напряжение в узле.
        /// </summary>
        /// <param name="nodeNumber">Номер узла.</param>
        /// <param name="voltage">Заданное напряжение, кВ.</param>
        void SetNodeSetVoltage(int nodeNumber, double voltage);

        /// <summary>
        /// Отменяет последнее изменение параметров МТ.
        /// </summary>
        void UndoLastChange();

        /// <summary>
        /// Отменяет все изменения параметров МТ, выполненные после выбора часа.
        /// </summary>
        void UndoAllChanges();
    }
}
