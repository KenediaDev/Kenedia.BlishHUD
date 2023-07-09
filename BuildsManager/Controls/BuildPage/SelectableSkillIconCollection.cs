using SkillSlot = Gw2Sharp.WebApi.V2.Models.SkillSlot;
using System.Collections.Generic;

namespace Kenedia.Modules.BuildsManager.Controls.BuildPage
{
    public class SkillIconSelectionCollection : Dictionary<SkillSlot, List<SkillIcon>>
    {
        public SkillIconSelectionCollection()
        {
            Add(SkillSlot.Heal, new());
            Add(SkillSlot.Utility, new());
            Add(SkillSlot.Elite, new());
        }
    }
}
