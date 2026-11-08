using VrdoPdg.Model.Entities;

namespace VrdoPdg.Model.Services
{
    /// <summary>
    /// Сведения, необходимые для формирования режима СКРМ: PQ-диаграммы генераторов
    /// и признаки состояний, заданных в файле smp_smzu.csv.
    /// </summary>
    internal interface IDeviceModeContext
    {
        /// <summary>
        /// Проверяет, задано ли состояние СКРМ в заданном часе в файле smp_smzu.csv.
        /// </summary>
        /// <param name="deviceId">Код СКРМ.</param>
        /// <param name="hour">Час суток (1–24).</param>
        /// <returns><c>true</c>, если состояние задано.</returns>
        bool IsFixed(int deviceId, int hour);

        /// <summary>
        /// Возвращает PQ-диаграмму генератора.
        /// </summary>
        /// <param name="deviceId">Код СКРМ генератора.</param>
        /// <returns>PQ-диаграмма или пустая диаграмма.</returns>
        PQDiagram GetPQDiagram(int deviceId);
    }
}
