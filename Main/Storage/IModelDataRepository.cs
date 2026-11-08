using System.Collections.Generic;

using VrdoPdg.Model.Entities;

namespace VrdoPdg.Model.Storage
{
    /// <summary>
    /// Репозиторий базы данных системы: доступ к данным об узлах и СКРМ,
    /// графикам напряжений в КП и таблицам эффективности.
    /// </summary>
    public interface IModelDataRepository
    {
        /// <summary>
        /// Создает таблицы БД, если файл БД новый или не содержит таблиц системы.
        /// </summary>
        void EnsureCreated();

        /// <summary>
        /// Возвращает энергосистемы, упорядоченные по коду.
        /// </summary>
        /// <returns>Список энергосистем.</returns>
        IReadOnlyList<PowerSystem> GetPowerSystems();

        /// <summary>
        /// Возвращает КП с узлами, упорядоченные по коду КП.
        /// </summary>
        /// <returns>Список КП.</returns>
        IReadOnlyList<ControlPoint> GetControlPoints();

        /// <summary>
        /// Возвращает справочник СКРМ.
        /// </summary>
        /// <returns>Список СКРМ.</returns>
        IReadOnlyList<Device> GetDevices();

        /// <summary>
        /// Возвращает строки таблиц эффективности всех КП.
        /// </summary>
        /// <returns>Строки таблиц эффективности.</returns>
        IReadOnlyList<EfficiencyEntry> GetEfficiencyEntries();

        /// <summary>
        /// Сохраняет действующую таблицу эффективности КП. СКРМ, не вошедшие в таблицу,
        /// удаляются из нее, если не входят в ПУР; у СКРМ, входящих в ПУР, очищается
        /// действующий приоритет.
        /// </summary>
        /// <param name="controlPointId">Код КП.</param>
        /// <param name="priorities">СКРМ действующей таблицы с приоритетами.</param>
        void SaveEfficiencyTable(int controlPointId, IReadOnlyList<DevicePriority> priorities);

        /// <summary>
        /// Возвращает сохраненные границы графиков напряжений в КП.
        /// </summary>
        /// <returns>Границы графиков напряжений.</returns>
        IReadOnlyList<VoltageBounds> GetVoltageSchedule();

        /// <summary>
        /// Сохраняет границы графиков напряжений в КП, заменяя ранее сохраненные.
        /// </summary>
        /// <param name="bounds">Границы графиков напряжений.</param>
        void SaveVoltageSchedule(IReadOnlyList<VoltageBounds> bounds);

        /// <summary>
        /// Возвращает PQ-диаграммы генераторов.
        /// </summary>
        /// <returns>PQ-диаграммы по коду СКРМ.</returns>
        IReadOnlyDictionary<int, PQDiagram> GetPQDiagrams();

        /// <summary>
        /// Сохраняет PQ-диаграмму генератора, заменяя ранее сохраненную.
        /// </summary>
        /// <param name="deviceId">Код СКРМ генератора.</param>
        /// <param name="points">Точки PQ-диаграммы.</param>
        void SavePQDiagram(int deviceId, IReadOnlyList<PQPoint> points);

        /// <summary>
        /// Сохраняет данные МТ последней загрузки: почасовые напряжения в узлах
        /// и режимы СКРМ. Ранее сохраненные данные удаляются.
        /// </summary>
        /// <param name="voltages">Напряжения в узлах.</param>
        /// <param name="modes">Режимы СКРМ.</param>
        void SaveRegime(IReadOnlyList<NodeVoltage> voltages, IReadOnlyList<DeviceMode> modes);
    }
}
