using Blish_HUD.Content;
using System.Drawing;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Kenedia.Modules.Core.Extensions;
using System.IO;
using RECT = Kenedia.Modules.Core.Utility.WindowsUtil.User32Dll.RECT;

namespace Kenedia.Modules.Core.Utility
{
    public static class ScreenCapture
    {
        public static AsyncTexture2D CaptureRegion(RECT wndBounds, Microsoft.Xna.Framework.Point p, Rectangle bounds, double factor, Microsoft.Xna.Framework.Point size)
        {
            using Bitmap bitmap = new((int)(bounds.Width * factor), (int)(bounds.Height * factor));
            using var g = Graphics.FromImage(bitmap);
            using MemoryStream s = new();

            int x = (int)(bounds.X * factor);
            int y = (int)(bounds.Y * factor);

            g.CopyFromScreen(new System.Drawing.Point(wndBounds.Left + p.X + x, wndBounds.Top + p.Y + y), System.Drawing.Point.Empty, new(size.X, size.Y));
            bitmap.Save(s, System.Drawing.Imaging.ImageFormat.Bmp);

            return s.CreateTexture2D();
        }
        public static AsyncTexture2D CaptureRegion(Rectangle window, Rectangle region)
        {
            using Bitmap bitmap = new(region.Width, region.Height);
            using var g = Graphics.FromImage(bitmap);
            using MemoryStream s = new();
            g.CopyFromScreen(new System.Drawing.Point(window.Left + region.Left, window.Top + region.Top), System.Drawing.Point.Empty, new(region.Width, region.Height));

            bitmap.Save(s, System.Drawing.Imaging.ImageFormat.Bmp);

            return s.CreateTexture2D();
        }

        public static AsyncTexture2D CaptureRegion(RECT window, Rectangle region)
        {
            using Bitmap bitmap = new(region.Width, region.Height);
            using var g = Graphics.FromImage(bitmap);
            using MemoryStream s = new();
            g.CopyFromScreen(new System.Drawing.Point(window.Left + region.Left, window.Top + region.Top), System.Drawing.Point.Empty, new(region.Width, region.Height));
            bitmap.Save(s, System.Drawing.Imaging.ImageFormat.Bmp);

            return s.CreateTexture2D();
        }
    }
}
