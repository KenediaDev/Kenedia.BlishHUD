using Gw2Sharp.Models;
using Microsoft.Xna.Framework;
using System;

namespace Kenedia.Modules.Core.Extensions
{
    public static class SnowCrowsColor
    {
        public static Color Guardian = new(103, 174, 203);
        public static Color Warrior = new(247, 157, 0);
        public static Color Engineer = new(152, 105, 44);
        public static Color Ranger = new(142, 165, 58);
        public static Color Thief = new(73, 85, 120);
        public static Color Elementalist = new(163, 54, 46);
        public static Color Mesmer = new(114, 65, 146);
        public static Color Necromancer = new(63, 88, 71);
        public static Color Revenant = new(87, 36, 53);
    }

    public static class PvPColor
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
    }

    public static class WikiColor
    {
        public static Color Guardian = new(85, 203, 212);
        public static Color Warrior = new(232, 194, 58);
        public static Color Engineer = new(202, 144, 66);
        public static Color Ranger = new(187, 215, 101);
        public static Color Thief = new(168, 108, 108);
        public static Color Elementalist = new(235, 83, 37);
        public static Color Mesmer = new(190, 113, 227);
        public static Color Necromancer = new(30, 194, 115);
        public static Color Revenant = new(171, 53, 53);
    }

    public static class ColorExtension
    {
        public static Color Guardian = new(103, 174, 203);
        public static Color Warrior = new(247, 157, 0);
        public static Color Engineer = new(152, 105, 44);
        public static Color Ranger = new(142, 165, 58);
        public static Color Thief = new(73, 85, 120);
        public static Color Elementalist = new(163, 54, 46);
        public static Color Mesmer = new(114, 65, 146);
        public static Color Necromancer = new(63, 88, 71);
        public static Color Revenant = new(87, 36, 53);

        public static Color GetProfessionColor(this ProfessionType profession)
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

        public static Color GetSnowCrowColor(this ProfessionType profession)
        {
            return profession switch
            {
                ProfessionType.Guardian => SnowCrowsColor.Guardian,
                ProfessionType.Warrior => SnowCrowsColor.Warrior,
                ProfessionType.Engineer => SnowCrowsColor.Engineer,
                ProfessionType.Ranger => SnowCrowsColor.Ranger,
                ProfessionType.Thief => SnowCrowsColor.Thief,
                ProfessionType.Elementalist => SnowCrowsColor.Elementalist,
                ProfessionType.Mesmer => SnowCrowsColor.Mesmer,
                ProfessionType.Necromancer => SnowCrowsColor.Necromancer,
                ProfessionType.Revenant => SnowCrowsColor.Revenant,
                _ => Color.White,
            };
        }
        public static Color GetWikiColor(this ProfessionType profession)
        {
            return profession switch
            {
                ProfessionType.Guardian => WikiColor.Guardian,
                ProfessionType.Warrior => WikiColor.Warrior,
                ProfessionType.Engineer => WikiColor.Engineer,
                ProfessionType.Ranger => WikiColor.Ranger,
                ProfessionType.Thief => WikiColor.Thief,
                ProfessionType.Elementalist => WikiColor.Elementalist,
                ProfessionType.Mesmer => WikiColor.Mesmer,
                ProfessionType.Necromancer => WikiColor.Necromancer,
                ProfessionType.Revenant => WikiColor.Revenant,
                _ => Color.White,
            };
        }

        public static Color GetPvPColor(this ProfessionType profession)
        {
            return profession switch
            {
                ProfessionType.Guardian => PvPColor.Guardian,
                ProfessionType.Warrior => PvPColor.Warrior,
                ProfessionType.Engineer => PvPColor.Engineer,
                ProfessionType.Ranger => PvPColor.Ranger,
                ProfessionType.Thief => PvPColor.Thief,
                ProfessionType.Elementalist => PvPColor.Elementalist,
                ProfessionType.Mesmer => PvPColor.Mesmer,
                ProfessionType.Necromancer => PvPColor.Necromancer,
                ProfessionType.Revenant => PvPColor.Revenant,
                _ => Color.White,
            };
        }

        public static Gw2Sharp.WebApi.V2.Models.Color ToApiColor(this Color col)
        {
            return new Gw2Sharp.WebApi.V2.Models.Color() 
            {
                BaseRgb = [
                    col.R,
                    col.G,
                    col.B,
                    col.A,
                    ],
            };
        }
        public static string ToHex(this Color col)
        {
            //var c = System.Drawing.Color.FromArgb(col.A, col.R, col.G, col.B);
            //return string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", col.A, col.R, col.G, col.B); //System.Drawing.ColorTranslator.ToHtml(c);
            return $"#{col.A:X2}{col.R:X2}{col.G:X2}{col.B:X2}";

        }

        public static bool ColorFromHex(this string col, out Color outColor)
        {
            try
            {
                var c = System.Drawing.ColorTranslator.FromHtml(col);
                outColor = new(c.R, c.G, c.B, c.A);
                return true;
            }
            catch (Exception ex)
            {

            }

            outColor = Color.Transparent;
            return false;
        }
    }
}
