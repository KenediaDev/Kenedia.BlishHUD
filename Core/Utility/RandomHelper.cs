using Kenedia.Modules.Core.Structs;
using Microsoft.Xna.Framework;
using System;

namespace Kenedia.Modules.Core.Utility
{
    public static class RandomHelper
    {
        public static readonly Random Rnd = new();

        public static Color RandomColor(Range? r = null, Range? g = null, Range? b = null, Range? a = null)
        {
            var red = r == null ? new(0, 255) : r.Value;
            var green = g == null ? new(0, 255) : g.Value;
            var blue = b == null ? new(0, 255) : b.Value;
            var alpha = a == null ? new(255, 255) : a.Value;

            return new Color(Rnd.Next(red.Start, red.End), Rnd.Next(green.Start, green.End), Rnd.Next(blue.Start, blue.End), Rnd.Next(alpha.Start, alpha.End));
        }
    }
}
