using Kenedia.Modules.BuildsManager.Models.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kenedia.Modules.BuildsManager.Extensions
{
    public static class GearSubSlotTypeExtension
    {
        public static GearSubSlotTypeType GetGroupType(this TemplateSlotType templateSlotType)
        {
            return templateSlotType switch
            {
                TemplateSlotType.Head or TemplateSlotType.Shoulder or TemplateSlotType.Chest or TemplateSlotType.Hand or TemplateSlotType.Leg or TemplateSlotType.Foot or TemplateSlotType.Back or TemplateSlotType.AquaBreather => GearSubSlotTypeType.Armor,
                TemplateSlotType.Amulet or TemplateSlotType.Accessory_1 or TemplateSlotType.Accessory_2 or TemplateSlotType.Ring_1 or TemplateSlotType.Ring_2 or TemplateSlotType.Back => GearSubSlotTypeType.Trinket,
                TemplateSlotType.MainHand or TemplateSlotType.OffHand or TemplateSlotType.Aquatic or TemplateSlotType.AltAquatic or TemplateSlotType.AltMainHand or TemplateSlotType.AltOffHand => GearSubSlotTypeType.Weapon,
                TemplateSlotType.Nourishment => GearSubSlotTypeType.Nourishment,
                TemplateSlotType.Enhancement => GearSubSlotTypeType.Enhancement,
                TemplateSlotType.PowerCore => GearSubSlotTypeType.PowerCore,
                TemplateSlotType.Relic => GearSubSlotTypeType.Relic,
                TemplateSlotType.None => GearSubSlotTypeType.None,
                TemplateSlotType.PvpAmulet => GearSubSlotTypeType.PvpAmulet,
                _ => GearSubSlotTypeType.None,
            };
        }
    }

    public enum GearSubSlotTypeType
    {
        None = -1,
        Armor,
        Weapon,
        Trinket,
        Relic,
        Nourishment,
        Enhancement,
        PowerCore,
        PvpAmulet,
    }
}
