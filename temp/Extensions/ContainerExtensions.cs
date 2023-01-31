using Blish_HUD.Controls;

namespace Kenedia.Modules.Core.Extensions
{
    public static class ContainerExtensions
    {
        public static void ToggleVisibility(this Container c)
        {
            c.Visible = !c.Visible;
        }
    }
}
