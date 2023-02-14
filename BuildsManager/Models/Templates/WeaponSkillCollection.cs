using Kenedia.Modules.BuildsManager.DataModels.Professions;
using System;
using System.Collections.Generic;

namespace Kenedia.Modules.BuildsManager.Models.Templates
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
            return (ushort)(TryGetValue(slot, out Skill skill) && skill != null ? skill.PaletteId : 0);
        }
    }
}
