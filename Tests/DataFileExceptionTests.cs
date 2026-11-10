using System;

using NUnit.Framework;

using VrdoPdg.Model;

namespace VrdoPdg.Tests
{
    /// <summary>
    /// Тесты ошибок файлов исходных данных и ошибок системы.
    /// </summary>
    [TestFixture]
    public sealed class DataFileExceptionTests
    {
        /// <summary>
        /// Сообщение определяется видом ошибки, подробности содержат путь к файлу и пояснение.
        /// </summary>
        [TestCase(DataFileError.NotFound, "Не найден файл с исходными данными")]
        [TestCase(DataFileError.UnknownFormat, "Формат файла не распознан")]
        [TestCase(DataFileError.AccessDenied, "Нет доступа к файлу")]
        [TestCase(DataFileError.InvalidContent, "Неправильно заполнен файл исходных данных")]
        public void Constructor_Error_SetsMessageAndDetails(DataFileError error, string message)
        {
            InvalidOperationException inner = new InvalidOperationException("занят");

            DataFileException exception = new DataFileException(error, "smp_smzu.csv", "строка 2", inner);

            Assert.That(exception.Message, Is.EqualTo(message));
            Assert.That(exception.Error, Is.EqualTo(error));
            Assert.That(exception.FilePath, Is.EqualTo("smp_smzu.csv"));
            Assert.That(exception.Details, Is.EqualTo("smp_smzu.csv — строка 2"));
            Assert.That(exception.InnerException, Is.SameAs(inner));
        }

        /// <summary>
        /// Без пояснения подробности содержат только путь к файлу, пустой путь заменяется пустой строкой.
        /// </summary>
        [Test]
        public void Constructor_NoDetails_UsesFilePath()
        {
            DataFileException withPath = new DataFileException(DataFileError.NotFound, "smzu.mptsmz", null);
            DataFileException withoutPath = new DataFileException(DataFileError.NotFound, null!, null);
            DataFileException inner = new DataFileException(
                DataFileError.NotFound, null!, null, new InvalidOperationException());

            Assert.That(withPath.Details, Is.EqualTo("smzu.mptsmz"));
            Assert.That(withoutPath.FilePath, Is.Empty);
            Assert.That(withoutPath.Details, Is.Empty);
            Assert.That(inner.FilePath, Is.Empty);
        }
    }
}
