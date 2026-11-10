using NUnit.Framework;

using VrdoPdg.Model;

namespace VrdoPdg.Tests
{
    /// <summary>
    /// Тесты форматирования значений.
    /// </summary>
    [TestFixture]
    public sealed class ValueFormatTests
    {
        /// <summary>
        /// Значения выводятся с запятой и заданным числом знаков после нее.
        /// </summary>
        [Test]
        public void Format_Values_UsesRussianCulture()
        {
            Assert.That(ValueFormat.Voltage(515.256), Is.EqualTo("515,26"));
            Assert.That(ValueFormat.ShortVoltage(515.25), Is.EqualTo("515,3"));
            Assert.That(ValueFormat.Power(-50), Is.EqualTo("-50,0"));
            Assert.That(ValueFormat.ShortPower(-50), Is.EqualTo("-50"));
            Assert.That(ValueFormat.ShortPower(12.5), Is.EqualTo("12,5"));
        }
    }
}
