using Kenedia.Modules.BuildsManager.Models.Templates;
using System;
using System.Linq;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.DataModels.Stats;
using Weapon = Kenedia.Modules.BuildsManager.DataModels.Items.Weapon;
using ItemWeaponType = Gw2Sharp.WebApi.V2.Models.ItemWeaponType;
using Kenedia.Modules.BuildsManager.Utility;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Models;

namespace Kenedia.Modules.BuildsManager.TemplateEntries
{
    public class AquaticWeaponTemplateEntry : TemplateEntry
    {
        private Weapon _weapon;
        private Sigil _sigil1;
        private Sigil _sigil2;
        private Infusion _infusion1;
        private Infusion _infusion2;
        private Stat _stat;

        public AquaticWeaponTemplateEntry(TemplateSlot slot) : base(slot)
        {
        }

        public event EventHandler<ValueChangedEventArgs<Stat>> StatChanged;
        public event EventHandler<ValueChangedEventArgs<Weapon>> WeaponChanged;
        public event EventHandler<ValueChangedEventArgs<Sigil>> Sigil1Changed;
        public event EventHandler<ValueChangedEventArgs<Sigil>> Sigil2Changed;
        public event EventHandler<ValueChangedEventArgs<Infusion>> Infusion1Changed;
        public event EventHandler<ValueChangedEventArgs<Infusion>> Infusion2Changed;

        public Weapon Weapon { get => _weapon; set => Common.SetProperty(ref _weapon, value, OnWeaponChanged); }

        public Sigil Sigil1 { get => _sigil1; set => Common.SetProperty(ref _sigil1, value, OnSigil1Changed); }

        public Sigil Sigil2 { get => _sigil2; set => Common.SetProperty(ref _sigil2, value, OnSigil2Changed); }

        public Infusion Infusion1 { get => _infusion1; set => Common.SetProperty(ref _infusion1, value, OnInfusion1Changed); }

        public Infusion Infusion2 { get => _infusion2; set => Common.SetProperty(ref _infusion2, value, OnInfusion2Changed); }

        public Stat Stat { get => _stat; set => Common.SetProperty(ref _stat, value, OnStatChanged); }

        private void OnSigil1Changed(object sender, ValueChangedEventArgs<Sigil> e)
        {
            Sigil1Changed?.Invoke(sender, e);
        }

        private void OnSigil2Changed(object sender, ValueChangedEventArgs<Sigil> e)
        {
            Sigil2Changed?.Invoke(sender, e);
        }

        private void OnInfusion1Changed(object sender, ValueChangedEventArgs<Infusion> e)
        {
            Infusion1Changed?.Invoke(sender, e);
        }

        private void OnInfusion2Changed(object sender, ValueChangedEventArgs<Infusion> e)
        {
            Infusion2Changed?.Invoke(sender, e);
        }

        private void OnStatChanged(object sender, ValueChangedEventArgs<Stat> e)
        {
            StatChanged?.Invoke(sender, e);
        }

        private void OnWeaponChanged(object sender, ValueChangedEventArgs<Weapon> e)
        {
            WeaponChanged?.Invoke(sender, e);
        }

        public override byte[] AddToCodeArray(byte[] array)
        {
            return array.Concat(new byte[]
            {
                (byte)(Weapon?.WeaponType ?? ItemWeaponType.Unknown),
                Stat ?.MappedId ?? 0,
                Sigil1 ?.MappedId ?? 0,
                Sigil2 ?.MappedId ?? 0,
                Infusion1 ?.MappedId ?? 0,
                Infusion2 ?.MappedId ?? 0,
            }).ToArray();
        }

        public override byte[] GetFromCodeArray(byte[] array)
        {
            int newStartIndex = 6;

            Weapon = Enum.TryParse($"{array[0]}", out ItemWeaponType weaponType) ? BuildsManager.Data.Weapons.Values.Where(e => e.WeaponType == weaponType).FirstOrDefault() : null;
            Stat = BuildsManager.Data.Stats.Where(e => e.Value.MappedId == array[1]).FirstOrDefault().Value;
            Sigil1 = BuildsManager.Data.PveSigils.Where(e => e.Value.MappedId == array[2]).FirstOrDefault().Value;
            Sigil2 = BuildsManager.Data.PveSigils.Where(e => e.Value.MappedId == array[3]).FirstOrDefault().Value;
            Infusion1 = BuildsManager.Data.Infusions.Where(e => e.Value.MappedId == array[4]).FirstOrDefault().Value;
            Infusion2 = BuildsManager.Data.Infusions.Where(e => e.Value.MappedId == array[5]).FirstOrDefault().Value;

            return GearTemplateCode.RemoveFromStart(array, newStartIndex);
        }
    }
}
