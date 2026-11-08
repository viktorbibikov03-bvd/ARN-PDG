namespace VrdoPdg.Model.Regime
{
    /// <summary>
    /// Состояние коммутируемого СКРМ по данным таблицы «Реакторы» МТ.
    /// </summary>
    public sealed class SwitchedDeviceState
    {
        /// <summary>
        /// Создает состояние коммутируемого СКРМ.
        /// </summary>
        /// <param name="isOn">Признак состояния «Включено».</param>
        /// <param name="reactivePower">Расчетная реактивная мощность, Мвар.</param>
        public SwitchedDeviceState(bool isOn, double reactivePower)
        {
            IsOn = isOn;
            ReactivePower = reactivePower;
        }

        /// <summary>
        /// Признак состояния «Включено».
        /// </summary>
        public bool IsOn { get; }

        /// <summary>
        /// Расчетная реактивная мощность, Мвар.
        /// </summary>
        public double ReactivePower { get; }
    }
}
