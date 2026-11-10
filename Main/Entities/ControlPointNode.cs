namespace VrdoPdg.Model.Entities
{
    /// <summary>
    /// Узел расчетной модели, напряжение в котором контролируется для КП.
    /// </summary>
    public sealed class ControlPointNode
    {
        /// <summary>
        /// Создает узел КП
        /// </summary>
        /// <param name="number">Номер узла в таблице «Узлы» МТ</param>
        /// <param name="isMain">Признак основного узла</param>
        public ControlPointNode(int number, bool isMain)
        {
            Number = number;
            IsMain = isMain;
        }

        /// <summary>
        /// Номер узла в таблице «Узлы» МТ.
        /// </summary>
        public int Number { get; }

        /// <summary>
        /// Признак основного узла, в котором осуществляется ввод режима
        /// по напряжению в допустимую область
        /// </summary>
        public bool IsMain { get; }
    }
}
