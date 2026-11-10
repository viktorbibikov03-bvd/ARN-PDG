using System.IO;

using NUnit.Framework;

using VrdoPdg.Model;
using VrdoPdg.Model.Services;

namespace VrdoPdg.Tests
{
    /// <summary>
    /// Тесты проверки файлов исходных данных.
    /// </summary>
    [TestFixture]
    public sealed class SourceFileValidatorTests
    {
        /// <summary>
        /// Каталог временных файлов теста.
        /// </summary>
        private string _directory = string.Empty;

        /// <summary>
        /// Создает каталог временных файлов.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _directory = Path.Combine(Path.GetTempPath(), "vrdo_src_" + Path.GetRandomFileName());
            Directory.CreateDirectory(_directory);
        }

        /// <summary>
        /// Удаляет временные файлы.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            Directory.Delete(_directory, true);
        }

        /// <summary>
        /// Доступный файл с ожидаемым расширением проходит проверку независимо от регистра расширения.
        /// </summary>
        [Test]
        public void Validate_ExistingFile_Succeeds()
        {
            string path = Path.Combine(_directory, "smzu_mega_act.MPTSMZ");
            File.WriteAllText(path, string.Empty);

            Assert.DoesNotThrow(() => SourceFileValidator.Validate(path, SourceFileValidator.MegapointExtension));
        }

        /// <summary>
        /// Пустой путь, другое расширение, отсутствующий и занятый файлы отклоняются.
        /// </summary>
        [Test]
        public void Validate_InvalidFiles_ThrowsByError()
        {
            string path = Path.Combine(_directory, "smp_smzu.csv");
            File.WriteAllText(path, string.Empty);

            Assert.That(GetError(" ", ".csv"), Is.EqualTo(DataFileError.NotFound));
            Assert.That(GetError(path, ".mptsmz"), Is.EqualTo(DataFileError.UnknownFormat));
            Assert.That(GetError(Path.Combine(_directory, "нет.csv"), ".csv"), Is.EqualTo(DataFileError.NotFound));
            using (new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                Assert.That(GetError(path, ".csv"), Is.EqualTo(DataFileError.AccessDenied));
            }
        }

        /// <summary>
        /// Проверяет файл и возвращает вид ошибки проверки.
        /// </summary>
        /// <param name="path">Путь к файлу.</param>
        /// <param name="extension">Ожидаемое расширение.</param>
        /// <returns>Вид ошибки.</returns>
        private static DataFileError GetError(string path, string extension)
        {
            return Assert.Throws<DataFileException>(() => SourceFileValidator.Validate(path, extension))!.Error;
        }
    }
}
