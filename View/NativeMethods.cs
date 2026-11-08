using System;
using System.Runtime.InteropServices;

namespace VrdoPdg.View
{
    /// <summary>
    /// Функции Windows API, используемые интерфейсом: оформление окон средствами DWM
    /// и перетаскивание окна без рамки.
    /// </summary>
    internal static class NativeMethods
    {
        /// <summary>
        /// Сообщение WM_NCHITTEST: определение области окна под указателем мыши.
        /// </summary>
        public const int WmNcHitTest = 0x0084;

        /// <summary>
        /// Сообщение WM_NCLBUTTONDOWN: нажатие левой кнопки мыши в неклиентской области окна.
        /// </summary>
        public const int WmNcLButtonDown = 0x00A1;

        /// <summary>
        /// Результат HTCLIENT: клиентская область окна.
        /// </summary>
        public const int HtClient = 1;

        /// <summary>
        /// Результат HTCAPTION: заголовок окна, за который окно перетаскивается.
        /// </summary>
        public const int HtCaption = 2;

        /// <summary>
        /// Устанавливает атрибут окна DWM.
        /// </summary>
        /// <param name="hwnd">Дескриптор окна.</param>
        /// <param name="attribute">Атрибут.</param>
        /// <param name="value">Значение.</param>
        /// <param name="size">Размер значения.</param>
        /// <returns>Код результата HRESULT.</returns>
        [DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

        /// <summary>
        /// Освобождает захват мыши.
        /// </summary>
        /// <returns><c>true</c>, если захват освобожден.</returns>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ReleaseCapture();

        /// <summary>
        /// Отправляет сообщение окну.
        /// </summary>
        /// <param name="hwnd">Дескриптор окна.</param>
        /// <param name="message">Сообщение.</param>
        /// <param name="wParam">Первый параметр сообщения.</param>
        /// <param name="lParam">Второй параметр сообщения.</param>
        /// <returns>Результат обработки сообщения.</returns>
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam);
    }
}
