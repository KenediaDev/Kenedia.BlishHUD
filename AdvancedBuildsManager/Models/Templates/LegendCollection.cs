using Kenedia.Modules.AdvancedBuildsManager.DataModels.Professions;
using Kenedia.Modules.Core.Models;
using System;

namespace Kenedia.Modules.AdvancedBuildsManager.Models.Templates
{
    public class LegendCollection : ObservableDictionary<LegendSlot, Legend>
    {
        public LegendCollection()
        {
            foreach (LegendSlot e in Enum.GetValues(typeof(LegendSlot)))
            {
                Add(e, null);
            }
        }

        public byte GetLegendByte(LegendSlot slot)
        {
            return (byte)(TryGetValue(slot, out Legend legend) && legend != null ? legend.Id : 0);
        }
    }
}
