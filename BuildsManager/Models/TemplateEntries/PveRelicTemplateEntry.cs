using Kenedia.Modules.BuildsManager.Models.Templates;
using System.Linq;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.Utility;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Models;
using System;

namespace Kenedia.Modules.BuildsManager.TemplateEntries
{
    public class PveRelicTemplateEntry : TemplateEntry, IDisposable
    {
        private bool _isDisposed;
        private Relic _relic;

        public PveRelicTemplateEntry(TemplateSlotType slot) : base(slot)
        {

        }

        public Relic Relic { get => _relic; private set => Common.SetProperty(ref _relic, value); }

        protected override void OnItemChanged(object sender, ValueChangedEventArgs<BaseItem> e)
        {
            base.OnItemChanged(sender, e);

            if (e.NewValue is Relic relic)
            {
                Relic = relic;
            }
        }

        public override byte[] AddToCodeArray(byte[] array)
        {
            return array.Concat(new byte[]
            {
                Relic ?.MappedId ?? 0,
            }).ToArray();
        }

        public override byte[] GetFromCodeArray(byte[] array)
        {
            int newStartIndex = 1;
            if (array is not null && array.Length > 0)
            {
                Relic = BuildsManager.Data.PveRelics.Values.Where(e => e.MappedId == array[0]).FirstOrDefault();
            }

            return array is not null && array.Length > 0 ? GearTemplateCode.RemoveFromStart(array, newStartIndex) : array;
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
