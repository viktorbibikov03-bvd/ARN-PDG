using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using VrdoPdg.View.Theme;

namespace VrdoPdg.View
{
    /// <summary>
    /// Окно без системной рамки со скругленными углами и тенью: «Загрузка данных»,
    /// «Таблица эффективности» и окно подтверждения. В Windows 11 углы скругляет система,
    /// в Windows 10 окно обрезается по контуру со скругленными углами и обводится рамкой.
    /// Окно перетаскивается за свободную область и за элементы, переданные методу
    /// <see cref="EnableWindowDrag"/>.
    /// </summary>
    internal class BorderlessForm : Form
    {
        /// <summary>
        /// Стиль класса окна CS_DROPSHADOW: тень под окном.
        /// </summary>
        private const int DropShadowClassStyle = 0x00020000;

        /// <summary>
        /// Радиус скругления углов по умолчанию, пикселей.
        /// </summary>
        private const int DefaultCornerRadius = 18;

        /// <summary>
        /// Радиус скругления углов в Windows 10, пикселей.
        /// </summary>
        private int _cornerRadius = DefaultCornerRadius;

        /// <summary>
        /// Признак скругления углов средствами системы (Windows 11).
        /// </summary>
        private bool _hasSystemCorners;

        /// <summary>
        /// Создает окно без рамки.
        /// </summary>
        public BorderlessForm()
        {
            FormBorderStyle = FormBorderStyle.None;
            MaximizeBox = false;
            MinimizeBox = false;
            ResizeRedraw = true;
        }

        /// <summary>
        /// Радиус скругления углов в Windows 10, пикселей. В Windows 11 углы скругляет система.
        /// </summary>
        [Category("Appearance")]
        [Description("Радиус скругления углов в Windows 10, пикселей. В Windows 11 углы скругляет система.")]
        [DefaultValue(DefaultCornerRadius)]
        public int CornerRadius
        {
            get
            {
                return _cornerRadius;
            }

            set
            {
                _cornerRadius = Math.Max(0, value);
                UpdateRegion();
                Invalidate();
            }
        }

        /// <summary>
        /// Параметры создания окна: добавляется тень под окном.
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams parameters = base.CreateParams;
                parameters.ClassStyle |= DropShadowClassStyle;
                return parameters;
            }
        }

        /// <summary>
        /// Делает элементы управления областью перетаскивания окна: заголовок, пояснения, панели.
        /// </summary>
        /// <param name="controls">Элементы управления.</param>
        protected void EnableWindowDrag(params Control[] controls)
        {
            foreach (Control control in controls)
            {
                control.MouseDown += DragArea_MouseDown;
            }
        }

        /// <summary>
        /// Скругляет углы окна после создания его дескриптора.
        /// </summary>
        /// <param name="e">Аргументы события.</param>
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            _hasSystemCorners = WindowTheme.ApplyRoundedCorners(this, Palette.Border);
            UpdateRegion();
        }

        /// <summary>
        /// Обновляет контур окна при изменении размера.
        /// </summary>
        /// <param name="e">Аргументы события.</param>
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            UpdateRegion();
        }

        /// <summary>
        /// Рисует рамку окна, если углы окна не скругляет система.
        /// </summary>
        /// <param name="e">Аргументы события.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (_hasSystemCorners)
            {
                return;
            }

            Graphics graphics = e.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            RectangleF bounds = UiDrawing.GetStrokeBounds(ClientSize, 1f, 0f);
            float radius = Math.Max(0f, LogicalToDeviceUnits(_cornerRadius) - 0.5f);
            using (GraphicsPath path = UiDrawing.CreateRoundedRectangle(bounds, radius))
            using (Pen pen = new Pen(Palette.Border))
            {
                graphics.DrawPath(pen, path);
            }
        }

        /// <summary>
        /// Обрабатывает сообщения окна: свободная область окна считается заголовком,
        /// за который окно перетаскивается.
        /// </summary>
        /// <param name="m">Сообщение.</param>
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (!DesignMode
                && m.Msg == NativeMethods.WmNcHitTest
                && m.Result == (IntPtr)NativeMethods.HtClient)
            {
                m.Result = (IntPtr)NativeMethods.HtCaption;
            }
        }

        /// <summary>
        /// Обрезает окно по контуру со скругленными углами, если углы окна не скругляет система.
        /// </summary>
        private void UpdateRegion()
        {
            if (!IsHandleCreated || _hasSystemCorners)
            {
                return;
            }

            Region? previous = Region;
            Rectangle bounds = new Rectangle(0, 0, Width, Height);
            using (GraphicsPath path = UiDrawing.CreateRoundedRectangle(bounds, LogicalToDeviceUnits(_cornerRadius)))
            {
                Region = new Region(path);
            }

            previous?.Dispose();
        }

        /// <summary>
        /// Начинает перетаскивание окна при нажатии левой кнопки мыши на области перетаскивания.
        /// </summary>
        /// <param name="sender">Область перетаскивания.</param>
        /// <param name="e">Аргументы события.</param>
        private void DragArea_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            NativeMethods.ReleaseCapture();
            NativeMethods.SendMessage(
                Handle,
                NativeMethods.WmNcLButtonDown,
                (IntPtr)NativeMethods.HtCaption,
                IntPtr.Zero);
        }
    }
}
