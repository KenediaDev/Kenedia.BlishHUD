using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.BuildsManager.Extensions;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using ItemType = Kenedia.Modules.Core.DataModels.ItemType;

namespace Kenedia.Modules.BuildsManager.DataModels.Items
{
    [DataContract]
    public class PvpAmulet : BaseItem
    {
        public PvpAmulet()
        {
            TemplateSlot = TemplateSlotType.PvpAmulet;
            Type = ItemType.PvpAmulet;
        }

        [DataMember]
        public ItemAttributes Attributes { get; set; } = new();

        public string AttributesString
        {
            get
            {
                List<(int amount, string text)> result = new();

                if (Attributes.Power is not null)
                {
                    result.Add(new((int)Attributes.Power, $"+ {Attributes.Power} {AttributeType.Power.GetDisplayName()}"));
                }

                if (Attributes.Precision is not null)
                {
                    result.Add(new((int)Attributes.Precision, $"+ {Attributes.Precision} {AttributeType.Precision.GetDisplayName()}"));
                }

                if (Attributes.Toughness is not null)
                {
                    result.Add(new((int)Attributes.Toughness, $"+ {Attributes.Toughness} {AttributeType.Toughness.GetDisplayName()}"));
                }

                if (Attributes.Vitality is not null)
                {
                    result.Add(new((int)Attributes.Vitality, $"+ {Attributes.Vitality} {AttributeType.Vitality.GetDisplayName()}"));
                }

                if (Attributes.CritDamage is not null)
                {
                    result.Add(new((int)Attributes.CritDamage, $"+ {Attributes.CritDamage} {AttributeType.CritDamage.GetDisplayName()}"));
                }

                if (Attributes.Healing is not null)
                {
                    result.Add(new((int)Attributes.Healing, $"+ {Attributes.Healing} {AttributeType.Healing.GetDisplayName()}"));
                }

                if (Attributes.ConditionDamage is not null)
                {
                    result.Add(new((int)Attributes.ConditionDamage, $"+ {Attributes.ConditionDamage} {AttributeType.ConditionDamage.GetDisplayName()}"));
                }

                if (Attributes.BoonDuration is not null)
                {
                    result.Add(new((int)Attributes.BoonDuration, $"+ {Attributes.BoonDuration} {AttributeType.BoonDuration.GetDisplayName()}"));
                }

                if (Attributes.ConditionDuration is not null)
                {
                    result.Add(new((int)Attributes.ConditionDuration, $"+ {Attributes.ConditionDuration} {AttributeType.ConditionDuration.GetDisplayName()}"));
                }

                return string.Join(Environment.NewLine, result.OrderByDescending(e => e.amount).Select(e => e.text));
            }
        }

        public override void Apply(Gw2Sharp.WebApi.V2.Models.PvpAmulet amulet)
        {
            base.Apply(amulet);
            Attributes = amulet.Attributes;
        }
    }
}
