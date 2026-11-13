using System;
using System.Collections.Generic;
using System.Linq;

using VrdoPdg.Model.Entities;
using VrdoPdg.Model.Regime;
using VrdoPdg.Model.SmpSmzu;

namespace VrdoPdg.Model.Services
{
    /// <summary>
    /// Исходные данные сеанса: загруженные МТ и файл smp_smzu.csv, справочные данные БД,
    /// графики напряжений в КП и исходный режим по часам.
    /// </summary>
    public sealed class SessionData
    {
        /// <summary>
        /// СКРМ по коду.
        /// </summary>
        private readonly Dictionary<int, Device> _devices;

        /// <summary>
        /// Номера узлов подключения генераторов по коду СКРМ.
        /// </summary>
        private readonly Dictionary<int, int> _generatorNodes;

        /// <summary>
        /// Справочник СКРМ, дополняемый СКРМ, добавленными в БД после загрузки данных
        /// </summary>
        private readonly List<Device> _deviceList;

        /// <summary>
        /// Создает исходные данные сеанса.
        /// </summary>
        /// <param name="megapointPath">Путь к файлу МТ.</param>
        /// <param name="smpSmzuPath">Путь к файлу smp_smzu.csv.</param>
        /// <param name="smpSmzuDocument">Содержимое файла smp_smzu.csv.</param>
        /// <param name="powerSystems">Энергосистемы.</param>
        /// <param name="controlPoints">КП.</param>
        /// <param name="devices">Справочник СКРМ.</param>
        /// <param name="schedule">Графики напряжений в КП.</param>
        /// <param name="initialRegime">Исходный режим по часам.</param>
        /// <param name="generatorNodes">Номера узлов подключения генераторов по коду СКРМ.</param>
        /// <param name="fixedDeviceHours">Часы, в которых состояние СКРМ задано в файле smp_smzu.csv.</param>
        /// <param name="loadedAt">Дата и время загрузки.</param>
        /// <param name="catalog">Перечень объектов МТ; <c>null</c> — пустой перечень</param>
        public SessionData(
            string megapointPath,
            string smpSmzuPath,
            SmpSmzuDocument smpSmzuDocument,
            IReadOnlyList<PowerSystem> powerSystems,
            IReadOnlyList<ControlPoint> controlPoints,
            IReadOnlyList<Device> devices,
            VoltageSchedule schedule,
            RegimeSnapshot initialRegime,
            IReadOnlyDictionary<int, int> generatorNodes,
            IEnumerable<(int DeviceId, int Hour)> fixedDeviceHours,
            DateTime loadedAt,
            MegapointCatalog? catalog = null)
        {
            MegapointPath = megapointPath;
            SmpSmzuPath = smpSmzuPath;
            SmpSmzuDocument = smpSmzuDocument;
            PowerSystems = powerSystems;
            ControlPoints = controlPoints;
            Schedule = schedule;
            InitialRegime = initialRegime;
            LoadedAt = loadedAt;
            _deviceList = devices.ToList();
            _devices = devices.ToDictionary(device => device.Id);
            _generatorNodes = new Dictionary<int, int>(generatorNodes);
            FixedDeviceHours = new HashSet<(int DeviceId, int Hour)>(fixedDeviceHours);
            Catalog = catalog ?? MegapointCatalog.Empty;
        }

        /// <summary>
        /// Путь к файлу МТ.
        /// </summary>
        public string MegapointPath { get; }

        /// <summary>
        /// Перечень узлов, реакторов и генераторов загруженной МТ
        /// </summary>
        public MegapointCatalog Catalog { get; }

        /// <summary>
        /// Путь к файлу smp_smzu.csv.
        /// </summary>
        public string SmpSmzuPath { get; }

        /// <summary>
        /// Содержимое файла smp_smzu.csv.
        /// </summary>
        public SmpSmzuDocument SmpSmzuDocument { get; }

        /// <summary>
        /// Энергосистемы.
        /// </summary>
        public IReadOnlyList<PowerSystem> PowerSystems { get; }

        /// <summary>
        /// КП.
        /// </summary>
        public IReadOnlyList<ControlPoint> ControlPoints { get; }

        /// <summary>
        /// Справочник СКРМ
        /// </summary>
        public IReadOnlyList<Device> Devices => _deviceList;

        /// <summary>
        /// Графики напряжений в КП.
        /// </summary>
        public VoltageSchedule Schedule { get; }

        /// <summary>
        /// Исходный режим по часам: напряжения в узлах КП и режимы СКРМ до расчета.
        /// </summary>
        public RegimeSnapshot InitialRegime { get; }

        /// <summary>
        /// Дата и время загрузки исходных данных.
        /// </summary>
        public DateTime LoadedAt { get; }

        /// <summary>
        /// Часы, в которых состояние СКРМ задано в файле smp_smzu.csv.
        /// </summary>
        public IReadOnlySet<(int DeviceId, int Hour)> FixedDeviceHours { get; }

        /// <summary>
        /// Проверяет, задано ли состояние СКРМ в заданном часе в файле smp_smzu.csv.
        /// </summary>
        /// <param name="deviceId">Код СКРМ.</param>
        /// <param name="hour">Час суток (1–24).</param>
        /// <returns><c>true</c>, если состояние СКРМ задано и алгоритм его не изменяет.</returns>
        public bool IsFixed(int deviceId, int hour)
        {
            return FixedDeviceHours.Contains((deviceId, hour));
        }

        /// <summary>
        /// Возвращает номер узла подключения генератора в МТ.
        /// </summary>
        /// <param name="deviceId">Код СКРМ генератора.</param>
        /// <returns>Номер узла или <c>null</c>, если генератор не найден в МТ.</returns>
        public int? GetGeneratorNode(int deviceId)
        {
            int nodeNumber;
            return _generatorNodes.TryGetValue(deviceId, out nodeNumber)
                ? nodeNumber
                : (int?)null;
        }

        /// <summary>
        /// Дополняет справочник СКРМ сеанса СКРМ, добавленными в БД после загрузки данных
        /// (например, СКРМ МТ, добавленными в таблицу эффективности). Узлы подключения
        /// новых генераторов определяются по перечню объектов МТ
        /// </summary>
        /// <param name="devices">СКРМ из БД</param>
        /// <returns>СКРМ, которых не было в справочнике сеанса</returns>
        public IReadOnlyList<Device> AddDevices(IEnumerable<Device> devices)
        {
            List<Device> added = new List<Device>();
            foreach (Device device in devices.Where(item => !_devices.ContainsKey(item.Id)))
            {
                _devices[device.Id] = device;
                _deviceList.Add(device);
                MegapointGenerator? generator = device.Type == DeviceType.Generator
                    ? Catalog.GetGenerator(device.Number)
                    : null;
                if (generator != null)
                {
                    _generatorNodes[device.Id] = generator.NodeNumber;
                }

                added.Add(device);
            }

            return added;
        }
    }
}
