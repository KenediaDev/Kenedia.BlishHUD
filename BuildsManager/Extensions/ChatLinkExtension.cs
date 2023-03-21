using Gw2Sharp.ChatLinks;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.BuildsManager.Models.Templates;
using static Kenedia.Modules.BuildsManager.DataModels.Professions.Weapon;
using Color = Microsoft.Xna.Framework.Color;

namespace Kenedia.Modules.BuildsManager.Extensions
{
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
        public static bool IsArmor(this GearTemplateSlot slot)
        {
            return slot is GearTemplateSlot.Head or GearTemplateSlot.Shoulder or GearTemplateSlot.Chest or GearTemplateSlot.Hand or GearTemplateSlot.Leg or GearTemplateSlot.Foot or GearTemplateSlot.AquaBreather;
        }

        public static bool IsWeapon(this GearTemplateSlot slot)
        {
            return slot is GearTemplateSlot.MainHand or GearTemplateSlot.AltMainHand or GearTemplateSlot.OffHand or GearTemplateSlot.AltOffHand or GearTemplateSlot.Aquatic or GearTemplateSlot.AltAquatic;
        }

        public static bool IsJuwellery(this GearTemplateSlot slot)
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
