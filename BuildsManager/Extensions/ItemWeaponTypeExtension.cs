using Gw2Sharp.WebApi.V2.Models;
using static Kenedia.Modules.BuildsManager.DataModels.Professions.Weapon;

namespace Kenedia.Modules.BuildsManager.Extensions
{
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
}
