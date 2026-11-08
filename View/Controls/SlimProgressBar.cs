using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using VrdoPdg.View.Theme;

namespace VrdoPdg.View.Controls
{
    /// <summary>
    /// Тонкая полоса хода расчета со скругленными краями, выводимая по центру элемента по вертикали.
    /// </summary>
    internal sealed class SlimProgressBar : Control
    {
        /// <summary>
        /// Максимальное значение по умолчанию.
        /// </summary>
        private const int DefaultMaximum = 100;

        /// <summary>
        /// Толщина полосы по умолчанию, пикселей.
        /// </summary>
        private const int DefaultBarHeight = 6;

        /// <summary>
        /// Текущее значение.
        /// </summary>
        private int _value;

        /// <summary>
        /// Максимальное значение.
        /// </summary>
        private int _maximum = DefaultMaximum;

        /// <summary>
        /// Толщина полосы, пикселей.
        /// </summary>
        private int _barHeight = DefaultBarHeight;

        /// <summary>
        /// Цвет незаполненной части полосы.
        /// </summary>
        private Color _trackColor = Palette.SurfaceRaised;

        /// <summary>
        /// Цвет заполненной части полосы.
        /// </summary>
        private Color _progressColor = Palette.Purple;

        /// <summary>
        /// Создает полосу хода расчета.
        /// </summary>
        public SlimProgressBar()
        {
            SetStyle(
                ControlStyles.UserPaint
                    | ControlStyles.AllPaintingInWmPaint
                    | ControlStyles.OptimizedDoubleBuffer
                    | ControlStyles.ResizeRedraw,
                true);
            SetStyle(ControlStyles.Selectable, false);
            TabStop = false;
            AccessibleRole = AccessibleRole.ProgressBar;
        }

        /// <summary>
        /// Текущее значение от 0 до <see cref="Maximum"/>.
        /// </summary>
        [Category("Behavior")]
        [Description("Текущее значение от 0 до максимального.")]
        [DefaultValue(0)]
        public int Value
        {
            get
            {
                return _value;
            }

            set
            {
                int clamped = Math.Max(0, Math.Min(_maximum, value));
                if (_value != clamped)
                {
                    _value = clamped;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Максимальное значение.
        /// </summary>
        [Category("Behavior")]
        [Description("Максимальное значение.")]
        [DefaultValue(DefaultMaximum)]
        public int Maximum
        {
            get
            {
                return _maximum;
            }

            set
            {
                _maximum = Math.Max(1, value);
                _value = Math.Min(_value, _maximum);
                Invalidate();
            }
        }

        /// <summary>
        /// Толщина полосы, пикселей.
        /// </summary>
        [Category("Appearance")]
        [Description("Толщина полосы, пикселей.")]
        [DefaultValue(DefaultBarHeight)]
        public int BarHeight
        {
            get
            {
                return _barHeight;
            }

            set
            {
                _barHeight = Math.Max(1, value);
                Invalidate();
            }
        }

        /// <summary>
        /// Цвет незаполненной части полосы.
        /// </summary>
        [Category("Appearance")]
        [Description("Цвет незаполненной части полосы.")]
        public Color TrackColor
        {
            get
            {
                return _trackColor;
            }

            set
            {
                _trackColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Цвет заполненной части полосы.
        /// </summary>
        [Category("Appearance")]
        [Description("Цвет заполненной части полосы.")]
        public Color ProgressColor
        {
            get
            {
                return _progressColor;
            }

            set
            {
                _progressColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Рисует незаполненную и заполненную части полосы.
        /// </summary>
        /// <param name="e">Аргументы события.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics graphics = e.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            float height = Math.Min(Height, LogicalToDeviceUnits(_barHeight));
            RectangleF track = new RectangleF(-0.5f, ((Height - height) / 2f) - 0.5f, Width, height);
            FillBar(graphics, _trackColor, track);

            if (_value > 0)
            {
                float width = Math.Max(height, Width * (float)_value / _maximum);
                FillBar(graphics, _progressColor, new RectangleF(track.X, track.Y, Math.Min(Width, width), height));
            }

            base.OnPaint(e);
        }

        /// <summary>
        /// Заливает участок полосы со скругленными краями.
        /// </summary>
        /// <param name="graphics">Поверхность рисования.</param>
        /// <param name="color">Цвет заливки.</param>
        /// <param name="bounds">Участок полосы.</param>
        private static void FillBar(Graphics graphics, Color color, RectangleF bounds)
        {
            using (GraphicsPath path = UiDrawing.CreateRoundedRectangle(bounds, bounds.Height / 2f))
            using (SolidBrush brush = new SolidBrush(color))
            {
                graphics.FillPath(brush, path);
            }
        }
    }
}
