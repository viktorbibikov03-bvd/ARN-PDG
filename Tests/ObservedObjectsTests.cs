using System;

using NUnit.Framework;

using VrdoPdg.Model.Regime;

namespace VrdoPdg.Tests
{
    /// <summary>
    /// Тесты перечня наблюдаемых объектов МТ.
    /// </summary>
    [TestFixture]
    public sealed class ObservedObjectsTests
    {
        /// <summary>
        /// Повторяющиеся номера исключаются, номера упорядочиваются.
        /// </summary>
        [Test]
        public void Constructor_Numbers_RemovesDuplicatesAndSorts()
        {
            ObservedObjects observed = new ObservedObjects(new[] { 6490, 564, 6490 }, new[] { 39, 9, 39 });

            Assert.That(observed.NodeNumbers, Is.EqualTo(new[] { 564, 6490 }));
            Assert.That(observed.ReactorNumbers, Is.EqualTo(new[] { 9, 39 }));
        }

        /// <summary>
        /// Перечни номеров обязательны.
        /// </summary>
        [Test]
        public void Constructor_NullNumbers_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new ObservedObjects(null!, new int[0]));
            Assert.Throws<ArgumentNullException>(() => new ObservedObjects(new int[0], null!));
        }
    }
}
