using Blish_HUD.Controls;

namespace Kenedia.Modules.Core.Extensions
{
    public static class ControlExtensions
    {
        public static void ToggleVisibility(this Control c)
        {
            c.Visible = !c.Visible;
        }
    }
}
