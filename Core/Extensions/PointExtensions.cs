using Blish_HUD;
using Microsoft.Xna.Framework;
using System;
using Point = Microsoft.Xna.Framework.Point;
using static Kenedia.Modules.Core.Utility.WindowsUtil.User32Dll;
using System.Diagnostics;

namespace Kenedia.Modules.Core.Extensions
{
    public static class PointExtensions
    {
        public static int Distance2D(this Point p1, Point p2)
        {
            return (int)Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
        }

        public static float Distance3D(this Vector3 p1, Vector3 p2)
        {
            float deltaX = p2.X - p1.X;
            float deltaY = p2.Y - p1.Y;
            float deltaZ = p2.Z - p1.Z;

            float distance = (float)Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY) + (deltaZ * deltaZ));
            return distance;
        }

        public static Point Add(this Point b, Point p)
        {
            return new Point(b.X + p.X, b.Y + p.Y);
        }

        public static Point Substract(this Point b, Point p)
        {
            return new Point(b.X - p.X, b.Y - p.Y);
        }

        public static Point Scale(this Point p, double factor)
        {
            return new Point((int)(p.X * factor), (int)(p.Y * factor));
        }

        public static string ConvertToString(this Point p)
        {
            return string.Format("X: {0}, Y: {1}", p.X, p.Y);
        }

        public static Point ClientToScreenPos(this Point p, bool scaleToUi = false)
        {
            if (scaleToUi)
            {
                p = p.ScaleToUi();
            }

            var hWnd = GameService.GameIntegration.Gw2Instance.Gw2WindowHandle;
            var point = new POINT() { X = p.X, Y = p.Y };
            _ = ClientToScreen(hWnd, ref point);

            return new(point.X, point.Y);
        }
    }
}
