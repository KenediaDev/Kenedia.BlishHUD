using Kenedia.Modules.BuildsManager.DataModels.Professions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Kenedia.Modules.BuildsManager.Models.Templates
{
    public class LegendCollection : IEnumerable<Legend>
    {
        public Legend? TerrestrialActive { get; set; }

        public Legend? TerrestrialInactive { get; set; }

        public Legend? AquaticActive { get; set; }

        public Legend? AquaticInactive { get; set; }

        public LegendCollection()
        {

        }

        public Legend this[LegendSlotType slot] => TryGetValue(slot, out Legend legend) ? legend : null;
        
        public LegendSlotType this[Legend legend] => legend switch
        {
            { } when legend == TerrestrialActive => LegendSlotType.TerrestrialActive,
            { } when legend == TerrestrialInactive => LegendSlotType.TerrestrialInactive,
            { } when legend == AquaticActive => LegendSlotType.AquaticActive,
            { } when legend == AquaticInactive => LegendSlotType.AquaticInactive,
            _ => throw new ArgumentOutOfRangeException(nameof(legend), legend, null)
        };

        public byte GetLegendByte(LegendSlotType slot)
        {
            return (byte)(TryGetValue(slot, out Legend legend) && legend is not null ? legend.Id : 0);
        }

        public bool TryGetValue(LegendSlotType slot, out Legend legend)
        {
            legend = slot switch
            {
                LegendSlotType.AquaticActive => AquaticActive,
                LegendSlotType.AquaticInactive => AquaticInactive,
                LegendSlotType.TerrestrialActive => TerrestrialActive,
                LegendSlotType.TerrestrialInactive => TerrestrialInactive,
                _ => null
            };

            return legend is not null;
        }

        public void SetLegend(LegendSlotType slot, Legend? legend)
        {
            switch (slot)
            {
                case LegendSlotType.AquaticActive:
                    AquaticActive = legend;
                    break;
                case LegendSlotType.AquaticInactive:
                    AquaticInactive = legend;
                    break;
                case LegendSlotType.TerrestrialActive:
                    TerrestrialActive = legend;
                    break;
                case LegendSlotType.TerrestrialInactive:
                    TerrestrialInactive = legend;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(slot), slot, null);
            }
        }

        public IEnumerator<Legend> GetEnumerator()
        {
            yield return TerrestrialActive;
            yield return TerrestrialInactive;
            yield return AquaticActive;
            yield return AquaticInactive;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Wipe()
        {
            TerrestrialActive = null;
            TerrestrialInactive = null;
            AquaticActive = null;
            AquaticInactive = null;
        }
    }
}
