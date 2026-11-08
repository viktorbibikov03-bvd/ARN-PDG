using System.Drawing;

namespace VrdoPdg.View.Theme
{
    /// <summary>
    /// Цвета темного оформления интерфейса согласно эскизам экранных форм.
    /// </summary>
    internal static class Palette
    {
        /// <summary>
        /// Фон окна.
        /// </summary>
        public static Color Background { get; } = Color.FromArgb(17, 17, 17);

        /// <summary>
        /// Фон панелей и карточек.
        /// </summary>
        public static Color Surface { get; } = Color.FromArgb(24, 24, 24);

        /// <summary>
        /// Фон приподнятых элементов: заголовков таблиц, выделенных строк.
        /// </summary>
        public static Color SurfaceRaised { get; } = Color.FromArgb(32, 32, 32);

        /// <summary>
        /// Фон панели протокола.
        /// </summary>
        public static Color SurfaceSunken { get; } = Color.FromArgb(12, 12, 12);

        /// <summary>
        /// Цвет рамок и разделителей.
        /// </summary>
        public static Color Border { get; } = Color.FromArgb(48, 48, 48);

        /// <summary>
        /// Основной цвет текста.
        /// </summary>
        public static Color Text { get; } = Color.FromArgb(236, 236, 236);

        /// <summary>
        /// Цвет пояснений.
        /// </summary>
        public static Color TextSecondary { get; } = Color.FromArgb(140, 140, 140);

        /// <summary>
        /// Цвет недоступных команд.
        /// </summary>
        public static Color TextMuted { get; } = Color.FromArgb(92, 92, 92);

        /// <summary>
        /// Зеленый цвет основного действия.
        /// </summary>
        public static Color Accent { get; } = Color.FromArgb(74, 222, 140);

        /// <summary>
        /// Темно-зеленый фон загруженных файлов и измененных строк.
        /// </summary>
        public static Color AccentDark { get; } = Color.FromArgb(20, 48, 32);

        /// <summary>
        /// Цвет текста на зеленой заливке.
        /// </summary>
        public static Color OnAccent { get; } = Color.FromArgb(12, 12, 12);

        /// <summary>
        /// Красный цвет кнопки «Остановить» и ошибок.
        /// </summary>
        public static Color Danger { get; } = Color.FromArgb(255, 92, 102);

        /// <summary>
        /// Темно-красный фон ошибок.
        /// </summary>
        public static Color DangerDark { get; } = Color.FromArgb(64, 22, 28);

        /// <summary>
        /// Желтый цвет предупреждений.
        /// </summary>
        public static Color Warning { get; } = Color.FromArgb(242, 180, 64);

        /// <summary>
        /// Темно-желтый фон предупреждений.
        /// </summary>
        public static Color WarningDark { get; } = Color.FromArgb(58, 42, 14);

        /// <summary>
        /// Синий цвет метки УР и напряжения выше графика.
        /// </summary>
        public static Color Blue { get; } = Color.FromArgb(96, 170, 255);

        /// <summary>
        /// Темно-синий фон напряжения выше верхней границы графика.
        /// </summary>
        public static Color BlueDark { get; } = Color.FromArgb(18, 56, 94);

        /// <summary>
        /// Фиолетовый цвет метки СКРМ и загрузки по реактивной мощности.
        /// </summary>
        public static Color Purple { get; } = Color.FromArgb(176, 150, 255);

        /// <summary>
        /// Темно-фиолетовый фон метки СКРМ.
        /// </summary>
        public static Color PurpleDark { get; } = Color.FromArgb(42, 32, 72);

        /// <summary>
        /// Красный фон напряжения ниже нижней границы графика.
        /// </summary>
        public static Color RedDark { get; } = Color.FromArgb(92, 28, 36);

        /// <summary>
        /// Цвет значений первого сравниваемого расчета.
        /// </summary>
        public static Color FirstRun { get; } = Color.FromArgb(150, 236, 186);

        /// <summary>
        /// Цвет значений второго сравниваемого расчета.
        /// </summary>
        public static Color SecondRun { get; } = Color.FromArgb(206, 192, 255);
    }
}
