using Kenedia.Modules.BuildsManager.Models.Templates;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kenedia.Modules.BuildsManager.Controls.BuildPage
{
    public class SkillIconCollection : Dictionary<SkillSlot, SkillIcon>
    {
        public SkillIconCollection(bool showSelector = false)
        {
            foreach (SkillSlot slot in Enum.GetValues(typeof(SkillSlot)))
            {
                if (slot >= SkillSlot.Heal)
                {
                    foreach (SkillSlot state in new SkillSlot[] { SkillSlot.Active, SkillSlot.Inactive })
                    {
                        foreach (SkillSlot enviroment in new SkillSlot[] { SkillSlot.Terrestrial, SkillSlot.Aquatic })
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
