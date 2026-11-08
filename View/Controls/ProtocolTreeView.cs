using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace VrdoPdg.View.Controls
{
    /// <summary>
    /// Дерево записей протокола с двойной буферизацией. Двойной щелчок не раскрывает
    /// группу повторно: группы раскрываются и сворачиваются одним щелчком.
    /// </summary>
    internal sealed class ProtocolTreeView : TreeView
    {
        /// <summary>
        /// Сообщение о двойном щелчке левой кнопкой мыши.
        /// </summary>
        private const int DoubleClickMessage = 0x0203;

        /// <summary>
        /// Сообщение установки расширенного стиля дерева.
        /// </summary>
        private const int SetExtendedStyleMessage = 0x1100 + 44;

        /// <summary>
        /// Расширенный стиль двойной буферизации дерева.
        /// </summary>
        private const int DoubleBufferStyle = 0x0004;

        /// <summary>
        /// Включает двойную буферизацию после создания окна дерева.
        /// </summary>
        /// <param name="e">Аргументы события.</param>
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            SendMessage(Handle, SetExtendedStyleMessage, (IntPtr)DoubleBufferStyle, (IntPtr)DoubleBufferStyle);
        }

        /// <summary>
        /// Пропускает двойной щелчок, чтобы группа не раскрывалась повторно.
        /// </summary>
        /// <param name="m">Сообщение окна.</param>
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == DoubleClickMessage)
            {
                return;
            }

            base.WndProc(ref m);
        }

        /// <summary>
        /// Отправляет сообщение окну.
        /// </summary>
        /// <param name="hwnd">Дескриптор окна.</param>
        /// <param name="message">Сообщение.</param>
        /// <param name="wParam">Первый параметр.</param>
        /// <param name="lParam">Второй параметр.</param>
        /// <returns>Результат обработки сообщения.</returns>
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam);
    }
}
