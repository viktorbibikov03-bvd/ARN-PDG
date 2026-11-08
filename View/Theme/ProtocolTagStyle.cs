using System.Drawing;

using VrdoPdg.Model.Protocol;

namespace VrdoPdg.View.Theme
{
    /// <summary>
    /// Цвета меток записей протокола расчета (таблица 2.2.4.3).
    /// </summary>
    internal static class ProtocolTagStyle
    {
        /// <summary>
        /// Серый цвет меток ЧАС и INFO.
        /// </summary>
        private static readonly Color _grayText = Color.FromArgb(200, 200, 200);

        /// <summary>
        /// Серый фон меток ЧАС и INFO.
        /// </summary>
        private static readonly Color _grayBack = Color.FromArgb(42, 42, 42);

        /// <summary>
        /// Синий фон метки УР.
        /// </summary>
        private static readonly Color _blueBack = Color.FromArgb(20, 44, 74);

        /// <summary>
        /// Возвращает цвет текста метки.
        /// </summary>
        /// <param name="tag">Метка.</param>
        /// <returns>Цвет текста.</returns>
        public static Color GetForeColor(ProtocolTag tag)
        {
            switch (tag)
            {
                case ProtocolTag.Vrdo:
                    return Palette.Accent;
                case ProtocolTag.Device:
                    return Palette.Purple;
                case ProtocolTag.Regime:
                    return Palette.Blue;
                case ProtocolTag.Warning:
                    return Palette.Warning;
                case ProtocolTag.Error:
                    return Palette.Danger;
                default:
                    return _grayText;
            }
        }

        /// <summary>
        /// Возвращает цвет фона метки.
        /// </summary>
        /// <param name="tag">Метка.</param>
        /// <returns>Цвет фона.</returns>
        public static Color GetBackColor(ProtocolTag tag)
        {
            switch (tag)
            {
                case ProtocolTag.Vrdo:
                    return Palette.AccentDark;
                case ProtocolTag.Device:
                    return Palette.PurpleDark;
                case ProtocolTag.Regime:
                    return _blueBack;
                case ProtocolTag.Warning:
                    return Palette.WarningDark;
                case ProtocolTag.Error:
                    return Palette.DangerDark;
                default:
                    return _grayBack;
            }
        }
    }
}
