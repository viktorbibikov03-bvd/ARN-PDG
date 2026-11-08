using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using VrdoPdg.View.Theme;

namespace VrdoPdg.View.Controls
{
    /// <summary>
    /// Кнопка со скругленными углами: заливка, рамка, значок и текст с отдельным оформлением
    /// при наведении указателя, при нажатии, в отмеченном и в недоступном состояниях.
    /// Отмеченное состояние используется для переключателей и задается программно.
    /// Кнопка рисуется средствами GDI+ и не требует сторонних библиотек.
    /// </summary>
    internal sealed class RoundedButton : Control, IButtonControl
    {
        /// <summary>
        /// Радиус скругления углов по умолчанию, пикселей.
        /// </summary>
        private const int DefaultBorderRadius = 8;

        /// <summary>
        /// Расстояние между значком и текстом, пикселей.
        /// </summary>
        private const int ImageSpacing = 8;

        /// <summary>
        /// Доля затемнения заливки нажатой кнопки, если оформление нажатия не задано.
        /// </summary>
        private const float PressedShade = 0.12f;

        /// <summary>
        /// Непрозрачность рамки фокуса ввода, выводимой цветом текста кнопки.
        /// </summary>
        private const int FocusFrameAlpha = 170;

        /// <summary>
        /// Параметры вывода текста кнопки.
        /// </summary>
        private const TextFormatFlags TextFlags = TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter
            | TextFormatFlags.Left | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding
            | TextFormatFlags.EndEllipsis;

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
        /// Значок.
        /// </summary>
        private Image? _image;

        /// <summary>
        /// Выравнивание значка и текста по горизонтали.
        /// </summary>
        private HorizontalAlignment _textAlign = HorizontalAlignment.Center;

        /// <summary>
        /// Признак отмеченного состояния.
        /// </summary>
        private bool _checked;

        /// <summary>
        /// Результат диалога, устанавливаемый окну при нажатии.
        /// </summary>
        private DialogResult _dialogResult = DialogResult.None;

        /// <summary>
        /// Признак нахождения указателя мыши над кнопкой.
        /// </summary>
        private bool _isHovered;

        /// <summary>
        /// Признак нажатия кнопки мышью.
        /// </summary>
        private bool _isMousePressed;

        /// <summary>
        /// Признак нажатия кнопки клавишей «Пробел».
        /// </summary>
        private bool _isKeyPressed;

        /// <summary>
        /// Создает кнопку.
        /// </summary>
        public RoundedButton()
        {
            SetStyle(
                ControlStyles.UserPaint
                    | ControlStyles.UserMouse
                    | ControlStyles.AllPaintingInWmPaint
                    | ControlStyles.OptimizedDoubleBuffer
                    | ControlStyles.ResizeRedraw
                    | ControlStyles.SupportsTransparentBackColor
                    | ControlStyles.StandardClick,
                true);
            SetStyle(ControlStyles.StandardDoubleClick, false);
            BackColor = Color.Transparent;
            AccessibleRole = AccessibleRole.PushButton;
            HoverState = CreateStateStyle();
            PressedState = CreateStateStyle();
            CheckedState = CreateStateStyle();
            DisabledState = CreateStateStyle();
        }

        /// <summary>
        /// Возникает при изменении отмеченного состояния.
        /// </summary>
        public event EventHandler? CheckedChanged;

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
        /// Признак подбора ширины кнопки по значку и тексту; высота кнопки не меняется.
        /// </summary>
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue(false)]
        public override bool AutoSize
        {
            get
            {
                return base.AutoSize;
            }

            set
            {
                base.AutoSize = value;
                AdjustWidth();
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
                Invalidate();
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
                Invalidate();
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
        /// Значок слева от текста; без текста значок выводится по центру.
        /// </summary>
        [Category("Appearance")]
        [Description("Значок слева от текста; без текста значок выводится по центру.")]
        [DefaultValue(null)]
        public Image? Image
        {
            get
            {
                return _image;
            }

            set
            {
                _image = value;
                AdjustWidth();
                Invalidate();
            }
        }

        /// <summary>
        /// Выравнивание значка и текста по горизонтали.
        /// </summary>
        [Category("Appearance")]
        [Description("Выравнивание значка и текста по горизонтали.")]
        [DefaultValue(HorizontalAlignment.Center)]
        public HorizontalAlignment TextAlign
        {
            get
            {
                return _textAlign;
            }

            set
            {
                _textAlign = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Признак отмеченного состояния переключателя.
        /// </summary>
        [Category("Behavior")]
        [Description("Признак отмеченного состояния переключателя.")]
        [DefaultValue(false)]
        public bool Checked
        {
            get
            {
                return _checked;
            }

            set
            {
                if (_checked == value)
                {
                    return;
                }

                _checked = value;
                Invalidate();
                CheckedChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Оформление при наведении указателя мыши.
        /// </summary>
        [Category("Appearance")]
        [Description("Оформление при наведении указателя мыши.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ButtonStateStyle HoverState { get; }

        /// <summary>
        /// Оформление при нажатии; если заливка не задана, затемняется заливка при наведении.
        /// </summary>
        [Category("Appearance")]
        [Description("Оформление при нажатии.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ButtonStateStyle PressedState { get; }

        /// <summary>
        /// Оформление в отмеченном состоянии.
        /// </summary>
        [Category("Appearance")]
        [Description("Оформление в отмеченном состоянии.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ButtonStateStyle CheckedState { get; }

        /// <summary>
        /// Оформление недоступной кнопки.
        /// </summary>
        [Category("Appearance")]
        [Description("Оформление недоступной кнопки.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ButtonStateStyle DisabledState { get; }

        /// <summary>
        /// Результат диалога, устанавливаемый окну при нажатии кнопки.
        /// </summary>
        [Category("Behavior")]
        [Description("Результат диалога, устанавливаемый окну при нажатии кнопки.")]
        [DefaultValue(DialogResult.None)]
        public DialogResult DialogResult
        {
            get
            {
                return _dialogResult;
            }

            set
            {
                _dialogResult = value;
            }
        }

        /// <summary>
        /// Отступы содержимого по умолчанию.
        /// </summary>
        protected override Padding DefaultPadding => new Padding(14, 0, 14, 0);

        /// <summary>
        /// Размер кнопки по умолчанию.
        /// </summary>
        protected override Size DefaultSize => new Size(120, 40);

        /// <summary>
        /// Возвращает размер кнопки, вмещающий значок и текст, при текущей высоте кнопки.
        /// </summary>
        /// <param name="proposedSize">Предлагаемый размер.</param>
        /// <returns>Предпочтительный размер.</returns>
        public override Size GetPreferredSize(Size proposedSize)
        {
            string text = Text;
            int width = Padding.Horizontal;
            if (text.Length > 0)
            {
                width += TextRenderer.MeasureText(text, Font, new Size(int.MaxValue, int.MaxValue), TextFlags).Width;
            }

            if (_image != null)
            {
                width += _image.Width;
                if (text.Length > 0)
                {
                    width += LogicalToDeviceUnits(ImageSpacing);
                }
            }

            return new Size(width, Height);
        }

        /// <summary>
        /// Сообщает кнопке, что она стала кнопкой окна по умолчанию. Оформление при этом не меняется.
        /// </summary>
        /// <param name="value">Признак кнопки по умолчанию.</param>
        public void NotifyDefault(bool value)
        {
        }

        /// <summary>
        /// Нажимает кнопку программно.
        /// </summary>
        public void PerformClick()
        {
            if (Enabled && Visible)
            {
                OnClick(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Устанавливает окну результат диалога и вызывает событие нажатия.
        /// </summary>
        /// <param name="e">Аргументы события.</param>
        protected override void OnClick(EventArgs e)
        {
            Form? form = FindForm();
            if (form != null && _dialogResult != DialogResult.None)
            {
                form.DialogResult = _dialogResult;
            }

            base.OnClick(e);
        }

        /// <summary>
        /// Запрещает получение фокуса кнопке, исключенной из обхода по клавише Tab.
        /// </summary>
        /// <param name="e">Аргументы события.</param>
        protected override void OnTabStopChanged(EventArgs e)
        {
            SetStyle(ControlStyles.Selectable, TabStop);
            base.OnTabStopChanged(e);
        }

        /// <summary>
        /// Перерисовывает кнопку при изменении текста.
        /// </summary>
        /// <param name="e">Аргументы события.</param>
        protected override void OnTextChanged(EventArgs e)
        {
            AdjustWidth();
            Invalidate();
            base.OnTextChanged(e);
        }

        /// <summary>
        /// Подбирает ширину кнопки при изменении шрифта.
        /// </summary>
        /// <param name="e">Аргументы события.</param>
        protected override void OnFontChanged(EventArgs e)
        {
            AdjustWidth();
            base.OnFontChanged(e);
        }

        /// <summary>
        /// Подбирает ширину кнопки при изменении отступов содержимого.
        /// </summary>
        /// <param name="e">Аргументы события.</param>
        protected override void OnPaddingChanged(EventArgs e)
        {
            AdjustWidth();
            base.OnPaddingChanged(e);
        }

        /// <summary>
        /// Сбрасывает наведение и нажатие при изменении доступности кнопки.
        /// </summary>
        /// <param name="e">Аргументы события.</param>
        protected override void OnEnabledChanged(EventArgs e)
        {
            _isHovered = false;
            _isMousePressed = false;
            _isKeyPressed = false;
            Invalidate();
            base.OnEnabledChanged(e);
        }

        /// <summary>
        /// Перерисовывает кнопку при получении фокуса.
        /// </summary>
        /// <param name="e">Аргументы события.</param>
        protected override void OnGotFocus(EventArgs e)
        {
            Invalidate();
            base.OnGotFocus(e);
        }

        /// <summary>
        /// Сбрасывает нажатие клавишей и перерисовывает кнопку при потере фокуса.
        /// </summary>
        /// <param name="e">Аргументы события.</param>
        protected override void OnLostFocus(EventArgs e)
        {
            _isKeyPressed = false;
            Invalidate();
            base.OnLostFocus(e);
        }

        /// <summary>
        /// Отмечает наведение указателя мыши.
        /// </summary>
        /// <param name="e">Аргументы события.</param>
        protected override void OnMouseEnter(EventArgs e)
        {
            _isHovered = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        /// <summary>
        /// Снимает отметку наведения указателя мыши.
        /// </summary>
        /// <param name="e">Аргументы события.</param>
        protected override void OnMouseLeave(EventArgs e)
        {
            _isHovered = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        /// <summary>
        /// Отмечает нажатие кнопки мышью.
        /// </summary>
        /// <param name="e">Аргументы события.</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isMousePressed = true;
                Invalidate();
            }

            base.OnMouseDown(e);
        }

        /// <summary>
        /// Снимает отметку нажатия кнопки мышью.
        /// </summary>
        /// <param name="e">Аргументы события.</param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isMousePressed = false;
                Invalidate();
            }

            base.OnMouseUp(e);
        }

        /// <summary>
        /// Отмечает нажатие кнопки клавишей «Пробел».
        /// </summary>
        /// <param name="e">Аргументы события.</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space && !e.Alt && !e.Control)
            {
                _isKeyPressed = true;
                Invalidate();
                e.Handled = true;
            }

            base.OnKeyDown(e);
        }

        /// <summary>
        /// Нажимает кнопку при отпускании клавиши «Пробел».
        /// </summary>
        /// <param name="e">Аргументы события.</param>
        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space && _isKeyPressed)
            {
                _isKeyPressed = false;
                Invalidate();
                PerformClick();
                e.Handled = true;
            }

            base.OnKeyUp(e);
        }

        /// <summary>
        /// Рисует кнопку: заливку, рамку, значок, текст и рамку фокуса ввода.
        /// Фон под скругленными углами рисуется до вызова метода с учетом прозрачности.
        /// </summary>
        /// <param name="e">Аргументы события.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            Color fillColor;
            Color foreColor;
            Color borderColor;
            Image? image;
            ResolveAppearance(out fillColor, out foreColor, out borderColor, out image);

            Graphics graphics = e.Graphics;
            SmoothingMode smoothingMode = graphics.SmoothingMode;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            float radius = LogicalToDeviceUnits(_borderRadius);

            if (fillColor.A > 0)
            {
                using (GraphicsPath path = UiDrawing.CreateRoundedRectangle(UiDrawing.GetFillBounds(Size), radius))
                using (SolidBrush brush = new SolidBrush(fillColor))
                {
                    graphics.FillPath(brush, path);
                }
            }

            if (borderColor.A > 0 && _borderThickness > 0)
            {
                DrawFrame(graphics, borderColor, _borderThickness, 0f, radius);
            }

            if (Focused && ShowFocusCues)
            {
                DrawFrame(graphics, Color.FromArgb(FocusFrameAlpha, foreColor), 1, 2f, radius - 2f);
            }

            graphics.SmoothingMode = smoothingMode;
            DrawContent(graphics, foreColor, image);
            base.OnPaint(e);
        }

        /// <summary>
        /// Устанавливает ширину кнопки по значку и тексту, если включен подбор размера.
        /// </summary>
        private void AdjustWidth()
        {
            if (AutoSize)
            {
                Width = GetPreferredSize(Size.Empty).Width;
            }
        }

        /// <summary>
        /// Создает оформление состояния, при изменении которого кнопка перерисовывается.
        /// </summary>
        /// <returns>Оформление состояния.</returns>
        private ButtonStateStyle CreateStateStyle()
        {
            ButtonStateStyle style = new ButtonStateStyle();
            style.Changed += StateStyle_Changed;
            return style;
        }

        /// <summary>
        /// Определяет цвета и значок кнопки по ее текущему состоянию.
        /// </summary>
        /// <param name="fillColor">Цвет заливки.</param>
        /// <param name="foreColor">Цвет текста.</param>
        /// <param name="borderColor">Цвет рамки.</param>
        /// <param name="image">Значок.</param>
        private void ResolveAppearance(
            out Color fillColor,
            out Color foreColor,
            out Color borderColor,
            out Image? image)
        {
            fillColor = _fillColor;
            foreColor = ForeColor;
            borderColor = _borderColor;
            image = _image;

            if (!Enabled)
            {
                ApplyStateStyle(DisabledState, ref fillColor, ref foreColor, ref borderColor, ref image);
                return;
            }

            if (_checked)
            {
                ApplyStateStyle(CheckedState, ref fillColor, ref foreColor, ref borderColor, ref image);
                return;
            }

            bool isPressed = _isKeyPressed || (_isMousePressed && _isHovered);
            if (_isHovered || isPressed)
            {
                ApplyStateStyle(HoverState, ref fillColor, ref foreColor, ref borderColor, ref image);
            }

            if (isPressed)
            {
                if (PressedState.FillColor.IsEmpty)
                {
                    fillColor = UiDrawing.Blend(fillColor, Color.Black, PressedShade);
                }

                ApplyStateStyle(PressedState, ref fillColor, ref foreColor, ref borderColor, ref image);
            }
        }

        /// <summary>
        /// Заменяет цвета и значок заданными в оформлении состояния.
        /// </summary>
        /// <param name="style">Оформление состояния.</param>
        /// <param name="fillColor">Цвет заливки.</param>
        /// <param name="foreColor">Цвет текста.</param>
        /// <param name="borderColor">Цвет рамки.</param>
        /// <param name="image">Значок.</param>
        private static void ApplyStateStyle(
            ButtonStateStyle style,
            ref Color fillColor,
            ref Color foreColor,
            ref Color borderColor,
            ref Image? image)
        {
            if (!style.FillColor.IsEmpty)
            {
                fillColor = style.FillColor;
            }

            if (!style.ForeColor.IsEmpty)
            {
                foreColor = style.ForeColor;
            }

            if (!style.BorderColor.IsEmpty)
            {
                borderColor = style.BorderColor;
            }

            if (style.Image != null)
            {
                image = style.Image;
            }
        }

        /// <summary>
        /// Рисует рамку со скругленными углами внутри границ кнопки.
        /// </summary>
        /// <param name="graphics">Поверхность рисования.</param>
        /// <param name="color">Цвет рамки.</param>
        /// <param name="thickness">Толщина рамки, пикселей.</param>
        /// <param name="inset">Отступ рамки от края кнопки, пикселей.</param>
        /// <param name="radius">Радиус скругления по внешнему краю рамки.</param>
        private void DrawFrame(Graphics graphics, Color color, float thickness, float inset, float radius)
        {
            RectangleF bounds = UiDrawing.GetStrokeBounds(Size, thickness, inset);
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            float pathRadius = Math.Max(0f, radius - (thickness / 2f));
            using (GraphicsPath path = UiDrawing.CreateRoundedRectangle(bounds, pathRadius))
            using (Pen pen = new Pen(color, thickness))
            {
                graphics.DrawPath(pen, path);
            }
        }

        /// <summary>
        /// Рисует значок и текст кнопки.
        /// </summary>
        /// <param name="graphics">Поверхность рисования.</param>
        /// <param name="foreColor">Цвет текста.</param>
        /// <param name="image">Значок.</param>
        private void DrawContent(Graphics graphics, Color foreColor, Image? image)
        {
            string text = Text;
            if (text.Length == 0)
            {
                if (image != null)
                {
                    Rectangle imageBounds = new Rectangle(
                        (Width - image.Width) / 2,
                        (Height - image.Height) / 2,
                        image.Width,
                        image.Height);
                    graphics.DrawImage(image, imageBounds);
                }

                return;
            }

            Rectangle area = new Rectangle(
                Padding.Left,
                Padding.Top,
                Math.Max(0, Width - Padding.Horizontal),
                Math.Max(0, Height - Padding.Vertical));
            Size textSize = TextRenderer.MeasureText(
                graphics,
                text,
                Font,
                new Size(int.MaxValue, int.MaxValue),
                TextFlags);
            int imageWidth = image == null
                ? 0
                : image.Width + LogicalToDeviceUnits(ImageSpacing);
            int contentWidth = Math.Min(area.Width, imageWidth + textSize.Width);

            int left = area.Left;
            if (_textAlign == HorizontalAlignment.Center)
            {
                left += (area.Width - contentWidth) / 2;
            }
            else if (_textAlign == HorizontalAlignment.Right)
            {
                left = area.Right - contentWidth;
            }

            if (image != null)
            {
                graphics.DrawImage(
                    image,
                    new Rectangle(left, (Height - image.Height) / 2, image.Width, image.Height));
                left += imageWidth;
            }

            Rectangle textBounds = new Rectangle(left, area.Top, Math.Max(0, area.Right - left), area.Height);
            TextRenderer.DrawText(graphics, text, Font, textBounds, foreColor, TextFlags);
        }

        /// <summary>
        /// Перерисовывает кнопку при изменении оформления состояния.
        /// </summary>
        /// <param name="sender">Оформление состояния.</param>
        /// <param name="e">Аргументы события.</param>
        private void StateStyle_Changed(object? sender, EventArgs e)
        {
            Invalidate();
        }
    }
}
