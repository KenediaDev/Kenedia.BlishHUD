using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.Core.Utility;
using System.Runtime.Serialization;

namespace Kenedia.Modules.BuildsManager.DataModels.LegendaryItems
{
    [DataContract]
    public class LegendaryUpgrade : LegendaryItem
    {
        public LegendaryUpgrade()
        {

        }

        public LegendaryUpgrade(ItemUpgradeComponent upgrade)
        {
            Apply(upgrade);
        }

        public void Apply(ItemUpgradeComponent upgrade)
        {
            Id = upgrade.Id;
            Name = upgrade.Name;
            AssetId = Common.GetAssetIdFromRenderUrl(upgrade.Icon);
            Chatlink = upgrade.ChatLink;
        }
    }
}
