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
