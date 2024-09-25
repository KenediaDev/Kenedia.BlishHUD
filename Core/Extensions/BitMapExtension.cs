using System;
using System.Collections.Generic;
using System.Linq;
using Bitmap = System.Drawing.Bitmap;
using Point = System.Drawing.Point;
using Color = System.Drawing.Color;

namespace Kenedia.Modules.Core.Extensions
{
    public static class BitMapExtension
    {
        public static Bitmap ToBlackWhite(this Bitmap b, float colorThreshold = 0.5f)
        {
            var black = Color.FromArgb(255, 0, 0, 0);
            var white = Color.FromArgb(255, 255, 255, 255);

            for (int i = 0; i < b.Width; i++)
            {
                for (int j = 0; j < b.Height; j++)
                {
                    Color oc = b.GetPixel(i, j);
                    b.SetPixel(i, j, oc.GetBrightness() > colorThreshold ? black : white);
                }
            }

            return b;
        }

        public static (Bitmap, bool, double) IsColorAndCheckFilled(this Bitmap b, double threshold, Color startColor, Color endColor)
        {
            var black = Color.FromArgb(255, 0, 0, 0);
            var white = Color.FromArgb(255, 255, 255, 255);

            double total = b.Width * b.Height;
            double filled = 0;

            Point first = new(-1, -1);
            Point last = new(-1, -1);

            List<List<Point>> imageMap = [];

            for (int i = 0; i < b.Width; i++)
            {
                var column = new List<Point>();

                for (int j = 0; j < b.Height; j++)
                {
                    Color oc = b.GetPixel(i, j);

                    bool R = oc.R >= startColor.R || oc.R <= endColor.R;
                    bool G = oc.G >= startColor.G || oc.G <= endColor.G;
                    bool B = oc.B >= startColor.B || oc.B <= endColor.B;

                    bool isFilled = R & G & B;

                    if (isFilled)
                    {
                        if (first.X == -1 && first.Y == -1) first = new(i, j);
                        if (last.X == -1 && last.Y == -1) last = new(i, j);

                        first.X = Math.Min(first.X, i);
                        first.Y = Math.Min(first.Y, j);

                        last.X = Math.Max(last.X, i);
                        last.Y = Math.Max(last.Y, j);

                        column.Add(new(i, j));
                    }
                }

                if (column.Count > 0)
                {
                    imageMap.Add(column);
                }
                else if (imageMap.Count > 0 && i >= b.Width / 2)
                {
                    break;
                }
            }

            int? maxHeight = imageMap.Select(l => l.Max(e => e.Y))?.ToList()?.FirstOrDefault();
            var bitmap = new Bitmap(last.X - first.X + 1, last.Y - first.Y + 1);

            if (imageMap.Count > 0 && maxHeight is not null && maxHeight > 0)
            {
                foreach (List<Point> column in imageMap)
                {
                    foreach (Point p in column)
                    {
                        int x = p.X - first.X;
                        int y = p.Y - first.Y;
                        bitmap.SetPixel(x, y, black);
                    }
                }
            }

            return (bitmap, filled / total >= threshold, filled / total);
        }

        public static (Bitmap, bool, double) IsNotBlackAndCheckFilled(this Bitmap b, double threshold)
        {
            var white = Color.FromArgb(255, 255, 255, 255);

            Point first = new(-1, -1);
            Point last = new(-1, -1);

            List<(Point, Color)> imageMap = [];

            int emptyInRow = 0;
            for (int i = 0; i < b.Width; i++)
            {
                var column = new List<(Point, Color)>();

                for (int j = 0; j < b.Height; j++)
                {
                    Color oc = b.GetPixel(i, j);
                    bool isFilled = oc.R < 5 && oc.G < 5 && oc.B < 5;

                    if (isFilled)
                    {
                        if (first.X == -1 && first.Y == -1) first = new(i, j);
                        if (last.X == -1 && last.Y == -1) last = new(i, j);

                        first.X = Math.Min(first.X, i);
                        first.Y = Math.Min(first.Y, j);

                        last.X = Math.Max(last.X, i);
                        last.Y = Math.Max(last.Y, j);

                        column.Add((new(i, j), oc));
                    }
                }

                if (column.Count > 0)
                {
                    imageMap.AddRange(column);
                    emptyInRow = 0;
                }
                else
                {
                    emptyInRow++;

                    if (emptyInRow >= 5 && first.X > -1) break;
                }
            }

            var bitmap = new Bitmap(last.X - first.X + 1, last.Y - first.Y + 1);
            double total = bitmap.Width * bitmap.Height;
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    bitmap.SetPixel(i, j, white);
                }
            }

            if (imageMap.Count > 0)
            {
                foreach ((Point, Color) t in imageMap)
                {
                    int x = t.Item1.X - first.X;
                    int y = t.Item1.Y - first.Y;
                    bitmap.SetPixel(x, y, t.Item2);
                }
            }

            return (bitmap, imageMap.Count / total >= threshold, imageMap.Count / total);
        }

        public static (Bitmap, bool, double) IsCutAndCheckFilled(this Bitmap b, double threshold, float colorThreshold = 0.5f)
        {
            var white = Color.FromArgb(255, 255, 255, 255);

            Point first = new(-1, -1);
            Point last = new(-1, -1);

            List<(Point, Color)> imageMap = [];

            int emptyInRow = 0;
            for (int i = 0; i < b.Width; i++)
            {
                var column = new List<(Point, Color)>();

                for (int j = 0; j < b.Height; j++)
                {
                    Color oc = b.GetPixel(i, j);
                    bool isFilled = oc.GetBrightness() > colorThreshold;

                    if (isFilled)
                    {
                        if (first.X == -1 && first.Y == -1) first = new(i, j);
                        if (last.X == -1 && last.Y == -1) last = new(i, j);

                        first.X = Math.Min(first.X, i);
                        first.Y = Math.Min(first.Y, j);

                        last.X = Math.Max(last.X, i);
                        last.Y = Math.Max(last.Y, j);

                        column.Add((new(i, j), oc));
                    }
                }

                if (column.Count > 0)
                {
                    imageMap.AddRange(column);
                    emptyInRow = 0;
                }
                else
                {
                    emptyInRow++;

                    if (emptyInRow >= 5 && first.X > -1) break;
                }
            }

            var bitmap = new Bitmap(last.X - first.X + 1, last.Y - first.Y + 1);
            double total = bitmap.Width * bitmap.Height;
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    bitmap.SetPixel(i, j, white);
                }
            }

            if (imageMap.Count > 0)
            {
                foreach ((Point, Color) t in imageMap)
                {
                    int x = t.Item1.X - first.X;
                    int y = t.Item1.Y - first.Y;
                    bitmap.SetPixel(x, y, t.Item2);
                }
            }

            return (bitmap, imageMap.Count / total >= threshold, imageMap.Count / total);
        }

        public static (Bitmap, bool, double) IsFilled(this Bitmap b, double threshold, float colorThreshold = 0.5f)
        {
            var black = Color.FromArgb(255, 0, 0, 0);
            var white = Color.FromArgb(255, 255, 255, 255);

            double total = b.Width * b.Height;
            double filled = 0;

            for (int i = 0; i < b.Width; i++)
            {
                for (int j = 0; j < b.Height; j++)
                {
                    Color oc = b.GetPixel(i, j);
                    bool isFilled = oc.GetBrightness() > colorThreshold;

                    filled += isFilled ? 1 : 0;
                    b.SetPixel(i, j, oc.GetBrightness() > colorThreshold ? black : white);
                }
            }

            return (b, filled / total >= threshold, filled / total);
        }
    }
}
