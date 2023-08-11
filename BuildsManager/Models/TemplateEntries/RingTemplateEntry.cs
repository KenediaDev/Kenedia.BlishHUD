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
    public class 
        RingTemplateEntry : TemplateEntry
    {
        private Infusion _infusion1;
        private Infusion _infusion2;
        private Infusion _infusion3;
        private Stat _stat;

        public RingTemplateEntry(TemplateSlotType slot) : base(slot)
        {
        }

        public event EventHandler<ValueChangedEventArgs<Stat>> StatChanged;
        public event EventHandler<ValueChangedEventArgs<Infusion>> Infusion1Changed;
        public event EventHandler<ValueChangedEventArgs<Infusion>> Infusion2Changed;
        public event EventHandler<ValueChangedEventArgs<Infusion>> Infusion3Changed;

        public Trinket Ring { get; } = BuildsManager.Data.Trinkets.TryGetValue(80058, out Trinket ring) ? ring : null;

        public Infusion Infusion1 { get => _infusion1; set => Common.SetProperty(ref _infusion1, value, OnInfusion1Changed); }

        public Infusion Infusion2 { get=> _infusion2; set => Common.SetProperty(ref _infusion2, value, OnInfusion2Changed); }

        public Infusion Infusion3 { get => _infusion3; set => Common.SetProperty(ref _infusion3, value, OnInfusion3Changed); }

        public Stat Stat { get => _stat; set => Common.SetProperty(ref _stat, value, OnStatChanged); }

        private void OnInfusion1Changed(object sender, ValueChangedEventArgs<Infusion> e)
        {
            Infusion1Changed?.Invoke(this, e);
        }

        private void OnInfusion2Changed(object sender, ValueChangedEventArgs<Infusion> e)
        {
            Infusion2Changed?.Invoke(this, e);
        }

        private void OnInfusion3Changed(object sender, ValueChangedEventArgs<Infusion> e)
        {
            Infusion3Changed?.Invoke(this, e);
        }

        private void OnStatChanged(object sender, ValueChangedEventArgs<Stat> e)
        {
            StatChanged?.Invoke(this, e);
        }

        public override byte[] AddToCodeArray(byte[] array)
        {
            return array.Concat(new byte[]
            {
                Stat ?.MappedId ?? 0,
                Infusion1 ?.MappedId ?? 0,
                Infusion2 ?.MappedId ?? 0,
                Infusion3 ?.MappedId ?? 0,
            }).ToArray();
        }

        public override byte[] GetFromCodeArray(byte[] array)
        {
            int newStartIndex = 4;

            Stat = BuildsManager.Data.Stats.Where(e => e.Value.MappedId == array[0]).FirstOrDefault().Value;
            Infusion1 = BuildsManager.Data.Infusions.Where(e => e.Value.MappedId == array[1]).FirstOrDefault().Value;
            Infusion2 = BuildsManager.Data.Infusions.Where(e => e.Value.MappedId == array[2]).FirstOrDefault().Value;
            Infusion3 = BuildsManager.Data.Infusions.Where(e => e.Value.MappedId == array[3]).FirstOrDefault().Value;

            return GearTemplateCode.RemoveFromStart(array, newStartIndex);
        }
    }
}
