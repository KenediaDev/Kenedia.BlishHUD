using Microsoft.Xna.Framework;
using ItemRarity = Gw2Sharp.WebApi.V2.Models.ItemRarity;

namespace Kenedia.Modules.Core.Extensions
{
    public static class RarityExtension
    {
        public static Color GetColor(this ItemRarity rarity)
        {
            return rarity switch
            {
                ItemRarity.Junk => Color.DarkGray,
                ItemRarity.Basic => new(200, 200, 200),
                ItemRarity.Fine => new(74, 146, 236),
                ItemRarity.Masterwork => new(43, 184, 14),
                ItemRarity.Rare => new(237, 214, 30),
                ItemRarity.Exotic => new(235, 154, 1),
                ItemRarity.Ascended => new(234, 58, 132),
                ItemRarity.Legendary => new(159, 47, 244),
                _ => Color.White
            };
        }
    }
}
