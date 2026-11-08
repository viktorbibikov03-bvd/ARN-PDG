using System.Drawing;
using System.Windows.Forms;

using Guna.UI2.WinForms;

namespace VrdoPdg.View.Theme
{
    /// <summary>
    /// Применение оформления к элементам управления: кнопкам панели инструментов,
    /// кнопкам основного и второстепенного действия, меткам-индикаторам и таблицам.
    /// </summary>
    internal static class UiStyle
    {
        /// <summary>
        /// Радиус скругления кнопок.
        /// </summary>
        public const int ButtonRadius = 8;

        /// <summary>
        /// Размер значков кнопок, пикселей.
        /// </summary>
        public const int IconSize = 18;

        /// <summary>
        /// Оформляет кнопку без заливки (панель инструментов, «Загрузить», «Справка»).
        /// </summary>
        /// <param name="button">Кнопка.</param>
        /// <param name="glyph">Значок.</param>
        public static void ApplyFlat(Guna2Button button, char? glyph)
        {
            ApplyCommon(button);
            button.FillColor = Color.Transparent;
            button.ForeColor = Palette.Text;
            button.BorderThickness = 0;
            button.HoverState.FillColor = Palette.SurfaceRaised;
            button.HoverState.ForeColor = Palette.Text;
            button.DisabledState.FillColor = Color.Transparent;
            button.DisabledState.ForeColor = Palette.TextMuted;
            button.DisabledState.BorderColor = Color.Transparent;
            SetIcon(button, glyph, Palette.Text, Palette.TextMuted);
        }

        /// <summary>
        /// Оформляет кнопку с рамкой (второстепенное действие: «Предпросмотр», «Отмена»).
        /// </summary>
        /// <param name="button">Кнопка.</param>
        /// <param name="glyph">Значок.</param>
        public static void ApplyOutlined(Guna2Button button, char? glyph)
        {
            ApplyCommon(button);
            button.FillColor = Color.Transparent;
            button.ForeColor = Palette.Text;
            button.BorderColor = Palette.Border;
            button.BorderThickness = 1;
            button.HoverState.FillColor = Palette.SurfaceRaised;
            button.HoverState.ForeColor = Palette.Text;
            button.HoverState.BorderColor = Palette.TextMuted;
            button.DisabledState.FillColor = Color.Transparent;
            button.DisabledState.ForeColor = Palette.TextMuted;
            button.DisabledState.BorderColor = Palette.Border;
            SetIcon(button, glyph, Palette.Text, Palette.TextMuted);
        }

        /// <summary>
        /// Оформляет кнопку основного действия с зеленой заливкой
        /// («Запустить», «Готово», «Сохранить», «Сравнить»).
        /// </summary>
        /// <param name="button">Кнопка.</param>
        /// <param name="glyph">Значок.</param>
        public static void ApplyPrimary(Guna2Button button, char? glyph)
        {
            ApplyFilled(button, glyph, Palette.Accent, Palette.OnAccent);
        }

        /// <summary>
        /// Оформляет кнопку опасного действия с красной заливкой («Остановить»).
        /// </summary>
        /// <param name="button">Кнопка.</param>
        /// <param name="glyph">Значок.</param>
        public static void ApplyDanger(Guna2Button button, char? glyph)
        {
            ApplyFilled(button, glyph, Palette.Danger, Palette.OnAccent);
        }

        /// <summary>
        /// Оформляет метку-индикатор со скругленными краями.
        /// </summary>
        /// <param name="button">Кнопка, используемая как метка.</param>
        /// <param name="foreColor">Цвет текста.</param>
        /// <param name="fillColor">Цвет заливки.</param>
        /// <param name="glyph">Значок.</param>
        public static void ApplyPill(Guna2Button button, Color foreColor, Color fillColor, char? glyph)
        {
            button.Animated = false;
            button.BorderRadius = 10;
            button.Cursor = Cursors.Default;
            button.TabStop = false;
            button.Font = UiFonts.Monospace(9.5f);
            button.FillColor = fillColor;
            button.ForeColor = foreColor;
            button.BorderThickness = 0;
            button.HoverState.FillColor = fillColor;
            button.HoverState.ForeColor = foreColor;
            button.PressedDepth = 0;
            button.PressedColor = fillColor;
            button.Image = glyph.HasValue
                ? IconFactory.Create(glyph.Value, foreColor, 14)
                : null;
            button.ImageSize = new Size(14, 14);
        }

        /// <summary>
        /// Оформляет таблицу DataGridView в темной теме.
        /// </summary>
        /// <param name="grid">Таблица.</param>
        public static void ApplyGrid(DataGridView grid)
        {
            grid.BackgroundColor = Palette.Background;
            grid.BorderStyle = BorderStyle.None;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.GridColor = Palette.Border;
            grid.EnableHeadersVisualStyles = false;
            grid.RowHeadersVisible = false;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeRows = false;
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Palette.SurfaceRaised;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Palette.TextSecondary;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Palette.SurfaceRaised;
            grid.ColumnHeadersDefaultCellStyle.Font = UiFonts.Monospace(9f);
            grid.DefaultCellStyle.BackColor = Palette.Background;
            grid.DefaultCellStyle.ForeColor = Palette.Text;
            grid.DefaultCellStyle.SelectionBackColor = Palette.SurfaceRaised;
            grid.DefaultCellStyle.SelectionForeColor = Palette.Text;
            grid.DefaultCellStyle.Font = UiFonts.Regular(10f);
        }

        /// <summary>
        /// Устанавливает значок кнопки для обычного и недоступного состояний.
        /// </summary>
        /// <param name="button">Кнопка.</param>
        /// <param name="glyph">Значок.</param>
        /// <param name="color">Цвет значка.</param>
        /// <param name="disabledColor">Цвет значка недоступной кнопки.</param>
        public static void SetIcon(Guna2Button button, char? glyph, Color color, Color disabledColor)
        {
            if (!glyph.HasValue)
            {
                button.Image = null;
                button.DisabledState.Image = null;
                return;
            }

            button.Image = IconFactory.Create(glyph.Value, color, IconSize);
            button.HoverState.Image = button.Image;
            button.DisabledState.Image = IconFactory.Create(glyph.Value, disabledColor, IconSize);
            button.ImageSize = new Size(IconSize, IconSize);
            button.ImageAlign = HorizontalAlignment.Left;
        }

        /// <summary>
        /// Оформляет кнопку с заливкой.
        /// </summary>
        /// <param name="button">Кнопка.</param>
        /// <param name="glyph">Значок.</param>
        /// <param name="fillColor">Цвет заливки.</param>
        /// <param name="foreColor">Цвет текста.</param>
        private static void ApplyFilled(Guna2Button button, char? glyph, Color fillColor, Color foreColor)
        {
            ApplyCommon(button);
            button.FillColor = fillColor;
            button.ForeColor = foreColor;
            button.BorderThickness = 0;
            button.HoverState.FillColor = ControlPaint.Light(fillColor, 0.15f);
            button.HoverState.ForeColor = foreColor;
            button.DisabledState.FillColor = Palette.SurfaceRaised;
            button.DisabledState.ForeColor = Palette.TextMuted;
            button.DisabledState.BorderColor = Palette.SurfaceRaised;
            SetIcon(button, glyph, foreColor, Palette.TextMuted);
        }

        /// <summary>
        /// Задает общие свойства кнопок.
        /// </summary>
        /// <param name="button">Кнопка.</param>
        private static void ApplyCommon(Guna2Button button)
        {
            button.Animated = true;
            button.BorderRadius = ButtonRadius;
            button.Cursor = Cursors.Hand;
            button.Font = UiFonts.Semibold(10.5f);
            button.TextAlign = HorizontalAlignment.Center;
            button.UseTransparentBackground = true;
        }
    }
}
