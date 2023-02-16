using Kenedia.Modules.BuildsManager.Models.Templates;
using System;
using System.Collections.Generic;

namespace Kenedia.Modules.BuildsManager.Controls.BuildPage
{
    public class WeaponSkillIconCollection : Dictionary<WeaponSkillSlot, SkillIcon>
    {
        public WeaponSkillIconCollection()
        {
            foreach (WeaponSkillSlot e in Enum.GetValues(typeof(WeaponSkillSlot)))
            {
                Add(e, new());
            }
        }
    }
}
