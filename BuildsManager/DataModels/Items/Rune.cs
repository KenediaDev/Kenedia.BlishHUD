using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.BuildsManager.Models.Templates;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Kenedia.Modules.BuildsManager.DataModels.Items
{
    [DataContract]
    public class Rune : BaseItem
    {
        public Rune()
        {
            TemplateSlot = GearTemplateSlot.None;

        }

        [DataMember]
        public List<BonusStat> Bonuses { get; set; } = new();

        [DataMember]
        public RuneBonuses BonusDescriptions { get; set; } = new();

        public override void Apply(Item item)
        {
            base.Apply(item);

            if (item.Type == ItemType.UpgradeComponent)
            {
                var upgrade = (ItemUpgradeComponent)item;
                if (upgrade.Details.Bonuses != null)
                {
                    BonusDescriptions.AddBonuses(upgrade.Details.Bonuses);
                }
            }
        }
    }
}
