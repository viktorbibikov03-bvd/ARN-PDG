namespace VrdoPdg.Model.SmpSmzu
{
    /// <summary>
    /// Запись файла smp_smzu.csv: изменение параметра элемента сети в диапазоне часов.
    /// Формат строки: «тип;час начала;час конца;адрес 1;адрес 2;адрес 3;значение 1;значение 2;значение 3».
    /// </summary>
    public sealed class SmpSmzuRecord
    {
        /// <summary>
        /// Создает запись файла smp_smzu.csv
        /// </summary>
        /// <param name="typeCode">Код типа записи.</param>
        /// <param name="startHour">Номер часа начала действия смены параметра.</param>
        /// <param name="endHour">Номер часа конца действия смены параметра.</param>
        /// <param name="address1">Адрес 1: номер узла, реактора или узла начала ветви.</param>
        /// <param name="address2">Адрес 2: номер узла конца ветви.</param>
        /// <param name="address3">Адрес 3: номер параллельности ветви.</param>
        /// <param name="value1">Значение 1.</param>
        /// <param name="value2">Значение 2.</param>
        /// <param name="value3">Значение 3.</param>
        /// <param name="lineNumber">Номер строки в файле (0 — запись сформирована системой).</param>
        public SmpSmzuRecord(
            int typeCode,
            int startHour,
            int endHour,
            int address1,
            int address2,
            int address3,
            double? value1,
            double? value2,
            double? value3,
            int lineNumber)
        {
            TypeCode = typeCode;
            StartHour = startHour;
            EndHour = endHour;
            Address1 = address1;
            Address2 = address2;
            Address3 = address3;
            Value1 = value1;
            Value2 = value2;
            Value3 = value3;
            LineNumber = lineNumber;
        }

        /// <summary>
        /// Код типа записи
        /// </summary>
        public int TypeCode { get; }

        /// <summary>
        /// Тип записи
        /// </summary>
        public SmpSmzuRecordType Type => (SmpSmzuRecordType)TypeCode;

        /// <summary>
        /// Номер часа начала действия смены параметра
        /// </summary>
        public int StartHour { get; }

        /// <summary>
        /// Номер часа конца действия смены параметра
        /// </summary>
        public int EndHour { get; }

        /// <summary>
        /// Адрес 1: номер узла, номер СКРМ в таблице «Реакторы» или номер узла начала ветви
        /// </summary>
        public int Address1 { get; }

        /// <summary>
        /// Адрес 2: номер узла окончания ветви
        /// </summary>
        public int Address2 { get; }

        /// <summary>
        /// Адрес 3: номер параллельности ветви.
        /// </summary>
        public int Address3 { get; }

        /// <summary>
        /// Значение 1; <c>null</c>, если поле не заполнено.
        /// </summary>
        public double? Value1 { get; }

        /// <summary>
        /// Значение 2; <c>null</c>, если поле не заполнено.
        /// </summary>
        public double? Value2 { get; }

        /// <summary>
        /// Значение 3; <c>null</c>, если поле не заполнено.
        /// </summary>
        public double? Value3 { get; }

        /// <summary>
        /// Номер строки в файле; 0 — запись сформирована системой.
        /// </summary>
        public int LineNumber { get; }
    }
}
