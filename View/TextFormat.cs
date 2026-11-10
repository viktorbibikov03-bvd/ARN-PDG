namespace VrdoPdg.View
{
    /// <summary>
    /// Форматирование текстов интерфейса
    /// </summary>
    internal static class TextFormat
    {
        /// <summary>
        /// Возвращает число со словом в нужной форме, например «1 запись», «3 записи», «5 записей».
        /// </summary>
        /// <param name="count">Число.</param>
        /// <param name="one">Форма для 1.</param>
        /// <param name="few">Форма для 2–4.</param>
        /// <param name="many">Форма для 5–20.</param>
        /// <returns>Строка с числом и словом.</returns>
        public static string Plural(int count, string one, string few, string many)
        {
            int lastTwoDigits = count % 100;
            int lastDigit = count % 10;
            string word;

            if (lastTwoDigits >= 11 && lastTwoDigits <= 14)
            {
                word = many;
            }
            else if (lastDigit == 1)
            {
                word = one;
            }
            else if (lastDigit >= 2 && lastDigit <= 4)
            {
                word = few;
            }
            else
            {
                word = many;
            }

            return count + " " + word;
        }
    }
}
