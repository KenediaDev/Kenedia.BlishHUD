using Kenedia.Modules.Characters.Enums;
using static Kenedia.Modules.Characters.Services.Data;

namespace Kenedia.Modules.Characters.Extensions
{
    public static class Gw2SharpExtension
    {
        public static CrafingProfession GetData(this int key)
        {
            _ = Characters.ModuleInstance.Data.CrafingProfessions.TryGetValue(key, out CrafingProfession value);

            return value;
        }

        public static Profession GetData(this Gw2Sharp.Models.ProfessionType key)
        {
            _ = Characters.ModuleInstance.Data.Professions.TryGetValue(key, out Profession value);

            return value;
        }

        public static Specialization GetData(this SpecializationType key)
        {
            _ = Characters.ModuleInstance.Data.Specializations.TryGetValue(key, out Specialization value);

            return value;
        }

        public static Race GetData(this Gw2Sharp.Models.RaceType key)
        {
            _ = Characters.ModuleInstance.Data.Races.TryGetValue(key, out Race value);

            return value;
        }
    }
}
