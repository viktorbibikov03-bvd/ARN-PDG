namespace VrdoPdg.Model.SmpSmzu
{
    /// <summary>
    /// Модуль чтения файла smp_smzu.csv.
    /// </summary>
    public interface ISmpSmzuCsvReader
    {
        /// <summary>
        /// Читает и разбирает файл smp_smzu.csv.
        /// </summary>
        /// <param name="path">Путь к файлу.</param>
        /// <returns>Содержимое файла.</returns>
        /// <exception cref="DataFileException">Файл не найден, недоступен или неправильно заполнен.</exception>
        SmpSmzuDocument Read(string path);
    }
}
