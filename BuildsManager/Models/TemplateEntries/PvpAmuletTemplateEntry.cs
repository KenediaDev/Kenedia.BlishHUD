using Kenedia.Modules.BuildsManager.Models.Templates;
using System.Linq;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.Utility;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Models;
using System;
using Kenedia.Modules.BuildsManager.Interfaces;

namespace Kenedia.Modules.BuildsManager.TemplateEntries
{
    public class PvpAmuletTemplateEntry : TemplateEntry, IDisposable, IRuneTemplateEntry
    {
        private bool _isDisposed;
        private PvpAmulet _pvpAmulet;
        private Rune _rune;

        public PvpAmuletTemplateEntry(TemplateSlotType slot) : base(slot)
        {
        }

        public PvpAmulet PvpAmulet { get => _pvpAmulet; private set => Common.SetProperty(ref _pvpAmulet, value); }

        public Rune Rune { get => _rune; private set => Common.SetProperty(ref _rune, value); }

        override protected void OnItemChanged(object sender, ValueChangedEventArgs<BaseItem> e)
        {
            base.OnItemChanged(sender, e);

            if (e.NewValue is PvpAmulet pvpAmulet)
            {
                PvpAmulet = pvpAmulet;
            }
        }

        public override byte[] AddToCodeArray(byte[] array)
        {
            return array.Concat(new byte[]
            {
                PvpAmulet ?.MappedId ?? 0,
                Rune ?.MappedId ?? 0,
            }).ToArray();
        }

        public override byte[] GetFromCodeArray(byte[] array)
        {
            int newStartIndex = 2;

            if (array is not null && array.Length > 0)
            {
                PvpAmulet = BuildsManager.Data.PvpAmulets.Items.Values.Where(e => e.MappedId == array[0]).FirstOrDefault();
                Rune = BuildsManager.Data.PvpRunes.Items.Where(e => e.Value.MappedId == array[1]).FirstOrDefault().Value;
            }

            return array is not null && array.Length > 0 ? GearTemplateCode.RemoveFromStart(array, newStartIndex) : array;
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            PvpAmulet = null;
            Rune = null;
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
                else if (obj is PvpAmulet pvpAmulet)
                {
                    Item = pvpAmulet;
                    return true;
                }
            }
            else if (subSlot == TemplateSubSlotType.Rune)
            {
                if (obj?.Equals(Rune) is true)
                {
                    return false;
                }

                if (obj is null)
                {
                    Rune = null;
                    return true;
                }
                else if (obj is Rune rune)
                {
                    Rune = rune;
                    return true;
                }
            }

            return false;
        }
    }
}
