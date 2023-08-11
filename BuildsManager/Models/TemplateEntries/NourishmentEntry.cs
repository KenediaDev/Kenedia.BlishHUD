﻿using Kenedia.Modules.BuildsManager.Models.Templates;
using System.Linq;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.Utility;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Models;
using System;

namespace Kenedia.Modules.BuildsManager.TemplateEntries
{
    public class NourishmentEntry : TemplateEntry
    {
        private Nourishment _nourishment;

        public NourishmentEntry(TemplateSlot slot) : base(slot)
        {
        }

        public event EventHandler<ValueChangedEventArgs<Nourishment>> NourishmentChanged;

        public Nourishment Nourishment { get => _nourishment; set => Common.SetProperty(ref _nourishment, value, OnNourishmentChanged); }

        private void OnNourishmentChanged(object sender, ValueChangedEventArgs<Nourishment> e)
        {
            NourishmentChanged?.Invoke(sender, e);
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

            Nourishment = BuildsManager.Data.Nourishments.Values.Where(e => e.MappedId == array[0]).FirstOrDefault();

            return GearTemplateCode.RemoveFromStart(array, newStartIndex);
        }
    }
}
