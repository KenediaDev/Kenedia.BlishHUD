using Kenedia.Modules.BuildsManager.Models.Templates;
using System.Linq;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.Utility;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Models;
using System;

namespace Kenedia.Modules.BuildsManager.TemplateEntries
{
    public class PvpAmuletTemplateEntry : TemplateEntry, IDisposable
    {
        private bool _isDisposed;
        private PvpAmulet _pvpAmulet;
        private Rune _rune;

        public PvpAmuletTemplateEntry(TemplateSlotType slot) : base(slot)
        {
        }

        public event EventHandler<ValueChangedEventArgs<PvpAmulet>> PvpAmuletChanged;
        public event EventHandler<ValueChangedEventArgs<Rune>> RuneChanged;

        public PvpAmulet PvpAmulet { get => _pvpAmulet; set => Common.SetProperty(ref _pvpAmulet, value, OnPvpAmuletChanged); }

        public Rune Rune{ get => _rune; set => Common.SetProperty(ref _rune, value, OnRuneChanged); }

        private void OnPvpAmuletChanged(object sender, ValueChangedEventArgs<PvpAmulet> e)
        {
            PvpAmuletChanged?.Invoke(sender, e);
        }

        private void OnRuneChanged(object sender, ValueChangedEventArgs<Rune> e)
        {
            RuneChanged?.Invoke(sender, e);
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

            PvpAmulet = BuildsManager.Data.PvpAmulets.Items.Values.Where(e => e.MappedId == array[0]).FirstOrDefault();
            Rune = BuildsManager.Data.PvpRunes.Items.Where(e => e.Value.MappedId == array[1]).FirstOrDefault().Value;

            return GearTemplateCode.RemoveFromStart(array, newStartIndex);
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            PvpAmulet = null;
            Rune = null;
        }
    }
}
