using Kenedia.Modules.BuildsManager.Models.Templates;
using System;
using System.Collections.Generic;

namespace Kenedia.Modules.BuildsManager.Controls_Old.BuildPage
{
    public class SkillSlotCollection<T> : Dictionary<BuildSkillSlotType, T> where T : class, new()
    {
        public SkillSlotCollection()
        {
            foreach (BuildSkillSlotType e in Enum.GetValues(typeof(BuildSkillSlotType)))
            {
                Add(e, new());
            }
        }
    }
}
