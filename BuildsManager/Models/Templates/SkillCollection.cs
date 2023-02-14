using Kenedia.Modules.BuildsManager.DataModels.Professions;
using System;
using System.Collections.Generic;

namespace Kenedia.Modules.BuildsManager.Models.Templates
{
    public class SkillCollection : Dictionary<BuildSkillSlot, Skill>
    {
        public SkillCollection()
        {
            foreach (BuildSkillSlot e in Enum.GetValues(typeof(BuildSkillSlot)))
            {
                Add(e, null);
            }

        }

        public ushort GetPaletteId(BuildSkillSlot slot)
        {
            return (ushort)(TryGetValue(slot, out Skill skill) && skill != null ? skill.PaletteId : 0);
        }
    }
}
