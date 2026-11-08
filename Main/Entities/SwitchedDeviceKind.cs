namespace VrdoPdg.Model.Entities
{
    /// <summary>
    /// Вид коммутируемого СКРМ.
    /// </summary>
    public enum SwitchedDeviceKind
    {
        /// <summary>
        /// Шунтирующий реактор: потребляет реактивную мощность
        /// и при включении понижает напряжение.
        /// </summary>
        Reactor,

        /// <summary>
        /// Батарея статических конденсаторов: выдает реактивную мощность
        /// и при включении повышает напряжение.
        /// </summary>
        CapacitorBank
    }
}
