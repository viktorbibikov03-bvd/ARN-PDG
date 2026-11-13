using System;
using System.Collections.Generic;
using System.Linq;

using VrdoPdg.Model.Entities;
using VrdoPdg.Model.Protocol;
using VrdoPdg.Model.Regime;
using VrdoPdg.Model.Storage;

namespace VrdoPdg.Model.Services
{
    /// <summary>
    /// Сервис таблицы эффективности: проверяет корректность таблицы эффективности КП,
    /// сохраняет изменения в БД и восстанавливает состав СКРМ согласно ПУР.
    /// </summary>
    public sealed class EfficiencyTableService
    {
        /// <summary>
        /// Репозиторий БД.
        /// </summary>
        private readonly IModelDataRepository _repository;

        /// <summary>
        /// Протокол расчета.
        /// </summary>
        private readonly CalculationProtocol _protocol;

        /// <summary>
        /// Создает сервис таблицы эффективности.
        /// </summary>
        /// <param name="repository">Репозиторий БД.</param>
        /// <param name="protocol">Протокол расчета.</param>
        public EfficiencyTableService(IModelDataRepository repository, CalculationProtocol protocol)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
        }

        /// <summary>
        /// Возвращает КП, хранящиеся в БД.
        /// </summary>
        /// <returns>Список КП.</returns>
        public IReadOnlyList<ControlPoint> GetControlPoints()
        {
            return _repository.GetControlPoints();
        }

        /// <summary>
        /// Создает сопоставление строк таблицы со СКРМ по справочнику СКРМ из БД и загруженной МТ
        /// </summary>
        /// <param name="catalog">Перечень объектов загруженной МТ; <c>null</c> — МТ не загружена</param>
        /// <returns>Сопоставление строк таблицы со СКРМ</returns>
        public EfficiencyDeviceResolver CreateResolver(MegapointCatalog? catalog)
        {
            return new EfficiencyDeviceResolver(_repository.GetDevices(), catalog);
        }

        /// <summary>
        /// Возвращает количество СКРМ в действующих таблицах эффективности КП.
        /// </summary>
        /// <returns>Количество СКРМ по коду КП.</returns>
        public IReadOnlyDictionary<int, int> GetActiveDeviceCounts()
        {
            return _repository.GetEfficiencyEntries()
                .Where(entry => entry.Priority.HasValue)
                .GroupBy(entry => entry.ControlPointId)
                .ToDictionary(group => group.Key, group => group.Count());
        }

        /// <summary>
        /// Возвращает действующую таблицу эффективности КП, упорядоченную по приоритету.
        /// </summary>
        /// <param name="controlPointId">Код КП.</param>
        /// <param name="resolver">Сопоставление строк таблицы со СКРМ</param>
        /// <returns>Строки таблицы.</returns>
        public IReadOnlyList<EfficiencyRow> GetActiveTable(int controlPointId, EfficiencyDeviceResolver resolver)
        {
            Dictionary<int, Device> devices = _repository.GetDevices().ToDictionary(device => device.Id);
            return _repository.GetEfficiencyEntries()
                .Where(entry => entry.ControlPointId == controlPointId
                    && entry.Priority.HasValue
                    && devices.ContainsKey(entry.DeviceId))
                .OrderBy(entry => entry.Priority)
                .Select(entry => resolver.CreateRow(entry.Priority, devices[entry.DeviceId]))
                .ToList();
        }

        /// <summary>
        /// Возвращает таблицу эффективности КП согласно ПУР: состав СКРМ и приоритеты по умолчанию.
        /// </summary>
        /// <param name="controlPointId">Код КП.</param>
        /// <param name="resolver">Сопоставление строк таблицы со СКРМ</param>
        /// <returns>Строки таблицы.</returns>
        public IReadOnlyList<EfficiencyRow> GetDefaultTable(int controlPointId, EfficiencyDeviceResolver resolver)
        {
            Dictionary<int, Device> devices = _repository.GetDevices().ToDictionary(device => device.Id);
            return _repository.GetEfficiencyEntries()
                .Where(entry => entry.ControlPointId == controlPointId
                    && entry.DefaultPriority.HasValue
                    && devices.ContainsKey(entry.DeviceId))
                .OrderBy(entry => entry.DefaultPriority)
                .Select(entry => resolver.CreateRow(entry.DefaultPriority, devices[entry.DeviceId]))
                .ToList();
        }

        /// <summary>
        /// Формирует действующие таблицы эффективности КП: СКРМ упорядочены группами
        /// (управляемые, коммутируемые, генераторы), внутри группы — по приоритету.
        /// </summary>
        /// <param name="entries">Строки таблиц эффективности.</param>
        /// <param name="devices">Справочник СКРМ.</param>
        /// <returns>Таблицы эффективности по коду КП.</returns>
        public static IReadOnlyDictionary<int, IReadOnlyList<EfficiencyItem>> BuildActiveTables(
            IEnumerable<EfficiencyEntry> entries,
            IEnumerable<Device> devices)
        {
            Dictionary<int, Device> devicesById = devices.ToDictionary(device => device.Id);
            Dictionary<int, IReadOnlyList<EfficiencyItem>> tables =
                new Dictionary<int, IReadOnlyList<EfficiencyItem>>();
            foreach (IGrouping<int, EfficiencyEntry> group in entries
                .Where(entry => entry.Priority.HasValue && devicesById.ContainsKey(entry.DeviceId))
                .GroupBy(entry => entry.ControlPointId))
            {
                tables[group.Key] = group
                    .Select(entry => new EfficiencyItem(
                        devicesById[entry.DeviceId],
                        entry.Priority.GetValueOrDefault(),
                        entry.DefaultPriority))
                    .OrderBy(item => item.Device.Type)
                    .ThenBy(item => item.Priority)
                    .ToList();
            }

            return tables;
        }

        /// <summary>
        /// Проверяет корректность таблицы эффективности: заполнение приоритетов и номеров,
        /// отсутствие повторяющихся приоритетов и СКРМ, наличие СКРМ в загруженной МТ
        /// (без МТ — в БД) и непрерывность приоритетов 1, 2, ..., N
        /// </summary>
        /// <param name="rows">Строки таблицы.</param>
        /// <param name="resolver">Сопоставление строк таблицы со СКРМ</param>
        /// <returns>Ошибки проверки; пустой список, если таблица корректна.</returns>
        public static IReadOnlyList<EfficiencyValidationError> Validate(
            IReadOnlyList<EfficiencyRow> rows,
            EfficiencyDeviceResolver resolver)
        {
            List<EfficiencyValidationError> errors = new List<EfficiencyValidationError>();
            for (int index = 0; index < rows.Count; index++)
            {
                EfficiencyRow row = rows[index];
                if (!row.Priority.HasValue || row.Priority.Value <= 0)
                {
                    errors.Add(new EfficiencyValidationError(index, "Не задан приоритет"));
                }

                if (!row.DeviceNumber.HasValue)
                {
                    errors.Add(new EfficiencyValidationError(index, "Не задан номер узла или реактора"));
                }
            }

            resolver.Resolve(rows, errors);

            foreach (IGrouping<int, int> group in Enumerable.Range(0, rows.Count)
                .Where(index => rows[index].Priority.HasValue && rows[index].Priority.GetValueOrDefault() > 0)
                .GroupBy(index => rows[index].Priority.GetValueOrDefault())
                .Where(group => group.Count() > 1))
            {
                foreach (int index in group)
                {
                    errors.Add(new EfficiencyValidationError(index, "Приоритет " + group.Key + " повторяется"));
                }
            }

            if (errors.Count == 0)
            {
                List<int> priorities = rows
                    .Select(row => row.Priority.GetValueOrDefault())
                    .OrderBy(priority => priority)
                    .ToList();
                for (int index = 0; index < priorities.Count; index++)
                {
                    if (priorities[index] != index + 1)
                    {
                        errors.Add(new EfficiencyValidationError(
                            EfficiencyValidationError.TableRowIndex,
                            "Приоритеты должны образовывать последовательность 1, 2, ..., " + priorities.Count));
                        break;
                    }
                }
            }

            return errors;
        }

        /// <summary>
        /// Проверяет и сохраняет действующую таблицу эффективности КП.
        /// Изменения действуют на следующие расчеты.
        /// </summary>
        /// <param name="controlPoint">КП.</param>
        /// <param name="rows">Строки таблицы.</param>
        /// <param name="resolver">Сопоставление строк таблицы со СКРМ</param>
        /// <exception cref="VrdoException">Таблица некорректна.</exception>
        public void Save(
            ControlPoint controlPoint,
            IReadOnlyList<EfficiencyRow> rows,
            EfficiencyDeviceResolver resolver)
        {
            if (controlPoint == null)
            {
                throw new ArgumentNullException(nameof(controlPoint));
            }

            if (resolver == null)
            {
                throw new ArgumentNullException(nameof(resolver));
            }

            IReadOnlyList<EfficiencyValidationError> errors = Validate(rows, resolver);
            if (errors.Count > 0)
            {
                throw new VrdoException(
                    "Изменения не сохранены: данные некорректны",
                    string.Join("; ", errors.Select(error => error.Message).Distinct()));
            }

            IReadOnlyList<EfficiencyDeviceMatch?> matches =
                resolver.Resolve(rows, new List<EfficiencyValidationError>());
            List<DevicePriority> priorities = new List<DevicePriority>();
            List<string> added = new List<string>();
            for (int index = 0; index < rows.Count; index++)
            {
                EfficiencyDeviceMatch match = matches[index]!;
                Device device = match.Device
                    ?? _repository.AddDevice(match.Type, match.Name, match.Number, match.SwitchedKind);
                if (match.IsNew)
                {
                    added.Add(device.ToString());
                }

                priorities.Add(new DevicePriority(device.Id, rows[index].Priority.GetValueOrDefault()));
            }

            _repository.SaveEfficiencyTable(controlPoint.Id, priorities);
            _protocol.Add(
                ProtocolTag.Info,
                "Таблица эффективности КП «" + controlPoint.Name + "» сохранена",
                "СКРМ в таблице: " + priorities.Count
                    + (added.Count == 0 ? string.Empty : "; добавлены в БД из МТ: " + string.Join(", ", added)));
        }
    }
}
