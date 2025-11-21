using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Models;
using System;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.Services;

namespace Kenedia.Modules.BuildsManager.TemplateEntries
{
    public class EnhancementTemplateEntry : TemplateEntry, IDisposable
    {
        private bool _isDisposed;
        private Enhancement _enhancement;

        public EnhancementTemplateEntry(TemplateSlotType slot, Data data) : base(slot, data)
        {
        }

        public Enhancement Enhancement { get => _enhancement; private set => Common.SetProperty(ref _enhancement, value); }

        protected override void OnItemChanged(object sender, ValueChangedEventArgs<BaseItem> e)
        {
            base.OnItemChanged(sender, e);

            if (e.NewValue is null)
            {
                Enhancement = null;
            }
            else if (e.NewValue is Enhancement enhancement)
            {
                Enhancement = enhancement;
            }
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            Enhancement = null;
        }

        public override bool SetValue(TemplateSlotType slot, TemplateSubSlotType subSlot, object obj)
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
                else if(obj is Enhancement enhancement)
                {
                    Item = enhancement;
                    return true;
                }
            }

            return false;
        }
    }
}
