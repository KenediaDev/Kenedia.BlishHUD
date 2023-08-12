using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kenedia.Modules.BuildsManager.Utility
{
    public static class WeaponTypeConversion
    {
        public static ItemWeaponType ToItemWeaponType(this Weapon.WeaponType professionWeaponType)
        {
            string enumName =
                professionWeaponType.ToString();

            _ = Enum.TryParse(enumName, out ItemWeaponType itemWeaponType);
            return itemWeaponType;
        }
    }
}
