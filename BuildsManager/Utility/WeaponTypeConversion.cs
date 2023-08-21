using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using System;
using System.Collections.Generic;

namespace Kenedia.Modules.BuildsManager.Utility
{
    public static class WeaponTypeConversion
    {
        public static ItemWeaponType ToItemWeaponType(this Weapon.WeaponType professionWeaponType)
        {
            Dictionary<Weapon.WeaponType, string> names = new()
            {
                {Weapon.WeaponType.Longbow, "LongBow"},
            };

            string enumName = names.TryGetValue(professionWeaponType, out string streamlinedName) ? streamlinedName :
                professionWeaponType.ToString();

            _ = Enum.TryParse(enumName, out ItemWeaponType itemWeaponType);
            return itemWeaponType;
        }
    }
}
