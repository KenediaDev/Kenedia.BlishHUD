using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Models;
using System;
using Kenedia.Modules.BuildsManager.Services;

namespace Kenedia.Modules.BuildsManager.TemplateEntries
{
    public class PveRelicTemplateEntry : TemplateEntry, IDisposable
    {
        private bool _isDisposed;

        public PveRelicTemplateEntry(TemplateSlotType slot, Data data) : base(slot, data)
        {

        }

        public Relic Relic { get; private set => Common.SetProperty(ref field, value); }

        protected override void OnItemChanged(object sender, ValueChangedEventArgs<BaseItem> e)
        {
            base.OnItemChanged(sender, e);

            if (e.NewValue is null)
            {
                Relic = null;
            }
            else if (e.NewValue is Relic relic)
            {
                Relic = relic;
            }
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            Relic = null;
        }

        public override bool SetValue(TemplateSlotType slot, TemplateSubSlotType subSlot, object? obj)
        {
            if (subSlot == TemplateSubSlotType.Item)
            {
                if (obj?.Equals(Item) is true)
                {
                    return false;
                }

                if (obj is null)
                {
                    Item = null;
                    return true;
                }
                else if (obj is Relic relic)
                {
                    Item = relic;
                    return true;
                }
            }

            return false;
        }
    }
}
