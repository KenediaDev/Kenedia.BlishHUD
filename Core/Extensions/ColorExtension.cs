using Microsoft.Xna.Framework;
using System;

namespace Kenedia.Modules.Core.Extensions
{
    public static class ColorExtension
    {
        public static string ToHex(this Color col)
        {
            var c = System.Drawing.Color.FromArgb(col.A, col.R, col.G, col.B);
            return string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", col.A, col.R, col.G, col.B); //System.Drawing.ColorTranslator.ToHtml(c);
        }

        public static bool ColorFromHex(this string col, out Color outColor)
        {
            try
            {
                var c = System.Drawing.ColorTranslator.FromHtml(col);
                outColor = new(c.R, c.G, c.B, c.A);
                return true;
            }
            catch(Exception ex)
            {

            }

            outColor = Color.Transparent;
            return false;
        }
    }
}
