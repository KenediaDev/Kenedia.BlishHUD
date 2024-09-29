﻿using Kenedia.Modules.BuildsManager.Models.Templates;
using System.Linq;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.Utility;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Models;
using System;

namespace Kenedia.Modules.BuildsManager.TemplateEntries
{
    public class NourishmentTemplateEntry : TemplateEntry, IDisposable
    {
        private bool _isDisposed;
        private Nourishment _nourishment;

        public NourishmentTemplateEntry(TemplateSlotType slot) : base(slot)
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

        public override byte[] AddToCodeArray(byte[] array)
        {
            return array.Concat(new byte[]
            {
                Nourishment ?.MappedId ?? 0,
            }).ToArray();
        }

        public override byte[] GetFromCodeArray(byte[] array)
        {
            int newStartIndex = 1;

            if (array is not null && array.Length > 0)
            {
                Nourishment = BuildsManager.Data.Nourishments.Values.Where(e => e.MappedId == array[0]).FirstOrDefault();
            }

            return array is not null && array.Length > 0 ? GearTemplateCode.RemoveFromStart(array, newStartIndex) : array;
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
