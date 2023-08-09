using Gw2Sharp.ChatLinks;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.BuildsManager.Models.Templates;
using static Kenedia.Modules.BuildsManager.DataModels.Professions.Weapon;
using Color = Microsoft.Xna.Framework.Color;

namespace Kenedia.Modules.BuildsManager.Extensions
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

    public static class RarityExtension
    {
        public static Color GetColor(this ItemRarity rarity)
        {
            return rarity switch
            {
                ItemRarity.Junk => Color.DarkGray,
                ItemRarity.Basic => new(200, 200, 200),
                ItemRarity.Fine => new(74, 146, 236),
                ItemRarity.Masterwork => new(43, 184, 14),
                ItemRarity.Rare => new(237, 214, 30),
                ItemRarity.Exotic => new(235, 154, 1),
                ItemRarity.Ascended => new(234, 58, 132),
                ItemRarity.Legendary => new(159, 47, 244),
                _ => Color.White
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
        public static bool IsArmor(this TemplateSlot slot)
        {
            return slot is TemplateSlot.Head or TemplateSlot.Shoulder or TemplateSlot.Chest or TemplateSlot.Hand or TemplateSlot.Leg or TemplateSlot.Foot or TemplateSlot.AquaBreather;
        }

        public static bool IsWeapon(this TemplateSlot slot)
        {
            return slot is TemplateSlot.MainHand or TemplateSlot.AltMainHand or TemplateSlot.OffHand or TemplateSlot.AltOffHand or TemplateSlot.Aquatic or TemplateSlot.AltAquatic;
        }

        public static bool IsJuwellery(this TemplateSlot slot)
        {
            return slot is TemplateSlot.Back or TemplateSlot.Amulet or TemplateSlot.Ring_1 or TemplateSlot.Ring_2 or TemplateSlot.Accessory_1 or TemplateSlot.Accessory_2;
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
