using Gw2Sharp.Models;
using Gw2Sharp.WebApi.V2.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kenedia.Modules.Core.Extensions
{
    public static class ProfessionTypeExtension
    {
        public static ItemWeightType GetArmorType(this ProfessionType prof)
        {
            return prof switch
            {
                ProfessionType.Warrior or ProfessionType.Guardian or ProfessionType.Revenant => ItemWeightType.Heavy,
                ProfessionType.Ranger or ProfessionType.Thief or ProfessionType.Engineer => ItemWeightType.Medium,
                ProfessionType.Mesmer or ProfessionType.Elementalist or ProfessionType.Necromancer => ItemWeightType.Medium,
                _ => ItemWeightType.Unknown,
            };
        }
    }
}
