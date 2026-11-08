using System.Collections.Generic;
using System.Drawing;

namespace VrdoPdg.View.Theme
{
    /// <summary>
    /// Шрифты интерфейса. Шрифты создаются один раз и используются повторно.
    /// </summary>
    internal static class UiFonts
    {
        /// <summary>
        /// Основной шрифт интерфейса.
        /// </summary>
        private const string RegularFamily = "Segoe UI";

        /// <summary>
        /// Полужирный шрифт заголовков.
        /// </summary>
        private const string SemiboldFamily = "Segoe UI Semibold";

        /// <summary>
        /// Моноширинный шрифт протокола и номеров.
        /// </summary>
        private const string MonospaceFamily = "Consolas";

        /// <summary>
        /// Созданные шрифты по семейству, размеру и стилю.
        /// </summary>
        private static readonly Dictionary<(string Family, float Size, FontStyle Style), Font> _fonts =
            new Dictionary<(string Family, float Size, FontStyle Style), Font>();

        /// <summary>
        /// Возвращает основной шрифт.
        /// </summary>
        /// <param name="size">Размер, пт.</param>
        /// <returns>Шрифт.</returns>
        public static Font Regular(float size)
        {
            return Get(RegularFamily, size, FontStyle.Regular);
        }

        /// <summary>
        /// Возвращает полужирный шрифт.
        /// </summary>
        /// <param name="size">Размер, пт.</param>
        /// <returns>Шрифт.</returns>
        public static Font Semibold(float size)
        {
            return Get(SemiboldFamily, size, FontStyle.Regular);
        }

        /// <summary>
        /// Возвращает жирный шрифт.
        /// </summary>
        /// <param name="size">Размер, пт.</param>
        /// <returns>Шрифт.</returns>
        public static Font Bold(float size)
        {
            return Get(RegularFamily, size, FontStyle.Bold);
        }

        /// <summary>
        /// Возвращает моноширинный шрифт.
        /// </summary>
        /// <param name="size">Размер, пт.</param>
        /// <returns>Шрифт.</returns>
        public static Font Monospace(float size)
        {
            return Get(MonospaceFamily, size, FontStyle.Regular);
        }

        /// <summary>
        /// Возвращает моноширинный жирный шрифт.
        /// </summary>
        /// <param name="size">Размер, пт.</param>
        /// <returns>Шрифт.</returns>
        public static Font MonospaceBold(float size)
        {
            return Get(MonospaceFamily, size, FontStyle.Bold);
        }

        /// <summary>
        /// Возвращает шрифт из кэша, создавая его при первом обращении.
        /// </summary>
        /// <param name="family">Семейство шрифта.</param>
        /// <param name="size">Размер, пт.</param>
        /// <param name="style">Стиль.</param>
        /// <returns>Шрифт.</returns>
        private static Font Get(string family, float size, FontStyle style)
        {
            Font? font;
            if (!_fonts.TryGetValue((family, size, style), out font))
            {
                font = new Font(family, size, style, GraphicsUnit.Point);
                _fonts[(family, size, style)] = font;
            }

            return font;
        }
    }
}
