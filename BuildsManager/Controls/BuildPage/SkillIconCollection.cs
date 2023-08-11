using Kenedia.Modules.BuildsManager.Models.Templates;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kenedia.Modules.BuildsManager.Controls.BuildPage
{
    public class SkillIconCollection : Dictionary<SkillSlotType, SkillIcon>
    {
        public SkillIconCollection(bool showSelector = false)
        {
            foreach (SkillSlotType slot in Enum.GetValues(typeof(SkillSlotType)))
            {
                if (slot >= SkillSlotType.Heal)
                {
                    foreach (SkillSlotType state in new SkillSlotType[] { SkillSlotType.Active, SkillSlotType.Inactive })
                    {
                        foreach (SkillSlotType enviroment in new SkillSlotType[] { SkillSlotType.Terrestrial, SkillSlotType.Aquatic })
                        {
                            Add(state | enviroment | slot, new() { ShowSelector = showSelector });
                        }
                    }
                }
            }
        }

        public void Wipe()
        {
            foreach (var key in Keys.ToList())
            {
                if(this[key] != null) this[key].Skill = default;
            }
        }
    }
}
