using Kenedia.Modules.BuildsManager.Models.Templates;
using System;

namespace Kenedia.Modules.BuildsManager.Models
{
    public class TemplateSlotChangedEventArgs : EventArgs
    {
        public TemplateSlotChangedEventArgs(TemplateSlotType slot, TemplateSubSlotType subSlotType, object value)
        {
            Slot = slot;
            SubSlotType = subSlotType;
            Value = value;
        }

        public TemplateSlotType Slot { get; }

        public TemplateSubSlotType SubSlotType { get; }

        public object Value { get; }
    }
}
