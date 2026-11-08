using NUnit.Framework;

using VrdoPdg.Model.Entities;

namespace VrdoPdg.Tests
{
    /// <summary>
    /// Тесты определения пределов реактивной мощности по PQ-диаграмме.
    /// </summary>
    [TestFixture]
    public sealed class PQDiagramTests
    {
        /// <summary>
        /// PQ-диаграмма из трех точек.
        /// </summary>
        private static readonly PQDiagram _diagram = new PQDiagram(new[]
        {
            new PQPoint(200, -60, 120),
            new PQPoint(0, -100, 150),
            new PQPoint(100, -80, 140)
        });

        /// <summary>
        /// Пределы между точками определяются линейной интерполяцией,
        /// за пределами диаграммы — по крайним точкам.
        /// </summary>
        /// <param name="activePower">Активная мощность, МВт.</param>
        /// <param name="min">Ожидаемый Qmin, Мвар.</param>
        /// <param name="max">Ожидаемый Qmax, Мвар.</param>
        [TestCase(0, -100, 150)]
        [TestCase(50, -90, 145)]
        [TestCase(150, -70, 130)]
        [TestCase(-10, -100, 150)]
        [TestCase(250, -60, 120)]
        public void GetLimits_ActivePower_ReturnsInterpolatedLimits(double activePower, double min, double max)
        {
            ReactivePowerLimits limits = _diagram.GetLimits(activePower);

            Assert.That(limits.Min, Is.EqualTo(min).Within(1e-9));
            Assert.That(limits.Max, Is.EqualTo(max).Within(1e-9));
        }

        /// <summary>
        /// Пустая PQ-диаграмма дает нулевые пределы.
        /// </summary>
        [Test]
        public void GetLimits_EmptyDiagram_ReturnsZeroRange()
        {
            Assert.That(PQDiagram.Empty.IsEmpty, Is.True);
            Assert.That(PQDiagram.Empty.GetLimits(100).Range, Is.Zero);
        }
    }
}
