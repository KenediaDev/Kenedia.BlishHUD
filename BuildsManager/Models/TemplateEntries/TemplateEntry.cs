using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.Interfaces;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.BuildsManager.Services;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using System;

namespace Kenedia.Modules.BuildsManager.TemplateEntries
{
    public abstract class TemplateEntry : IItemTemplateEntry
    {
        public TemplateEntry(TemplateSlotType slot, Data data)
        {
            Slot = slot;

            Data = data;
            Data.Loaded += OnDataLoaded;

            if (Data.IsLoaded)
            {
                OnDataLoaded();
            }
        }

        private void OnDataLoaded(object sender, EventArgs e)
        {
            OnDataLoaded();
        }

        protected virtual void OnDataLoaded()
        {

        }

        public TemplateSlotType Slot { get; }

        public Data Data { get; }

        public BaseItem Item { get; set => Common.SetProperty(field, value, v => field = v, OnItemChanged); }

        protected virtual void OnItemChanged(object sender, ValueChangedEventArgs<BaseItem> e)
        {
        }

        public abstract bool SetValue(TemplateSlotType slot, TemplateSubSlotType subSlot, object? obj);
    }
}
