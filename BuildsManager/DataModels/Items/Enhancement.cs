using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.BuildsManager.Models.Templates;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Kenedia.Modules.BuildsManager.DataModels.Items
{
    [DataContract]
    public class Enhancement : BaseItem
    {
        public Enhancement()
        {
            TemplateSlot = TemplateSlotType.Enhancement;

        }

        [DataMember]
        public List<BonusStat> Bonuses { get; set; } = [];

        [DataMember]
        public ConsumableDetails Details { get; set; } = new();

        public override void Apply(Item item)
        {
            base.Apply(item);

            if (item.Type == ItemType.Consumable)
            {
                var consumable = (ItemConsumable)item;

                Details.Description = consumable.Details.Description;
                Details.DurationMs = consumable.Details.DurationMs;
                Details.Name = consumable.Details.Name;

                //Details.AssetId = consumable.Details.Icon
            }
        }
    }
}
