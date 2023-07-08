using Kenedia.Modules.BuildsManager.Models.Templates;
using System;
using System.Collections.Generic;

namespace Kenedia.Modules.BuildsManager.Controls.BuildPage
{
    public class SkillSlotCollection<T> : Dictionary<BuildSkillSlot, T> where T : class, new()
    {
        public SkillSlotCollection()
        {
            foreach (BuildSkillSlot e in Enum.GetValues(typeof(BuildSkillSlot)))
            {
                Add(e, new());
            }
        }
    }
}
