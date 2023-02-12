using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.Core.Utility;
using System.Runtime.Serialization;

namespace Kenedia.Modules.BuildsManager.DataModels.ItemUpgrades
{
    [DataContract]
    public class Rune : Upgrade
    {
        public Rune() { }

        public Rune(ItemUpgradeComponent rune)
        {
            AssetId = rune.Icon.GetAssetIdFromRenderUrl();
            Id = rune.Id;
            Rarity = rune.Rarity;
        }

        internal void ApplyLanguage(ItemUpgradeComponent rune)
        {
            Name = rune.Name;
            Description = rune.Description;
        }
    }
}
