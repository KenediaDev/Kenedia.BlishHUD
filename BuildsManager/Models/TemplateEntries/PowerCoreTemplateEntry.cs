using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Models;
using System;
using Kenedia.Modules.BuildsManager.Services;

namespace Kenedia.Modules.BuildsManager.TemplateEntries
{
    public class PowerCoreTemplateEntry : TemplateEntry, IDisposable
    {
        private bool _isDisposed;
        private PowerCore _powerCore;

        public PowerCoreTemplateEntry(TemplateSlotType slot, Data data) : base(slot, data)
        {
        }

        public PowerCore PowerCore { get => _powerCore; private set => Common.SetProperty(ref _powerCore, value); }

        protected override void OnItemChanged(object sender, ValueChangedEventArgs<BaseItem> e)
        {
            base.OnItemChanged(sender, e);

            if (e.NewValue is null)
            {
                PowerCore = null;
            }
            else if (e.NewValue is PowerCore powerCore)
            {
                PowerCore = powerCore;
            }
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            PowerCore = null;
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
                else if (obj is PowerCore powerCore)
                {
                    Item = powerCore;
                    return true;
                }
            }

            return false;
        }
    }
}
