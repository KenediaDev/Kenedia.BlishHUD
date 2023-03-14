using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.Core.Models;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Kenedia.Modules.BuildsManager.DataModels.Items
{
    [DataContract]
    public class Enrichment : BaseItem
    {
        [DataMember]
        public LocalizedString Bonuses { get; set; } = new();
        public string Bonus
        {
            get => Bonuses.Text;
            set => Bonuses.Text = value;
        }

        public List<BonusStat> Attributes { get; set; } = new();

        public override void Apply(Item item)
        {
            base.Apply(item);

            if (item.Type == ItemType.UpgradeComponent)
            {
                var upgrade = (ItemUpgradeComponent)item;

                if (upgrade.Details.InfixUpgrade != null && upgrade.Details.InfixUpgrade.Attributes != null)
                {
                    Bonus = upgrade.Details.InfixUpgrade.Buff.Description;
                    DisplayText = upgrade.Details.InfixUpgrade.Buff.Description.Trim();

                    foreach (var b in upgrade.Details.InfixUpgrade.Attributes)
                    {
                        Attributes.Add(new()
                        {
                            Amount = b.Modifier,
                            Type = Enum.TryParse(b.Attribute.ToString(), out BonusType type) ? type : BonusType.Unkown,
                        });
                    }
                }
            }
        }
    }
}
