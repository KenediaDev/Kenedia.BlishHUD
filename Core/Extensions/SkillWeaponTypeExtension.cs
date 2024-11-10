using Gw2Sharp.WebApi.V2.Models;
using System;

namespace Kenedia.Modules.Core.Extensions
{
    public static class SkillWeaponTypeExtension
    {
        public static ItemWeaponType ToItemWeapon(this SkillWeaponType skillWeaponType)
        {
            string enumName =
                skillWeaponType == SkillWeaponType.Spear ? "Harpoon" :
                skillWeaponType.ToString();

            _ = Enum.TryParse(enumName, out ItemWeaponType itemWeaponType);
            return itemWeaponType;
        }
    }

    public static class ItemWeaponTypeExtension
    {
        public static bool IsTwoHanded(this ItemWeaponType itemWeaponType)
        {
            return itemWeaponType
                 is ItemWeaponType.Greatsword
                 or ItemWeaponType.Hammer
                 or ItemWeaponType.LongBow
                 or ItemWeaponType.Rifle
                 or ItemWeaponType.ShortBow
                 or ItemWeaponType.Staff
                 or ItemWeaponType.Harpoon
                 or ItemWeaponType.Speargun
                 or ItemWeaponType.Trident;
        }

        public static bool IsOneHanded(this ItemWeaponType itemWeaponType)
        {
            return itemWeaponType is
                ItemWeaponType.Axe or
                ItemWeaponType.Dagger or
                ItemWeaponType.Mace or
                ItemWeaponType.Pistol or
                ItemWeaponType.Scepter or
                ItemWeaponType.Sword;
        }

        public static bool IsOffHand(this ItemWeaponType itemWeaponType)
        {
            return itemWeaponType is
                ItemWeaponType.Focus or
                ItemWeaponType.Shield or
                ItemWeaponType.Torch or
                ItemWeaponType.Warhorn;
        }

        public static bool IsMainHand(this ItemWeaponType itemWeaponType)
        {
            return itemWeaponType is
                ItemWeaponType.Scepter;
        }

        public static SkillWeaponType ToSkillWeapon(this ItemWeaponType itemWeaponType)
        {
            string enumName =
                itemWeaponType == ItemWeaponType.Harpoon ? "Spear" :
                itemWeaponType.ToString();

            _ = Enum.TryParse(enumName, out SkillWeaponType skillWeaponType);
            return skillWeaponType;
        }
    }
}
