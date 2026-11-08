using System.Drawing;
using System.Windows.Forms;

namespace VrdoPdg.View.Theme
{
    /// <summary>
    /// Оформление окон средствами DWM: темный заголовок (Windows 10 20H1 и новее)
    /// и скругленные углы окон без рамки (Windows 11).
    /// </summary>
    internal static class WindowTheme
    {
        /// <summary>
        /// Атрибут DWM темного заголовка окна.
        /// </summary>
        private const int DarkModeAttribute = 20;

        /// <summary>
        /// Атрибут DWM темного заголовка окна в ранних сборках Windows 10.
        /// </summary>
        private const int DarkModeAttributeLegacy = 19;

        /// <summary>
        /// Атрибут DWM скругления углов окна (Windows 11).
        /// </summary>
        private const int CornerPreferenceAttribute = 33;

        /// <summary>
        /// Значение атрибута скругления углов: стандартное скругление.
        /// </summary>
        private const int RoundCorners = 2;

        /// <summary>
        /// Атрибут DWM цвета рамки окна (Windows 11).
        /// </summary>
        private const int BorderColorAttribute = 34;

        /// <summary>
        /// Атрибут DWM цвета заголовка окна (Windows 11).
        /// </summary>
        private const int CaptionColorAttribute = 35;

        /// <summary>
        /// Включает темный заголовок окна.
        /// </summary>
        /// <param name="form">Окно.</param>
        public static void ApplyDarkTitleBar(Form form)
        {
            if (!form.IsHandleCreated)
            {
                return;
            }

            int enabled = 1;
            if (NativeMethods.DwmSetWindowAttribute(form.Handle, DarkModeAttribute, ref enabled, sizeof(int)) != 0)
            {
                NativeMethods.DwmSetWindowAttribute(form.Handle, DarkModeAttributeLegacy, ref enabled, sizeof(int));
            }

            int captionColor = ToColorRef(Palette.Background);
            NativeMethods.DwmSetWindowAttribute(form.Handle, CaptionColorAttribute, ref captionColor, sizeof(int));
        }

        /// <summary>
        /// Включает системное скругление углов окна и задает цвет его рамки (Windows 11).
        /// </summary>
        /// <param name="form">Окно.</param>
        /// <param name="borderColor">Цвет рамки окна.</param>
        /// <returns><c>true</c>, если углы окна скругляет система; <c>false</c>, если система
        /// этого не поддерживает (Windows 10).</returns>
        public static bool ApplyRoundedCorners(Form form, Color borderColor)
        {
            if (!form.IsHandleCreated)
            {
                return false;
            }

            int preference = RoundCorners;
            if (NativeMethods.DwmSetWindowAttribute(form.Handle, CornerPreferenceAttribute, ref preference, sizeof(int))
                != 0)
            {
                return false;
            }

            int color = ToColorRef(borderColor);
            NativeMethods.DwmSetWindowAttribute(form.Handle, BorderColorAttribute, ref color, sizeof(int));
            return true;
        }

        /// <summary>
        /// Возвращает цвет в формате COLORREF.
        /// </summary>
        /// <param name="color">Цвет.</param>
        /// <returns>Значение COLORREF.</returns>
        private static int ToColorRef(Color color)
        {
            return color.R | (color.G << 8) | (color.B << 16);
        }
    }
}
