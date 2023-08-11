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
    public class ArmorTemplateEntry : TemplateEntry
    {
        private Stat _stat;
        private Infusion _infusion;
        private Rune _rune;
        private Armor _armor;

        public ArmorTemplateEntry(TemplateSlot slot) : base(slot)
        {
        }

        public event EventHandler<ValueChangedEventArgs<Rune>> RuneChanged;
        public event EventHandler<ValueChangedEventArgs<Infusion>> InfusionChanged;
        public event EventHandler<ValueChangedEventArgs<Stat>> StatChanged;

        public Armor Armor { get => _armor; set => Common.SetProperty(ref _armor, value); }

        public Rune Rune { get => _rune; set => Common.SetProperty(ref _rune, value, OnRuneChanged); }

        public Infusion Infusion { get => _infusion; set => Common.SetProperty(ref _infusion, value, OnInfusionChanged); }

        public Stat Stat { get => _stat; set => Common.SetProperty(ref _stat, value, OnStatChanged); }


        private void OnRuneChanged(object sender, ValueChangedEventArgs<Rune> e)
        {
            RuneChanged?.Invoke(this, e);
        }

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
                Rune ?.MappedId ?? 0,
                Infusion ?.MappedId ?? 0,
            }).ToArray();
        }

        public override byte[] GetFromCodeArray(byte[] array)
        {
            int newStartIndex = 3;

            Stat = BuildsManager.Data.Stats.Where(e => e.Value.MappedId == array[0]).FirstOrDefault().Value;
            Rune = BuildsManager.Data.PveRunes.Where(e => e.Value.MappedId == array[1]).FirstOrDefault().Value;
            Infusion = BuildsManager.Data.Infusions.Where(e => e.Value.MappedId == array[2]).FirstOrDefault().Value;

            return GearTemplateCode.RemoveFromStart(array, newStartIndex);
        }
    }
}
