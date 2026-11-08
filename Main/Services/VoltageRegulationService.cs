using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using VrdoPdg.Model.Entities;
using VrdoPdg.Model.Protocol;
using VrdoPdg.Model.Regime;
using VrdoPdg.Model.Storage;

namespace VrdoPdg.Model.Services
{
    /// <summary>
    /// Сервис ввода режима по напряжению в допустимую область. Для каждого часа и каждого КП
    /// сравнивает напряжение с границами графика и в порядке, заданном таблицей эффективности,
    /// изменяет загрузку управляемых СКРМ, состояние коммутируемых СКРМ (кроме заданных в файле
    /// smp_smzu.csv) и заданное напряжение генераторов.
    /// </summary>
    /// <remarks>
    /// Методы сервиса обращаются к ПО «Барс» и должны вызываться из потока,
    /// в котором работает модуль взаимодействия с ПО «Барс».
    /// </remarks>
    public sealed class VoltageRegulationService
    {
        /// <summary>
        /// Шаг изменения заданного напряжения на шинах генератора, кВ.
        /// </summary>
        public const double GeneratorVoltageStep = 0.3;

        /// <summary>
        /// Наибольшее число шагов изменения заданного напряжения одного генератора в часе.
        /// </summary>
        public const int MaxGeneratorSteps = 30;

        /// <summary>
        /// Наибольшее число проходов по КП в часе: повторные проходы вводят в допустимую
        /// область КП, режим в которых нарушен действиями для других КП.
        /// </summary>
        public const int MaxPasses = 3;

        /// <summary>
        /// Допустимая погрешность сравнения реактивной мощности, Мвар.
        /// </summary>
        private const double ReactivePowerTolerance = 0.01;

        /// <summary>
        /// Модуль взаимодействия с ПО «Барс».
        /// </summary>
        private readonly IRastrAdapter _rastrAdapter;

        /// <summary>
        /// Репозиторий БД.
        /// </summary>
        private readonly IModelDataRepository _repository;

        /// <summary>
        /// Сеанс и архив расчетов.
        /// </summary>
        private readonly CalculationArchiveService _archiveService;

        /// <summary>
        /// Протокол расчета.
        /// </summary>
        private readonly CalculationProtocol _protocol;

        /// <summary>
        /// Создает сервис ввода режима.
        /// </summary>
        /// <param name="rastrAdapter">Модуль взаимодействия с ПО «Барс».</param>
        /// <param name="repository">Репозиторий БД.</param>
        /// <param name="archiveService">Сеанс и архив расчетов.</param>
        /// <param name="protocol">Протокол расчета.</param>
        public VoltageRegulationService(
            IRastrAdapter rastrAdapter,
            IModelDataRepository repository,
            CalculationArchiveService archiveService,
            CalculationProtocol protocol)
        {
            _rastrAdapter = rastrAdapter ?? throw new ArgumentNullException(nameof(rastrAdapter));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _archiveService = archiveService ?? throw new ArgumentNullException(nameof(archiveService));
            _protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
        }

        /// <summary>
        /// Выполняет ввод режима по напряжению в допустимую область для всех часов МТ
        /// и помещает завершенный расчет в архив. Каждый расчет начинается с исходного
        /// состояния МТ, загруженной технологом.
        /// </summary>
        /// <param name="session">Исходные данные сеанса.</param>
        /// <param name="progress">Получатель процента выполнения расчета.</param>
        /// <param name="cancellationToken">Токен остановки расчета.</param>
        /// <returns>Расчет, помещенный в архив.</returns>
        /// <exception cref="OperationCanceledException">Расчет остановлен технологом.</exception>
        /// <exception cref="VrdoException">Ошибка обращения к ПО «Барс» или БД.</exception>
        public CalculationResult Run(SessionData session, IProgress<int>? progress, CancellationToken cancellationToken)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            DateTime startedAt = DateTime.Now;
            _protocol.Add(
                ProtocolTag.Info,
                "Запуск процесса ВРДО",
                "МТ: " + session.MegapointPath);
            try
            {
                IReadOnlyDictionary<int, IReadOnlyList<EfficiencyItem>> tables = LoadEfficiencyTables(session);
                IReadOnlyList<ControlPoint> controlPoints = SelectControlPoints(session);
                RegulationRun run = new RegulationRun(session, tables, controlPoints, _protocol);

                OpenMegapoint(session.MegapointPath);
                for (int hour = 1; hour <= RegimeSnapshot.HourCount; hour++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    ProcessHour(run, hour, progress, cancellationToken);
                }

                progress?.Report(100);
                CalculationResult result = _archiveService.Add(
                    session,
                    startedAt,
                    DateTime.Now,
                    tables,
                    run.After,
                    run.Unresolved.ToList());
                WriteSummary(run, result);
                return result;
            }
            catch (OperationCanceledException)
            {
                _protocol.Add(
                    ProtocolTag.Info,
                    "Расчет остановлен",
                    "результаты незавершенного расчета в архив не сохраняются");
                throw;
            }
            catch (VrdoException exception)
            {
                _protocol.Add(ProtocolTag.Error, exception.Message, exception.Details);
                throw;
            }
            catch (Exception exception)
            {
                _protocol.Add(ProtocolTag.Error, DataLoadService.BarsErrorMessage, exception.Message);
                throw new VrdoException(DataLoadService.BarsErrorMessage, exception.Message, exception);
            }
        }

        /// <summary>
        /// Определяет направление ввода режима.
        /// </summary>
        /// <param name="voltage">Напряжение, кВ.</param>
        /// <param name="bounds">Границы графика.</param>
        /// <returns>Направление или <c>null</c>, если напряжение в допустимой области.</returns>
        private static RegulationDirection? GetDirection(double voltage, VoltageBounds bounds)
        {
            if (bounds.IsAbove(voltage))
            {
                return RegulationDirection.Decrease;
            }

            if (bounds.IsBelow(voltage))
            {
                return RegulationDirection.Increase;
            }

            return null;
        }

        /// <summary>
        /// Описывает положение напряжения относительно границ графика для записи УР.
        /// </summary>
        /// <param name="voltage">Напряжение, кВ.</param>
        /// <param name="bounds">Границы графика.</param>
        /// <returns>Пояснение к записи протокола.</returns>
        private static string DescribePosition(double voltage, VoltageBounds bounds)
        {
            if (bounds.IsAbove(voltage))
            {
                return "выше границы графика";
            }

            return bounds.IsBelow(voltage)
                ? "ниже границы графика"
                : "в допустимой области";
        }

        /// <summary>
        /// Считывает действующие таблицы эффективности из БД.
        /// </summary>
        /// <param name="session">Исходные данные сеанса.</param>
        /// <returns>Таблицы эффективности по коду КП.</returns>
        private IReadOnlyDictionary<int, IReadOnlyList<EfficiencyItem>> LoadEfficiencyTables(SessionData session)
        {
            try
            {
                return EfficiencyTableService.BuildActiveTables(_repository.GetEfficiencyEntries(), session.Devices);
            }
            catch (Exception exception)
            {
                throw new VrdoException(DataLoadService.DatabaseErrorMessage, exception.Message, exception);
            }
        }

        /// <summary>
        /// Отбирает КП, участвующие в расчете: с основным узлом и графиком напряжения на все часы.
        /// </summary>
        /// <param name="session">Исходные данные сеанса.</param>
        /// <returns>КП, участвующие в расчете.</returns>
        private IReadOnlyList<ControlPoint> SelectControlPoints(SessionData session)
        {
            List<ControlPoint> result = new List<ControlPoint>();
            foreach (ControlPoint controlPoint in session.ControlPoints)
            {
                if (controlPoint.MainNode == null)
                {
                    _protocol.Add(
                        ProtocolTag.Warning,
                        "КП не участвует в расчете",
                        controlPoint.Name + ": не задан основной узел");
                    continue;
                }

                if (!session.Schedule.IsComplete(controlPoint.Id, RegimeSnapshot.HourCount))
                {
                    _protocol.Add(
                        ProtocolTag.Warning,
                        "КП не участвует в расчете",
                        controlPoint.Name + ": не задан график напряжения");
                    continue;
                }

                result.Add(controlPoint);
            }

            return result;
        }

        /// <summary>
        /// Открывает МТ, загруженную технологом.
        /// </summary>
        /// <param name="path">Путь к файлу МТ.</param>
        private void OpenMegapoint(string path)
        {
            try
            {
                _rastrAdapter.Open(path);
            }
            catch (Exception exception) when (!(exception is VrdoException))
            {
                throw new VrdoException(DataLoadService.BarsErrorMessage, exception.Message, exception);
            }
        }

        /// <summary>
        /// Выполняет ввод режима в одном часе МТ и считывает режим после расчета.
        /// Изменения параметров МТ после обработки часа отменяются, чтобы следующий
        /// час рассчитывался от исходного состояния.
        /// </summary>
        /// <param name="run">Данные расчета.</param>
        /// <param name="hour">Час суток (1–24).</param>
        /// <param name="progress">Получатель процента выполнения.</param>
        /// <param name="cancellationToken">Токен остановки расчета.</param>
        private void ProcessHour(
            RegulationRun run,
            int hour,
            IProgress<int>? progress,
            CancellationToken cancellationToken)
        {
            if (!_rastrAdapter.SelectHour(hour))
            {
                _protocol.Add(
                    ProtocolTag.Warning,
                    "Час " + hour + ": расчет режима МТ не сошелся",
                    "ввод режима в этом часе не выполнялся");
                run.After.CopyHour(run.Session.InitialRegime, hour);
                progress?.Report(hour * 100 / RegimeSnapshot.HourCount);
                return;
            }

            HourState hourState = new HourState(hour);
            try
            {
                int count = run.ControlPoints.Count;
                for (int pass = 1; pass <= MaxPasses; pass++)
                {
                    bool isChanged = false;
                    for (int index = 0; index < count; index++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        isChanged |= RegulateControlPoint(run, hourState, run.ControlPoints[index], cancellationToken);
                        if (pass == 1)
                        {
                            int done = (hour - 1) * count + index + 1;
                            progress?.Report(done * 100 / (RegimeSnapshot.HourCount * count));
                        }
                    }

                    if (!isChanged)
                    {
                        break;
                    }
                }

                CheckControlPoints(run, hour);
                RegimeReader.ReadHour(
                    _rastrAdapter,
                    hour,
                    run.Session.ControlPoints,
                    run.Session.Devices,
                    run.Session,
                    run.After);
                progress?.Report(hour * 100 / RegimeSnapshot.HourCount);
            }
            finally
            {
                _rastrAdapter.UndoAllChanges();
            }
        }

        /// <summary>
        /// Вводит в допустимую область напряжение в основном узле КП (рисунки В.1, В.2).
        /// </summary>
        /// <param name="run">Данные расчета.</param>
        /// <param name="hourState">Состояние часа.</param>
        /// <param name="controlPoint">КП.</param>
        /// <param name="cancellationToken">Токен остановки расчета.</param>
        /// <returns><c>true</c>, если выполнено хотя бы одно изменение СКРМ.</returns>
        private bool RegulateControlPoint(
            RegulationRun run,
            HourState hourState,
            ControlPoint controlPoint,
            CancellationToken cancellationToken)
        {
            VoltageBounds? bounds = run.Session.Schedule.GetBounds(controlPoint.Id, hourState.Hour);
            ControlPointNode? mainNode = controlPoint.MainNode;
            if (bounds == null || mainNode == null)
            {
                return false;
            }

            double? voltage = _rastrAdapter.GetNodeVoltage(mainNode.Number);
            if (!voltage.HasValue)
            {
                return false;
            }

            RegulationDirection? direction = GetDirection(voltage.Value, bounds);
            if (!direction.HasValue)
            {
                return false;
            }

            ProtocolEntry hourEntry = run.GetHourEntry(controlPoint, hourState.Hour, voltage.Value, bounds);
            IReadOnlyList<EfficiencyItem> items = run.GetEfficiencyTable(controlPoint.Id);
            bool isChanged = false;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                DeviceChange? change = FindNextChange(run, hourState, controlPoint, items, direction.Value);
                if (change == null)
                {
                    break;
                }

                if (ApplyChange(hourState, controlPoint, change, hourEntry, mainNode.Number, bounds))
                {
                    isChanged = true;
                }

                voltage = _rastrAdapter.GetNodeVoltage(mainNode.Number);
                if (!voltage.HasValue || GetDirection(voltage.Value, bounds) != direction)
                {
                    break;
                }
            }

            return isChanged;
        }

        /// <summary>
        /// Ищет первое по порядку таблицы эффективности СКРМ, у которого есть резерв
        /// реактивной мощности в требуемом направлении.
        /// </summary>
        /// <param name="run">Данные расчета.</param>
        /// <param name="hourState">Состояние часа.</param>
        /// <param name="controlPoint">КП.</param>
        /// <param name="items">Таблица эффективности КП.</param>
        /// <param name="direction">Направление ввода режима.</param>
        /// <returns>Изменение СКРМ или <c>null</c>, если резервы исчерпаны.</returns>
        private DeviceChange? FindNextChange(
            RegulationRun run,
            HourState hourState,
            ControlPoint controlPoint,
            IReadOnlyList<EfficiencyItem> items,
            RegulationDirection direction)
        {
            foreach (EfficiencyItem item in items)
            {
                Device device = item.Device;
                if (hourState.IsExhausted(controlPoint.Id, device.Id) || hourState.IsLocked(device.Id, direction))
                {
                    continue;
                }

                DeviceChange? change;
                switch (device.Type)
                {
                    case DeviceType.Controlled:
                        change = CreateControlledChange(device, direction);
                        break;
                    case DeviceType.Switched:
                        change = run.Session.IsFixed(device.Id, hourState.Hour)
                            ? null
                            : CreateSwitchedChange(device, direction);
                        break;
                    default:
                        change = CreateGeneratorChange(run, hourState, device, direction);
                        break;
                }

                if (change != null)
                {
                    return change;
                }
            }

            return null;
        }

        /// <summary>
        /// Формирует изменение загрузки управляемого СКРМ: разгрузку до Qmin при снижении
        /// напряжения или загрузку до Qmax при повышении. Отключенное СКРМ пропускается.
        /// </summary>
        /// <param name="device">СКРМ.</param>
        /// <param name="direction">Направление ввода режима.</param>
        /// <returns>Изменение или <c>null</c>, если резерва нет.</returns>
        private DeviceChange? CreateControlledChange(Device device, RegulationDirection direction)
        {
            ControlledDeviceState? state = _rastrAdapter.GetControlledDevice(device.Number);
            if (state == null || !state.IsOn)
            {
                return null;
            }

            double target;
            string limitName;
            if (direction == RegulationDirection.Decrease)
            {
                if (state.ReactivePower <= state.MinReactivePower + ReactivePowerTolerance)
                {
                    return null;
                }

                target = state.MinReactivePower;
                limitName = "Qmin";
            }
            else
            {
                if (state.ReactivePower >= state.MaxReactivePower - ReactivePowerTolerance)
                {
                    return null;
                }

                target = state.MaxReactivePower;
                limitName = "Qmax";
            }

            string description = device.TypeName + " · Q: " + ValueFormat.Power(state.ReactivePower)
                + " → " + ValueFormat.Power(target) + " Мвар (" + limitName + ")";
            return new DeviceChange(
                device,
                direction,
                description,
                () => _rastrAdapter.SetControlledDeviceReactivePower(device.Number, target),
                null);
        }

        /// <summary>
        /// Формирует изменение состояния коммутируемого СКРМ: при снижении напряжения
        /// включается реактор или отключается БСК, при повышении — отключается реактор
        /// или включается БСК.
        /// </summary>
        /// <param name="device">СКРМ.</param>
        /// <param name="direction">Направление ввода режима.</param>
        /// <returns>Изменение или <c>null</c>, если СКРМ уже в нужном состоянии.</returns>
        private DeviceChange? CreateSwitchedChange(Device device, RegulationDirection direction)
        {
            SwitchedDeviceState? state = _rastrAdapter.GetSwitchedDevice(device.Number);
            if (state == null)
            {
                return null;
            }

            bool isCapacitorBank = device.IsCapacitorBank;
            bool targetState = direction == RegulationDirection.Decrease
                ? !isCapacitorBank
                : isCapacitorBank;
            if (state.IsOn == targetState)
            {
                return null;
            }

            string stateText;
            if (isCapacitorBank)
            {
                stateText = targetState
                    ? "БСК включена"
                    : "БСК отключена";
            }
            else
            {
                stateText = targetState
                    ? "реактор включен"
                    : "реактор отключен";
            }

            return new DeviceChange(
                device,
                direction,
                device.TypeName + " · " + stateText,
                () => _rastrAdapter.SetSwitchedDeviceState(device.Number, targetState),
                null);
        }

        /// <summary>
        /// Формирует изменение заданного напряжения vzd узла генератора на 0,3 кВ: снижение,
        /// если загрузка узла qg выше qmin, или повышение, если qg ниже qmax. Генератор изменяется,
        /// пока qg не станет равна qmin или qmax; отключенный генератор пропускается.
        /// </summary>
        /// <param name="run">Данные расчета.</param>
        /// <param name="hourState">Состояние часа.</param>
        /// <param name="device">СКРМ генератора.</param>
        /// <param name="direction">Направление ввода режима.</param>
        /// <returns>Изменение или <c>null</c>, если резерва нет.</returns>
        private DeviceChange? CreateGeneratorChange(
            RegulationRun run,
            HourState hourState,
            Device device,
            RegulationDirection direction)
        {
            if (hourState.GetGeneratorSteps(device.Id) >= MaxGeneratorSteps)
            {
                return null;
            }

            GeneratorState? state = _rastrAdapter.GetGenerator(device.Number);
            if (state == null || !state.IsOn || state.SetVoltage <= 0)
            {
                return null;
            }

            ReactivePowerLimits limits = RegimeReader.GetGeneratorLimits(state);
            if (limits.Range <= ReactivePowerTolerance)
            {
                return null;
            }

            double target;
            if (direction == RegulationDirection.Decrease)
            {
                if (state.ReactivePower <= limits.Min + ReactivePowerTolerance)
                {
                    return null;
                }

                target = state.SetVoltage - GeneratorVoltageStep;
            }
            else
            {
                if (state.ReactivePower >= limits.Max - ReactivePowerTolerance)
                {
                    return null;
                }

                target = state.SetVoltage + GeneratorVoltageStep;
            }

            target = Math.Round(target, 3);
            int nodeNumber = state.NodeNumber;
            string description = device.TypeName + " · Uзад: " + ValueFormat.Voltage(state.SetVoltage)
                + " → " + ValueFormat.Voltage(target) + " кВ (qg = " + ValueFormat.Power(state.ReactivePower)
                + " Мвар, qmin…qmax = " + ValueFormat.Power(limits.Min) + "…" + ValueFormat.Power(limits.Max) + ")";
            return new DeviceChange(
                device,
                direction,
                description,
                () => _rastrAdapter.SetNodeSetVoltage(nodeNumber, target),
                state.ReactivePower);
        }

        /// <summary>
        /// Применяет изменение СКРМ и рассчитывает установившийся режим. Если режим
        /// не сошелся или изменение генератора не повлияло на его загрузку, изменение
        /// отменяется и СКРМ исключается из перебора для КП в этом часе.
        /// </summary>
        /// <param name="hourState">Состояние часа.</param>
        /// <param name="controlPoint">КП.</param>
        /// <param name="change">Изменение СКРМ.</param>
        /// <param name="hourEntry">Запись протокола о часе КП.</param>
        /// <param name="mainNodeNumber">Номер основного узла КП.</param>
        /// <param name="bounds">Границы графика.</param>
        /// <returns><c>true</c>, если изменение выполнено.</returns>
        private bool ApplyChange(
            HourState hourState,
            ControlPoint controlPoint,
            DeviceChange change,
            ProtocolEntry hourEntry,
            int mainNodeNumber,
            VoltageBounds bounds)
        {
            Device device = change.Device;
            change.Apply();
            _protocol.Add(ProtocolTag.Device, device.ToString(), change.Description, hourEntry);

            if (!_rastrAdapter.CalculateRegime())
            {
                _rastrAdapter.UndoLastChange();
                bool isRestored = _rastrAdapter.CalculateRegime();
                string details = isRestored
                    ? device.ToString()
                    : device.ToString() + "; повторный расчет исходного режима не сошелся";
                _protocol.Add(ProtocolTag.Warning, "Режим не сошелся, изменение отменено", details, hourEntry);
                hourState.MarkExhausted(controlPoint.Id, device.Id);
                return false;
            }

            if (device.Type == DeviceType.Generator)
            {
                hourState.AddGeneratorStep(device.Id);
                GeneratorState? generator = _rastrAdapter.GetGenerator(device.Number);
                if (generator != null
                    && change.ReactivePowerBefore.HasValue
                    && Math.Abs(generator.ReactivePower - change.ReactivePowerBefore.Value) < ReactivePowerTolerance)
                {
                    _rastrAdapter.UndoLastChange();
                    _rastrAdapter.CalculateRegime();
                    _protocol.Add(
                        ProtocolTag.Warning,
                        "Загрузка генератора не изменилась, изменение отменено",
                        device.ToString(),
                        hourEntry);
                    hourState.MarkExhausted(controlPoint.Id, device.Id);
                    return false;
                }
            }

            hourState.RegisterChange(device.Id, change.Direction);
            double? voltage = _rastrAdapter.GetNodeVoltage(mainNodeNumber);
            if (voltage.HasValue)
            {
                _protocol.Add(
                    ProtocolTag.Regime,
                    "Расчет УР: U = " + ValueFormat.Voltage(voltage.Value) + " кВ",
                    DescribePosition(voltage.Value, bounds),
                    hourEntry);
            }
            else
            {
                _protocol.Add(ProtocolTag.Regime, "Расчет УР: нет напряжения в основном узле КП", null, hourEntry);
            }

            return true;
        }

        /// <summary>
        /// Проверяет напряжение во всех КП после ввода режима в часе и регистрирует
        /// КП, в которых напряжение не введено в допустимую область.
        /// </summary>
        /// <param name="run">Данные расчета.</param>
        /// <param name="hour">Час суток (1–24).</param>
        private void CheckControlPoints(RegulationRun run, int hour)
        {
            foreach (ControlPoint controlPoint in run.ControlPoints)
            {
                VoltageBounds? bounds = run.Session.Schedule.GetBounds(controlPoint.Id, hour);
                ControlPointNode? mainNode = controlPoint.MainNode;
                if (bounds == null || mainNode == null)
                {
                    continue;
                }

                double? voltage = _rastrAdapter.GetNodeVoltage(mainNode.Number);
                if (!voltage.HasValue || bounds.Contains(voltage.Value))
                {
                    continue;
                }

                ProtocolEntry hourEntry = run.GetHourEntry(controlPoint, hour, voltage.Value, bounds);
                _protocol.Add(
                    ProtocolTag.Warning,
                    "Не введено",
                    RegulationRun.DescribeVoltage(voltage.Value, bounds),
                    hourEntry);
                run.Unresolved.Add(new UnresolvedViolation(controlPoint, hour, voltage.Value, bounds));
            }
        }

        /// <summary>
        /// Записывает в протокол итог расчета.
        /// </summary>
        /// <param name="run">Данные расчета.</param>
        /// <param name="result">Расчет, помещенный в архив.</param>
        private void WriteSummary(RegulationRun run, CalculationResult result)
        {
            string details = "КП в расчете: " + run.ControlPoints.Count
                + "; часов с выходом за границы графика: " + run.ViolationCount
                + "; не введено: " + run.Unresolved.Count;
            if (run.Unresolved.Count == 0)
            {
                _protocol.Add(
                    ProtocolTag.Info,
                    "Расчет завершен успешно",
                    details + "; напряжение во всех КП в допустимой области");
            }
            else
            {
                _protocol.Add(ProtocolTag.Info, "Расчет завершен", details);
                string list = string.Join(
                    "; ",
                    run.Unresolved
                        .GroupBy(item => item.ControlPoint.Name)
                        .Select(group => group.Key + ": " + FormatHours(group.Select(item => item.Hour))));
                _protocol.Add(ProtocolTag.Warning, "Напряжение не введено в допустимую область", list);
            }

            _protocol.Add(
                ProtocolTag.Info,
                "Расчет №" + result.Number + " сохранен в архив",
                result.StartedAt.ToString("dd.MM.yyyy HH:mm:ss"));
        }

        /// <summary>
        /// Форматирует перечень часов.
        /// </summary>
        /// <param name="hours">Часы суток.</param>
        /// <returns>Строка вида «час 7» или «часы 1, 2, 5».</returns>
        private static string FormatHours(IEnumerable<int> hours)
        {
            List<int> list = hours
                .Distinct()
                .OrderBy(hour => hour)
                .ToList();
            return (list.Count == 1 ? "час " : "часы ") + string.Join(", ", list);
        }
    }
}
