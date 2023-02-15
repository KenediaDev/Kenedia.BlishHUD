using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.Core.Models;
using System;
using System.Collections.Generic;

namespace Kenedia.Modules.BuildsManager.Models.Templates
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
