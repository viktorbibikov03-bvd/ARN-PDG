namespace VrdoPdg.Model.Regime
{
    /// <summary>
    /// Состояние наблюдаемого реактора таблицы «Реакторы» МТ в определенном часе суток
    /// </summary>
    public sealed class ObservedReactorState
    {
        /// <summary>
        /// Создает состояние реактора
        /// </summary>
        /// <param name="reactorNumber">Номер реактора</param>
        /// <param name="hour">Час суток</param>
        /// <param name="isOn">Признак состояния «Включено»</param>
        public ObservedReactorState(int reactorNumber, int hour, bool isOn)
        {
            ReactorNumber = reactorNumber;
            Hour = hour;
            IsOn = isOn;
        }

        /// <summary>
        /// Номер реактора
        /// </summary>
        public int ReactorNumber { get; }

        /// <summary>
        /// Час суток
        /// </summary>
        public int Hour { get; }

        /// <summary>
        /// Признак состояния «Включено»
        /// </summary>
        public bool IsOn { get; }
    }
}
