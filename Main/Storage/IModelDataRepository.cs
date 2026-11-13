using System.Collections.Generic;

using VrdoPdg.Model.Entities;

namespace VrdoPdg.Model.Storage
{
    /// <summary>
    /// Репозиторий базы данных системы: доступ к данным об узлах и СКРМ,
    /// графикам напряжений в КП и таблицам эффективности
    /// </summary>
    public interface IModelDataRepository
    {
        /// <summary>
        /// Проверяет, что файл БД существует и содержит таблицы системы. БД не создается.
        /// </summary>
        /// <exception cref="VrdoException">Файл БД не найден или в нем нет таблиц системы</exception>
        void CheckDatabase();

        /// <summary>
        /// Возвращает энергосистемы, упорядоченные по коду
        /// </summary>
        /// <returns>Список энергосистем.</returns>
        IReadOnlyList<PowerSystem> GetPowerSystems();

        /// <summary>
        /// Возвращает КП с узлами, упорядоченные по коду КП
        /// </summary>
        /// <returns>Список КП</returns>
        IReadOnlyList<ControlPoint> GetControlPoints();

        /// <summary>
        /// Возвращает справочник СКРМ
        /// </summary>
        /// <returns>Список СКРМ</returns>
        IReadOnlyList<Device> GetDevices();

        /// <summary>
        /// Возвращает строки таблиц эффективности всех КП
        /// </summary>
        /// <returns>Строки таблиц эффективности</returns>
        IReadOnlyList<EfficiencyEntry> GetEfficiencyEntries();

        /// <summary>
        /// Добавляет в справочник СКРМ, которое есть в МТ, но отсутствует в БД
        /// </summary>
        /// <param name="type">Тип СКРМ</param>
        /// <param name="name">Наименование СКРМ</param>
        /// <param name="number">Номер узла, реактора или генератора (Num) в МТ</param>
        /// <param name="switchedKind">Вид коммутируемого СКРМ</param>
        /// <returns>Добавленное СКРМ с кодом в БД</returns>
        Device AddDevice(DeviceType type, string name, int number, SwitchedDeviceKind? switchedKind);

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
        /// Сохраняет данные МТ последней загрузки: почасовые напряжения в узлах
        /// и режимы СКРМ. Ранее сохраненные данные удаляются.
        /// </summary>
        /// <param name="voltages">Напряжения в узлах.</param>
        /// <param name="modes">Режимы СКРМ.</param>
        void SaveRegime(IReadOnlyList<NodeVoltage> voltages, IReadOnlyList<DeviceMode> modes);
    }
}
