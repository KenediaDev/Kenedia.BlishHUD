using SkillSlot = Gw2Sharp.WebApi.V2.Models.SkillSlot;
using System.Collections.Generic;

namespace Kenedia.Modules.BuildsManager.Controls.BuildPage
{
    public class SelectableSkillIconCollection : Dictionary<SkillSlot, List<SkillIcon>>
    {
        public SelectableSkillIconCollection()
        {
            Add(SkillSlot.Heal, new());
            Add(SkillSlot.Utility, new());
            Add(SkillSlot.Elite, new());
        }
    }
}
