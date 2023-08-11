using Kenedia.Modules.BuildsManager.Models.Templates;
using SkillSlot = Gw2Sharp.WebApi.V2.Models.SkillSlot;

namespace Kenedia.Modules.BuildsManager.Utility
{
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

        public static BuildSkillSlotType GetBuildSkillSlot(this SkillSlot? slot)
        {
            return slot switch
            {
                SkillSlot.Elite=> BuildSkillSlotType.Elite,
                SkillSlot.Heal=> BuildSkillSlotType.Heal,
                SkillSlot.Utility=> BuildSkillSlotType.Utility_1,
                _ => BuildSkillSlotType.Utility_1,
            };
        }
    }
}
