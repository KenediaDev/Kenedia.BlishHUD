using Kenedia.Modules.BuildsManager.Models.Templates;

namespace Kenedia.Modules.BuildsManager.Extensions
{
    public static class GearTemplateSlotExtension
    {
        public static bool IsArmor(this TemplateSlotType slot)
        {
            return slot is TemplateSlotType.Head or TemplateSlotType.Shoulder or TemplateSlotType.Chest or TemplateSlotType.Hand or TemplateSlotType.Leg or TemplateSlotType.Foot or TemplateSlotType.AquaBreather;
        }

        public static bool IsWeapon(this TemplateSlotType slot)
        {
            return slot is TemplateSlotType.MainHand or TemplateSlotType.AltMainHand or TemplateSlotType.OffHand or TemplateSlotType.AltOffHand or TemplateSlotType.Aquatic or TemplateSlotType.AltAquatic;
        }

        public static bool IsAquatic(this TemplateSlotType slot)
        {
            return slot is TemplateSlotType.Aquatic or TemplateSlotType.AltAquatic or TemplateSlotType.AquaBreather;
        }

        public static bool IsOffhand(this TemplateSlotType slot)
        {
            return slot is TemplateSlotType.OffHand or TemplateSlotType.AltOffHand;
        }

        public static TemplateSlotType? GetOffhand(this TemplateSlotType slot)
        {
            return slot switch
            {
                TemplateSlotType.MainHand => TemplateSlotType.OffHand,
                TemplateSlotType.AltMainHand => TemplateSlotType.AltOffHand,
                _ => null,
            };
        }

        public static TemplateSlotType? GetMainHand(this TemplateSlotType slot)
        {
            return slot switch
            {
                TemplateSlotType.OffHand => TemplateSlotType.MainHand,
                TemplateSlotType.AltOffHand => TemplateSlotType.AltMainHand,
                _ => null,
            };
        }

        public static bool IsJewellery(this TemplateSlotType slot)
        {
            return slot is TemplateSlotType.Back or TemplateSlotType.Amulet or TemplateSlotType.Ring_1 or TemplateSlotType.Ring_2 or TemplateSlotType.Accessory_1 or TemplateSlotType.Accessory_2;
        }
    }
}
