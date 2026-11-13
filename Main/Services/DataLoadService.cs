using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using VrdoPdg.Model.Entities;
using VrdoPdg.Model.Protocol;
using VrdoPdg.Model.Regime;
using VrdoPdg.Model.Schedules;
using VrdoPdg.Model.SmpSmzu;
using VrdoPdg.Model.Storage;

namespace VrdoPdg.Model.Services
{
    /// <summary>
    /// Сервис загрузки данных: проверяет исходные файлы, считывает МТ и файл smp_smzu.csv,
    /// получает границы графиков напряжений, сохраняет данные в БД и передает исходные
    /// данные в сеанс.
    /// </summary>
    /// <remarks>
    /// Методы сервиса обращаются к ПО «Барс» и должны вызываться из потока,
    /// в котором работает модуль взаимодействия с ПО «Барс».
    /// </remarks>
    public sealed class DataLoadService
    {
        /// <summary>
        /// Сообщение об ошибке обращения к ПО «Барс».
        /// </summary>
        public const string BarsErrorMessage = "Ошибка обращения к ПО «Барс»";

        /// <summary>
        /// Сообщение об ошибке обращения к БД.
        /// </summary>
        public const string DatabaseErrorMessage = "Ошибка обращения к базе данных";

        /// <summary>
        /// Предупреждение о недоступности ОИК «СК-11».
        /// </summary>
        public const string ScheduleNotUpdatedMessage = "Не обновлены данные для проведения расчета";

        /// <summary>
        /// Модуль взаимодействия с ПО «Барс».
        /// </summary>
        private readonly IRastrAdapter _rastrAdapter;

        /// <summary>
        /// Репозиторий БД.
        /// </summary>
        private readonly IModelDataRepository _repository;

        /// <summary>
        /// Модуль взаимодействия с ОИК «СК-11».
        /// </summary>
        private readonly ICkModel _ckModel;

        /// <summary>
        /// Модуль чтения файла smp_smzu.csv.
        /// </summary>
        private readonly ISmpSmzuCsvReader _smpSmzuReader;

        /// <summary>
        /// Протокол расчета.
        /// </summary>
        private readonly CalculationProtocol _protocol;

        /// <summary>
        /// Объекты МТ, значения которых считываются для форм просмотра режима.
        /// </summary>
        private readonly ObservedObjects _observedObjects;

        /// <summary>
        /// Создает сервис загрузки данных.
        /// </summary>
        /// <param name="rastrAdapter">Модуль взаимодействия с ПО «Барс».</param>
        /// <param name="repository">Репозиторий БД.</param>
        /// <param name="ckModel">Модуль взаимодействия с ОИК «СК-11».</param>
        /// <param name="smpSmzuReader">Модуль чтения файла smp_smzu.csv.</param>
        /// <param name="protocol">Протокол расчета.</param>
        /// <param name="observedObjects">Объекты МТ, значения которых считываются для форм просмотра режима.</param>
        public DataLoadService(
            IRastrAdapter rastrAdapter,
            IModelDataRepository repository,
            ICkModel ckModel,
            ISmpSmzuCsvReader smpSmzuReader,
            CalculationProtocol protocol,
            ObservedObjects observedObjects)
        {
            _rastrAdapter = rastrAdapter ?? throw new ArgumentNullException(nameof(rastrAdapter));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _ckModel = ckModel ?? throw new ArgumentNullException(nameof(ckModel));
            _smpSmzuReader = smpSmzuReader ?? throw new ArgumentNullException(nameof(smpSmzuReader));
            _protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
            _observedObjects = observedObjects ?? throw new ArgumentNullException(nameof(observedObjects));
        }

        /// <summary>
        /// Загружает исходные данные для ввода режима по напряжению в допустимую область.
        /// </summary>
        /// <param name="megapointPath">Путь к файлу МТ (smzu_mega_act.mptsmz).</param>
        /// <param name="smpSmzuPath">Путь к файлу smp_smzu.csv.</param>
        /// <returns>Исходные данные сеанса.</returns>
        /// <exception cref="VrdoException">Ошибка загрузки; ошибка записана в протокол.</exception>
        public SessionData Load(string megapointPath, string smpSmzuPath)
        {
            ProtocolEntry group = _protocol.Add(
                ProtocolTag.Info,
                "Загрузка исходных данных",
                Path.GetFileName(megapointPath) + ", " + Path.GetFileName(smpSmzuPath));
            try
            {
                _protocol.Add(ProtocolTag.Info, "Проверка файлов исходных данных", null, group);
                SourceFileValidator.Validate(megapointPath, SourceFileValidator.MegapointExtension);
                SourceFileValidator.Validate(smpSmzuPath, SourceFileValidator.SmpSmzuExtension);

                ReferenceData reference = ReadReferenceData(group);
                SmpSmzuDocument document = ReadSmpSmzu(smpSmzuPath, group);
                HashSet<(int DeviceId, int Hour)> fixedDeviceHours = GetFixedDeviceHours(document, reference.Devices);
                MegapointData megapoint = ReadMegapoint(megapointPath, reference, fixedDeviceHours, group);
                VoltageSchedule schedule = ReadVoltageSchedule(reference.ControlPoints, group);
                SaveMegapointData(megapoint, group);

                SessionData session = new SessionData(
                    megapointPath,
                    smpSmzuPath,
                    document,
                    reference.PowerSystems,
                    reference.ControlPoints,
                    reference.Devices,
                    schedule,
                    megapoint.Regime,
                    megapoint.GeneratorNodes,
                    fixedDeviceHours,
                    DateTime.Now,
                    megapoint.Catalog);
                _protocol.Add(
                    ProtocolTag.Info,
                    "Загрузка исходных данных завершена",
                    "КП: " + reference.ControlPoints.Count + "; СКРМ: " + reference.Devices.Count,
                    group);
                return session;
            }
            catch (VrdoException exception)
            {
                _protocol.Add(ProtocolTag.Error, exception.Message, exception.Details, group);
                throw;
            }
        }

        /// <summary>
        /// Определяет часы, в которых состояние коммутируемых СКРМ задано в файле smp_smzu.csv.
        /// </summary>
        /// <param name="document">Содержимое файла smp_smzu.csv.</param>
        /// <param name="devices">Справочник СКРМ.</param>
        /// <returns>Пары «код СКРМ — час».</returns>
        internal static HashSet<(int DeviceId, int Hour)> GetFixedDeviceHours(
            SmpSmzuDocument document,
            IReadOnlyList<Device> devices)
        {
            Dictionary<int, Device> reactors = new Dictionary<int, Device>();
            foreach (Device device in devices.Where(item => item.Type == DeviceType.Switched))
            {
                reactors[device.Number] = device;
            }

            HashSet<(int DeviceId, int Hour)> result = new HashSet<(int DeviceId, int Hour)>();
            foreach (SmpSmzuRecord record in document.Records.Where(item => item.Type == SmpSmzuRecordType.Reactor))
            {
                Device? device;
                if (!reactors.TryGetValue(record.Address1, out device))
                {
                    continue;
                }

                int startHour = Math.Max(1, record.StartHour);
                int endHour = Math.Min(RegimeSnapshot.HourCount, record.EndHour);
                for (int hour = startHour; hour <= endHour; hour++)
                {
                    result.Add((device.Id, hour));
                }
            }

            return result;
        }

        /// <summary>
        /// Считывает из БД справочные данные: энергосистемы, КП и СКРМ.
        /// </summary>
        /// <param name="group">Запись протокола о загрузке.</param>
        /// <returns>Справочные данные.</returns>
        private ReferenceData ReadReferenceData(ProtocolEntry group)
        {
            try
            {
                ReferenceData reference = new ReferenceData(
                    _repository.GetPowerSystems(),
                    _repository.GetControlPoints(),
                    _repository.GetDevices());
                if (reference.ControlPoints.Count == 0)
                {
                    _protocol.Add(
                        ProtocolTag.Warning,
                        "В базе данных нет контрольных пунктов",
                        "ввод режима выполняться не будет",
                        group);
                }

                return reference;
            }
            catch (Exception exception) when (!(exception is VrdoException))
            {
                throw new VrdoException(DatabaseErrorMessage, exception.Message, exception);
            }
        }

        /// <summary>
        /// Считывает файл smp_smzu.csv.
        /// </summary>
        /// <param name="path">Путь к файлу.</param>
        /// <param name="group">Запись протокола о загрузке.</param>
        /// <returns>Содержимое файла.</returns>
        private SmpSmzuDocument ReadSmpSmzu(string path, ProtocolEntry group)
        {
            _protocol.Add(ProtocolTag.Info, "Начало чтения файла smp_smzu.csv", path, group);
            SmpSmzuDocument document;
            try
            {
                document = _smpSmzuReader.Read(path);
            }
            catch (Exception exception) when (!(exception is VrdoException))
            {
                throw new DataFileException(DataFileError.InvalidContent, path, exception.Message, exception);
            }

            int reactorRecordCount = document.Records.Count(record => record.Type == SmpSmzuRecordType.Reactor);
            _protocol.Add(
                ProtocolTag.Info,
                "Окончание чтения файла smp_smzu.csv",
                "записей: " + document.Records.Count + "; записей о состоянии реакторов и БСК: " + reactorRecordCount,
                group);
            return document;
        }

        /// <summary>
        /// Считывает из МТ напряжения в узлах КП и режимы СКРМ по каждому часу.
        /// </summary>
        /// <param name="path">Путь к файлу МТ.</param>
        /// <param name="reference">Справочные данные.</param>
        /// <param name="fixedDeviceHours">Часы, в которых состояние СКРМ задано в файле smp_smzu.csv.</param>
        /// <param name="group">Запись протокола о загрузке.</param>
        /// <returns>Данные МТ.</returns>
        private MegapointData ReadMegapoint(
            string path,
            ReferenceData reference,
            HashSet<(int DeviceId, int Hour)> fixedDeviceHours,
            ProtocolEntry group)
        {
            _protocol.Add(ProtocolTag.Info, "Начало чтения МТ", path, group);
            try
            {
                _rastrAdapter.Open(path);
                MegapointCatalog catalog = _rastrAdapter.ReadCatalog();

                RegimeSnapshot regime = new RegimeSnapshot();
                Dictionary<int, int> generatorNodes = new Dictionary<int, int>();
                for (int hour = 1; hour <= RegimeSnapshot.HourCount; hour++)
                {
                    if (!_rastrAdapter.SelectHour(hour))
                    {
                        _protocol.Add(
                            ProtocolTag.Warning,
                            "Час " + hour + ": расчет режима МТ не сошелся",
                            "значения напряжений часа могут быть недостоверны",
                            group);
                    }

                    if (hour == 1)
                    {
                        CheckMegapointObjects(reference, generatorNodes, group);
                    }

                    RegimeReader.ReadHour(
                        _rastrAdapter,
                        hour,
                        reference.ControlPoints,
                        reference.Devices,
                        fixedDeviceHours,
                        _observedObjects,
                        regime);
                }

                _protocol.Add(
                    ProtocolTag.Info,
                    "Окончание чтения МТ",
                    "часов: " + RegimeSnapshot.HourCount
                        + "; значений напряжения: " + regime.Voltages.Count
                        + "; режимов СКРМ: " + regime.Modes.Count,
                    group);
                return new MegapointData(regime, generatorNodes, catalog);
            }
            catch (Exception exception) when (!(exception is VrdoException))
            {
                throw new VrdoException(BarsErrorMessage, exception.Message, exception);
            }
        }

        /// <summary>
        /// Проверяет наличие узлов КП и СКРМ в МТ и считывает узлы подключения генераторов.
        /// </summary>
        /// <param name="reference">Справочные данные.</param>
        /// <param name="generatorNodes">Узлы подключения генераторов по коду СКРМ.</param>
        /// <param name="group">Запись протокола о загрузке.</param>
        private void CheckMegapointObjects(
            ReferenceData reference,
            Dictionary<int, int> generatorNodes,
            ProtocolEntry group)
        {
            foreach (ControlPoint controlPoint in reference.ControlPoints)
            {
                if (controlPoint.MainNode == null)
                {
                    _protocol.Add(
                        ProtocolTag.Warning,
                        "Не задан основной узел КП",
                        controlPoint.Name + " — КП не участвует в расчете",
                        group);
                }

                foreach (ControlPointNode node in controlPoint.Nodes)
                {
                    if (_rastrAdapter.GetNodeVoltage(node.Number) == null)
                    {
                        _protocol.Add(
                            ProtocolTag.Warning,
                            "Узел КП не найден в МТ или отключен",
                            controlPoint.Name + ": узел " + node.Number,
                            group);
                    }
                }
            }

            foreach (Device device in reference.Devices)
            {
                bool isFound;
                switch (device.Type)
                {
                    case DeviceType.Controlled:
                        isFound = _rastrAdapter.GetControlledDevice(device.Number) != null;
                        break;
                    case DeviceType.Switched:
                        isFound = _rastrAdapter.GetSwitchedDevice(device.Number) != null;
                        break;
                    default:
                        isFound = ReadGenerator(device, generatorNodes);
                        break;
                }

                if (!isFound)
                {
                    _protocol.Add(ProtocolTag.Warning, "СКРМ не найдено в МТ", device.ToString(), group);
                }
            }
        }

        /// <summary>
        /// Считывает узел подключения генератора.
        /// </summary>
        /// <param name="device">СКРМ генератора.</param>
        /// <param name="generatorNodes">Узлы подключения генераторов по коду СКРМ.</param>
        /// <returns><c>true</c>, если генератор найден в МТ.</returns>
        private bool ReadGenerator(Device device, Dictionary<int, int> generatorNodes)
        {
            GeneratorState? generator = _rastrAdapter.GetGenerator(device.Number);
            if (generator == null)
            {
                return false;
            }

            generatorNodes[device.Id] = generator.NodeNumber;
            return true;
        }

        /// <summary>
        /// Получает границы графиков напряжений в КП из ОИК «СК-11», а при его
        /// недоступности — последние сохраненные в БД границы.
        /// </summary>
        /// <param name="controlPoints">КП.</param>
        /// <param name="group">Запись протокола о загрузке.</param>
        /// <returns>Графики напряжений в КП.</returns>
        private VoltageSchedule ReadVoltageSchedule(IReadOnlyList<ControlPoint> controlPoints, ProtocolEntry group)
        {
            _protocol.Add(ProtocolTag.Info, "Запрос границ графиков напряжений в КП", _ckModel.SourceName, group);

            IReadOnlyList<VoltageBounds>? received = null;
            try
            {
                received = _ckModel.GetVoltageSchedule(controlPoints, DateTime.Today);
            }
            catch (Exception exception)
            {
                _protocol.Add(ProtocolTag.Warning, ScheduleNotUpdatedMessage, exception.Message, group);
            }

            IReadOnlyList<VoltageBounds> bounds;
            try
            {
                if (received != null)
                {
                    _repository.SaveVoltageSchedule(received);
                    bounds = received;
                    _protocol.Add(
                        ProtocolTag.Info,
                        "Границы графиков напряжений получены и сохранены в БД",
                        _ckModel.SourceName + "; КП: " + controlPoints.Count,
                        group);
                }
                else
                {
                    bounds = _repository.GetVoltageSchedule();
                    string receivedAt = bounds.Count == 0
                        ? "сохраненных границ нет"
                        : "получены " + bounds.Max(item => item.ReceivedAt).ToString("dd.MM.yyyy HH:mm:ss");
                    _protocol.Add(
                        ProtocolTag.Info,
                        "Использованы границы графиков, сохраненные в БД",
                        receivedAt,
                        group);
                }
            }
            catch (Exception exception)
            {
                throw new VrdoException(DatabaseErrorMessage, exception.Message, exception);
            }

            VoltageSchedule schedule = new VoltageSchedule(bounds);
            foreach (ControlPoint controlPoint in controlPoints)
            {
                if (!schedule.IsComplete(controlPoint.Id, RegimeSnapshot.HourCount))
                {
                    _protocol.Add(
                        ProtocolTag.Warning,
                        "Не задан график напряжения КП",
                        controlPoint.Name + " — КП не участвует в расчете",
                        group);
                }
            }

            return schedule;
        }

        /// <summary>
        /// Сохраняет данные МТ в БД.
        /// </summary>
        /// <param name="megapoint">Данные МТ.</param>
        /// <param name="group">Запись протокола о загрузке.</param>
        private void SaveMegapointData(MegapointData megapoint, ProtocolEntry group)
        {
            try
            {
                IReadOnlyList<NodeVoltage> voltages = megapoint.Regime.Voltages;
                IReadOnlyList<DeviceMode> modes = megapoint.Regime.Modes;
                _repository.SaveRegime(voltages, modes);
                _protocol.Add(
                    ProtocolTag.Info,
                    "Данные МТ сохранены в БД",
                    "напряжений: " + voltages.Count + "; режимов СКРМ: " + modes.Count,
                    group);
            }
            catch (Exception exception)
            {
                throw new VrdoException(DatabaseErrorMessage, exception.Message, exception);
            }
        }

        /// <summary>
        /// Справочные данные БД.
        /// </summary>
        private sealed class ReferenceData
        {
            /// <summary>
            /// Создает справочные данные.
            /// </summary>
            /// <param name="powerSystems">Энергосистемы.</param>
            /// <param name="controlPoints">КП.</param>
            /// <param name="devices">СКРМ.</param>
            public ReferenceData(
                IReadOnlyList<PowerSystem> powerSystems,
                IReadOnlyList<ControlPoint> controlPoints,
                IReadOnlyList<Device> devices)
            {
                PowerSystems = powerSystems;
                ControlPoints = controlPoints;
                Devices = devices;
            }

            /// <summary>
            /// Энергосистемы.
            /// </summary>
            public IReadOnlyList<PowerSystem> PowerSystems { get; }

            /// <summary>
            /// КП.
            /// </summary>
            public IReadOnlyList<ControlPoint> ControlPoints { get; }

            /// <summary>
            /// СКРМ.
            /// </summary>
            public IReadOnlyList<Device> Devices { get; }
        }

        /// <summary>
        /// Данные, считанные из МТ.
        /// </summary>
        private sealed class MegapointData
        {
            /// <summary>
            /// Создает данные МТ.
            /// </summary>
            /// <param name="regime">Режим по часам.</param>
            /// <param name="generatorNodes">Узлы подключения генераторов.</param>
            /// <param name="catalog">Перечень объектов МТ</param>
            public MegapointData(
                RegimeSnapshot regime,
                IReadOnlyDictionary<int, int> generatorNodes,
                MegapointCatalog catalog)
            {
                Regime = regime;
                GeneratorNodes = generatorNodes;
                Catalog = catalog;
            }

            /// <summary>
            /// Режим по часам.
            /// </summary>
            public RegimeSnapshot Regime { get; }

            /// <summary>
            /// Узлы подключения генераторов по коду СКРМ.
            /// </summary>
            public IReadOnlyDictionary<int, int> GeneratorNodes { get; }

            /// <summary>
            /// Перечень объектов МТ
            /// </summary>
            public MegapointCatalog Catalog { get; }
        }
    }
}
