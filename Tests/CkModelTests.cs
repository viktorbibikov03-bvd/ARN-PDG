using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using VrdoPdg.Infrastructure;
using VrdoPdg.Model.Entities;

namespace VrdoPdg.Tests
{
    /// <summary>
    /// Тесты временной модели получения графиков напряжений из ОИК «СК-11».
    /// </summary>
    [TestFixture]
    public sealed class CkModelTests
    {
        /// <summary>
        /// Для каждого КП возвращаются границы на 24 часа, по умолчанию 500–525 кВ.
        /// </summary>
        [Test]
        public void GetVoltageSchedule_DefaultBounds_ReturnsAllHours()
        {
            ControlPoint[] controlPoints =
            {
                new ControlPoint(1, 1, "Барнаульская", new ControlPointNode[0]),
                new ControlPoint(2, 1, "Рубцовская", new ControlPointNode[0])
            };
            CkModel model = new CkModel();

            IReadOnlyList<VoltageBounds> schedule = model.GetVoltageSchedule(controlPoints, DateTime.Today);

            Assert.That(schedule, Has.Count.EqualTo(48));
            Assert.That(schedule.Select(bounds => bounds.Hour).Distinct().Count(), Is.EqualTo(24));
            Assert.That(schedule.All(bounds => bounds.LowerBound == 500 && bounds.UpperBound == 525), Is.True);
            Assert.That(model.SourceName, Does.Contain("500–525 кВ"));
        }

        /// <summary>
        /// Нижняя граница не может быть больше верхней, перечень КП обязателен.
        /// </summary>
        [Test]
        public void Constructor_InvalidArguments_Throw()
        {
            Assert.Throws<ArgumentException>(() => new CkModel(530, 500));
            Assert.Throws<ArgumentNullException>(() => new CkModel(495, 530).GetVoltageSchedule(null!, DateTime.Today));
        }
    }
}
