using System;

namespace Kenedia.Modules.BuildsManager.Models
{
    public class TemplateChangedEventArgs<T> : EventArgs
    {
        public TemplateSlots TemplateSlot { get; set; }

        public T? Value { get; set; }

        public T? OldValue { get; set; }

        public TemplateChangedEventArgs(TemplateSlots templateSlot, T? value, T? oldValue = default)
        {
            TemplateSlot = templateSlot;
            Value = value;
            OldValue = oldValue;
        }
    }
}
