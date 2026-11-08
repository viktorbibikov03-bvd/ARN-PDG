using System;
using System.ComponentModel;
using System.Drawing;

namespace VrdoPdg.View.Controls
{
    /// <summary>
    /// Оформление кнопки <see cref="RoundedButton"/> в одном из состояний: при наведении указателя,
    /// при нажатии, в отмеченном и в недоступном состояниях. Незаданные цвета и значок
    /// берутся из обычного состояния кнопки.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    internal sealed class ButtonStateStyle
    {
        /// <summary>
        /// Цвет заливки.
        /// </summary>
        private Color _fillColor = Color.Empty;

        /// <summary>
        /// Цвет текста.
        /// </summary>
        private Color _foreColor = Color.Empty;

        /// <summary>
        /// Цвет рамки.
        /// </summary>
        private Color _borderColor = Color.Empty;

        /// <summary>
        /// Значок.
        /// </summary>
        private Image? _image;

        /// <summary>
        /// Возникает при изменении оформления.
        /// </summary>
        public event EventHandler? Changed;

        /// <summary>
        /// Цвет заливки; пустой цвет — цвет обычного состояния.
        /// </summary>
        [Description("Цвет заливки; пустой цвет — цвет обычного состояния.")]
        [DefaultValue(typeof(Color), "")]
        public Color FillColor
        {
            get
            {
                return _fillColor;
            }

            set
            {
                _fillColor = value;
                OnChanged();
            }
        }

        /// <summary>
        /// Цвет текста; пустой цвет — цвет обычного состояния.
        /// </summary>
        [Description("Цвет текста; пустой цвет — цвет обычного состояния.")]
        [DefaultValue(typeof(Color), "")]
        public Color ForeColor
        {
            get
            {
                return _foreColor;
            }

            set
            {
                _foreColor = value;
                OnChanged();
            }
        }

        /// <summary>
        /// Цвет рамки; пустой цвет — цвет обычного состояния.
        /// </summary>
        [Description("Цвет рамки; пустой цвет — цвет обычного состояния.")]
        [DefaultValue(typeof(Color), "")]
        public Color BorderColor
        {
            get
            {
                return _borderColor;
            }

            set
            {
                _borderColor = value;
                OnChanged();
            }
        }

        /// <summary>
        /// Значок; <c>null</c> — значок обычного состояния.
        /// </summary>
        [Description("Значок; пусто — значок обычного состояния.")]
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
                OnChanged();
            }
        }

        /// <summary>
        /// Возвращает пустую строку, чтобы в окне свойств не выводилось имя типа.
        /// </summary>
        /// <returns>Пустая строка.</returns>
        public override string ToString()
        {
            return string.Empty;
        }

        /// <summary>
        /// Сообщает об изменении оформления.
        /// </summary>
        private void OnChanged()
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }
}
