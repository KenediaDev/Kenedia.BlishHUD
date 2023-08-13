using Kenedia.Modules.BuildsManager.Models.Templates;
using System.Linq;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.DataModels.Stats;
using Kenedia.Modules.BuildsManager.Utility;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Models;
using System;

namespace Kenedia.Modules.BuildsManager.TemplateEntries
{
    public class BackTemplateEntry : TemplateEntry, IDisposable
    {
        private bool _isDisposed;
        private Stat _stat;
        private Infusion _infusion1;
        private Infusion _infusion2;

        public BackTemplateEntry(TemplateSlotType slot) : base(slot)
        {
        }

        public event EventHandler<ValueChangedEventArgs<Stat>> StatChanged;
        public event EventHandler<ValueChangedEventArgs<Infusion>> Infusion1Changed;
        public event EventHandler<ValueChangedEventArgs<Infusion>> Infusion2Changed;

        public Stat Stat { get => _stat; set => Common.SetProperty(ref _stat, value, OnStatChanged); }

        public Trinket Back { get; private set; } = BuildsManager.Data.Backs.TryGetValue(94947, out Trinket back) ? back : null;

        public Infusion Infusion1 { get => _infusion1; set => Common.SetProperty(ref _infusion1, value, OnInfusion1Changed); }

        public Infusion Infusion2 { get => _infusion2; set => Common.SetProperty(ref _infusion2, value, OnInfusion2Changed); }

        private void OnStatChanged(object sender, ValueChangedEventArgs<Stat> e)
        {
            StatChanged?.Invoke(this, e);
        }

        private void OnInfusion1Changed(object sender, ValueChangedEventArgs<Infusion> e)
        {
            Infusion1Changed?.Invoke(this, e);
        }

        private void OnInfusion2Changed(object sender, ValueChangedEventArgs<Infusion> e)
        {
            Infusion2Changed?.Invoke(this, e);
        }

        public override byte[] AddToCodeArray(byte[] array)
        {
            return array.Concat(new byte[]
            {
                Stat ?.MappedId ?? 0,
                Infusion1 ?.MappedId ?? 0,
                Infusion2 ?.MappedId ?? 0,
            }).ToArray();
        }

        public override byte[] GetFromCodeArray(byte[] array)
        {
            int newStartIndex = 3;

            Stat = BuildsManager.Data.Stats.Where(e => e.Value.MappedId == array[0]).FirstOrDefault().Value;
            Infusion1 = BuildsManager.Data.Infusions.Where(e => e.Value.MappedId == array[1]).FirstOrDefault().Value;
            Infusion2 = BuildsManager.Data.Infusions.Where(e => e.Value.MappedId == array[2]).FirstOrDefault().Value;

            return GearTemplateCode.RemoveFromStart(array, newStartIndex);
        }

        public void Dispose()
        {
            if(_isDisposed) return;
            _isDisposed = true;

            Stat = null;
            Infusion1 = null;
            Infusion2 = null;
            Back = null;
        }
    }
}
