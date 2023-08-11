using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.Core.Models;
using System;

namespace Kenedia.Modules.BuildsManager.Models.Templates
{
    public class LegendCollection : ObservableDictionary<LegendSlotType, Legend>
    {
        public LegendCollection()
        {
            foreach (LegendSlotType e in Enum.GetValues(typeof(LegendSlotType)))
            {
                Add(e, null);
            }
        }

        public byte GetLegendByte(LegendSlotType slot)
        {
            return (byte)(TryGetValue(slot, out Legend legend) && legend != null ? legend.Id : 0);
        }
    }
}
