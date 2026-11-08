namespace VrdoPdg.Model.SmpSmzu
{
    /// <summary>
    /// Тип записи файла smp_smzu.csv (код в первом поле записи).
    /// Значения совпадают с типами действий таблицы «Изменение параметров схемы замещения» МТ.
    /// </summary>
    public enum SmpSmzuRecordType
    {
        /// <summary>
        /// Узел: изменено значение параметра «Состояние» (код 00).
        /// </summary>
        NodeState = 0,

        /// <summary>
        /// Узел: изменено значение параметра «Напряжение» (код 01).
        /// </summary>
        NodeVoltage = 1,

        /// <summary>
        /// Узел: изменены значения параметров «Qузл_макс», «Qузл_мин» (код 03).
        /// </summary>
        NodeReactivePowerLimits = 3,

        /// <summary>
        /// Ветвь: изменено значение параметра «Состояние» (код 04).
        /// </summary>
        BranchState = 4,

        /// <summary>
        /// Узел: изменено значение параметра «Балансирующий» (код 12).
        /// </summary>
        BalancingNode = 12,

        /// <summary>
        /// Узел: изменены значения параметров «Рн», «Рмакс», «Рмин» (код 16).
        /// </summary>
        NodeActivePowerLimits = 16,

        /// <summary>
        /// Реакторы: изменено значение параметра «Состояние» (код 20).
        /// </summary>
        Reactor = 20
    }
}
