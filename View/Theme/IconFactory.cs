using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;

namespace VrdoPdg.View.Theme
{
    /// <summary>
    /// Создание изображений значков из шрифта Segoe MDL2 Assets.
    /// Если шрифт не установлен, значки не выводятся.
    /// </summary>
    internal static class IconFactory
    {
        /// <summary>
        /// Шрифт значков Windows 10 и 11.
        /// </summary>
        private const string IconFamily = "Segoe MDL2 Assets";

        /// <summary>
        /// Созданные изображения по значку, цвету и размеру.
        /// </summary>
        private static readonly Dictionary<(char Glyph, int Color, int Size), Image> _images =
            new Dictionary<(char Glyph, int Color, int Size), Image>();

        /// <summary>
        /// Признак наличия шрифта значков.
        /// </summary>
        private static readonly bool _isFontAvailable = IsFontInstalled();

        /// <summary>
        /// Возвращает изображение значка.
        /// </summary>
        /// <param name="glyph">Код значка.</param>
        /// <param name="color">Цвет значка.</param>
        /// <param name="size">Размер изображения, пикселей.</param>
        /// <returns>Изображение или <c>null</c>, если шрифт значков не установлен.</returns>
        public static Image? Create(char glyph, Color color, int size)
        {
            if (!_isFontAvailable)
            {
                return null;
            }

            Image? image;
            if (_images.TryGetValue((glyph, color.ToArgb(), size), out image))
            {
                return image;
            }

            Bitmap bitmap = new Bitmap(size, size);
            using (Graphics graphics = Graphics.FromImage(bitmap))
            using (Font font = new Font(IconFamily, size * 0.62f, FontStyle.Regular, GraphicsUnit.Pixel))
            using (SolidBrush brush = new SolidBrush(color))
            using (StringFormat format = new StringFormat())
            {
                graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;
                graphics.DrawString(glyph.ToString(), font, brush, new RectangleF(0, 0, size, size), format);
            }

            _images[(glyph, color.ToArgb(), size)] = bitmap;
            return bitmap;
        }

        /// <summary>
        /// Проверяет, установлен ли шрифт значков.
        /// </summary>
        /// <returns><c>true</c>, если шрифт установлен.</returns>
        private static bool IsFontInstalled()
        {
            using (InstalledFontCollection fonts = new InstalledFontCollection())
            {
                foreach (FontFamily family in fonts.Families)
                {
                    if (family.Name == IconFamily)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
