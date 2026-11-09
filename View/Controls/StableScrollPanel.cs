using System.Drawing;
using System.Windows.Forms;

namespace VrdoPdg.View.Controls
{
    /// <summary>
    /// Панель с прокруткой, которая не прокручивается к элементу при получении им фокуса:
    /// щелчок по таблице блока не сдвигает список блоков к ее началу.
    /// </summary>
    internal sealed class StableScrollPanel : Panel
    {
        /// <summary>
        /// Сохраняет текущее положение прокрутки при активации вложенного элемента.
        /// </summary>
        /// <param name="activeControl">Активируемый элемент.</param>
        /// <returns>Текущее положение прокрутки.</returns>
        protected override Point ScrollToControl(Control activeControl)
        {
            return DisplayRectangle.Location;
        }
    }
}
