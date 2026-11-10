namespace VrdoPdg.Model.Entities
{
    /// <summary>
    /// Напряжение в узле КП в определенном часе суток
    /// </summary>
    public sealed class NodeVoltage
    {
        /// <summary>
        /// Создает значение напряжения в узле
        /// </summary>
        /// <param name="nodeNumber">Номер узла</param>
        /// <param name="hour">Час суток</param>
        /// <param name="voltage">Напряжение, кВ</param>
        public NodeVoltage(int nodeNumber, int hour, double voltage)
        {
            NodeNumber = nodeNumber;
            Hour = hour;
            Voltage = voltage;
        }

        /// <summary>
        /// Номер узла
        /// </summary>
        public int NodeNumber { get; }

        /// <summary>
        /// Час суток
        /// </summary>
        public int Hour { get; }

        /// <summary>
        /// Напряжение, кВ
        /// </summary>
        public double Voltage { get; }
    }
}
