﻿using Gw2Sharp.ChatLinks;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.AdvancedBuildsManager.Models.Templates;
using static Kenedia.Modules.AdvancedBuildsManager.DataModels.Professions.Weapon;
using Color = Microsoft.Xna.Framework.Color;

namespace Kenedia.Modules.AdvancedBuildsManager.Extensions
{
    public static class AttributeTypeExtension
    {
        public static string GetDisplayName(this AttributeType attribute)
        {
            string concentration = "Concentration";
            string expertise = "Expertise";
            string ConditionDamage = "Condition Damage";
            string Healing = "Healing Power";
            string CritDamage = "Ferocity";

            return attribute switch
            {
                AttributeType.BoonDuration => concentration,
                AttributeType.ConditionDuration => expertise,
                AttributeType.ConditionDamage => ConditionDamage,
                AttributeType.Healing => Healing,
                AttributeType.CritDamage => CritDamage,
                _ => attribute.ToString(),
            };
        }
    }

    public static class ItemWeaponTypeExtension
    {
        public static bool IsWeaponType(this ItemWeaponType type, WeaponType weaponType)
        {
            return
                type == ItemWeaponType.Harpoon
                ? weaponType is WeaponType.Harpoon or WeaponType.Spear

                : type == ItemWeaponType.ShortBow
                ? weaponType is WeaponType.ShortBow or WeaponType.Shortbow

                : type == ItemWeaponType.LongBow
                ? weaponType is WeaponType.LongBow or WeaponType.Longbow

                : weaponType.ToString() == type.ToString();
        }
    }

    public static class WeaponTypeExtension
    {
        public static bool IsItemWeaponType(this WeaponType weaponType, ItemWeaponType type)
        {
            return
                type == ItemWeaponType.Harpoon
                ? weaponType is WeaponType.Harpoon or WeaponType.Spear

                : type == ItemWeaponType.ShortBow
                ? weaponType is WeaponType.ShortBow or WeaponType.Shortbow

                : type == ItemWeaponType.LongBow
                ? weaponType is WeaponType.LongBow or WeaponType.Longbow

                : weaponType.ToString() == type.ToString();
        }

        public static bool IsAquatic(this WeaponType weaponType)
        {
            return weaponType switch
            {
                WeaponType.Spear => true,
                WeaponType.Trident => true,
                WeaponType.Speargun => true,
                _ => false
            };
        }

        public static bool IsTwoHanded(this WeaponType weaponType)
        {
            return weaponType switch
            {
                WeaponType.Greatsword => true,
                WeaponType.Hammer => true,
                WeaponType.Longbow => true,
                WeaponType.Rifle => true,
                WeaponType.Shortbow => true,
                WeaponType.Staff => true,
                WeaponType.Spear => true,
                WeaponType.Trident => true,
                WeaponType.Speargun => true,
                _ => false
            };
        }
    }

    public static class GearTemplateSlotExtension
    {
        public static bool IsArmor(this GearTemplateSlot slot)
        {
            return slot is GearTemplateSlot.Head or GearTemplateSlot.Shoulder or GearTemplateSlot.Chest or GearTemplateSlot.Hand or GearTemplateSlot.Leg or GearTemplateSlot.Foot or GearTemplateSlot.AquaBreather;
        }

        public static bool IsWeapon(this GearTemplateSlot slot)
        {
            return slot is GearTemplateSlot.MainHand or GearTemplateSlot.AltMainHand or GearTemplateSlot.OffHand or GearTemplateSlot.AltOffHand or GearTemplateSlot.Aquatic or GearTemplateSlot.AltAquatic;
        }

        public static bool IsJewellery(this GearTemplateSlot slot)
        {
            return slot is GearTemplateSlot.Back or GearTemplateSlot.Amulet or GearTemplateSlot.Ring_1 or GearTemplateSlot.Ring_2 or GearTemplateSlot.Accessory_1 or GearTemplateSlot.Accessory_2;
        }
    }

    public static class ChatLinkExtension
    {
        public static string CreateChatLink(this Trait trait)
        {
            var link = new TraitChatLink() { TraitId = trait.Id };
            byte[] bytes = link.ToArray();
            link.Parse(bytes);

            return link.ToString();
        }
    }
}
