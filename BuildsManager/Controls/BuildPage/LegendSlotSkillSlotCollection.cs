using Kenedia.Modules.BuildsManager.Models.Templates;
using System;
using System.Collections.Generic;

namespace Kenedia.Modules.BuildsManager.Controls.BuildPage
{
    public class LegendSlotSkillSlotCollection<T> : Dictionary<LegendSlot, SkillSlotCollection<T>> where T : class, new()
    {
        public LegendSlotSkillSlotCollection()
        {
            foreach (LegendSlot e in Enum.GetValues(typeof(LegendSlot)))
            {
                Add(e, new());
            }
        }
    }
}
