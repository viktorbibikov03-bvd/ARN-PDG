using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using VrdoPdg.Model.Entities;
using VrdoPdg.Model.Protocol;
using VrdoPdg.Model.Regime;
using VrdoPdg.Model.Services;
using VrdoPdg.Model.SmpSmzu;

namespace VrdoPdg.Tests.Fakes
{
    /// <summary>
    /// Типовые исходные данные для тестов ввода режима: КП «Барнаульская» с основным узлом 564,
    /// управляемым СКРМ, реактором, БСК и генератором в таблице эффективности.
    /// По умолчанию напряжение во всех часах 515 кВ, границы графика 500–525 кВ.
    /// </summary>
    internal sealed class RegulationScenario
    {
        /// <summary>
        /// Код КП.
        /// </summary>
        public const int ControlPointId = 1;

        /// <summary>
        /// Основной узел КП.
        /// </summary>
        public const int MainNode = 564;

        /// <summary>
        /// Дополнительный узел КП.
        /// </summary>
        public const int ExtraNode = 563;

        /// <summary>
        /// Код управляемого СКРМ.
        /// </summary>
        public const int ControlledId = 1;

        /// <summary>
        /// Узел управляемого СКРМ.
        /// </summary>
        public const int ControlledNode = 520;

        /// <summary>
        /// Код реактора.
        /// </summary>
        public const int ReactorId = 5;

        /// <summary>
        /// Номер реактора.
        /// </summary>
        public const int ReactorNumber = 9;

        /// <summary>
        /// Код БСК.
        /// </summary>
        public const int CapacitorId = 17;

        /// <summary>
        /// Номер БСК в таблице «Реакторы».
        /// </summary>
        public const int CapacitorNumber = 125;

        /// <summary>
        /// Код генератора.
        /// </summary>
        public const int GeneratorId = 41;

        /// <summary>
        /// Номер генератора.
        /// </summary>
        public const int GeneratorNumber = 13;

        /// <summary>
        /// Узел подключения генератора.
        /// </summary>
        public const int GeneratorNode = 6000;

        /// <summary>
        /// Исходное заданное напряжение генератора, кВ.
        /// </summary>
        public const double GeneratorSetVoltage = 236;

        /// <summary>
        /// Создает исходные данные.
        /// </summary>
        public RegulationScenario()
        {
            Adapter.SetBaseVoltage(MainNode, 515);
            Adapter.SetBaseVoltage(ExtraNode, 515);
            Adapter.ControlledDevices[ControlledNode] = new FakeControlledDevice(0, -100, 100, 0.05, MainNode);
            Adapter.SwitchedDevices[ReactorNumber] = new FakeSwitchedDevice(false, -6, MainNode);
            Adapter.SwitchedDevices[CapacitorNumber] = new FakeSwitchedDevice(true, 4, MainNode);
            Adapter.Generators[GeneratorNumber] = new FakeGenerator(
                GeneratorNode, GeneratorSetVoltage, 50, -50, 100, 10, 0.05, MainNode);

            Repository.PowerSystems.Add(new PowerSystem(1, "ОЭС Сибири"));
            Repository.ControlPoints.Add(new ControlPoint(
                ControlPointId,
                1,
                "Барнаульская",
                new[] { new ControlPointNode(ExtraNode, false), new ControlPointNode(MainNode, true) }));
            Repository.Devices.Add(new Device(ControlledId, DeviceType.Controlled, "ПС 220 кВ Светлая СШ 220 кВ",
                ControlledNode, null));
            Repository.Devices.Add(new Device(ReactorId, DeviceType.Switched, "Р-1 ПС 500 кВ Барнаульская",
                ReactorNumber, SwitchedDeviceKind.Reactor));
            Repository.Devices.Add(new Device(CapacitorId, DeviceType.Switched, "БСК-1-220 ПС 220 кВ Светлая",
                CapacitorNumber, SwitchedDeviceKind.CapacitorBank));
            Repository.Devices.Add(new Device(GeneratorId, DeviceType.Generator, "Барнаульская ТЭЦ-2, ТГ-5",
                GeneratorNumber, null));
            Repository.EfficiencyEntries.Add(new EfficiencyEntry(ControlPointId, ControlledId, 1, 1));
            Repository.EfficiencyEntries.Add(new EfficiencyEntry(ControlPointId, ReactorId, 2, 2));
            Repository.EfficiencyEntries.Add(new EfficiencyEntry(ControlPointId, CapacitorId, 3, 3));
            Repository.EfficiencyEntries.Add(new EfficiencyEntry(ControlPointId, GeneratorId, 4, 4));

            Service = new VoltageRegulationService(Adapter, Repository, Archive, Protocol);
        }

        /// <summary>
        /// Тестовая модель режима.
        /// </summary>
        public FakeRastrAdapter Adapter { get; } = new FakeRastrAdapter();

        /// <summary>
        /// Репозиторий БД.
        /// </summary>
        public FakeModelDataRepository Repository { get; } = new FakeModelDataRepository();

        /// <summary>
        /// Протокол расчета.
        /// </summary>
        public CalculationProtocol Protocol { get; } = new CalculationProtocol();

        /// <summary>
        /// Сеанс и архив расчетов.
        /// </summary>
        public CalculationArchiveService Archive { get; } = new CalculationArchiveService();

        /// <summary>
        /// Сервис ввода режима.
        /// </summary>
        public VoltageRegulationService Service { get; }

        /// <summary>
        /// Часы, в которых состояние СКРМ задано в файле smp_smzu.csv.
        /// </summary>
        public HashSet<(int DeviceId, int Hour)> FixedDeviceHours { get; } = new HashSet<(int DeviceId, int Hour)>();

        /// <summary>
        /// Устанавливает базовое напряжение основного узла КП в часе.
        /// </summary>
        /// <param name="hour">Час суток.</param>
        /// <param name="voltage">Напряжение, кВ.</param>
        public void SetVoltage(int hour, double voltage)
        {
            Adapter.BaseVoltages[MainNode][hour] = voltage;
        }

        /// <summary>
        /// Создает исходные данные сеанса, считывая исходный режим из тестовой модели.
        /// </summary>
        /// <returns>Исходные данные сеанса.</returns>
        public SessionData CreateSession()
        {
            Dictionary<int, PQDiagram> diagrams = new Dictionary<int, PQDiagram>();
            DeviceModeContext context = new DeviceModeContext(
                new HashSet<(int DeviceId, int Hour)>(FixedDeviceHours),
                diagrams);
            RegimeSnapshot initial = new RegimeSnapshot();
            for (int hour = 1; hour <= RegimeSnapshot.HourCount; hour++)
            {
                Adapter.SelectHour(hour);
                RegimeReader.ReadHour(Adapter, hour, Repository.ControlPoints, Repository.Devices, context, initial);
            }

            List<VoltageBounds> bounds = new List<VoltageBounds>();
            for (int hour = 1; hour <= RegimeSnapshot.HourCount; hour++)
            {
                bounds.Add(new VoltageBounds(ControlPointId, hour, 500, 525, DateTime.Now));
            }

            SmpSmzuDocument document = new SmpSmzuDocument(
                new[] { "20;1;24;185;0;0;1;0;0" },
                new SmpSmzuRecord[0],
                "\r\n",
                new UTF8Encoding(false),
                null);
            return new SessionData(
                @"D:\BarsMDP\Results\131026\smzu_mega_act.mptsmz",
                @"D:\BarsMDP\Results\131026\BarsCSV\smp_smzu.csv",
                document,
                Repository.PowerSystems,
                Repository.ControlPoints,
                Repository.Devices,
                new VoltageSchedule(bounds),
                initial,
                diagrams,
                new Dictionary<int, int> { { GeneratorId, GeneratorNode } },
                FixedDeviceHours,
                DateTime.Now);
        }

        /// <summary>
        /// Выполняет расчет.
        /// </summary>
        /// <returns>Результат расчета.</returns>
        public CalculationResult Run()
        {
            return Service.Run(CreateSession(), null, CancellationToken.None);
        }
    }
}
