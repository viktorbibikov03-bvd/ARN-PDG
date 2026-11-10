using NUnit.Framework;

using VrdoPdg.Model.Entities;
using VrdoPdg.Model.Services;

namespace VrdoPdg.Tests
{
    /// <summary>
    /// Тесты строки редактируемой таблицы эффективности.
    /// </summary>
    [TestFixture]
    public sealed class EfficiencyRowTests
    {
        /// <summary>
        /// Копия строки совпадает с исходной и не зависит от нее.
        /// </summary>
        [Test]
        public void Clone_Row_ReturnsIndependentEqualCopy()
        {
            EfficiencyRow row = new EfficiencyRow(1, DeviceType.Switched, 9);

            EfficiencyRow copy = row.Clone();
            copy.Priority = 2;

            Assert.That(copy, Is.Not.SameAs(row));
            Assert.That(row.Clone().IsSameAs(row), Is.True);
            Assert.That(copy.IsSameAs(row), Is.False);
        }

        /// <summary>
        /// Строки различаются по приоритету, типу и номеру СКРМ.
        /// </summary>
        [Test]
        public void IsSameAs_DifferentRows_ReturnsFalse()
        {
            EfficiencyRow row = new EfficiencyRow(1, DeviceType.Switched, 9);

            Assert.That(row.IsSameAs(null), Is.False);
            Assert.That(row.IsSameAs(new EfficiencyRow(1, DeviceType.Controlled, 9)), Is.False);
            Assert.That(row.IsSameAs(new EfficiencyRow(1, DeviceType.Switched, 10)), Is.False);
        }
    }
}
