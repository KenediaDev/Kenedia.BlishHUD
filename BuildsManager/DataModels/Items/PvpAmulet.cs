using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Utility;
using System.Runtime.Serialization;

namespace Kenedia.Modules.BuildsManager.DataModels.Items
{
    [DataContract]
    public class PvpAmulet : Trinket
    {
        public PvpAmulet()
        {
            TemplateSlot = TemplateSlotType.PvpAmulet;
        }

        public PvpAmulet(Gw2Sharp.WebApi.V2.Models.PvpAmulet apiAmulet) : this()
        {
            Id = apiAmulet.Id;
            Name = apiAmulet.Name;
            AssetId = apiAmulet.Icon.GetAssetIdFromRenderUrl();
            Rarity = ItemRarity.Basic;
            Type = ItemType.Unknown;
            Attributes = apiAmulet.Attributes;
        }

        [DataMember]
        public ItemAttributes Attributes { get; set; } = new();
    }
}
