using Blish_HUD.Controls;

namespace Kenedia.Modules.Characters.Extensions
{
    internal static class ControlExtensions
    {
        public static void ToggleVisibility(this Control c)
        {
            c.Visible = !c.Visible;
        }
    }
}
