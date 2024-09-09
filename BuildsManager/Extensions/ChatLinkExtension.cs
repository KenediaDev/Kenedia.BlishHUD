using Gw2Sharp.ChatLinks;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.BuildsManager.Res;
using static Kenedia.Modules.BuildsManager.DataModels.Professions.Weapon;
using Color = Microsoft.Xna.Framework.Color;

namespace Kenedia.Modules.BuildsManager.Extensions
{
    public static class VersionExtension
    {
        public static SemVer.Version Increment(this SemVer.Version version)
        {
            return version = new(version.Major, version.Minor, version.Patch + 1);            
        }
    }
    public static class AttributeTypeExtension
    {
        public static string GetDisplayName(this AttributeType attribute)
        {
            return attribute switch
            {
                AttributeType.BoonDuration => strings.BoonDuration,
                AttributeType.ConditionDamage => strings.ConditionDamage,
                AttributeType.ConditionDuration => strings.ConditionDuration,
                AttributeType.CritDamage => strings.CritDamage,
                AttributeType.Healing => strings.Healing,
                AttributeType.Power => strings.Power,
                AttributeType.Precision => strings.Precision,
                AttributeType.Toughness => strings.Toughness,
                AttributeType.Vitality => strings.Vitality,
                AttributeType.AgonyResistance => strings.AgonyResistance,
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
        public static bool IsArmor(this TemplateSlotType slot)
        {
            return slot is TemplateSlotType.Head or TemplateSlotType.Shoulder or TemplateSlotType.Chest or TemplateSlotType.Hand or TemplateSlotType.Leg or TemplateSlotType.Foot or TemplateSlotType.AquaBreather;
        }

        public static bool IsWeapon(this TemplateSlotType slot)
        {
            return slot is TemplateSlotType.MainHand or TemplateSlotType.AltMainHand or TemplateSlotType.OffHand or TemplateSlotType.AltOffHand or TemplateSlotType.Aquatic or TemplateSlotType.AltAquatic;
        }

        public static bool IsJewellery(this TemplateSlotType slot)
        {
            return slot is TemplateSlotType.Back or TemplateSlotType.Amulet or TemplateSlotType.Ring_1 or TemplateSlotType.Ring_2 or TemplateSlotType.Accessory_1 or TemplateSlotType.Accessory_2;
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
