using Kenedia.Modules.BuildsManager.Models.Templates;
using SkillSlot = Gw2Sharp.WebApi.V2.Models.SkillSlot;

namespace Kenedia.Modules.BuildsManager.Utility
{
    public class StatUtility
    {
        public static string UniformAttributeName(string statName)
        {
            return statName switch
            {
                "ConditionDamage" => "Condition Damage",
                "BoonDuration" => "Concentration",
                "ConditionDuration" => "Expertise",
                "Healing" => "Healing Power",
                "CritDamage" => "Ferocity",
                _ => statName,
            };
        }
    }

   public static class SkillSlotConversion
    {
        public static WeaponSkillSlot GetSkillSlot(this SkillSlot? slot)
        {
            return slot switch
            {
                SkillSlot.Weapon1 => WeaponSkillSlot.Weapon_1,
                SkillSlot.Weapon2 => WeaponSkillSlot.Weapon_2,
                SkillSlot.Weapon3 => WeaponSkillSlot.Weapon_3,
                SkillSlot.Weapon4 => WeaponSkillSlot.Weapon_4,
                SkillSlot.Weapon5 => WeaponSkillSlot.Weapon_5,
                _ => WeaponSkillSlot.Weapon_1,
            };
        }

        public static BuildSkillSlot GetBuildSkillSlot(this SkillSlot? slot)
        {
            return slot switch
            {
                SkillSlot.Elite=> BuildSkillSlot.Elite,
                SkillSlot.Heal=> BuildSkillSlot.Heal,
                SkillSlot.Utility=> BuildSkillSlot.Utility_1,
                _ => BuildSkillSlot.Utility_1,
            };
        }
    }
}
