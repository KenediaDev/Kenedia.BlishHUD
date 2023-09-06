using Gw2Sharp.WebApi.V2.Models;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Kenedia.Modules.BuildsManager.DataModels.Items
{
    [DataContract]
    public class EquipmentItem : BaseItem
    {
        public EquipmentItem()
        {
            Rarity = ItemRarity.Legendary;
        }

        [DataMember]
        public double AttributeAdjustment { get; set; }

        [DataMember]
        public ItemEquipmentSlotType Slot { get; set; }

        [DataMember]
        public IReadOnlyList<int> StatChoices { get; set; }

        [DataMember]
        public int[] InfusionSlots { get; set; }

    }
}
