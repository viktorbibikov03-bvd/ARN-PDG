using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using VrdoPdg.View.Theme;

namespace VrdoPdg.View.Controls
{
    /// <summary>
    /// Панель со скругленными углами, заливкой и рамкой. Фон панели прозрачен,
    /// поэтому под скругленными углами виден фон родителя, а вложенные элементы
    /// с прозрачным фоном выводятся поверх заливки панели.
    /// </summary>
    internal class RoundedPanel : Panel
    {
        /// <summary>
        /// Радиус скругления углов по умолчанию, пикселей.
        /// </summary>
        private const int DefaultBorderRadius = 12;

        /// <summary>
        /// Радиус скругления углов, пикселей.
        /// </summary>
        private int _borderRadius = DefaultBorderRadius;

        /// <summary>
        /// Цвет заливки.
        /// </summary>
        private Color _fillColor = Color.Transparent;

        /// <summary>
        /// Цвет рамки.
        /// </summary>
        private Color _borderColor = Color.Transparent;

        /// <summary>
        /// Толщина рамки, пикселей.
        /// </summary>
        private int _borderThickness;

        /// <summary>
        /// Стиль линии рамки.
        /// </summary>
        private DashStyle _borderDashStyle = DashStyle.Solid;

        /// <summary>
        /// Создает панель.
        /// </summary>
        public RoundedPanel()
        {
            SetStyle(
                ControlStyles.UserPaint
                    | ControlStyles.AllPaintingInWmPaint
                    | ControlStyles.OptimizedDoubleBuffer
                    | ControlStyles.ResizeRedraw
                    | ControlStyles.SupportsTransparentBackColor,
                true);
            BackColor = Color.Transparent;
        }

        /// <summary>
        /// Цвет фона под скругленными углами; по умолчанию прозрачный — виден фон родителя.
        /// </summary>
        [DefaultValue(typeof(Color), "Transparent")]
        public override Color BackColor
        {
            get
            {
                return base.BackColor;
            }

            set
            {
                base.BackColor = value;
            }
        }

        /// <summary>
        /// Радиус скругления углов, пикселей.
        /// </summary>
        [Category("Appearance")]
        [Description("Радиус скругления углов, пикселей.")]
        [DefaultValue(DefaultBorderRadius)]
        public int BorderRadius
        {
            get
            {
                return _borderRadius;
            }

            set
            {
                _borderRadius = Math.Max(0, value);
                Invalidate(true);
            }
        }

        /// <summary>
        /// Цвет заливки.
        /// </summary>
        [Category("Appearance")]
        [Description("Цвет заливки.")]
        [DefaultValue(typeof(Color), "Transparent")]
        public Color FillColor
        {
            get
            {
                return _fillColor;
            }

            set
            {
                _fillColor = value;
                OnFillColorChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Цвет рамки.
        /// </summary>
        [Category("Appearance")]
        [Description("Цвет рамки.")]
        [DefaultValue(typeof(Color), "Transparent")]
        public Color BorderColor
        {
            get
            {
                return _borderColor;
            }

            set
            {
                _borderColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Толщина рамки, пикселей; 0 — без рамки.
        /// </summary>
        [Category("Appearance")]
        [Description("Толщина рамки, пикселей; 0 — без рамки.")]
        [DefaultValue(0)]
        public int BorderThickness
        {
            get
            {
                return _borderThickness;
            }

            set
            {
                _borderThickness = Math.Max(0, value);
                Invalidate();
            }
        }

        /// <summary>
        /// Стиль линии рамки: сплошная, пунктирная.
        /// </summary>
        [Category("Appearance")]
        [Description("Стиль линии рамки.")]
        [DefaultValue(DashStyle.Solid)]
        public DashStyle BorderDashStyle
        {
            get
            {
                return _borderDashStyle;
            }

            set
            {
                _borderDashStyle = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Возвращает цвет рамки для текущего состояния панели.
        /// </summary>
        /// <returns>Цвет рамки.</returns>
        protected virtual Color GetCurrentBorderColor()
        {
            return _borderColor;
        }

        /// <summary>
        /// Перерисовывает панель и вложенные элементы при изменении цвета заливки.
        /// </summary>
        /// <param name="e">Аргументы события.</param>
        protected virtual void OnFillColorChanged(EventArgs e)
        {
            Invalidate(true);
        }

        /// <summary>
        /// Рисует фон родителя под скругленными углами, заливку и рамку панели.
        /// </summary>
        /// <param name="e">Аргументы события.</param>
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);

            Graphics graphics = e.Graphics;
            SmoothingMode smoothingMode = graphics.SmoothingMode;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            float radius = LogicalToDeviceUnits(_borderRadius);

            if (_fillColor.A > 0)
            {
                using (GraphicsPath path = UiDrawing.CreateRoundedRectangle(UiDrawing.GetFillBounds(Size), radius))
                using (SolidBrush brush = new SolidBrush(_fillColor))
                {
                    graphics.FillPath(brush, path);
                }
            }

            Color borderColor = GetCurrentBorderColor();
            if (borderColor.A > 0 && _borderThickness > 0)
            {
                RectangleF bounds = UiDrawing.GetStrokeBounds(Size, _borderThickness, 0f);
                float pathRadius = Math.Max(0f, radius - (_borderThickness / 2f));
                using (GraphicsPath path = UiDrawing.CreateRoundedRectangle(bounds, pathRadius))
                using (Pen pen = new Pen(borderColor, _borderThickness))
                {
                    pen.DashStyle = _borderDashStyle;
                    graphics.DrawPath(pen, path);
                }
            }

            graphics.SmoothingMode = smoothingMode;
        }
    }
}
