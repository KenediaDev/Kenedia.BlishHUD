using System.ComponentModel;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using System;
using System.Linq;
using static Kenedia.Modules.BuildsManager.DataModels.Professions.Weapon;
using Kenedia.Modules.Core.Models;
using System.Data;

namespace Kenedia.Modules.BuildsManager.Models.Templates
{
    public class WeaponEntry : JuwelleryEntry
    {
        private WeaponType _weapon = WeaponType.Unknown;
        private Sigil _sigil;
        private Sigil _pvpSigil;
        private ObservableList<int> _sigilIds = new();
        private Sigil _sigil2;

        public WeaponEntry()
        {
        }

        private void SigilIds_Changed(object sender, PropertyChangedEventArgs e)
        {
            if (_sigilIds != null && _sigilIds.Count > 0) _sigil = BuildsManager.Data.PveSigils.Values.Where(e => e.MappedId == _sigilIds[0]).FirstOrDefault();
            if (_sigilIds != null && _sigilIds.Count > 1 && Slot is not TemplateSlot.Aquatic and not TemplateSlot.AltAquatic) _pvpSigil = BuildsManager.Data.PvpSigils.Values.Where(e => e.MappedId == _sigilIds[1]).FirstOrDefault();
            if (_sigilIds != null && _sigilIds.Count > 1 && Slot is TemplateSlot.Aquatic or TemplateSlot.AltAquatic) _sigil2 = BuildsManager.Data.PveSigils.Values.Where(e => e.MappedId == _sigilIds[1]).FirstOrDefault();

            OnPropertyChanged(sender, e);
        }

        public ObservableList<int> SigilIds
        {
            get => _sigilIds;
            set
            {
                var temp = _sigilIds;
                if (Common.SetProperty(ref _sigilIds, value))
                {
                    if (temp != null) temp.PropertyChanged -= SigilIds_Changed;
                    if (_sigilIds != null)
                    {
                        _sigilIds.PropertyChanged += SigilIds_Changed;
                    }
                }
            }
        }

        public WeaponType Weapon { get => _weapon; set => Common.SetProperty(ref _weapon, value, OnPropertyChanged); }

        public Sigil Sigil
        {
            get => _sigil; set
            {
                if (Common.SetProperty(ref _sigil, value))
                {
                    _sigilIds[0] = _sigil?.MappedId ?? -1;
                }
            }
        }

        public Sigil Sigil2
        {
            get => _sigil2; set
            {
                if (_sigilIds.Count > 1 && Common.SetProperty(ref _sigil2, value))
                {
                    _sigilIds[1] = _sigil2?.MappedId ?? -1;
                }
            }
        }

        public Sigil PvpSigil
        {
            get => _pvpSigil; set
            {
                if (Common.SetProperty(ref _pvpSigil, value))
                {
                    _sigilIds[0] = _pvpSigil?.MappedId ?? -1;
                }
            }
        }

        public override void OnItemChanged()
        {
            base.OnItemChanged();

            Weapon = Enum.TryParse((Item as Weapon)?.WeaponType.ToString(), out WeaponType weaponType) ? weaponType : WeaponType.Unknown;
        }

        public override void OnSlotApply()
        {
            base.OnSlotApply();

            SigilIds = Slot switch
            {
                TemplateSlot.Aquatic or TemplateSlot.AltAquatic => new() { -1, -1 },
                _ => new() { -1, -1, },
            };
        }

        public override string ToCode()
        {
            string infusions = string.Join("|", InfusionIds);
            string sigils = string.Join("|", SigilIds);

            return $"[{MappedId}|{Stat?.MappedId ?? -1}|{infusions}|{sigils}]";
        }

        public override void FromCode(string code)
        {
            string[] parts = code.Split('|');

            if (Slot is TemplateSlot.Aquatic or TemplateSlot.AltAquatic)
            {
                if (parts.Length == 6)
                {
                    MappedId = int.TryParse(parts[0], out int mappedId) ? mappedId : -1;
                    Stat = int.TryParse(parts[1], out int stat) ? BuildsManager.Data.Stats.Where(e => e.Value.MappedId == stat).FirstOrDefault().Value : null;
                    InfusionIds[0] = int.TryParse(parts[2], out int infusion1) ? infusion1 : -1;
                    InfusionIds[1] = int.TryParse(parts[3], out int infusion2) ? infusion2 : -1;
                    SigilIds[0] = int.TryParse(parts[4], out int sigil1) ? sigil1 : -1;
                    SigilIds[1] = int.TryParse(parts[5], out int sigil2) ? sigil2 : -1;
                }
            }
            else if (parts.Length == 5)
            {
                MappedId = int.TryParse(parts[0], out int mappedId) ? mappedId : -1;
                Stat = int.TryParse(parts[1], out int stat) ? BuildsManager.Data.Stats.Where(e => e.Value.MappedId == stat).FirstOrDefault().Value : null;
                InfusionIds[0] = int.TryParse(parts[2], out int infusionId) ? infusionId : -1;
                SigilIds[0] = int.TryParse(parts[3], out int sigil) ? sigil : -1;
                SigilIds[1] = int.TryParse(parts[4], out int pvpsigil) ? pvpsigil : -1;
            }

            if (MappedId > -1)
            {
                Item = BuildsManager.Data.Weapons.Values.Where(e => e.MappedId == MappedId).FirstOrDefault();
            }
        }

        public override void ResetUpgrades()
        {
            base.ResetUpgrades();

            ResetSigils();
        }

        public void ResetSigils(int? index = null)
        {
            switch (index)
            {
                case null:
                    Sigil = null;
                    Sigil2 = null;
                    PvpSigil = null;
                    break;

                case 0:
                    Sigil = null;
                    break;

                case 1:
                    Sigil2 = null;
                    break;

                case 2:
                    PvpSigil = null;
                    break;
            }
        }
    }
}
