namespace VrdoPdg.Model.Services
{
    /// <summary>
    /// Направление ввода режима по напряжению в допустимую область.
    /// </summary>
    public enum RegulationDirection
    {
        /// <summary>
        /// Напряжение выше верхней границы графика — его нужно снизить.
        /// </summary>
        Decrease,

        /// <summary>
        /// Напряжение ниже нижней границы графика — его нужно повысить.
        /// </summary>
        Increase
    }
}
