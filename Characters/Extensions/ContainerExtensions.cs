using Blish_HUD.Controls;

namespace Kenedia.Modules.Characters.Extensions
{
    internal static class ContainerExtensions
    {
        public static void ToggleVisibility(this Container c)
        {
            c.Visible = !c.Visible;
        }
    }
}
