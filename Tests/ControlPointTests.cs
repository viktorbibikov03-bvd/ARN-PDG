using NUnit.Framework;

using VrdoPdg.Model.Entities;

namespace VrdoPdg.Tests
{
    /// <summary>
    /// Тесты КП и энергосистемы.
    /// </summary>
    [TestFixture]
    public sealed class ControlPointTests
    {
        /// <summary>
        /// Узлы КП упорядочиваются по номеру, основной узел определяется признаком узла,
        /// пустые наименования заменяются пустой строкой.
        /// </summary>
        [Test]
        public void Constructor_Nodes_SetsMainNodeAndPowerSystem()
        {
            ControlPoint controlPoint = new ControlPoint(
                1,
                2,
                null!,
                new[] { new ControlPointNode(564, true), new ControlPointNode(563, false) });
            PowerSystem powerSystem = new PowerSystem(2, null!);

            Assert.That(controlPoint.Name, Is.Empty);
            Assert.That(controlPoint.PowerSystemId, Is.EqualTo(2));
            Assert.That(controlPoint.Nodes[0].Number, Is.EqualTo(563));
            Assert.That(controlPoint.MainNode!.Number, Is.EqualTo(564));
            Assert.That(powerSystem.Id, Is.EqualTo(2));
            Assert.That(powerSystem.Name, Is.Empty);
        }
    }
}
