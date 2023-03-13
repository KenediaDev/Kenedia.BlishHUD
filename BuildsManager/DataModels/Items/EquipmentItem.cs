using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.BuildsManager.DataModels.Stats;
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
        public double AttributeAdjustment { get; protected set; }

        [DataMember]
        public ItemEquipmentSlotType Slot { get; protected set; }

        [DataMember]
        public IReadOnlyList<int> StatChoices { get; protected set; }

        [DataMember]
        public int[] InfusionSlots { get; protected set; }

    }
}
