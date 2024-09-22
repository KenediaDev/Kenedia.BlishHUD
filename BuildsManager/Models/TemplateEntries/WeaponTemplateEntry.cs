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
using Kenedia.Modules.Core.Extensions;

namespace Kenedia.Modules.BuildsManager.TemplateEntries
{

    public class WeaponTemplateEntry : TemplateEntry, IDisposable
    {
        private bool _isDisposed;
        private Weapon _weapon;
        private Sigil _sigil;
        private Sigil _pvpSigil;
        private Infusion _infusion;
        private Stat _stat;
        private WeaponTemplateEntry _pairedWeapon;

        public WeaponTemplateEntry(TemplateSlotType slot) : base(slot)
        {
        }

        // All properties shall be created by the following pattern:
        // public Armor Armor { get => _armor; set => Common.SetProperty(ref _armor, value, OnArmorChanged); }
        // if required generate a field for the property
        // if required generate a EventHandler like this:
        // public event EventHandler<ValueChangedEventArgs<Armor>> ArmorChanged;
        // if required generate a method like this:
        // private void OnArmorChanged(object sender, ValueChangedEventArgs<Armor> e)
        // {
        //     ArmorChanged?.Invoke(this, e);
        // }

        public event EventHandler<(TemplateSlotType slot, BaseItem item, Stat stat)> TemplateSlotChanged;
        public event EventHandler<ValueChangedEventArgs<Weapon>> WeaponChanged;
        public event EventHandler<ValueChangedEventArgs<Sigil>> SigilChanged;
        public event EventHandler<ValueChangedEventArgs<Sigil>> PvpSigilChanged;
        public event EventHandler<ValueChangedEventArgs<Infusion>> InfusionChanged;
        public event EventHandler<ValueChangedEventArgs<Stat>> StatChanged;

        public Weapon Weapon { get => _weapon; set => Common.SetProperty(ref _weapon, value, OnWeaponChanged); }

        public Sigil Sigil { get => _sigil; set => Common.SetProperty(ref _sigil, value, OnSigilChanged); }

        public Sigil PvpSigil { get => _pvpSigil; set => Common.SetProperty(ref _pvpSigil, value, OnPvpSigilChanged); }

        public Infusion Infusion { get => _infusion; set => Common.SetProperty(ref _infusion, value, OnInfusionChanged); }

        public Stat Stat { get => _stat; set => Common.SetProperty(ref _stat, value, OnStatChanged); }

        public WeaponTemplateEntry PairedWeapon { get => _pairedWeapon; set => Common.SetProperty(ref _pairedWeapon, value, SetPairedWeapon); }

        private void SetPairedWeapon(object sender, ValueChangedEventArgs<WeaponTemplateEntry> e)
        {
            if (e.OldValue != null)
            {
                e.OldValue.WeaponChanged -= OnPairedWeaponChanged;
                e.OldValue.StatChanged -= OnPairedStatChanged;
            }

            if (e.NewValue != null)
            {
                e.NewValue.WeaponChanged += OnPairedWeaponChanged;
                e.NewValue.StatChanged += OnPairedStatChanged;
            }
        }

        private void OnPairedStatChanged(object sender, ValueChangedEventArgs<Stat> e)
        {
            if (PairedWeapon is not null)
            {
                if (PairedWeapon.Slot is TemplateSlotType.MainHand or TemplateSlotType.AltMainHand)
                {
                    if (PairedWeapon.Weapon is not null && PairedWeapon.Weapon.WeaponType.IsTwoHanded())
                    {
                        Stat = PairedWeapon.Stat;
                    }
                    else if (Weapon is not null && Weapon.WeaponType.IsTwoHanded())
                    {

                    }
                }
            }
        }

        private void OnPairedWeaponChanged(object sender, ValueChangedEventArgs<Weapon> e)
        {
            if (PairedWeapon is not null)
            {
                if (PairedWeapon.Slot is TemplateSlotType.MainHand or TemplateSlotType.AltMainHand)
                {
                    if (PairedWeapon.Weapon is not null && PairedWeapon.Weapon.WeaponType.IsTwoHanded())
                    {
                        Weapon = PairedWeapon.Weapon;
                        Stat = PairedWeapon.Stat;
                    }
                    else if (Weapon is not null && Weapon.WeaponType.IsTwoHanded())
                    {
                        Weapon = null;
                    }
                }
            }
        }

        private void OnWeaponChanged(object sender, ValueChangedEventArgs<Weapon> e)
        {
            WeaponChanged?.Invoke(this, e);
            TemplateSlotChanged?.Invoke(this, (Slot, e.NewValue, Stat));
        }

        private void OnSigilChanged(object sender, ValueChangedEventArgs<Sigil> e)
        {
            SigilChanged?.Invoke(this, e);
        }

        private void OnPvpSigilChanged(object sender, ValueChangedEventArgs<Sigil> e)
        {
            PvpSigilChanged?.Invoke(this, e);
        }

        private void OnInfusionChanged(object sender, ValueChangedEventArgs<Infusion> e)
        {
            InfusionChanged?.Invoke(this, e);
        }

        private void OnStatChanged(object sender, ValueChangedEventArgs<Stat> e)
        {
            StatChanged?.Invoke(this, e);
            TemplateSlotChanged?.Invoke(this, (Slot, Weapon, e.NewValue));
        }

        public override byte[] AddToCodeArray(byte[] array)
        {
            return array.Concat(new byte[]
            {
                (byte)(Weapon?.WeaponType ?? ItemWeaponType.Unknown),
                Stat ?.MappedId ?? 0,
                Sigil ?.MappedId ?? 0,
                PvpSigil ?.MappedId ?? 0,
                Infusion ?.MappedId ?? 0,
            }).ToArray();
        }

        public override byte[] GetFromCodeArray(byte[] array)
        {
            int newStartIndex = 5;

            if (array is not null && array.Length > 0)
            {
                Weapon = Enum.TryParse($"{array[0]}", out ItemWeaponType weaponType) ? BuildsManager.Data.Weapons.Values.Where(e => e.WeaponType == weaponType).FirstOrDefault() : null;
                Stat = BuildsManager.Data.Stats.Items.Where(e => e.Value.MappedId == array[1]).FirstOrDefault().Value;
                Sigil = BuildsManager.Data.PveSigils.Items.Where(e => e.Value.MappedId == array[2]).FirstOrDefault().Value;
                PvpSigil = BuildsManager.Data.PvpSigils.Items.Where(e => e.Value.MappedId == array[3]).FirstOrDefault().Value;
                Infusion = BuildsManager.Data.Infusions.Items.Where(e => e.Value.MappedId == array[4]).FirstOrDefault().Value;
            }

            return array is not null && array.Length > 0 ? GearTemplateCode.RemoveFromStart(array, newStartIndex) : array;
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            Weapon = null;
            Sigil = null;
            PvpSigil = null;
            Infusion = null;
            Stat = null;
        }
    }
}
