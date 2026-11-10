using System;

using NUnit.Framework;

using VrdoPdg.Model.Entities;

namespace VrdoPdg.Tests
{
    /// <summary>
    /// Тесты СКРМ и наименований типов СКРМ.
    /// </summary>
    [TestFixture]
    public sealed class DeviceTests
    {
        /// <summary>
        /// Наименование, номер и вид СКРМ выводятся по типу СКРМ.
        /// </summary>
        [Test]
        public void Constructor_Types_SetsNamesAndKind()
        {
            Device controlled = new Device(1, DeviceType.Controlled, null!, 520, SwitchedDeviceKind.Reactor);
            Device capacitor = new Device(17, DeviceType.Switched, "БСК-1", 125, SwitchedDeviceKind.CapacitorBank);
            Device generator = new Device(41, DeviceType.Generator, "ТГ-5", 13, null);

            Assert.That(controlled.Name, Is.Empty);
            Assert.That(controlled.SwitchedKind, Is.Null);
            Assert.That(controlled.TypeName, Is.EqualTo("Управляемые"));
            Assert.That(controlled.NumberText, Is.EqualTo("узел 520"));
            Assert.That(capacitor.IsCapacitorBank, Is.True);
            Assert.That(capacitor.TypeName, Is.EqualTo("Коммутируемые"));
            Assert.That(capacitor.ToString(), Is.EqualTo("БСК-1 (реактор 125)"));
            Assert.That(generator.IsCapacitorBank, Is.False);
            Assert.That(generator.TypeName, Is.EqualTo("Генераторы"));
            Assert.That(generator.NumberText, Is.EqualTo("генератор 13"));
        }

        /// <summary>
        /// Для коммутируемого СКРМ вид обязателен.
        /// </summary>
        [Test]
        public void Constructor_SwitchedWithoutKind_Throws()
        {
            Assert.Throws<ArgumentException>(() => new Device(5, DeviceType.Switched, "Р-1", 9, null));
        }

        /// <summary>
        /// Наименования типов и видов СКРМ из БД разбираются, неизвестные наименования отклоняются.
        /// </summary>
        [Test]
        public void TryParse_Names_ParsesKnownNames()
        {
            DeviceType type;
            SwitchedDeviceKind kind;

            Assert.That(DeviceNames.TryParseType("Генераторы", out type), Is.True);
            Assert.That(type, Is.EqualTo(DeviceType.Generator));
            Assert.That(DeviceNames.TryParseType("Синхронные компенсаторы", out type), Is.False);
            Assert.That(DeviceNames.TryParseType(null, out type), Is.False);
            Assert.That(DeviceNames.TryParseKind("БСК", out kind), Is.True);
            Assert.That(kind, Is.EqualTo(SwitchedDeviceKind.CapacitorBank));
            Assert.That(DeviceNames.TryParseKind("Реактор", out kind), Is.True);
            Assert.That(kind, Is.EqualTo(SwitchedDeviceKind.Reactor));
            Assert.That(DeviceNames.TryParseKind("СТК", out kind), Is.False);
        }

        /// <summary>
        /// Для неизвестного типа СКРМ наименование не определено.
        /// </summary>
        [Test]
        public void GetTypeName_UnknownType_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => DeviceNames.GetTypeName((DeviceType)99));
        }
    }
}
