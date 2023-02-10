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
            Description.Text = back.Description;
            Names.Text = back.Name;
            StatChoices = back.Details.StatChoices;
            InfusionSlots = new int[back.Details.InfusionSlots.Count];
            AssetId = Common.GetAssetIdFromRenderUrl(back.Icon);
        }

        public void Apply(ItemTrinket trinket)
        {
            Id = trinket.Id;
            AttributeAdjustment = trinket.Details.AttributeAdjustment;
            Description.Text = trinket.Description;
            Names.Text = trinket.Name;
            StatChoices = trinket.Details.StatChoices;
            InfusionSlots = new int[trinket.Details.InfusionSlots.Count];
            AssetId = Common.GetAssetIdFromRenderUrl(trinket.Icon);
        }

        [DataMember]
        public LegendaryItemType ItemType { get; } = LegendaryItemType.Trinket;
    }
}
