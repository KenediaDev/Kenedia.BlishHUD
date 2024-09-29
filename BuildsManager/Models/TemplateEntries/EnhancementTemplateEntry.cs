using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.BuildsManager.Utility;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Models;
using System.Linq;
using System;
using Kenedia.Modules.BuildsManager.DataModels.Items;

namespace Kenedia.Modules.BuildsManager.TemplateEntries
{
    public class EnhancementTemplateEntry : TemplateEntry, IDisposable
    {
        private bool _isDisposed;
        private Enhancement _enhancement;

        public EnhancementTemplateEntry(TemplateSlotType slot) : base(slot)
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

        public override byte[] AddToCodeArray(byte[] array)
        {
            return array.Concat(new byte[]
            {
                Enhancement ?.MappedId ?? 0,
            }).ToArray();
        }

        public override byte[] GetFromCodeArray(byte[] array)
        {
            int newStartIndex = 1;

            if (array is not null && array.Length > 0)
            {
                Enhancement = BuildsManager.Data.Enhancements.Items.Values.Where(e => e.MappedId == array[0]).FirstOrDefault();
            }

            return array is not null && array.Length > 0 ? GearTemplateCode.RemoveFromStart(array, newStartIndex) : array;
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
