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
using Kenedia.Modules.BuildsManager.Interfaces;
using System.Diagnostics;
using Kenedia.Modules.BuildsManager.Extensions;
using Kenedia.Modules.BuildsManager.Services;

namespace Kenedia.Modules.BuildsManager.TemplateEntries
{

    public class WeaponTemplateEntry : TemplateEntry, IDisposable, IWeaponTemplateEntry, IStatTemplateEntry, ISingleSigilTemplateEntry, IPvpSigilTemplateEntry, ISingleInfusionTemplateEntry
    {
        private bool _isDisposed;
        private Weapon _weapon;
        private Sigil _sigil1;
        private Sigil _pvpSigil;
        private Infusion _infusion1;
        private Stat _stat;
        private WeaponTemplateEntry _pairedWeapon;

        public WeaponTemplateEntry(TemplateSlotType slot, Data data) : base(slot, data)
        {
        }

        public Weapon Weapon { get => _weapon; private set => Common.SetProperty(ref _weapon, value); }

        public Sigil Sigil1 { get => _sigil1; private set => Common.SetProperty(ref _sigil1, value); }

        public Sigil PvpSigil { get => _pvpSigil; private set => Common.SetProperty(ref _pvpSigil, value); }

        public Infusion Infusion1 { get => _infusion1; private set => Common.SetProperty(ref _infusion1, value); }

        public Stat Stat { get => _stat; private set => Common.SetProperty(ref _stat, value); }

        public WeaponTemplateEntry PairedWeapon { get => _pairedWeapon; set => Common.SetProperty(ref _pairedWeapon, value); }

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

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            Weapon = null;
            Sigil1 = null;
            PvpSigil = null;
            Infusion1 = null;
            Stat = null;
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
            else if (subSlot == TemplateSubSlotType.PvpSigil)
            {
                if (obj?.Equals(PvpSigil) is true)
                {
                    return false;
                }

                if (obj is null)
                {
                    PvpSigil = null;
                    return true;
                }
                else if (obj is Sigil sigil)
                {
                    PvpSigil = sigil;
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

            return false;
        }
    }
}
