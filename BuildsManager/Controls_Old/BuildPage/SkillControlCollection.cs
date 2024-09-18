using Kenedia.Modules.BuildsManager.Models.Templates;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kenedia.Modules.BuildsManager.Controls_Old.BuildPage
{
    public class SkillControlCollection : Dictionary<SkillSlotType, SkillControl>
    {
        public SkillControlCollection(bool showSelector = false)
        {
            foreach (SkillSlotType slot in Enum.GetValues(typeof(SkillSlotType)))
            {
                if (slot >= SkillSlotType.Heal)
                {
                    foreach (SkillSlotType state in new SkillSlotType[] { SkillSlotType.Active, SkillSlotType.Inactive })
                    {
                        foreach (SkillSlotType enviroment in new SkillSlotType[] { SkillSlotType.Terrestrial, SkillSlotType.Aquatic })
                        {
                            Add(state | enviroment | slot, new() { });
                        }
                    }
                }
            }
        }

        public void Wipe()
        {
            foreach (var key in Keys.ToList())
            {
                if(this[key] is not null) this[key].Skill = default;
            }
        }
    }
}
