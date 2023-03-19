using Microsoft.Xna.Framework;

namespace Kenedia.Modules.Core.Extensions
{
    public static class RectangleExtension
    {
        public static bool Equals(this Rectangle a, Rectangle b)
        {
            bool height = a.Height == b.Height;
            bool width = a.Width == b.Width;
            bool left = a.Left == b.Left;
            bool right = a.Right == b.Right;
            bool top = a.Top == b.Top;
            bool bottom = a.Bottom == b.Bottom;

            return height && width && left && right && top && bottom;
        }
    }
}
