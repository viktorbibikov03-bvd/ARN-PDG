using System;

using NUnit.Framework;

using VrdoPdg.Model;

namespace VrdoPdg.Tests
{
    /// <summary>
    /// Тесты ошибки системы.
    /// </summary>
    [TestFixture]
    public sealed class VrdoExceptionTests
    {
        /// <summary>
        /// Ошибка хранит сообщение, подробности и исходную ошибку; пустые подробности
        /// заменяются пустой строкой.
        /// </summary>
        [Test]
        public void Constructor_Arguments_SetsProperties()
        {
            InvalidOperationException inner = new InvalidOperationException("БД занята");

            VrdoException exception = new VrdoException("Ошибка базы данных", null, inner);
            VrdoException simple = new VrdoException("Ошибка", "подробности");
            VrdoException empty = new VrdoException("Ошибка", null);

            Assert.That(exception.Message, Is.EqualTo("Ошибка базы данных"));
            Assert.That(exception.Details, Is.Empty);
            Assert.That(exception.InnerException, Is.SameAs(inner));
            Assert.That(simple.Details, Is.EqualTo("подробности"));
            Assert.That(empty.Details, Is.Empty);
        }
    }
}
