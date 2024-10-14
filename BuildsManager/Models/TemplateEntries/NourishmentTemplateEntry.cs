using Kenedia.Modules.BuildsManager.Models.Templates;
using System.Linq;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.Utility;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Models;
using System;
using Kenedia.Modules.BuildsManager.Services;

namespace Kenedia.Modules.BuildsManager.TemplateEntries
{
    public class NourishmentTemplateEntry : TemplateEntry, IDisposable
    {
        private bool _isDisposed;
        private Nourishment _nourishment;

        public NourishmentTemplateEntry(TemplateSlotType slot, Data data) : base(slot, data)
        {
        }

        public Nourishment Nourishment { get => _nourishment; private set => Common.SetProperty(ref _nourishment, value); }

        protected override void OnItemChanged(object sender, ValueChangedEventArgs<BaseItem> e)
        {
            base.OnItemChanged(sender, e);

            if (e.NewValue is null)
            {
                Nourishment = null;
            }
            else if (e.NewValue is Nourishment nourishment)
            {
                Nourishment = nourishment;
            }
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            Nourishment = null;
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
                else if (obj is Nourishment nourishment)
                {
                    Item = nourishment;
                    return true;
                }
            }

            return false;
        }
    }
}
