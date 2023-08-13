using Kenedia.Modules.AdvancedBuildsManager.DataModels.Professions;
using System;
using System.Collections.Generic;

namespace Kenedia.Modules.AdvancedBuildsManager.Models.Templates
{
    public class WeaponSkillCollection : Dictionary<WeaponSkillSlot, Skill>
    {
        public WeaponSkillCollection()
        {
            foreach (WeaponSkillSlot e in Enum.GetValues(typeof(WeaponSkillSlot)))
            {
                Add(e, null);
            }

        }

        public ushort GetPaletteId(WeaponSkillSlot slot)
        {
            return (ushort)(TryGetValue(slot, out Skill skill) && skill is not null ? skill.PaletteId : 0);
        }
    }
}
