using System.Collections.Generic;

namespace VrdoPdg.Model.SmpSmzu
{
    /// <summary>
    /// Модуль формирования файла smp_smzu.csv
    /// </summary>
    public interface ISmpSmzuCsvWriter
    {
        /// <summary>
        /// Записывает файл smp_smzu.csv: строки исходного файла сохраняются без изменений,
        /// новые записи добавляются во второй блок файла (записи о ручных изменениях)
        /// </summary>
        /// <param name="path">Путь к сохраняемому файлу</param>
        /// <param name="source">Содержимое исходного файла</param>
        /// <param name="addedRecords">Добавляемые записи</param>
        void Write(string path, SmpSmzuDocument source, IReadOnlyList<SmpSmzuRecord> addedRecords);
    }
}
