using Kenedia.Modules.BuildsManager.DataModels.Professions;
using System;
using System.Collections.Generic;

namespace Kenedia.Modules.BuildsManager.Models.Templates
{
    public class LegendCollection : Dictionary<LegendSlot, Legend>
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
