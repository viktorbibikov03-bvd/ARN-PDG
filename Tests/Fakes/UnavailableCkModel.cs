using System;
using System.Collections.Generic;

using VrdoPdg.Model.Entities;
using VrdoPdg.Model.Schedules;

namespace VrdoPdg.Tests.Fakes
{
    /// <summary>
    /// Модуль взаимодействия с ОИК «СК-11», имитирующий недоступность ОИК.
    /// </summary>
    internal sealed class UnavailableCkModel : ICkModel
    {
        /// <inheritdoc/>
        public string SourceName => "ОИК «СК-11» (недоступен)";

        /// <inheritdoc/>
        public IReadOnlyList<VoltageBounds> GetVoltageSchedule(IReadOnlyList<ControlPoint> controlPoints, DateTime date)
        {
            throw new InvalidOperationException("ОИК «СК-11» недоступен");
        }
    }
}
