using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.Core.Utility;
using System.Runtime.Serialization;

namespace Kenedia.Modules.BuildsManager.DataModels.ItemUpgrades
{
    [DataContract]
    public class Sigil : Upgrade
    {
        public Sigil() { }

        public Sigil(ItemUpgradeComponent sigil)
        {
            AssetId = sigil.Icon.GetAssetIdFromRenderUrl();
            Id = sigil.Id;
            Rarity = sigil.Rarity;
        }

        internal void ApplyLanguage(ItemUpgradeComponent sigil)
        {
            Name = sigil.Name;
            Description = sigil.Description;
        }
    }
}
