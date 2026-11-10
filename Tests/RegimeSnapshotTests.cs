using NUnit.Framework;

using VrdoPdg.Model.Entities;
using VrdoPdg.Model.Regime;

namespace VrdoPdg.Tests
{
    /// <summary>
    /// Тесты снимка режима по часам.
    /// </summary>
    [TestFixture]
    public sealed class RegimeSnapshotTests
    {
        /// <summary>
        /// Копируются все значения заданного часа, значения других часов не изменяются.
        /// </summary>
        [Test]
        public void CopyHour_Source_CopiesOnlyGivenHour()
        {
            RegimeSnapshot source = new RegimeSnapshot();
            for (int hour = 1; hour <= 2; hour++)
            {
                source.SetVoltage(new NodeVoltage(564, hour, 510 + hour));
                source.SetMode(new DeviceMode(1, hour, true, -50, -100, 100, null, null, false));
                source.SetObservedNode(new ObservedNodeState(564, hour, true, 510 + hour, 0, 0));
                source.SetObservedReactor(new ObservedReactorState(9, hour, true));
            }

            RegimeSnapshot target = new RegimeSnapshot();
            target.CopyHour(source, 2);

            Assert.That(target.GetVoltage(564, 2), Is.EqualTo(512));
            Assert.That(target.GetMode(1, 2)!.ReactivePower, Is.EqualTo(-50));
            Assert.That(target.GetObservedNode(564, 2)!.Voltage, Is.EqualTo(512));
            Assert.That(target.GetObservedReactor(9, 2)!.IsOn, Is.True);
            Assert.That(target.GetVoltage(564, 1), Is.Null);
            Assert.That(target.GetMode(1, 1), Is.Null);
            Assert.That(target.GetObservedNode(564, 1), Is.Null);
            Assert.That(target.GetObservedReactor(9, 1), Is.Null);
            Assert.That(target.GetObservedNode(564, 2)!.IsOn, Is.True);
            Assert.That(target.GetObservedNode(564, 2)!.SetVoltage, Is.Zero);
            Assert.That(target.Voltages, Has.Count.EqualTo(1));
            Assert.That(target.Modes, Has.Count.EqualTo(1));
        }
    }
}
