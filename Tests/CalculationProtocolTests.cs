using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using VrdoPdg.Model.Protocol;

namespace VrdoPdg.Tests
{
    /// <summary>
    /// Тесты протокола расчета.
    /// </summary>
    [TestFixture]
    public sealed class CalculationProtocolTests
    {
        /// <summary>
        /// Записи нумеруются по порядку, вложенные записи получают уровень родителя плюс один,
        /// о каждой записи сообщает событие.
        /// </summary>
        [Test]
        public void Add_NestedEntries_NumbersEntriesAndRaisesEvent()
        {
            CalculationProtocol protocol = new CalculationProtocol();
            List<ProtocolEntry> raised = new List<ProtocolEntry>();
            protocol.EntryAdded += (sender, e) => raised.Add(e.Entry);
            DateTime startedAt = DateTime.Now;

            ProtocolEntry group = protocol.Add(ProtocolTag.Vrdo, "Барнаульская", "Выход за границы графика!");
            ProtocolEntry hour = protocol.Add(ProtocolTag.Hour, "Час 12", null, group);
            ProtocolEntry device = protocol.Add(ProtocolTag.Device, null!, null, hour);

            Assert.That(protocol.Count, Is.EqualTo(3));
            Assert.That(raised, Is.EqualTo(new[] { group, hour, device }));
            Assert.That(protocol.GetEntries(), Is.EqualTo(new[] { group, hour, device }));
            Assert.That(device.Id, Is.EqualTo(3));
            Assert.That(device.Level, Is.EqualTo(2));
            Assert.That(device.Parent, Is.SameAs(hour));
            Assert.That(device.Message, Is.Empty);
            Assert.That(hour.Details, Is.Empty);
            Assert.That(group.Tag, Is.EqualTo(ProtocolTag.Vrdo));
            Assert.That(group.Time, Is.InRange(startedAt, DateTime.Now));
        }

        /// <summary>
        /// Число записей считается по меткам.
        /// </summary>
        [Test]
        public void GetCount_Tags_CountsEntriesByTag()
        {
            CalculationProtocol protocol = new CalculationProtocol();
            protocol.Add(ProtocolTag.Warning, "Нет данных ПУР для КП");
            protocol.Add(ProtocolTag.Warning, "Не задан график напряжения КП");
            protocol.Add(ProtocolTag.Error, "Режим не сошелся");

            Assert.That(protocol.GetCount(ProtocolTag.Warning), Is.EqualTo(2));
            Assert.That(protocol.GetCount(ProtocolTag.Error), Is.EqualTo(1));
            Assert.That(protocol.GetCount(ProtocolTag.Info), Is.Zero);
        }

        /// <summary>
        /// Каждая метка входит в перечень меток протокола и имеет подпись.
        /// </summary>
        [Test]
        public void ProtocolTags_AllTags_HaveLabels()
        {
            ProtocolTag[] tags = (ProtocolTag[])Enum.GetValues(typeof(ProtocolTag));

            Assert.That(ProtocolTags.All, Is.EquivalentTo(tags));
            Assert.That(
                tags.Select(ProtocolTags.GetLabel),
                Is.EqualTo(new[] { "ВРДО", "ЧАС", "СКРМ", "УР", "WARN", "INFO", "ERR" }));
        }

        /// <summary>
        /// После очистки подписчики сохраняются, счетчики начинаются заново,
        /// а идентификаторы не переиспользуются.
        /// </summary>
        [Test]
        public void Add_AfterClear_PreservesSubscribersAndContinuesNumbering()
        {
            CalculationProtocol protocol = new CalculationProtocol();
            List<ProtocolEntry> received = new List<ProtocolEntry>();
            protocol.EntryAdded += (_, e) => received.Add(e.Entry);
            ProtocolEntry first = protocol.Add(ProtocolTag.Info, "До очистки");

            protocol.Clear();
            ProtocolEntry second = protocol.Add(ProtocolTag.Info, "После очистки");

            Assert.That(protocol.Count, Is.EqualTo(1));
            Assert.That(protocol.GetCount(ProtocolTag.Info), Is.EqualTo(1));
            Assert.That(protocol.GetEntries(), Is.EqualTo(new[] { second }));
            Assert.That(second.Id, Is.GreaterThan(first.Id));
            Assert.That(received, Is.EqualTo(new[] { first, second }));
        }

        /// <summary>Запись не сохраняет ссылку на удаленного при очистке родителя.</summary>
        [Test]
        public void Add_WithClearedParent_CreatesRootEntry()
        {
            CalculationProtocol protocol = new CalculationProtocol();
            ProtocolEntry oldParent = protocol.Add(ProtocolTag.Vrdo, "Старая группа");
            protocol.Clear();

            ProtocolEntry entry = protocol.Add(
                ProtocolTag.Hour, "Час после очистки", parent: oldParent);

            Assert.That(entry.Parent, Is.Null);
            Assert.That(entry.Level, Is.Zero);
            Assert.That(protocol.GetEntries(), Is.EqualTo(new[] { entry }));
        }
    }
}
