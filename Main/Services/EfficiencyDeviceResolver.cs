using System;
using System.Collections.Generic;
using System.Linq;

using VrdoPdg.Model.Entities;
using VrdoPdg.Model.Regime;

namespace VrdoPdg.Model.Services
{
    /// <summary>
    /// Сопоставляет строки таблицы эффективности со СКРМ. Генераторы указываются в таблице номером
    /// узла подключения (ny таблицы «Узлы»), в БД хранится номер генератора (Num). Если МТ загружена,
    /// СКРМ проверяются по МТ: СКРМ, которого нет в БД, но есть в МТ, добавляется в БД при сохранении.
    /// Если МТ не загружена, СКРМ проверяются по БД, а генераторы указываются номером генератора
    /// </summary>
    public sealed class EfficiencyDeviceResolver
    {
        /// <summary>
        /// Признак вида «БСК» в наименовании коммутируемого СКРМ МТ
        /// </summary>
        private const string CapacitorBankMarker = "БСК";

        /// <summary>
        /// СКРМ из БД по коду
        /// </summary>
        private readonly Dictionary<int, Device> _devicesById;

        /// <summary>
        /// СКРМ из БД
        /// </summary>
        private readonly IReadOnlyList<Device> _devices;

        /// <summary>
        /// Перечень объектов загруженной МТ; <c>null</c> — МТ не загружена
        /// </summary>
        private readonly MegapointCatalog? _catalog;

        /// <summary>
        /// Создает сопоставление строк таблицы со СКРМ
        /// </summary>
        /// <param name="devices">Справочник СКРМ из БД</param>
        /// <param name="catalog">Перечень объектов загруженной МТ; <c>null</c> — МТ не загружена</param>
        public EfficiencyDeviceResolver(IReadOnlyList<Device> devices, MegapointCatalog? catalog)
        {
            _devices = devices ?? throw new ArgumentNullException(nameof(devices));
            _devicesById = devices.ToDictionary(device => device.Id);
            _catalog = catalog;
        }

        /// <summary>
        /// Признак загруженной МТ: СКРМ проверяются по МТ, генераторы указываются номером узла
        /// </summary>
        public bool IsMegapointLoaded => _catalog != null;

        /// <summary>
        /// Наименование источника, по которому проверяются СКРМ
        /// </summary>
        private string SourceName => _catalog == null
            ? "БД"
            : "МТ";

        /// <summary>
        /// Возвращает номер СКРМ для вывода в таблице: номер узла управляемого СКРМ, номер реактора,
        /// номер узла подключения генератора по МТ
        /// </summary>
        /// <param name="device">СКРМ из БД</param>
        /// <returns>Номер СКРМ в таблице; для генератора, которого нет в МТ, — номер генератора</returns>
        public int GetDisplayNumber(Device device)
        {
            if (device.Type != DeviceType.Generator || _catalog == null)
            {
                return device.Number;
            }

            return _catalog.GetGenerator(device.Number)?.NodeNumber ?? device.Number;
        }

        /// <summary>
        /// Создает строку таблицы для СКРМ из БД
        /// </summary>
        /// <param name="priority">Приоритет</param>
        /// <param name="device">СКРМ из БД</param>
        /// <returns>Строка таблицы</returns>
        public EfficiencyRow CreateRow(int? priority, Device device)
        {
            return new EfficiencyRow(priority, device.Type, GetDisplayNumber(device), device.Id);
        }

        /// <summary>
        /// Возвращает наименование СКРМ строки для вывода в таблице
        /// </summary>
        /// <param name="row">Строка таблицы</param>
        /// <returns>Наименование или пустая строка, если СКРМ не найдено</returns>
        public string GetName(EfficiencyRow row)
        {
            string? error;
            IReadOnlyList<EfficiencyDeviceMatch> candidates = GetCandidates(row, out error);
            return candidates.Count == 0
                ? string.Empty
                : candidates[0].Name;
        }

        /// <summary>
        /// Сопоставляет строки таблицы со СКРМ. Одно СКРМ не может занимать две строки: если в узле
        /// несколько генераторов, строкам с этим узлом сопоставляются разные генераторы
        /// </summary>
        /// <param name="rows">Строки таблицы</param>
        /// <param name="errors">Ошибки сопоставления</param>
        /// <returns>СКРМ по индексу строки; <c>null</c> для строк с ошибкой или без номера</returns>
        public IReadOnlyList<EfficiencyDeviceMatch?> Resolve(
            IReadOnlyList<EfficiencyRow> rows,
            List<EfficiencyValidationError> errors)
        {
            List<EfficiencyDeviceMatch?> result = new List<EfficiencyDeviceMatch?>();
            Dictionary<(DeviceType Type, int Number), int> usedRows = new Dictionary<(DeviceType Type, int Number), int>();
            for (int index = 0; index < rows.Count; index++)
            {
                EfficiencyRow row = rows[index];
                if (!row.DeviceNumber.HasValue)
                {
                    result.Add(null);
                    continue;
                }

                string? error;
                IReadOnlyList<EfficiencyDeviceMatch> candidates = GetCandidates(row, out error);
                if (candidates.Count == 0)
                {
                    errors.Add(new EfficiencyValidationError(index, error!));
                    result.Add(null);
                    continue;
                }

                EfficiencyDeviceMatch? match = candidates
                    .FirstOrDefault(candidate => !usedRows.ContainsKey((candidate.Type, candidate.Number)));
                if (match == null)
                {
                    int firstRow = usedRows[(candidates[0].Type, candidates[0].Number)];
                    errors.Add(new EfficiencyValidationError(
                        index,
                        "СКРМ уже есть в таблице (строка " + (firstRow + 1) + ")"));
                    result.Add(null);
                    continue;
                }

                usedRows[(match.Type, match.Number)] = index;
                result.Add(match);
            }

            return result;
        }

        /// <summary>
        /// Определяет вид коммутируемого СКРМ МТ по наименованию
        /// </summary>
        /// <param name="name">Наименование реактора в МТ</param>
        /// <returns>«БСК», если наименование содержит «БСК», иначе «Реактор»</returns>
        private static SwitchedDeviceKind GetSwitchedKind(string name)
        {
            return name.IndexOf(CapacitorBankMarker, StringComparison.OrdinalIgnoreCase) >= 0
                ? SwitchedDeviceKind.CapacitorBank
                : SwitchedDeviceKind.Reactor;
        }

        /// <summary>
        /// Возвращает СКРМ, которые могут соответствовать строке, в порядке предпочтения:
        /// СКРМ строки из БД, затем СКРМ из БД, затем СКРМ МТ, которых нет в БД
        /// </summary>
        /// <param name="row">Строка таблицы</param>
        /// <param name="error">Описание ошибки, если СКРМ не найдено</param>
        /// <returns>СКРМ, которые могут соответствовать строке</returns>
        private IReadOnlyList<EfficiencyDeviceMatch> GetCandidates(EfficiencyRow row, out string? error)
        {
            error = null;
            List<EfficiencyDeviceMatch> result = new List<EfficiencyDeviceMatch>();
            if (!row.DeviceNumber.HasValue)
            {
                return result;
            }

            int number = row.DeviceNumber.Value;
            Device? own;
            if (row.DeviceId.HasValue
                && _devicesById.TryGetValue(row.DeviceId.Value, out own)
                && own.Type == row.DeviceType
                && GetDisplayNumber(own) == number
                && IsInMegapoint(own))
            {
                result.Add(new EfficiencyDeviceMatch(own));
            }

            switch (row.DeviceType)
            {
                case DeviceType.Controlled:
                    AddNumberedCandidates(result, DeviceType.Controlled, number, _catalog?.GetNodeName(number));
                    error = "Узел " + number + " не найден в " + SourceName;
                    break;
                case DeviceType.Switched:
                    AddNumberedCandidates(result, DeviceType.Switched, number, _catalog?.GetReactorName(number));
                    error = "Реактор " + number + " не найден в " + SourceName;
                    break;
                default:
                    AddGeneratorCandidates(result, number);
                    error = _catalog == null
                        ? "Генератор " + number + " не найден в БД"
                        : "Генератор в узле " + number + " не найден в МТ";
                    break;
            }

            if (result.Count > 0)
            {
                error = null;
            }

            return result;
        }

        /// <summary>
        /// Добавляет управляемое СКРМ узла или коммутируемое СКРМ: СКРМ из БД с этим номером,
        /// а если его нет в БД — СКРМ МТ
        /// </summary>
        /// <param name="result">СКРМ, которые могут соответствовать строке</param>
        /// <param name="type">Тип СКРМ</param>
        /// <param name="number">Номер узла или реактора</param>
        /// <param name="megapointName">Наименование объекта МТ; <c>null</c> — объекта нет в МТ или МТ не загружена</param>
        private void AddNumberedCandidates(
            List<EfficiencyDeviceMatch> result,
            DeviceType type,
            int number,
            string? megapointName)
        {
            if (_catalog != null && megapointName == null)
            {
                result.Clear();
                return;
            }

            Device? device = _devices.FirstOrDefault(item => item.Type == type && item.Number == number);
            if (device != null)
            {
                AddUnique(result, new EfficiencyDeviceMatch(device));
            }
            else if (megapointName != null)
            {
                string name = megapointName.Length > 0
                    ? megapointName
                    : DeviceNames.GetNumberCaption(type) + " " + number;
                SwitchedDeviceKind? kind = type == DeviceType.Switched
                    ? GetSwitchedKind(name)
                    : (SwitchedDeviceKind?)null;
                AddUnique(result, new EfficiencyDeviceMatch(type, number, name, kind));
            }
        }

        /// <summary>
        /// Добавляет генераторы: при загруженной МТ — генераторы узла подключения, сначала из БД,
        /// затем остальные генераторы МТ; без МТ — генератор из БД с этим номером генератора
        /// </summary>
        /// <param name="result">СКРМ, которые могут соответствовать строке</param>
        /// <param name="number">Номер узла подключения или, без МТ, номер генератора</param>
        private void AddGeneratorCandidates(List<EfficiencyDeviceMatch> result, int number)
        {
            if (_catalog == null)
            {
                Device? device = _devices.FirstOrDefault(
                    item => item.Type == DeviceType.Generator && item.Number == number);
                if (device != null)
                {
                    AddUnique(result, new EfficiencyDeviceMatch(device));
                }

                return;
            }

            IReadOnlyList<MegapointGenerator> generators = _catalog.GetGeneratorsAtNode(number);
            if (generators.Count == 0)
            {
                result.Clear();
                return;
            }

            Dictionary<int, Device> stored = _devices
                .Where(device => device.Type == DeviceType.Generator)
                .GroupBy(device => device.Number)
                .ToDictionary(group => group.Key, group => group.First());
            foreach (MegapointGenerator generator in generators.Where(item => stored.ContainsKey(item.Number)))
            {
                AddUnique(result, new EfficiencyDeviceMatch(stored[generator.Number]));
            }

            foreach (MegapointGenerator generator in generators.Where(item => !stored.ContainsKey(item.Number)))
            {
                string name = generator.Name.Length > 0
                    ? generator.Name
                    : "Генератор " + generator.Number;
                AddUnique(result, new EfficiencyDeviceMatch(DeviceType.Generator, generator.Number, name, null));
            }
        }

        /// <summary>
        /// Проверяет, есть ли СКРМ из БД в загруженной МТ; без МТ проверка не выполняется
        /// </summary>
        /// <param name="device">СКРМ из БД</param>
        /// <returns><c>true</c>, если СКРМ есть в МТ или МТ не загружена</returns>
        private bool IsInMegapoint(Device device)
        {
            if (_catalog == null)
            {
                return true;
            }

            switch (device.Type)
            {
                case DeviceType.Controlled:
                    return _catalog.GetNodeName(device.Number) != null;
                case DeviceType.Switched:
                    return _catalog.GetReactorName(device.Number) != null;
                default:
                    return _catalog.GetGenerator(device.Number) != null;
            }
        }

        /// <summary>
        /// Добавляет СКРМ в перечень, если его там еще нет
        /// </summary>
        /// <param name="result">Перечень СКРМ</param>
        /// <param name="match">СКРМ</param>
        private static void AddUnique(List<EfficiencyDeviceMatch> result, EfficiencyDeviceMatch match)
        {
            if (!result.Any(item => item.Type == match.Type && item.Number == match.Number))
            {
                result.Add(match);
            }
        }
    }
}
