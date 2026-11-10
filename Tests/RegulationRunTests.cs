using System;

using NUnit.Framework;

using VrdoPdg.Model.Entities;
using VrdoPdg.Model.Services;

namespace VrdoPdg.Tests
{
    /// <summary>
    /// Тесты данных расчета ввода режима.
    /// </summary>
    [TestFixture]
    public sealed class RegulationRunTests
    {
        /// <summary>
        /// Положение напряжения описывается относительно границ графика.
        /// </summary>
        [Test]
        public void DescribeVoltage_Voltages_DescribesPosition()
        {
            VoltageBounds bounds = new VoltageBounds(1, 12, 500, 525, DateTime.Today);

            Assert.That(
                RegulationRun.DescribeVoltage(531.2, bounds),
                Is.EqualTo("U = 531,20 кВ, выше верхней границы 525,00 кВ"));
            Assert.That(
                RegulationRun.DescribeVoltage(498, bounds),
                Is.EqualTo("U = 498,00 кВ, ниже нижней границы 500,00 кВ"));
            Assert.That(RegulationRun.DescribeVoltage(510, bounds), Is.EqualTo("U = 510,00 кВ, в допустимой области"));
        }
    }
}
