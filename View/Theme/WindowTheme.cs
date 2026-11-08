using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace VrdoPdg.View.Theme
{
    /// <summary>
    /// Оформление заголовка окна в темной теме средствами DWM (Windows 10 20H1 и новее).
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
            if (DwmSetWindowAttribute(form.Handle, DarkModeAttribute, ref enabled, sizeof(int)) != 0)
            {
                DwmSetWindowAttribute(form.Handle, DarkModeAttributeLegacy, ref enabled, sizeof(int));
            }

            int captionColor = ToColorRef(Palette.Background);
            DwmSetWindowAttribute(form.Handle, CaptionColorAttribute, ref captionColor, sizeof(int));
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

        /// <summary>
        /// Устанавливает атрибут окна DWM.
        /// </summary>
        /// <param name="hwnd">Дескриптор окна.</param>
        /// <param name="attribute">Атрибут.</param>
        /// <param name="value">Значение.</param>
        /// <param name="size">Размер значения.</param>
        /// <returns>Код результата HRESULT.</returns>
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);
    }
}
