using Kenedia.Modules.BuildsManager.Models.Templates;
using System;
using System.Collections.Generic;

namespace Kenedia.Modules.BuildsManager.Controls.BuildPage
{
    public class SkillIconCollection : Dictionary<BuildSkillSlot, SkillIcon>
    {
        public SkillIconCollection()
        {
            foreach (BuildSkillSlot e in Enum.GetValues(typeof(BuildSkillSlot)))
            {
                Add(e, new());
            }
        }
    }
}
