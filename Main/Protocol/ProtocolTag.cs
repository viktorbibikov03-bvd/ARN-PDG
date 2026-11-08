namespace VrdoPdg.Model.Protocol
{
    /// <summary>
    /// Метка типа записи протокола расчета.
    /// </summary>
    public enum ProtocolTag
    {
        /// <summary>
        /// ВРДО: заголовок группы записей по КП, в котором выполнялся ввод режима.
        /// </summary>
        Vrdo,

        /// <summary>
        /// ЧАС: заголовок группы записей по часу суток.
        /// </summary>
        Hour,

        /// <summary>
        /// СКРМ: действие с СКРМ.
        /// </summary>
        Device,

        /// <summary>
        /// УР: результат расчета установившегося режима.
        /// </summary>
        Regime,

        /// <summary>
        /// WARN: предупреждение.
        /// </summary>
        Warning,

        /// <summary>
        /// INFO: информационное сообщение.
        /// </summary>
        Info,

        /// <summary>
        /// ERR: ошибка.
        /// </summary>
        Error
    }
}
