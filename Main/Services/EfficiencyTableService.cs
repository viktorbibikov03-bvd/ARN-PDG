using System;
using System.Collections.Generic;
using System.Linq;

using VrdoPdg.Model.Entities;
using VrdoPdg.Model.Protocol;
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
        /// Возвращает справочник СКРМ.
        /// </summary>
        /// <returns>Список СКРМ.</returns>
        public IReadOnlyList<Device> GetDevices()
        {
            return _repository.GetDevices();
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
        /// <returns>Строки таблицы.</returns>
        public IReadOnlyList<EfficiencyRow> GetActiveTable(int controlPointId)
        {
            Dictionary<int, Device> devices = _repository.GetDevices().ToDictionary(device => device.Id);
            return _repository.GetEfficiencyEntries()
                .Where(entry => entry.ControlPointId == controlPointId
                    && entry.Priority.HasValue
                    && devices.ContainsKey(entry.DeviceId))
                .OrderBy(entry => entry.Priority)
                .Select(entry => CreateRow(entry.Priority, devices[entry.DeviceId]))
                .ToList();
        }

        /// <summary>
        /// Возвращает таблицу эффективности КП согласно ПУР: состав СКРМ и приоритеты по умолчанию.
        /// </summary>
        /// <param name="controlPointId">Код КП.</param>
        /// <returns>Строки таблицы.</returns>
        public IReadOnlyList<EfficiencyRow> GetDefaultTable(int controlPointId)
        {
            Dictionary<int, Device> devices = _repository.GetDevices().ToDictionary(device => device.Id);
            return _repository.GetEfficiencyEntries()
                .Where(entry => entry.ControlPointId == controlPointId
                    && entry.DefaultPriority.HasValue
                    && devices.ContainsKey(entry.DeviceId))
                .OrderBy(entry => entry.DefaultPriority)
                .Select(entry => CreateRow(entry.DefaultPriority, devices[entry.DeviceId]))
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
        /// Возвращает действующие таблицы эффективности всех КП в порядке перебора алгоритмом.
        /// </summary>
        /// <param name="devices">Справочник СКРМ.</param>
        /// <returns>Таблицы эффективности по коду КП.</returns>
        public IReadOnlyDictionary<int, IReadOnlyList<EfficiencyItem>> GetActiveTables(IReadOnlyList<Device> devices)
        {
            return BuildActiveTables(_repository.GetEfficiencyEntries(), devices);
        }

        /// <summary>
        /// Ищет СКРМ по типу и номеру в МТ.
        /// </summary>
        /// <param name="devices">Справочник СКРМ.</param>
        /// <param name="type">Тип СКРМ.</param>
        /// <param name="number">Номер узла, реактора или генератора.</param>
        /// <returns>СКРМ или <c>null</c>, если СКРМ нет в БД.</returns>
        public static Device? FindDevice(IEnumerable<Device> devices, DeviceType type, int? number)
        {
            return number.HasValue
                ? devices.FirstOrDefault(device => device.Type == type && device.Number == number.Value)
                : null;
        }

        /// <summary>
        /// Проверяет корректность таблицы эффективности: заполнение приоритетов и номеров,
        /// отсутствие повторяющихся приоритетов и СКРМ, наличие СКРМ в БД
        /// и непрерывность приоритетов 1, 2, ..., N.
        /// </summary>
        /// <param name="rows">Строки таблицы.</param>
        /// <param name="devices">Справочник СКРМ.</param>
        /// <returns>Ошибки проверки; пустой список, если таблица корректна.</returns>
        public static IReadOnlyList<EfficiencyValidationError> Validate(
            IReadOnlyList<EfficiencyRow> rows,
            IReadOnlyList<Device> devices)
        {
            List<EfficiencyValidationError> errors = new List<EfficiencyValidationError>();
            Dictionary<int, int> deviceRows = new Dictionary<int, int>();
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
                    continue;
                }

                Device? device = FindDevice(devices, row.DeviceType, row.DeviceNumber);
                if (device == null)
                {
                    string caption = DeviceNames.GetNumberCaption(row.DeviceType);
                    errors.Add(new EfficiencyValidationError(
                        index,
                        char.ToUpper(caption[0]) + caption.Substring(1) + " " + row.DeviceNumber.Value
                            + " не найден в БД"));
                    continue;
                }

                int firstRow;
                if (deviceRows.TryGetValue(device.Id, out firstRow))
                {
                    errors.Add(new EfficiencyValidationError(
                        index,
                        "СКРМ уже есть в таблице (строка " + (firstRow + 1) + ")"));
                }
                else
                {
                    deviceRows[device.Id] = index;
                }
            }

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
        /// Проверяет корректность таблицы эффективности по справочнику СКРМ из БД.
        /// </summary>
        /// <param name="rows">Строки таблицы.</param>
        /// <returns>Ошибки проверки; пустой список, если таблица корректна.</returns>
        public IReadOnlyList<EfficiencyValidationError> Validate(IReadOnlyList<EfficiencyRow> rows)
        {
            return Validate(rows, _repository.GetDevices());
        }

        /// <summary>
        /// Проверяет и сохраняет действующую таблицу эффективности КП.
        /// Изменения действуют на следующие расчеты.
        /// </summary>
        /// <param name="controlPoint">КП.</param>
        /// <param name="rows">Строки таблицы.</param>
        /// <exception cref="VrdoException">Таблица некорректна.</exception>
        public void Save(ControlPoint controlPoint, IReadOnlyList<EfficiencyRow> rows)
        {
            if (controlPoint == null)
            {
                throw new ArgumentNullException(nameof(controlPoint));
            }

            IReadOnlyList<Device> devices = _repository.GetDevices();
            IReadOnlyList<EfficiencyValidationError> errors = Validate(rows, devices);
            if (errors.Count > 0)
            {
                throw new VrdoException(
                    "Изменения не сохранены: данные некорректны",
                    string.Join("; ", errors.Select(error => error.Message).Distinct()));
            }

            List<DevicePriority> priorities = new List<DevicePriority>();
            foreach (EfficiencyRow row in rows)
            {
                Device device = FindDevice(devices, row.DeviceType, row.DeviceNumber)!;
                priorities.Add(new DevicePriority(device.Id, row.Priority.GetValueOrDefault()));
            }

            _repository.SaveEfficiencyTable(controlPoint.Id, priorities);
            _protocol.Add(
                ProtocolTag.Info,
                "Таблица эффективности КП «" + controlPoint.Name + "» сохранена",
                "СКРМ в таблице: " + priorities.Count);
        }

        /// <summary>
        /// Создает строку таблицы для СКРМ.
        /// </summary>
        /// <param name="priority">Приоритет.</param>
        /// <param name="device">СКРМ.</param>
        /// <returns>Строка таблицы.</returns>
        private static EfficiencyRow CreateRow(int? priority, Device device)
        {
            return new EfficiencyRow(priority, device.Type, device.Number);
        }
    }
}
