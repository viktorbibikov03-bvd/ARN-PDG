using System;

using NUnit.Framework;

using VrdoPdg.Model.Entities;

namespace VrdoPdg.Tests
{
    /// <summary>
    /// Тесты графиков напряжений в КП.
    /// </summary>
    [TestFixture]
    public sealed class VoltageScheduleTests
    {
        /// <summary>
        /// Границы графика возвращаются по КП и часу, график полон, если заданы все часы.
        /// </summary>
        [Test]
        public void GetBounds_Hours_ReturnsBoundsAndCompleteness()
        {
            VoltageSchedule schedule = new VoltageSchedule(new[]
            {
                new VoltageBounds(1, 1, 500, 525, DateTime.Today),
                new VoltageBounds(1, 2, 505, 520, DateTime.Today)
            });

            Assert.That(schedule.GetBounds(1, 2)!.LowerBound, Is.EqualTo(505));
            Assert.That(schedule.GetBounds(1, 3), Is.Null);
            Assert.That(schedule.IsComplete(1, 2), Is.True);
            Assert.That(schedule.IsComplete(1, 3), Is.False);
            Assert.That(schedule.IsComplete(2, 1), Is.False);
        }

        /// <summary>
        /// Напряжение сравнивается с границами графика с допуском, отклонение считается от ближайшей
        /// нарушенной границы, нижняя граница не может быть больше верхней
        /// </summary>
        [Test]
        public void VoltageBounds_Voltages_AreComparedWithBounds()
        {
            VoltageBounds bounds = new VoltageBounds(1, 12, 500, 525, DateTime.Today);

            Assert.That(bounds.IsAbove(526), Is.True);
            Assert.That(bounds.IsBelow(499), Is.True);
            Assert.That(bounds.Contains(525), Is.True);
            Assert.That(bounds.Contains(500), Is.True);
            Assert.That(bounds.GetDeviation(531.5), Is.EqualTo(6.5).Within(1e-9));
            Assert.That(bounds.GetDeviation(497), Is.EqualTo(3).Within(1e-9));
            Assert.That(bounds.GetDeviation(510), Is.Zero);
            Assert.Throws<ArgumentException>(() => new VoltageBounds(1, 12, 525, 500, DateTime.Today));
        }
    }
}
