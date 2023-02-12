using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.Core.Utility;
using System.Runtime.Serialization;

namespace Kenedia.Modules.BuildsManager.DataModels.LegendaryItems
{
    [DataContract]
    public class LegendaryTrinket : LegendaryItem
    {
        public LegendaryTrinket() { }

        public LegendaryTrinket(ItemTrinket trinket)
        {
            Apply(trinket);
        }

        public LegendaryTrinket(ItemBack back)
        {
            Apply(back);
        }

        public void Apply(ItemBack back)
        {
            Id = back.Id;
            AttributeAdjustment = back.Details.AttributeAdjustment;
            Description = back.Description;
            Name = back.Name;
            StatChoices = back.Details.StatChoices;
            InfusionSlots = new int[back.Details.InfusionSlots.Count];
            AssetId = Common.GetAssetIdFromRenderUrl(back.Icon);
            Chatlink = back.ChatLink;
        }

        public void Apply(ItemTrinket trinket)
        {
            Id = trinket.Id;
            AttributeAdjustment = trinket.Details.AttributeAdjustment;
            Description = trinket.Description;
            Name = trinket.Name;
            StatChoices = trinket.Details.StatChoices;
            InfusionSlots = new int[trinket.Details.InfusionSlots.Count];
            AssetId = Common.GetAssetIdFromRenderUrl(trinket.Icon);
            Chatlink = trinket.ChatLink;
        }

        [DataMember]
        public LegendaryItemType ItemType { get; } = LegendaryItemType.Trinket;
    }
}
