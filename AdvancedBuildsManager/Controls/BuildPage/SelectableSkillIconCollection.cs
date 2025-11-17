using SkillSlot = Gw2Sharp.WebApi.V2.Models.SkillSlot;
using System.Collections.Generic;

namespace Kenedia.Modules.AdvancedBuildsManager.Controls.BuildPage
{
    public class SelectableSkillIconCollection : Dictionary<SkillSlot, List<SkillIcon>>
    {
        public SelectableSkillIconCollection()
        {
            Add(SkillSlot.Heal, []);
            Add(SkillSlot.Utility, []);
            Add(SkillSlot.Elite, []);
        }
    }
}
