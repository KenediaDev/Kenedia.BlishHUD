using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.BuildsManager.Models.Templates;
using System;

namespace Kenedia.Modules.BuildsManager.Models
{
    public class LegendChangedEventArgs : EventArgs
    {
        public LegendChangedEventArgs(LegendSlotType slot, Legend? oldValue, Legend? newValue)
        {
            Slot = slot;
            OldValue = oldValue;
            NewValue = newValue;
        }

        public LegendSlotType Slot { get; }

        public Legend? OldValue { get; }

        public Legend? NewValue { get; }
    }
}
