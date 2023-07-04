using System;
using System.Collections.Generic;

namespace Kenedia.Modules.AdvancedBuildsManager.Models.Templates
{
    public class UpgradeCollection : Dictionary<UpgradeSlot, int>
    {
        public UpgradeCollection()
        {
            foreach (UpgradeSlot e in Enum.GetValues(typeof(UpgradeSlot)))
            {
                Add(e, 0);
            }
        }
    }
}
