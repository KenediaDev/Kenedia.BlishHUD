using Kenedia.Modules.BuildsManager.Models.Templates;
using System;
using System.Collections.Generic;

namespace Kenedia.Modules.BuildsManager.Controls_Old.BuildPage
{
    public class LegendSlotSkillSlotCollection<T> : Dictionary<LegendSlotType, SkillSlotCollection<T>> where T : class, new()
    {
        public LegendSlotSkillSlotCollection()
        {
            foreach (LegendSlotType e in Enum.GetValues(typeof(LegendSlotType)))
            {
                Add(e, new());
            }
        }
    }
}
