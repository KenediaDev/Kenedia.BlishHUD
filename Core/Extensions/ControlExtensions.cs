using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;

namespace Kenedia.Modules.Core.Extensions
{
    public static class ControlExtensions
    {
        public static bool IsVisible(this Control control)
        {
            bool IsParentSetAndVisible(Control ctrl)
            {
                return ctrl.Visible && ctrl.Parent is not null && 
                    ((ctrl.Parent == GameService.Graphics.SpriteScreen && ctrl.Parent.Visible) ||
                    (ctrl.Parent.Visible && IsParentSetAndVisible(ctrl.Parent)));
            }

            return IsParentSetAndVisible(control);
        }

        public static bool ToggleVisibility(this Control c, bool? visible = null)
        {
            c.Visible = visible ?? !c.Visible;
            return c.Visible;
        }

        public static void SetLocation(this Control c, int? x = null, int? y = null)
        {
            x ??= c.Location.X;
            y ??= c.Location.Y;

            c.Location = new((int)x, (int)y);
        }

        public static void SetLocation(this Control c, Point location)
        {
            c.Location = location;
        }

        public static void SetSize(this Control c, int? width = null, int? height = null)
        {
            width ??= c.Width;
            height ??= c.Height;

            c.Size = new((int)width, (int)height);
        }

        public static void SetSize(this Control c, Point size)
        {
            c.Size = size;
        }

        public static void SetBounds(this Control c, Rectangle bounds)
        {
            c.SetLocation(bounds.Location);
            c.SetSize(bounds.Size);
        }
    }
}
