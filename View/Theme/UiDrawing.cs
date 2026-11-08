using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace VrdoPdg.View.Theme
{
    /// <summary>
    /// Вспомогательные методы рисования элементов интерфейса.
    /// </summary>
    internal static class UiDrawing
    {
        /// <summary>
        /// Создает контур прямоугольника со скругленными углами.
        /// </summary>
        /// <param name="bounds">Прямоугольник.</param>
        /// <param name="radius">Радиус скругления.</param>
        /// <returns>Контур.</returns>
        public static GraphicsPath CreateRoundedRectangle(Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = Math.Max(1, Math.Min(radius * 2, Math.Min(bounds.Width, bounds.Height)));
            Rectangle arc = new Rectangle(bounds.Location, new Size(diameter, diameter));
            path.AddArc(arc, 180, 90);
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }

        /// <summary>
        /// Создает контур прямоугольника со скругленными углами в дробных координатах.
        /// Используется при сглаживании со смещением пикселей на половину пикселя.
        /// </summary>
        /// <param name="bounds">Прямоугольник.</param>
        /// <param name="radius">Радиус скругления.</param>
        /// <returns>Контур.</returns>
        public static GraphicsPath CreateRoundedRectangle(RectangleF bounds, float radius)
        {
            GraphicsPath path = new GraphicsPath();
            float diameter = Math.Min(radius * 2, Math.Min(bounds.Width, bounds.Height));
            if (diameter <= 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            RectangleF arc = new RectangleF(bounds.Location, new SizeF(diameter, diameter));
            path.AddArc(arc, 180, 90);
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }

        /// <summary>
        /// Возвращает прямоугольник заливки элемента. Без смещения пикселей пиксель i занимает
        /// отрезок [i - 0,5; i + 0,5], поэтому заливка от -0,5 покрывает крайние пиксели целиком.
        /// </summary>
        /// <param name="size">Размер элемента.</param>
        /// <returns>Прямоугольник заливки.</returns>
        public static RectangleF GetFillBounds(Size size)
        {
            return new RectangleF(-0.5f, -0.5f, size.Width, size.Height);
        }

        /// <summary>
        /// Возвращает прямоугольник средней линии рамки: рамка заданной толщины занимает
        /// целые пиксели начиная с отступа от края элемента.
        /// </summary>
        /// <param name="size">Размер элемента.</param>
        /// <param name="thickness">Толщина рамки, пикселей.</param>
        /// <param name="inset">Отступ рамки от края элемента, пикселей.</param>
        /// <returns>Прямоугольник средней линии рамки.</returns>
        public static RectangleF GetStrokeBounds(Size size, float thickness, float inset)
        {
            float offset = inset + ((thickness - 1f) / 2f);
            return new RectangleF(
                offset,
                offset,
                size.Width - 1f - (offset * 2f),
                size.Height - 1f - (offset * 2f));
        }

        /// <summary>
        /// Смешивает цвет с другим цветом, сохраняя прозрачность исходного цвета.
        /// </summary>
        /// <param name="color">Исходный цвет.</param>
        /// <param name="other">Цвет, с которым смешивается исходный.</param>
        /// <param name="amount">Доля второго цвета от 0 до 1.</param>
        /// <returns>Смешанный цвет.</returns>
        public static Color Blend(Color color, Color other, float amount)
        {
            float weight = Math.Max(0f, Math.Min(1f, amount));
            return Color.FromArgb(
                color.A,
                (int)Math.Round(color.R + ((other.R - color.R) * weight)),
                (int)Math.Round(color.G + ((other.G - color.G) * weight)),
                (int)Math.Round(color.B + ((other.B - color.B) * weight)));
        }

        /// <summary>
        /// Заливает прямоугольник со скругленными углами.
        /// </summary>
        /// <param name="graphics">Поверхность рисования.</param>
        /// <param name="color">Цвет заливки.</param>
        /// <param name="bounds">Прямоугольник.</param>
        /// <param name="radius">Радиус скругления.</param>
        public static void FillRoundedRectangle(Graphics graphics, Color color, Rectangle bounds, int radius)
        {
            SmoothingMode mode = graphics.SmoothingMode;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath path = CreateRoundedRectangle(bounds, radius))
            using (SolidBrush brush = new SolidBrush(color))
            {
                graphics.FillPath(brush, path);
            }

            graphics.SmoothingMode = mode;
        }

        /// <summary>
        /// Рисует рамку прямоугольника со скругленными углами.
        /// </summary>
        /// <param name="graphics">Поверхность рисования.</param>
        /// <param name="color">Цвет рамки.</param>
        /// <param name="bounds">Прямоугольник.</param>
        /// <param name="radius">Радиус скругления.</param>
        public static void DrawRoundedRectangle(Graphics graphics, Color color, Rectangle bounds, int radius)
        {
            SmoothingMode mode = graphics.SmoothingMode;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath path = CreateRoundedRectangle(bounds, radius))
            using (Pen pen = new Pen(color))
            {
                graphics.DrawPath(pen, path);
            }

            graphics.SmoothingMode = mode;
        }

        /// <summary>
        /// Рисует стрелку раскрытия группы: «›» для свернутой и «⌄» для развернутой.
        /// </summary>
        /// <param name="graphics">Поверхность рисования.</param>
        /// <param name="color">Цвет стрелки.</param>
        /// <param name="center">Центр стрелки.</param>
        /// <param name="isExpanded">Признак развернутой группы.</param>
        public static void DrawChevron(Graphics graphics, Color color, Point center, bool isExpanded)
        {
            SmoothingMode mode = graphics.SmoothingMode;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (Pen pen = new Pen(color, 1.6f))
            {
                Point[] points = isExpanded
                    ? new[]
                    {
                        new Point(center.X - 4, center.Y - 2),
                        new Point(center.X, center.Y + 2),
                        new Point(center.X + 4, center.Y - 2)
                    }
                    : new[]
                    {
                        new Point(center.X - 2, center.Y - 4),
                        new Point(center.X + 2, center.Y),
                        new Point(center.X - 2, center.Y + 4)
                    };
                graphics.DrawLines(pen, points);
            }

            graphics.SmoothingMode = mode;
        }
    }
}
