using Gw2Sharp.Models;
using Kenedia.Modules.Characters.Enums;
using System.Collections.Generic;
using static Kenedia.Modules.Characters.Services.Data;

namespace Kenedia.Modules.Characters.Extensions
{
    public static class Gw2SharpExtension
    {
        public static CraftingProfession GetData(this int key, Dictionary<int, CraftingProfession> data)
        {
            _ = data.TryGetValue(key, out CraftingProfession value);

            return value;
        }

        public static Profession GetData(this ProfessionType key, Dictionary<ProfessionType, Profession> data)
        {
            _ = data.TryGetValue(key, out Profession value);
            return value;
        }

        public static Specialization GetData(this SpecializationType key, Dictionary<SpecializationType, Specialization> data)
        {
            _ = data.TryGetValue(key, out Specialization value);

            return value;
        }

        public static Race GetData(this RaceType key, Dictionary<RaceType, Race> data)
        {
            _ = data.TryGetValue(key, out Race value);

            return value;
        }
    }
}
