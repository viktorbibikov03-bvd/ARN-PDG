using System;
using System.Linq;
using System.Threading;

using NUnit.Framework;

using VrdoPdg.Model.Entities;
using VrdoPdg.Model.Protocol;
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
            Assert.That(scenario.Archive.GetLastResult(session), Is.SameAs(second));
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
    }
}
