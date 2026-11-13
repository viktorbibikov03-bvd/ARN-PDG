using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;

using VrdoPdg.Model.Entities;
using VrdoPdg.Model.Regime;
using VrdoPdg.Model.Services;
using VrdoPdg.Model.SmpSmzu;

namespace VrdoPdg.Tests
{
    /// <summary>
    /// Тесты исходных данных сеанса
    /// </summary>
    [TestFixture]
    public sealed class SessionDataTests
    {
        /// <summary>
        /// Справочник СКРМ сеанса дополняется только новыми СКРМ; узел подключения нового генератора
        /// определяется по перечню объектов МТ; без перечня используется пустой перечень
        /// </summary>
        [Test]
        public void AddDevices_NewDevices_AddsDevicesAndGeneratorNodes()
        {
            Device reactor = new Device(1, DeviceType.Switched, "Р-1", 9, SwitchedDeviceKind.Reactor);
            MegapointCatalog catalog = new MegapointCatalog(
                new Dictionary<int, string>(),
                new Dictionary<int, string>(),
                new[] { new MegapointGenerator(313, 6400, "ТГ-9") });
            SessionData session = CreateSession(new[] { reactor }, catalog);

            IReadOnlyList<Device> added = session.AddDevices(new[]
            {
                reactor,
                new Device(2, DeviceType.Generator, "ТГ-9", 313, null),
                new Device(3, DeviceType.Generator, "ТГ-10", 314, null)
            });

            Assert.That(added, Has.Count.EqualTo(2));
            Assert.That(session.Devices, Has.Count.EqualTo(3));
            Assert.That(session.GetGeneratorNode(2), Is.EqualTo(6400));
            Assert.That(session.GetGeneratorNode(3), Is.Null);
            Assert.That(CreateSession(new Device[0], null).Catalog, Is.SameAs(MegapointCatalog.Empty));
        }

        /// <summary>
        /// Создает исходные данные сеанса
        /// </summary>
        /// <param name="devices">Справочник СКРМ</param>
        /// <param name="catalog">Перечень объектов МТ</param>
        /// <returns>Исходные данные сеанса</returns>
        private static SessionData CreateSession(IReadOnlyList<Device> devices, MegapointCatalog? catalog)
        {
            return new SessionData(
                "smzu_mega_act.mptsmz",
                "smp_smzu.csv",
                new SmpSmzuDocument(new string[0], new SmpSmzuRecord[0], "\r\n", new UTF8Encoding(false), null),
                new PowerSystem[0],
                new ControlPoint[0],
                devices,
                new VoltageSchedule(new VoltageBounds[0]),
                new RegimeSnapshot(),
                new Dictionary<int, int>(),
                new (int DeviceId, int Hour)[0],
                DateTime.Now,
                catalog);
        }
    }
}
