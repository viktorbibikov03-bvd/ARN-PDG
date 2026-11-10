using System;

using VrdoPdg.Model.SmpSmzu;

namespace VrdoPdg.Tests.Fakes
{
    /// <summary>
    /// Модуль чтения файла smp_smzu.csv, завершающийся непредвиденной ошибкой.
    /// </summary>
    internal sealed class FailingSmpSmzuCsvReader : ISmpSmzuCsvReader
    {
        /// <inheritdoc/>
        public SmpSmzuDocument Read(string path)
        {
            throw new InvalidOperationException("Неожиданный конец файла");
        }
    }
}
