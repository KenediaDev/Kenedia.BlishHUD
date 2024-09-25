using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.DataModels.Stats;
using Kenedia.Modules.BuildsManager.Interfaces;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using System;

namespace Kenedia.Modules.BuildsManager.TemplateEntries
{
    public abstract class TemplateEntry : IItemTemplateEntry
    {
        private BaseItem _item;

        public TemplateEntry(TemplateSlotType slot)
        {
            Slot = slot;
        }

        public TemplateSlotType Slot { get; }

        public BaseItem Item { get => _item; set => Common.SetProperty(ref _item , value, OnItemChanged); }

        protected virtual void OnItemChanged(object sender, ValueChangedEventArgs<BaseItem> e)
        {
        }

        public abstract byte[] AddToCodeArray(byte[] array);

        public abstract byte[] GetFromCodeArray(byte[] array);

        public abstract bool SetValue(TemplateSlotType slot, TemplateSubSlotType subSlot, object? obj);
    }
}
