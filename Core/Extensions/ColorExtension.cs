using Gw2Sharp.Models;
using Microsoft.Xna.Framework;
using System;

namespace Kenedia.Modules.Core.Extensions
{
    public static class ColorExtension
    {
        public static Color Guardian = new(0, 180, 255);
        public static Color Warrior = new(247, 157, 0);
        public static Color Engineer = new(255, 222, 0);
        public static Color Ranger = new(234, 255, 0);
        public static Color Thief = new(255, 83, 0);
        public static Color Elementalist = new(247, 0, 116);
        public static Color Mesmer = new(255, 0, 240);
        public static Color Necromancer = new(192, 255, 0);
        public static Color Revenant = new(255, 0, 0);

        public static Color GetProfessionColor(ProfessionType profession)
        {
            return profession switch
            {
                ProfessionType.Guardian => Guardian,
                ProfessionType.Warrior => Warrior,
                ProfessionType.Engineer => Engineer,
                ProfessionType.Ranger => Ranger,
                ProfessionType.Thief => Thief,
                ProfessionType.Elementalist => Elementalist,
                ProfessionType.Mesmer => Mesmer,
                ProfessionType.Necromancer => Necromancer,
                ProfessionType.Revenant => Revenant,
                _ => Color.White,
            };
        }

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
