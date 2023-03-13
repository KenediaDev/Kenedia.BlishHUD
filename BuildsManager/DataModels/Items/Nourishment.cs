using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.BuildsManager.Models.Templates;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace Kenedia.Modules.BuildsManager.DataModels.Items
{
    [DataContract]
    public class Nourishment : BaseItem
    {
        public Nourishment()
        {
            TemplateSlot = GearTemplateSlot.Nourishment;

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
