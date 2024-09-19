using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.BuildsManager.Models.Templates;
using static Kenedia.Modules.BuildsManager.DataModels.Professions.Weapon;

namespace Kenedia.Modules.BuildsManager.Extensions
{
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

        public static bool IsAquatic(this WeaponType weaponType, TemplateSlotType templateSlotType = TemplateSlotType.Aquatic)
        {
            return weaponType switch
            {
                WeaponType.Spear => templateSlotType is TemplateSlotType.Aquatic or TemplateSlotType.AltAquatic,
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
}
