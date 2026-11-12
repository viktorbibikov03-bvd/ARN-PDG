using System;
using System.Collections.Generic;

using VrdoPdg.Model.Entities;

namespace VrdoPdg.Model.Schedules
{
    /// <summary>
    /// Модуль взаимодействия с ОИК «СК-11»: запрос границ графиков напряжений в КП
    /// </summary>
    public interface ICkModel
    {
        /// <summary>
        /// Наименование источника границ графиков для протокола.
        /// </summary>
        string SourceName { get; }

        /// <summary>
        /// Запрашивает верхние и нижние границы графиков напряжений КП на 24 часа
        /// </summary>
        /// <param name="controlPoints">КП, для которых запрашиваются графики</param>
        /// <param name="date">Дата, на которую запрашиваются графики</param>
        /// <returns>Границы графиков по КП и часам</returns>
        /// <exception cref="Exception">ОИК «СК-11» недоступен</exception>
        IReadOnlyList<VoltageBounds> GetVoltageSchedule(IReadOnlyList<ControlPoint> controlPoints, DateTime date);
    }
}
