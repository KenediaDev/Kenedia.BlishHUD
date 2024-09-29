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
using Kenedia.Modules.BuildsManager.Interfaces;

namespace Kenedia.Modules.BuildsManager.TemplateEntries
{
    public class AquaticWeaponTemplateEntry : TemplateEntry, IDisposable, IStatTemplateEntry, IDoubleSigilTemplateEntry, IDoubleInfusionTemplateEntry, IWeaponTemplateEntry
    {
        private bool _isDisposed;
        private Weapon _weapon;
        private Sigil _sigil1;
        private Sigil _sigil2;
        private Infusion _infusion1;
        private Infusion _infusion2;
        private Stat _stat;

        public AquaticWeaponTemplateEntry(TemplateSlotType slot) : base(slot)
        {
        }

        public Weapon Weapon { get => _weapon; private set => Common.SetProperty(ref _weapon, value); }

        public Sigil Sigil1 { get => _sigil1; private set => Common.SetProperty(ref _sigil1, value); }

        public Sigil Sigil2 { get => _sigil2; private set => Common.SetProperty(ref _sigil2, value); }

        public Infusion Infusion1 { get => _infusion1; private set => Common.SetProperty(ref _infusion1, value); }

        public Infusion Infusion2 { get => _infusion2; private set => Common.SetProperty(ref _infusion2, value); }

        public Stat Stat { get => _stat; private set => Common.SetProperty(ref _stat, value); }

        protected override void OnItemChanged(object sender, ValueChangedEventArgs<BaseItem> e)
        {
            base.OnItemChanged(sender, e);

            if (e.NewValue is null)
            {
                Weapon = null;
            }
            else if (e.NewValue is Weapon weapon)
            {
                Weapon = weapon;
            }
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

            if (array is not null && array.Length > 0)
            {
                Weapon = Enum.TryParse($"{array[0]}", out ItemWeaponType weaponType) ? BuildsManager.Data.Weapons.Values.Where(e => e.WeaponType == weaponType).FirstOrDefault() : null;
                Stat = BuildsManager.Data.Stats.Items.Where(e => e.Value.MappedId == array[1]).FirstOrDefault().Value;
                Sigil1 = BuildsManager.Data.PveSigils.Items.Where(e => e.Value.MappedId == array[2]).FirstOrDefault().Value;
                Sigil2 = BuildsManager.Data.PveSigils.Items.Where(e => e.Value.MappedId == array[3]).FirstOrDefault().Value;
                Infusion1 = BuildsManager.Data.Infusions.Items.Where(e => e.Value.MappedId == array[4]).FirstOrDefault().Value;
                Infusion2 = BuildsManager.Data.Infusions.Items.Where(e => e.Value.MappedId == array[5]).FirstOrDefault().Value;
            }

            return array is not null && array.Length > 0 ? GearTemplateCode.RemoveFromStart(array, newStartIndex) : array;
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            Weapon = null;
            Stat = null;
            Sigil1 = null;
            Sigil2 = null;
            Infusion1 = null;
            Infusion2 = null;
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
                else if (obj is Weapon weapon)
                {
                    Item = weapon;
                    return true;
                }
            }
            else if (subSlot is TemplateSubSlotType.Stat)
            {
                if (obj?.Equals(Stat) is true)
                {
                    return false;
                }

                if (obj is null)
                {
                    Stat = null;
                    return true;
                }
                else if (obj is Stat stat)
                {
                    Stat = stat;
                    return true;
                }
            }
            else if (subSlot == TemplateSubSlotType.Sigil1)
            {
                if (obj?.Equals(Sigil1) is true)
                {
                    return false;
                }

                if (obj is null)
                {
                    Sigil1 = null;
                    return true;
                }
                else if (obj is Sigil sigil)
                {
                    Sigil1 = sigil;
                    return true;
                }
            }
            else if (subSlot == TemplateSubSlotType.Sigil2)
            {
                if (obj?.Equals(Sigil2) is true)
                {
                    return false;
                }

                if (obj is null)
                {
                    Sigil2 = null;
                    return true;
                }
                else if (obj is Sigil sigil)
                {
                    Sigil2 = sigil;
                    return true;
                }
            }
            else if (subSlot == TemplateSubSlotType.Infusion1)
            {
                if (obj?.Equals(Infusion1) is true)
                {
                    return false;
                }

                if (obj is null)
                {
                    Infusion1 = null;
                    return true;
                }
                else if (obj is Infusion infusion)
                {
                    Infusion1 = infusion;
                    return true;
                }
            }
            else if (subSlot == TemplateSubSlotType.Infusion2)
            {
                if (obj?.Equals(Infusion2) is true)
                {
                    return false;
                }

                if (obj is null)
                {
                    Infusion2 = null;
                    return true;
                }
                else if (obj is Infusion infusion)
                {
                    Infusion2 = infusion;
                    return true;
                }
            }

            return false;
        }
    }
}
