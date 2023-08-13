using Kenedia.Modules.BuildsManager.Models.Templates;
using System.Linq;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.DataModels.Stats;
using Kenedia.Modules.BuildsManager.Utility;
using System;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Models;

namespace Kenedia.Modules.BuildsManager.TemplateEntries
{
    public class AccessoireTemplateEntry : TemplateEntry, IDisposable
    {
        private bool _isDisposed;
        private Stat _stat;
        private Infusion _infusion;

        public AccessoireTemplateEntry(TemplateSlotType slot) : base(slot)
        {
        }

        public event EventHandler<ValueChangedEventArgs<Infusion>> InfusionChanged;
        public event EventHandler<ValueChangedEventArgs<Stat>> StatChanged;

        public Trinket Accessoire { get; private set; } = BuildsManager.Data.Trinkets.TryGetValue(80002, out Trinket accessoire) ? accessoire : null;

        public Stat Stat { get => _stat; set => Common.SetProperty(ref _stat, value, OnStatChanged); }

        public Infusion Infusion { get => _infusion; set => Common.SetProperty(ref _infusion, value, OnInfusionChanged); }

        private void OnInfusionChanged(object sender, ValueChangedEventArgs<Infusion> e)
        {
            InfusionChanged?.Invoke(this, e);
        }

        private void OnStatChanged(object sender, ValueChangedEventArgs<Stat> e)
        {
            StatChanged?.Invoke(this, e);
        }

        public override byte[] AddToCodeArray(byte[] array)
        {
            return array.Concat(new byte[]
            {
                Stat?.MappedId ?? 0,
                Infusion?.MappedId ?? 0,
            }).ToArray();
        }

        public override byte[] GetFromCodeArray(byte[] array)
        {
            int newStartIndex = 2;

            Stat = BuildsManager.Data.Stats.Where(e => e.Value.MappedId == array[0]).FirstOrDefault().Value;
            Infusion = BuildsManager.Data.Infusions.Where(e => e.Value.MappedId == array[1]).FirstOrDefault().Value;

            return GearTemplateCode.RemoveFromStart(array, newStartIndex);
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            Stat = null;
            Infusion = null;
            Accessoire = null;
        }
    }
}
