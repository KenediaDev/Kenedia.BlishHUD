using Kenedia.Modules.BuildsManager.Models.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kenedia.Modules.BuildsManager.Extensions
{
    public static class TemplateSlotTypeExtension
    {
        public static TemplateSlotType[] GetGroup(this TemplateSlotType slot)
        {
            return slot switch
            {
                TemplateSlotType.Head or TemplateSlotType.Shoulder or TemplateSlotType.Chest or TemplateSlotType.Hand or TemplateSlotType.Leg or TemplateSlotType.Foot or TemplateSlotType.AquaBreather => [TemplateSlotType.Head, TemplateSlotType.Shoulder, TemplateSlotType.Chest, TemplateSlotType.Hand, TemplateSlotType.Leg, TemplateSlotType.Foot, TemplateSlotType.AquaBreather],
                TemplateSlotType.MainHand or TemplateSlotType.OffHand or TemplateSlotType.Aquatic or TemplateSlotType.AltMainHand or TemplateSlotType.AltOffHand or TemplateSlotType.AltAquatic => [TemplateSlotType.MainHand, TemplateSlotType.AltMainHand, TemplateSlotType.OffHand, TemplateSlotType.AltOffHand, TemplateSlotType.Aquatic, TemplateSlotType.AltAquatic],
                TemplateSlotType.Back or TemplateSlotType.Amulet or TemplateSlotType.Accessory_1 or TemplateSlotType.Accessory_2 or TemplateSlotType.Ring_1 or TemplateSlotType.Ring_2 => [TemplateSlotType.Back, TemplateSlotType.Amulet, TemplateSlotType.Accessory_1, TemplateSlotType.Accessory_2, TemplateSlotType.Ring_1, TemplateSlotType.Ring_2],
                _ => [],
            };
        }
    }
}
