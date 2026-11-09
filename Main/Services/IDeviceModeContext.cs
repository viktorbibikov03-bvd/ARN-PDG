namespace VrdoPdg.Model.Services
{
    /// <summary>
    /// Сведения, необходимые для формирования режима СКРМ: признаки состояний,
    /// заданных в файле smp_smzu.csv.
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
    }
}
