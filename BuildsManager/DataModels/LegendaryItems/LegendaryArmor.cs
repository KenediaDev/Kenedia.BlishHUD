using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.Core.Utility;
using System.Runtime.Serialization;

namespace Kenedia.Modules.BuildsManager.DataModels.LegendaryItems
{
    [DataContract]
    public class LegendaryArmor : LegendaryItem
    {
        public LegendaryArmor() { }

        public LegendaryArmor(ItemArmor armor)
        {
            Apply(armor);
        }

        public void Apply(ItemArmor armor)
        {
            Id = armor.Id;
            AttributeAdjustment = armor.Details.AttributeAdjustment;
            Description = armor.Description;
            Name = armor.Name;
            Weight = armor.Details.WeightClass;
            StatChoices = armor.Details.StatChoices;
            InfusionSlots = new int[armor.Details.InfusionSlots.Count];
            AssetId = Common.GetAssetIdFromRenderUrl(armor.Icon);
            Chatlink = armor.ChatLink;
        }

        [DataMember]
        public LegendaryItemType ItemType { get; } = LegendaryItemType.Armor;

        [DataMember]
        public ItemWeightType Weight { get; protected set; }
    }
}
