using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.AdvancedBuildsManager.Models.Templates;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Kenedia.Modules.AdvancedBuildsManager.DataModels.Items
{
    [DataContract]
    public class Utility : BaseItem
    {
        public Utility()
        {
            TemplateSlot = GearTemplateSlot.Utility;

        }

        [DataMember]
        public List<BonusStat> Bonuses { get; set; } = new();

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
