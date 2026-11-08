using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;

using VrdoPdg.View.Theme;

namespace VrdoPdg.View.Controls
{
    /// <summary>
    /// Поле поиска со скругленной рамкой, значком и подсказкой в пустом поле.
    /// Рамка меняет цвет при наведении указателя и при вводе текста.
    /// </summary>
    internal sealed class SearchBox : RoundedPanel
    {
        /// <summary>
        /// Отступ значка и текста от краев поля, пикселей.
        /// </summary>
        private const int HorizontalPadding = 12;

        /// <summary>
        /// Расстояние между значком и текстом, пикселей.
        /// </summary>
        private const int ImageSpacing = 8;

        /// <summary>
        /// Поле ввода текста.
        /// </summary>
        private readonly TextBox _textBox;

        /// <summary>
        /// Значок слева от текста.
        /// </summary>
        private Image? _image;

        /// <summary>
        /// Цвет рамки при наведении указателя.
        /// </summary>
        private Color _hoverBorderColor = Color.Empty;

        /// <summary>
        /// Цвет рамки при вводе текста.
        /// </summary>
        private Color _focusedBorderColor = Color.Empty;

        /// <summary>
        /// Признак нахождения указателя мыши над полем.
        /// </summary>
        private bool _isHovered;

        /// <summary>
        /// Создает поле поиска.
        /// </summary>
        public SearchBox()
        {
            _textBox = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Location = new Point(HorizontalPadding, 0)
            };
            _textBox.TextChanged += TextBox_TextChanged;
            _textBox.Enter += TextBox_FocusChanged;
            _textBox.Leave += TextBox_FocusChanged;
            _textBox.MouseEnter += Part_MouseHoverChanged;
            _textBox.MouseLeave += Part_MouseHoverChanged;
            Controls.Add(_textBox);

            BorderRadius = 10;
            BorderThickness = 1;
            FillColor = Palette.Surface;
            BorderColor = Palette.Border;
            Cursor = Cursors.IBeam;
        }

        /// <summary>
        /// Возникает при изменении текста поля.
        /// </summary>
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public new event EventHandler? TextChanged
        {
            add
            {
                base.TextChanged += value;
            }

            remove
            {
                base.TextChanged -= value;
            }
        }

        /// <summary>
        /// Текст поля.
        /// </summary>
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [AllowNull]
        public override string Text
        {
            get
            {
                return _textBox.Text;
            }

            set
            {
                _textBox.Text = value;
            }
        }

        /// <summary>
        /// Подсказка, выводимая в пустом поле.
        /// </summary>
        [Category("Appearance")]
        [Description("Подсказка, выводимая в пустом поле.")]
        [DefaultValue("")]
        [AllowNull]
        public string PlaceholderText
        {
            get
            {
                return _textBox.PlaceholderText;
            }

            set
            {
                _textBox.PlaceholderText = value;
            }
        }

        /// <summary>
        /// Значок слева от текста.
        /// </summary>
        [Category("Appearance")]
        [Description("Значок слева от текста.")]
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
                PerformLayout();
                Invalidate();
            }
        }

        /// <summary>
        /// Цвет рамки при наведении указателя; пустой цвет — цвет обычного состояния.
        /// </summary>
        [Category("Appearance")]
        [Description("Цвет рамки при наведении указателя.")]
        [DefaultValue(typeof(Color), "")]
        public Color HoverBorderColor
        {
            get
            {
                return _hoverBorderColor;
            }

            set
            {
                _hoverBorderColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Цвет рамки при вводе текста; пустой цвет — цвет обычного состояния.
        /// </summary>
        [Category("Appearance")]
        [Description("Цвет рамки при вводе текста.")]
        [DefaultValue(typeof(Color), "")]
        public Color FocusedBorderColor
        {
            get
            {
                return _focusedBorderColor;
            }

            set
            {
                _focusedBorderColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Возвращает цвет рамки с учетом ввода текста и наведения указателя.
        /// </summary>
        /// <returns>Цвет рамки.</returns>
        protected override Color GetCurrentBorderColor()
        {
            if (_textBox.Focused && !_focusedBorderColor.IsEmpty)
            {
                return _focusedBorderColor;
            }

            if (_isHovered && !_hoverBorderColor.IsEmpty)
            {
                return _hoverBorderColor;
            }

            return base.GetCurrentBorderColor();
        }

        /// <summary>
        /// Передает цвет заливки полю ввода, которое не поддерживает прозрачный фон.
        /// </summary>
        /// <param name="e">Аргументы события.</param>
        protected override void OnFillColorChanged(EventArgs e)
        {
            _textBox.BackColor = FillColor.A == byte.MaxValue
                ? FillColor
                : Palette.Surface;
            base.OnFillColorChanged(e);
        }

        /// <summary>
        /// Размещает поле ввода по центру по вертикали правее значка.
        /// </summary>
        /// <param name="levent">Аргументы события.</param>
        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            int left = LogicalToDeviceUnits(HorizontalPadding);
            if (_image != null)
            {
                left += _image.Width + LogicalToDeviceUnits(ImageSpacing);
            }

            int width = Math.Max(0, Width - left - LogicalToDeviceUnits(HorizontalPadding));
            _textBox.SetBounds(left, (Height - _textBox.Height) / 2, width, _textBox.Height);
        }

        /// <summary>
        /// Пересчитывает размещение поля ввода при изменении шрифта.
        /// </summary>
        /// <param name="e">Аргументы события.</param>
        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            PerformLayout();
        }

        /// <summary>
        /// Рисует значок слева от текста.
        /// </summary>
        /// <param name="e">Аргументы события.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (_image == null)
            {
                return;
            }

            Rectangle bounds = new Rectangle(
                LogicalToDeviceUnits(HorizontalPadding),
                (Height - _image.Height) / 2,
                _image.Width,
                _image.Height);
            e.Graphics.DrawImage(_image, bounds);
        }

        /// <summary>
        /// Переводит ввод в поле при щелчке по рамке или значку.
        /// </summary>
        /// <param name="e">Аргументы события.</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            _textBox.Focus();
        }

        /// <summary>
        /// Отмечает наведение указателя мыши.
        /// </summary>
        /// <param name="e">Аргументы события.</param>
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            UpdateHover();
        }

        /// <summary>
        /// Снимает отметку наведения указателя мыши, если указатель покинул поле.
        /// </summary>
        /// <param name="e">Аргументы события.</param>
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            UpdateHover();
        }

        /// <summary>
        /// Определяет, находится ли указатель мыши над полем, и перерисовывает рамку.
        /// </summary>
        private void UpdateHover()
        {
            bool isHovered = ClientRectangle.Contains(PointToClient(MousePosition));
            if (_isHovered != isHovered)
            {
                _isHovered = isHovered;
                Invalidate();
            }
        }

        /// <summary>
        /// Передает изменение текста подписчикам поля поиска.
        /// </summary>
        /// <param name="sender">Поле ввода.</param>
        /// <param name="e">Аргументы события.</param>
        private void TextBox_TextChanged(object? sender, EventArgs e)
        {
            OnTextChanged(e);
        }

        /// <summary>
        /// Перерисовывает рамку при начале и окончании ввода текста.
        /// </summary>
        /// <param name="sender">Поле ввода.</param>
        /// <param name="e">Аргументы события.</param>
        private void TextBox_FocusChanged(object? sender, EventArgs e)
        {
            Invalidate();
        }

        /// <summary>
        /// Обновляет признак наведения при перемещении указателя над полем ввода.
        /// </summary>
        /// <param name="sender">Поле ввода.</param>
        /// <param name="e">Аргументы события.</param>
        private void Part_MouseHoverChanged(object? sender, EventArgs e)
        {
            UpdateHover();
        }
    }
}
