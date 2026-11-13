using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using NUnit.Framework;

using VrdoPdg.Infrastructure;
using VrdoPdg.Model;
using VrdoPdg.Model.Entities;
using VrdoPdg.Model.Protocol;
using VrdoPdg.Model.Regime;
using VrdoPdg.Model.Services;
using VrdoPdg.Tests.Fakes;

namespace VrdoPdg.Tests
{
    /// <summary>
    /// Тесты алгоритма ввода режима по напряжению в допустимую область (рисунки В.1, В.2).
    /// </summary>
    [TestFixture]
    public sealed class VoltageRegulationServiceTests
    {
        /// <summary>
        /// Допустимая погрешность сравнения значений.
        /// </summary>
        private const double Tolerance = 1e-6;

        /// <summary>
        /// Напряжение выше верхней границы: управляемое СКРМ разгружается до Qmin.
        /// </summary>
        [Test]
        public void Run_VoltageAboveUpperBound_UnloadsControlledDeviceToQmin()
        {
            RegulationScenario scenario = new RegulationScenario();
            scenario.SetVoltage(1, 530);

            CalculationResult result = scenario.Run();

            DeviceMode mode = result.After.GetMode(RegulationScenario.ControlledId, 1)!;
            Assert.That(mode.MinReactivePower, Is.EqualTo(-100).Within(Tolerance));
            Assert.That(mode.MaxReactivePower, Is.EqualTo(-100).Within(Tolerance));
            Assert.That(result.After.GetVoltage(RegulationScenario.MainNode, 1), Is.EqualTo(525).Within(Tolerance));
            Assert.That(result.After.GetMode(RegulationScenario.ReactorId, 1)!.IsOn, Is.False);
            Assert.That(result.UnresolvedViolations, Is.Empty);
        }

        /// <summary>
        /// Резерва управляемых СКРМ недостаточно: включается реактор.
        /// </summary>
        [Test]
        public void Run_ControlledReserveNotEnough_SwitchesReactorOn()
        {
            RegulationScenario scenario = new RegulationScenario();
            scenario.SetVoltage(1, 535);

            CalculationResult result = scenario.Run();

            Assert.That(result.After.GetMode(RegulationScenario.ReactorId, 1)!.IsOn, Is.True);
            Assert.That(result.After.GetMode(RegulationScenario.CapacitorId, 1)!.IsOn, Is.True);
            Assert.That(result.After.GetVoltage(RegulationScenario.MainNode, 1), Is.EqualTo(524).Within(Tolerance));
            Assert.That(result.UnresolvedViolations, Is.Empty);
        }

        /// <summary>
        /// Состояние реактора задано в файле smp_smzu.csv: реактор не изменяется,
        /// отключается БСК и снижается заданное напряжение генератора.
        /// </summary>
        [Test]
        public void Run_ReactorFixedInSmpSmzu_SkipsReactorAndUsesNextDevices()
        {
            RegulationScenario scenario = new RegulationScenario();
            scenario.SetVoltage(1, 535);
            scenario.FixedDeviceHours.Add((RegulationScenario.ReactorId, 1));

            CalculationResult result = scenario.Run();

            Assert.That(result.After.GetMode(RegulationScenario.ReactorId, 1)!.IsOn, Is.False);
            Assert.That(result.After.GetMode(RegulationScenario.CapacitorId, 1)!.IsOn, Is.False);
            Assert.That(result.After.GetMode(RegulationScenario.GeneratorId, 1)!.SetVoltage,
                Is.EqualTo(RegulationScenario.GeneratorSetVoltage - 2.1).Within(Tolerance));
            Assert.That(result.After.GetVoltage(RegulationScenario.MainNode, 1), Is.EqualTo(524.95).Within(Tolerance));
            Assert.That(result.UnresolvedViolations, Is.Empty);
        }

        /// <summary>
        /// Заданное напряжение генератора снижается шагами по 0,3 кВ до ввода режима.
        /// </summary>
        [Test]
        public void Run_OnlyGeneratorAvailable_DecreasesSetVoltageByStep()
        {
            RegulationScenario scenario = new RegulationScenario();
            scenario.SetVoltage(1, 526.5);
            scenario.Adapter.ControlledDevices[RegulationScenario.ControlledNode].IsOn = false;
            scenario.Adapter.SwitchedDevices[RegulationScenario.ReactorNumber] =
                new FakeSwitchedDevice(true, -6, RegulationScenario.MainNode);
            scenario.Adapter.SwitchedDevices[RegulationScenario.CapacitorNumber] =
                new FakeSwitchedDevice(false, 4, RegulationScenario.MainNode);

            CalculationResult result = scenario.Run();

            Assert.That(result.After.GetMode(RegulationScenario.GeneratorId, 1)!.SetVoltage,
                Is.EqualTo(RegulationScenario.GeneratorSetVoltage - 3.0).Within(Tolerance));
            Assert.That(result.After.GetVoltage(RegulationScenario.MainNode, 1), Is.EqualTo(525).Within(Tolerance));
        }

        /// <summary>
        /// Генератор изменяется шагами по 0,3 кВ, пока его загрузка не станет равной пределу;
        /// шаг, выводящий генератор на предел, — последнее действие с ним, после чего
        /// изменяется следующий генератор таблицы эффективности.
        /// </summary>
        [Test]
        public void Run_GeneratorReachesLimit_ContinuesWithNextGenerator()
        {
            const int secondId = 42;
            const int secondNumber = 14;
            const int secondNode = 6001;
            RegulationScenario scenario = new RegulationScenario();
            scenario.SetVoltage(1, 526.4);
            scenario.Adapter.ControlledDevices[RegulationScenario.ControlledNode].IsOn = false;
            scenario.Adapter.SwitchedDevices[RegulationScenario.ReactorNumber] =
                new FakeSwitchedDevice(true, -6, RegulationScenario.MainNode);
            scenario.Adapter.SwitchedDevices[RegulationScenario.CapacitorNumber] =
                new FakeSwitchedDevice(false, 4, RegulationScenario.MainNode);
            scenario.Adapter.Generators[RegulationScenario.GeneratorNumber] = new FakeGenerator(
                RegulationScenario.GeneratorNode, RegulationScenario.GeneratorSetVoltage, 4, -2, 100, 10, 0.05,
                RegulationScenario.MainNode);
            scenario.Adapter.Generators[secondNumber] = new FakeGenerator(
                secondNode, RegulationScenario.GeneratorSetVoltage, 50, -50, 100, 10, 0.05,
                RegulationScenario.MainNode);
            scenario.Repository.Devices.Add(
                new Device(secondId, DeviceType.Generator, "Барнаульская ТЭЦ-2, ТГ-6", secondNumber, null));
            scenario.Repository.EfficiencyEntries.Add(
                new EfficiencyEntry(RegulationScenario.ControlPointId, secondId, 5, 5));

            CalculationResult result = scenario.Run();

            DeviceMode firstMode = result.After.GetMode(RegulationScenario.GeneratorId, 1)!;
            Assert.That(firstMode.ReactivePower, Is.EqualTo(-2).Within(Tolerance));
            Assert.That(
                firstMode.SetVoltage,
                Is.EqualTo(RegulationScenario.GeneratorSetVoltage - 0.6).Within(Tolerance));
            Assert.That(result.After.GetMode(secondId, 1)!.SetVoltage,
                Is.EqualTo(RegulationScenario.GeneratorSetVoltage - 2.4).Within(Tolerance));
            Assert.That(result.After.GetVoltage(RegulationScenario.MainNode, 1), Is.EqualTo(524.9).Within(Tolerance));
            Assert.That(result.UnresolvedViolations, Is.Empty);
        }

        /// <summary>
        /// Значения наблюдаемых объектов форм просмотра считываются после ввода режима:
        /// включенный реактор и напряжение основного узла КП.
        /// </summary>
        [Test]
        public void Run_ObservedObjects_ReadAfterRegulation()
        {
            RegulationScenario scenario = new RegulationScenario();
            scenario.SetVoltage(1, 533);

            CalculationResult result = scenario.Run();

            Assert.That(result.After.GetMode(RegulationScenario.ReactorId, 1)!.IsOn, Is.True);
            Assert.That(result.After.GetObservedReactor(RegulationScenario.ReactorNumber, 1)!.IsOn, Is.True);
            Assert.That(result.Before.GetObservedReactor(RegulationScenario.ReactorNumber, 1)!.IsOn, Is.False);
            Assert.That(
                result.After.GetObservedNode(RegulationScenario.MainNode, 1)!.Voltage,
                Is.EqualTo(result.After.GetVoltage(RegulationScenario.MainNode, 1)!.Value).Within(Tolerance));
            Assert.That(
                result.After.GetObservedNode(RegulationScenario.MainNode, 2)!.Voltage,
                Is.EqualTo(result.Before.GetObservedNode(RegulationScenario.MainNode, 2)!.Voltage).Within(Tolerance));
        }

        /// <summary>
        /// Напряжение ниже нижней границы: управляемое СКРМ загружается до Qmax,
        /// включенный реактор отключается.
        /// </summary>
        [Test]
        public void Run_VoltageBelowLowerBound_LoadsControlledDeviceAndSwitchesReactorOff()
        {
            RegulationScenario scenario = new RegulationScenario();
            scenario.Adapter.SwitchedDevices[RegulationScenario.ReactorNumber] =
                new FakeSwitchedDevice(true, -6, RegulationScenario.MainNode);
            scenario.SetVoltage(2, 490);

            CalculationResult result = scenario.Run();

            DeviceMode controlled = result.After.GetMode(RegulationScenario.ControlledId, 2)!;
            Assert.That(controlled.MaxReactivePower, Is.EqualTo(100).Within(Tolerance));
            Assert.That(controlled.MinReactivePower, Is.EqualTo(100).Within(Tolerance));
            Assert.That(result.After.GetMode(RegulationScenario.ReactorId, 2)!.IsOn, Is.False);
            Assert.That(result.After.GetVoltage(RegulationScenario.MainNode, 2), Is.EqualTo(501).Within(Tolerance));
        }

        /// <summary>
        /// Напряжение ниже нижней границы: отключенная БСК включается.
        /// </summary>
        [Test]
        public void Run_VoltageBelowLowerBound_SwitchesCapacitorBankOn()
        {
            RegulationScenario scenario = new RegulationScenario();
            scenario.Adapter.SwitchedDevices[RegulationScenario.CapacitorNumber] =
                new FakeSwitchedDevice(false, 4, RegulationScenario.MainNode);
            scenario.SetVoltage(3, 492);

            CalculationResult result = scenario.Run();

            Assert.That(result.After.GetMode(RegulationScenario.CapacitorId, 3)!.IsOn, Is.True);
            Assert.That(result.After.GetVoltage(RegulationScenario.MainNode, 3), Is.EqualTo(501).Within(Tolerance));
        }

        /// <summary>
        /// Режим не сошелся после включения реактора: изменение отменяется,
        /// используется следующее СКРМ, в протокол записывается предупреждение.
        /// </summary>
        [Test]
        public void Run_RegimeDiverges_UndoesChangeAndUsesNextDevice()
        {
            RegulationScenario scenario = new RegulationScenario();
            scenario.SetVoltage(1, 535);
            FakeSwitchedDevice reactor = scenario.Adapter.SwitchedDevices[RegulationScenario.ReactorNumber];
            scenario.Adapter.DivergenceCondition = () => reactor.IsOn;

            CalculationResult result = scenario.Run();

            Assert.That(result.After.GetMode(RegulationScenario.ReactorId, 1)!.IsOn, Is.False);
            Assert.That(result.After.GetMode(RegulationScenario.CapacitorId, 1)!.IsOn, Is.False);
            Assert.That(scenario.Protocol.GetEntries().Any(entry => entry.Tag == ProtocolTag.Warning
                && entry.Message == "Режим не сошелся, изменение отменено"), Is.True);
            Assert.That(result.UnresolvedViolations, Is.Empty);
        }

        /// <summary>
        /// Резервы реактивной мощности исчерпаны: КП и час попадают в перечень невведенных.
        /// </summary>
        [Test]
        public void Run_ReservesExhausted_RegistersUnresolvedViolation()
        {
            RegulationScenario scenario = new RegulationScenario();
            scenario.SetVoltage(5, 560);

            CalculationResult result = scenario.Run();

            Assert.That(result.UnresolvedViolations, Has.Count.EqualTo(1));
            Assert.That(result.UnresolvedViolations[0].Hour, Is.EqualTo(5));
            Assert.That(result.UnresolvedViolations[0].ControlPoint.Id, Is.EqualTo(RegulationScenario.ControlPointId));
            Assert.That(scenario.Protocol.GetEntries().Any(entry => entry.Message == "Не введено"), Is.True);
            Assert.That(scenario.Protocol.GetEntries().Any(entry => entry.Tag == ProtocolTag.Warning
                && entry.Message == "Напряжение не введено в допустимую область"
                && entry.Details.Contains("Барнаульская: час 5")), Is.True);
        }

        /// <summary>
        /// Изменения одного часа не переносятся на следующий час.
        /// </summary>
        [Test]
        public void Run_ChangesOfHour_DoNotAffectNextHour()
        {
            RegulationScenario scenario = new RegulationScenario();
            scenario.SetVoltage(1, 535);
            scenario.SetVoltage(3, 535);

            CalculationResult result = scenario.Run();

            DeviceMode controlled = result.After.GetMode(RegulationScenario.ControlledId, 2)!;
            Assert.That(controlled.MinReactivePower, Is.EqualTo(-100).Within(Tolerance));
            Assert.That(controlled.MaxReactivePower, Is.EqualTo(100).Within(Tolerance));
            Assert.That(result.After.GetMode(RegulationScenario.ReactorId, 2)!.IsOn, Is.False);
            Assert.That(result.After.GetVoltage(RegulationScenario.MainNode, 2), Is.EqualTo(515).Within(Tolerance));
            Assert.That(result.After.GetMode(RegulationScenario.ReactorId, 3)!.IsOn, Is.True);
        }

        /// <summary>
        /// Напряжение во всех часах в допустимой области: изменений нет, групп ВРДО в протоколе нет.
        /// </summary>
        [Test]
        public void Run_VoltageWithinBounds_MakesNoChanges()
        {
            RegulationScenario scenario = new RegulationScenario();

            CalculationResult result = scenario.Run();

            Assert.That(scenario.Protocol.GetCount(ProtocolTag.Vrdo), Is.Zero);
            Assert.That(scenario.Protocol.GetCount(ProtocolTag.Device), Is.Zero);
            for (int hour = 1; hour <= 24; hour++)
            {
                DeviceMode before = result.Before.GetMode(RegulationScenario.ControlledId, hour)!;
                DeviceMode after = result.After.GetMode(RegulationScenario.ControlledId, hour)!;
                Assert.That(after.MinReactivePower, Is.EqualTo(before.MinReactivePower));
                Assert.That(after.MaxReactivePower, Is.EqualTo(before.MaxReactivePower));
            }
        }

        /// <summary>
        /// Каждый расчет начинается с открытия исходной МТ и помещается в архив с номером.
        /// </summary>
        [Test]
        public void Run_TwoCalculations_ReopenMegapointAndNumberResults()
        {
            RegulationScenario scenario = new RegulationScenario();
            SessionData session = scenario.CreateSession();

            CalculationResult first = scenario.Service.Run(session, null, CancellationToken.None);
            CalculationResult second = scenario.Service.Run(session, null, CancellationToken.None);

            Assert.That(scenario.Adapter.OpenCount, Is.EqualTo(2));
            Assert.That(first.Number, Is.EqualTo(1));
            Assert.That(second.Number, Is.EqualTo(2));
            Assert.That(scenario.Archive.GetResults().Last(), Is.SameAs(second));
        }

        /// <summary>
        /// Остановленный расчет не помещается в архив.
        /// </summary>
        [Test]
        public void Run_Cancelled_ThrowsAndDoesNotArchive()
        {
            RegulationScenario scenario = new RegulationScenario();
            scenario.SetVoltage(1, 535);
            SessionData session = scenario.CreateSession();
            using (CancellationTokenSource cancellation = new CancellationTokenSource())
            {
                cancellation.Cancel();

                Assert.Throws<OperationCanceledException>(
                    () => scenario.Service.Run(session, null, cancellation.Token));
            }

            Assert.That(scenario.Archive.GetResults(), Is.Empty);
            Assert.That(scenario.Protocol.GetEntries().Last().Message, Is.EqualTo("Расчет остановлен"));
        }

        /// <summary>
        /// Изменение заданного напряжения генератора без изменения его загрузки отменяется.
        /// </summary>
        [Test]
        public void Run_GeneratorWithoutEffect_UndoesSetVoltageChange()
        {
            RegulationScenario scenario = new RegulationScenario();
            scenario.SetVoltage(1, 545);
            scenario.Adapter.Generators[RegulationScenario.GeneratorNumber] = new FakeGenerator(
                RegulationScenario.GeneratorNode, RegulationScenario.GeneratorSetVoltage, 50, -50, 100, 0, 0.05,
                RegulationScenario.MainNode);

            CalculationResult result = scenario.Run();

            Assert.That(result.After.GetMode(RegulationScenario.GeneratorId, 1)!.SetVoltage,
                Is.EqualTo(RegulationScenario.GeneratorSetVoltage).Within(Tolerance));
            Assert.That(scenario.Protocol.GetEntries().Any(entry =>
                entry.Message == "Загрузка генератора не изменилась, изменение отменено"), Is.True);
            Assert.That(result.UnresolvedViolations, Has.Count.EqualTo(1));
        }

        /// <summary>
        /// Протокол содержит иерархию «КП → час → действия с СКРМ и результаты расчета УР».
        /// </summary>
        [Test]
        public void Run_VoltageAboveUpperBound_WritesProtocolHierarchy()
        {
            RegulationScenario scenario = new RegulationScenario();
            scenario.SetVoltage(1, 535);

            scenario.Run();

            ProtocolEntry controlPoint = scenario.Protocol.GetEntries().Single(entry => entry.Tag == ProtocolTag.Vrdo);
            ProtocolEntry hour = scenario.Protocol.GetEntries().Single(entry => entry.Tag == ProtocolTag.Hour);
            Assert.That(controlPoint.Message, Is.EqualTo("Барнаульская · узел 564"));
            Assert.That(hour.Parent, Is.SameAs(controlPoint));
            Assert.That(hour.Message, Is.EqualTo("Час 1"));
            Assert.That(scenario.Protocol.GetEntries()
                .Where(entry => entry.Tag == ProtocolTag.Device || entry.Tag == ProtocolTag.Regime)
                .All(entry => ReferenceEquals(entry.Parent, hour)), Is.True);
            Assert.That(scenario.Protocol.GetCount(ProtocolTag.Device), Is.EqualTo(2));
            Assert.That(scenario.Protocol.GetCount(ProtocolTag.Regime), Is.EqualTo(2));
        }

        /// <summary>
        /// КП без основного узла или без графика напряжения не участвуют в расчете,
        /// КП без таблицы эффективности попадает в перечень невведенных.
        /// </summary>
        [Test]
        public void Run_IncompleteControlPoints_SkipsOrRegistersThem()
        {
            RegulationScenario scenario = new RegulationScenario();
            scenario.Repository.ControlPoints.Add(new ControlPoint(
                2, 1, "Рубцовская", new[] { new ControlPointNode(RegulationScenario.ExtraNode, true) }));
            scenario.Repository.ControlPoints.Add(new ControlPoint(
                3, 1, "Алтай", new[] { new ControlPointNode(RegulationScenario.ExtraNode, false) }));
            scenario.Repository.ControlPoints.Add(new ControlPoint(
                4, 1, "Власиха", new[] { new ControlPointNode(RegulationScenario.ExtraNode, true) }));
            scenario.ControlPointsWithoutSchedule.Add(4);
            scenario.Adapter.BaseVoltages[RegulationScenario.ExtraNode][1] = 530;

            CalculationResult result = scenario.Run();

            string[] skipped = scenario.Protocol.GetEntries()
                .Where(entry => entry.Message == "КП не участвует в расчете")
                .Select(entry => entry.Details)
                .ToArray();
            Assert.That(
                skipped,
                Is.EqualTo(new[] { "Алтай: не задан основной узел", "Власиха: не задан график напряжения" }));
            UnresolvedViolation violation = result.UnresolvedViolations.Single();
            Assert.That(violation.ControlPoint.Name, Is.EqualTo("Рубцовская"));
            Assert.That(violation.Voltage, Is.EqualTo(530));
            Assert.That(violation.Bounds.UpperBound, Is.EqualTo(525));
        }

        /// <summary>
        /// Час, режим которого при выборе не сошелся, не обрабатывается: значения часа
        /// после расчета совпадают с исходными; ход расчета сообщается до 100 %.
        /// </summary>
        [Test]
        public void Run_HourDiverges_KeepsInitialRegimeAndReportsProgress()
        {
            RegulationScenario scenario = new RegulationScenario();
            scenario.SetVoltage(1, 535);
            scenario.SetVoltage(3, 535);
            SessionData session = scenario.CreateSession();
            scenario.Adapter.DivergentHours.Add(3);
            ProgressRecorder progress = new ProgressRecorder();

            CalculationResult result = scenario.Service.Run(session, progress, CancellationToken.None);

            Assert.That(result.After.GetVoltage(RegulationScenario.MainNode, 3), Is.EqualTo(535));
            Assert.That(
                result.After.GetMode(RegulationScenario.ControlledId, 3)!.MinReactivePower,
                Is.EqualTo(session.InitialRegime.GetMode(RegulationScenario.ControlledId, 3)!.MinReactivePower));
            Assert.That(scenario.Protocol.GetEntries().Any(entry => entry.Tag == ProtocolTag.Warning
                && entry.Message == "Час 3: расчет режима МТ не сошелся"), Is.True);
            Assert.That(progress.Values, Does.Contain(12));
            Assert.That(progress.Values.Last(), Is.EqualTo(100));
            Assert.That(result.FinishedAt, Is.GreaterThanOrEqualTo(result.StartedAt));
        }

        /// <summary>
        /// Ошибки БД и ПО «Барс» прерывают расчет и записываются в протокол.
        /// </summary>
        [Test]
        public void Run_DatabaseOrBarsFails_ThrowsSystemErrors()
        {
            RegulationScenario scenario = new RegulationScenario();
            SessionData session = scenario.CreateSession();

            scenario.Repository.FailingMethods.Add("GetEfficiencyEntries");
            VrdoException databaseError = Assert.Throws<VrdoException>(
                () => scenario.Service.Run(session, null, CancellationToken.None))!;
            scenario.Repository.FailingMethods.Clear();
            scenario.Adapter.OpenException = new InvalidOperationException("Нет лицензии");
            VrdoException openError = Assert.Throws<VrdoException>(
                () => scenario.Service.Run(session, null, CancellationToken.None))!;
            scenario.Adapter.OpenException = null;
            scenario.Adapter.SelectHourException = new InvalidOperationException("Сбой COM");
            VrdoException hourError = Assert.Throws<VrdoException>(
                () => scenario.Service.Run(session, null, CancellationToken.None))!;

            Assert.That(databaseError.Message, Is.EqualTo(DataLoadService.DatabaseErrorMessage));
            Assert.That(openError.Message, Is.EqualTo(DataLoadService.BarsErrorMessage));
            Assert.That(openError.Details, Is.EqualTo("Нет лицензии"));
            Assert.That(hourError.Message, Is.EqualTo(DataLoadService.BarsErrorMessage));
            Assert.That(hourError.Details, Is.EqualTo("Сбой COM"));
            Assert.That(scenario.Protocol.GetCount(ProtocolTag.Error), Is.EqualTo(3));
            Assert.That(scenario.Archive.GetResults(), Is.Empty);
        }

        /// <summary>
        /// СКРМ, не найденные в МТ, отключенные генераторы и генераторы без диапазона регулирования
        /// пропускаются; режим вводится оставшимися СКРМ.
        /// </summary>
        [Test]
        public void Run_UnavailableDevices_AreSkipped()
        {
            RegulationScenario scenario = new RegulationScenario();
            scenario.SetVoltage(1, 560);
            AddDevice(scenario, new Device(70, DeviceType.Controlled, "СТК-2", 7001, null), 5);
            AddDevice(scenario, new Device(71, DeviceType.Switched, "Р-2", 7002, SwitchedDeviceKind.Reactor), 6);
            AddDevice(scenario, new Device(72, DeviceType.Generator, "ТГ-7", 7003, null), 7);
            AddDevice(scenario, new Device(73, DeviceType.Generator, "ТГ-8", 7004, null), 8);
            AddDevice(scenario, new Device(74, DeviceType.Generator, "ТГ-9", 7005, null), 9);
            AddDevice(scenario, new Device(75, DeviceType.Generator, "ТГ-10", 7006, null), 10);
            scenario.Adapter.Generators[7004] = new FakeGenerator(6004, 236, 50, -50, 100, 10, 0.05)
            {
                IsOn = false
            };
            scenario.Adapter.Generators[7005] = new FakeGenerator(6005, 0, 50, -50, 100, 10, 0.05);
            scenario.Adapter.Generators[7006] = new FakeGenerator(6006, 236, 50, 50, 50, 10, 0.05);

            CalculationResult result = scenario.Run();

            Assert.That(result.After.GetMode(70, 1), Is.Null);
            Assert.That(result.After.GetMode(73, 1)!.SetVoltage, Is.EqualTo(236));
            Assert.That(result.After.GetMode(75, 1)!.SetVoltage, Is.EqualTo(236));
            Assert.That(result.After.GetMode(RegulationScenario.ReactorId, 1)!.IsOn, Is.True);
            Assert.That(result.UnresolvedViolations, Has.Count.EqualTo(1));
        }

        /// <summary>
        /// Напряжение ниже нижней границы: заданное напряжение генератора повышается шагами
        /// по 0,3 кВ, генератор на верхнем пределе не изменяется.
        /// </summary>
        [Test]
        public void Run_VoltageBelowLowerBound_IncreasesGeneratorSetVoltage()
        {
            RegulationScenario scenario = new RegulationScenario();
            scenario.SetVoltage(1, 499.6);
            scenario.SetVoltage(2, 499.6);
            scenario.Adapter.ControlledDevices[RegulationScenario.ControlledNode].IsOn = false;
            scenario.Adapter.Generators[14] = new FakeGenerator(6001, 236, 100, -50, 100, 10, 0.05);
            AddDevice(scenario, new Device(42, DeviceType.Generator, "ТГ-6", 14, null), 0);

            CalculationResult result = scenario.Run();

            Assert.That(result.After.GetMode(42, 1)!.SetVoltage, Is.EqualTo(236));
            Assert.That(
                result.After.GetMode(RegulationScenario.GeneratorId, 1)!.SetVoltage,
                Is.EqualTo(RegulationScenario.GeneratorSetVoltage + 0.9).Within(Tolerance));
            Assert.That(result.After.GetVoltage(RegulationScenario.MainNode, 1), Is.EqualTo(500.05).Within(Tolerance));
            Assert.That(result.UnresolvedViolations, Is.Empty);
        }

        /// <summary>
        /// Узел КП отключился после изменения СКРМ: в протокол записывается отсутствие напряжения,
        /// ввод режима КП в часе прекращается.
        /// </summary>
        [Test]
        public void Run_MainNodeLost_StopsRegulation()
        {
            RegulationScenario scenario = new RegulationScenario();
            scenario.SetVoltage(1, 535);
            FakeSwitchedDevice reactor = scenario.Adapter.SwitchedDevices[RegulationScenario.ReactorNumber];
            SessionData session = scenario.CreateSession();
            scenario.Adapter.NodeLossCondition = node => reactor.IsOn;

            CalculationResult result = scenario.Service.Run(session, null, CancellationToken.None);

            Assert.That(scenario.Protocol.GetEntries().Any(entry => entry.Tag == ProtocolTag.Regime
                && entry.Message == "Расчет УР: нет напряжения в основном узле КП"), Is.True);
            Assert.That(result.After.GetMode(RegulationScenario.CapacitorId, 1)!.IsOn, Is.True);
            Assert.That(result.UnresolvedViolations, Is.Empty);
        }

        /// <summary>
        /// Режим не сходится ни после изменения, ни после его отмены: в протокол записывается,
        /// что повторный расчет исходного режима не сошелся.
        /// </summary>
        [Test]
        public void Run_RestoredRegimeDiverges_WritesWarning()
        {
            RegulationScenario scenario = new RegulationScenario();
            scenario.SetVoltage(1, 535);
            scenario.SetVoltage(2, 535);
            scenario.Adapter.DivergenceCondition = () => true;

            CalculationResult result = scenario.Run();

            Assert.That(scenario.Protocol.GetEntries().Any(entry => entry.Tag == ProtocolTag.Warning
                && entry.Details.EndsWith("повторный расчет исходного режима не сошелся")), Is.True);
            Assert.That(scenario.Protocol.GetEntries().Any(
                entry => entry.Message == "Напряжение не введено в допустимую область"
                    && entry.Details == "Барнаульская: часы 1, 2"), Is.True);
            Assert.That(result.UnresolvedViolations, Has.Count.EqualTo(2));
        }

        /// <summary>
        /// Изменение СКРМ, выводящее за границы графика другой КП, который был в допустимой области,
        /// на первом этапе откладывается; режим вводится следующими СКРМ таблицы эффективности,
        /// и второй этап не нужен
        /// </summary>
        [Test]
        public void Run_ChangeViolatesOtherControlPoint_DefersChangeAndUsesNextDevice()
        {
            const int otherNode = 570;
            RegulationScenario scenario = new RegulationScenario();
            scenario.SetVoltage(1, 535);
            scenario.Adapter.SetBaseVoltage(otherNode, 502);
            scenario.Adapter.SwitchedDevices[RegulationScenario.ReactorNumber] =
                new FakeSwitchedDevice(false, -6, RegulationScenario.MainNode, otherNode);
            scenario.Repository.ControlPoints.Add(
                new ControlPoint(2, 1, "Рубцовская", new[] { new ControlPointNode(otherNode, true) }));

            CalculationResult result = scenario.Run();

            Assert.That(result.After.GetMode(RegulationScenario.ReactorId, 1)!.IsOn, Is.False);
            Assert.That(result.After.GetMode(RegulationScenario.CapacitorId, 1)!.IsOn, Is.False);
            Assert.That(result.After.GetVoltage(otherNode, 1), Is.EqualTo(502));
            Assert.That(result.After.GetVoltage(RegulationScenario.MainNode, 1), Is.EqualTo(525).Within(0.2));
            Assert.That(result.UnresolvedViolations, Is.Empty);
            ProtocolEntry warning = scenario.Protocol.GetEntries().Single(entry => entry.Message
                == "Изменение отложено: напряжение в другом КП вышло за границы графика");
            Assert.That(
                scenario.Protocol.GetEntries().Any(entry => entry.Message == "Час 1: второй этап ввода режима"),
                Is.False);
            Assert.That(
                warning.Details,
                Is.EqualTo("Р-1 ПС 500 кВ Барнаульская (реактор 9); "
                    + "Рубцовская: U = 496,00 кВ, ниже нижней границы 500,00 кВ"));
        }

        /// <summary>
        /// Изменение СКРМ, после которого в основном узле другого КП нет напряжения, откладывается
        /// и на втором этапе не принимается: наибольшее отклонение не уменьшается
        /// </summary>
        [Test]
        public void Run_ChangeDisconnectsOtherControlPoint_UndoesChange()
        {
            const int otherNode = 570;
            RegulationScenario scenario = new RegulationScenario();
            scenario.SetVoltage(1, 535);
            scenario.Adapter.SetBaseVoltage(otherNode, 510);
            scenario.Adapter.SwitchedDevices[RegulationScenario.CapacitorNumber] =
                new FakeSwitchedDevice(false, 4, RegulationScenario.MainNode);
            scenario.Adapter.Generators[RegulationScenario.GeneratorNumber].IsOn = false;
            FakeSwitchedDevice reactor = scenario.Adapter.SwitchedDevices[RegulationScenario.ReactorNumber];
            scenario.Repository.ControlPoints.Add(
                new ControlPoint(2, 1, "Рубцовская", new[] { new ControlPointNode(otherNode, true) }));
            SessionData session = scenario.CreateSession();
            scenario.Adapter.NodeLossCondition = node => node == otherNode && reactor.IsOn;

            CalculationResult result = scenario.Service.Run(session, null, CancellationToken.None);

            Assert.That(result.After.GetMode(RegulationScenario.ReactorId, 1)!.IsOn, Is.False);
            Assert.That(result.After.GetVoltage(otherNode, 1), Is.EqualTo(510));
            Assert.That(scenario.Protocol.GetEntries().Any(entry => entry.Details
                == "Р-1 ПС 500 кВ Барнаульская (реактор 9); Рубцовская: нет напряжения в основном узле КП"), Is.True);
            ProtocolEntry rejected = scenario.Protocol.GetEntries().Single(entry => entry.Message
                == "Изменение отменено: наибольшее отклонение напряжения в КП не уменьшилось");
            Assert.That(rejected.Details, Does.EndWith("наибольшее отклонение: 5,00 кВ → нет напряжения"));
            Assert.That(result.UnresolvedViolations.Single().ControlPoint.Name, Is.EqualTo("Барнаульская"));
        }

        /// <summary>
        /// Если отложенное изменение — единственный способ ввести режим, на втором этапе оно
        /// принимается: наибольшее отклонение среди КП уменьшается, хотя другой КП выходит
        /// за границы графика
        /// </summary>
        [Test]
        public void Run_SecondStage_AcceptsChangeReducingLargestDeviation()
        {
            const int otherNode = 570;
            RegulationScenario scenario = new RegulationScenario();
            scenario.SetVoltage(1, 544);
            scenario.Adapter.SetBaseVoltage(otherNode, 502);
            scenario.Adapter.SwitchedDevices[RegulationScenario.ReactorNumber] =
                new FakeSwitchedDevice(false, -6, RegulationScenario.MainNode, otherNode);
            scenario.Repository.ControlPoints.Add(
                new ControlPoint(2, 1, "Рубцовская", new[] { new ControlPointNode(otherNode, true) }));

            CalculationResult result = scenario.Run();

            Assert.That(result.After.GetMode(RegulationScenario.ReactorId, 1)!.IsOn, Is.True);
            Assert.That(result.After.GetVoltage(RegulationScenario.MainNode, 1), Is.EqualTo(524.5).Within(Tolerance));
            Assert.That(result.After.GetVoltage(otherNode, 1), Is.EqualTo(496).Within(Tolerance));
            UnresolvedViolation violation = result.UnresolvedViolations.Single();
            Assert.That(violation.ControlPoint.Name, Is.EqualTo("Рубцовская"));
            Assert.That(violation.Hour, Is.EqualTo(1));
            Assert.That(
                scenario.Protocol.GetEntries().Any(entry => entry.Message == "Час 1: второй этап ввода режима"),
                Is.True);
            ProtocolEntry accepted = scenario.Protocol.GetEntries().Single(entry => entry.Message
                == "Изменение принято с выходом за границы графика другого КП");
            Assert.That(accepted.Details, Is.EqualTo("Р-1 ПС 500 кВ Барнаульская (реактор 9); "
                + "Рубцовская: U = 496,00 кВ, ниже нижней границы 500,00 кВ; "
                + "наибольшее отклонение: 5,50 кВ → 4,00 кВ"));
        }

        /// <summary>
        /// На втором этапе отложенное изменение отменяется, если наибольшее отклонение среди КП
        /// не уменьшается: небольшое превышение в одном КП не исправляется ценой большего
        /// нарушения в другом
        /// </summary>
        [Test]
        public void Run_SecondStage_RejectsChangeIncreasingLargestDeviation()
        {
            const int otherNode = 570;
            RegulationScenario scenario = new RegulationScenario();
            scenario.SetVoltage(1, 532);
            scenario.Adapter.SetBaseVoltage(otherNode, 500.5);
            scenario.Adapter.SwitchedDevices[RegulationScenario.ReactorNumber] =
                new FakeSwitchedDevice(false, -6, RegulationScenario.MainNode, otherNode);
            scenario.Adapter.SwitchedDevices[RegulationScenario.CapacitorNumber] =
                new FakeSwitchedDevice(false, 4, RegulationScenario.MainNode);
            scenario.Adapter.Generators[RegulationScenario.GeneratorNumber].IsOn = false;
            scenario.Repository.ControlPoints.Add(
                new ControlPoint(2, 1, "Рубцовская", new[] { new ControlPointNode(otherNode, true) }));

            CalculationResult result = scenario.Run();

            Assert.That(result.After.GetMode(RegulationScenario.ReactorId, 1)!.IsOn, Is.False);
            Assert.That(result.After.GetVoltage(RegulationScenario.MainNode, 1), Is.EqualTo(527).Within(Tolerance));
            Assert.That(result.After.GetVoltage(otherNode, 1), Is.EqualTo(500.5).Within(Tolerance));
            UnresolvedViolation violation = result.UnresolvedViolations.Single();
            Assert.That(violation.ControlPoint.Name, Is.EqualTo("Барнаульская"));
            ProtocolEntry rejected = scenario.Protocol.GetEntries().Single(entry => entry.Message
                == "Изменение отменено: наибольшее отклонение напряжения в КП не уменьшилось");
            Assert.That(rejected.Details, Does.EndWith("наибольшее отклонение: 2,00 кВ → 5,50 кВ"));
        }

        /// <summary>
        /// КП, который до изменения уже был вне допустимой области, не защищается от изменения:
        /// его вводит повторный проход или он попадает в перечень невведенных
        /// </summary>
        [Test]
        public void Run_OtherControlPointAlreadyViolated_AcceptsChange()
        {
            const int otherNode = 570;
            RegulationScenario scenario = new RegulationScenario();
            scenario.SetVoltage(1, 535);
            scenario.Adapter.SetBaseVoltage(otherNode, 499);
            scenario.Adapter.SwitchedDevices[RegulationScenario.ReactorNumber] =
                new FakeSwitchedDevice(false, -6, RegulationScenario.MainNode, otherNode);
            scenario.Repository.ControlPoints.Add(
                new ControlPoint(2, 1, "Рубцовская", new[] { new ControlPointNode(otherNode, true) }));

            CalculationResult result = scenario.Run();

            Assert.That(result.After.GetMode(RegulationScenario.ReactorId, 1)!.IsOn, Is.True);
            UnresolvedViolation violation = result.UnresolvedViolations.Single(item => item.Hour == 1);
            Assert.That(violation.ControlPoint.Name, Is.EqualTo("Рубцовская"));
            Assert.That(violation.Voltage, Is.EqualTo(493).Within(Tolerance));
        }

        /// <summary>
        /// КП обрабатываются по убыванию отклонения напряжения от допустимой области:
        /// первым — КП с наибольшим отклонением
        /// </summary>
        [Test]
        public void Run_SeveralViolations_ProcessesLargestDeviationFirst()
        {
            const int otherNode = 570;
            RegulationScenario scenario = new RegulationScenario();
            scenario.SetVoltage(1, 527);
            scenario.SetVoltage(2, 540);
            scenario.Adapter.SetBaseVoltage(otherNode, 530);
            scenario.Repository.ControlPoints.Add(
                new ControlPoint(2, 1, "Рубцовская", new[] { new ControlPointNode(otherNode, true) }));

            scenario.Run();

            string[] order = scenario.Protocol.GetEntries()
                .Where(entry => entry.Tag == ProtocolTag.Hour)
                .Select(entry => entry.Parent!.Message.Split(' ')[0] + " " + entry.Message)
                .ToArray();
            Assert.That(order.Take(4), Is.EqualTo(new[]
            {
                "Рубцовская Час 1",
                "Барнаульская Час 1",
                "Барнаульская Час 2",
                "Рубцовская Час 2"
            }));
        }

        /// <summary>
        /// СКРМ, добавленное в БД после загрузки данных (например, из МТ через таблицу эффективности),
        /// участвует в расчете: справочник сеанса дополняется, исходный режим СКРМ считывается из МТ,
        /// а изменение СКРМ записывается в файл результатов
        /// </summary>
        [Test]
        public void Run_DeviceAddedAfterLoad_IsUsedAndExported()
        {
            const int reactorId = 50;
            const int reactorNumber = 30;
            RegulationScenario scenario = new RegulationScenario();
            scenario.SetVoltage(1, 535);
            SessionData session = scenario.CreateSession();
            scenario.Adapter.SwitchedDevices[reactorNumber] =
                new FakeSwitchedDevice(false, -6, RegulationScenario.MainNode);
            AddDevice(scenario, new Device(reactorId, DeviceType.Switched, "Р-3", reactorNumber,
                SwitchedDeviceKind.Reactor), 0);

            CalculationResult result = scenario.Service.Run(session, null, CancellationToken.None);

            Assert.That(session.Devices.Any(device => device.Id == reactorId), Is.True);
            Assert.That(session.InitialRegime.GetMode(reactorId, 1)!.IsOn, Is.False);
            Assert.That(result.After.GetMode(reactorId, 1)!.IsOn, Is.True);
            Assert.That(result.After.GetMode(RegulationScenario.ReactorId, 1)!.IsOn, Is.False);
            Assert.That(scenario.Protocol.GetEntries().Any(entry => entry.Message
                == "В расчет включены СКРМ, добавленные в БД после загрузки данных"
                && entry.Details == "Р-3 (реактор 30)"), Is.True);
            ResultExportService export = new ResultExportService(new SmpSmzuCsvWriter(), scenario.Protocol);
            Assert.That(
                export.CreateRecords(result).Select(record => SmpSmzuCsvWriter.FormatRecord(record, ',')),
                Does.Contain("20;1;1;30;0;0;1;0;0"));
        }

        /// <summary>
        /// Исходные данные сеанса и зависимости сервиса обязательны.
        /// </summary>
        [Test]
        public void Run_NullArguments_Throw()
        {
            RegulationScenario scenario = new RegulationScenario();
            FakeRastrAdapter adapter = scenario.Adapter;
            FakeModelDataRepository repository = scenario.Repository;
            CalculationArchiveService archive = scenario.Archive;
            CalculationProtocol protocol = scenario.Protocol;
            ObservedObjects observed = RegulationScenario.Observed;

            Assert.Throws<ArgumentNullException>(() => scenario.Service.Run(null!, null, CancellationToken.None));
            Assert.Throws<ArgumentNullException>(
                () => new VoltageRegulationService(null!, repository, archive, protocol, observed));
            Assert.Throws<ArgumentNullException>(
                () => new VoltageRegulationService(adapter, null!, archive, protocol, observed));
            Assert.Throws<ArgumentNullException>(
                () => new VoltageRegulationService(adapter, repository, null!, protocol, observed));
            Assert.Throws<ArgumentNullException>(
                () => new VoltageRegulationService(adapter, repository, archive, null!, observed));
            Assert.Throws<ArgumentNullException>(
                () => new VoltageRegulationService(adapter, repository, archive, protocol, null!));
        }

        /// <summary>
        /// Добавляет СКРМ в справочник и в таблицу эффективности КП.
        /// </summary>
        /// <param name="scenario">Тестовые исходные данные.</param>
        /// <param name="device">СКРМ.</param>
        /// <param name="priority">Приоритет СКРМ.</param>
        private static void AddDevice(RegulationScenario scenario, Device device, int priority)
        {
            scenario.Repository.Devices.Add(device);
            scenario.Repository.EfficiencyEntries.Add(
                new EfficiencyEntry(RegulationScenario.ControlPointId, device.Id, priority, priority));
        }

        /// <summary>
        /// Получатель хода расчета, запоминающий значения в порядке сообщения.
        /// </summary>
        private sealed class ProgressRecorder : IProgress<int>
        {
            /// <summary>
            /// Сообщенные значения хода расчета, %.
            /// </summary>
            public List<int> Values { get; } = new List<int>();

            /// <inheritdoc/>
            public void Report(int value)
            {
                Values.Add(value);
            }
        }
    }
}
