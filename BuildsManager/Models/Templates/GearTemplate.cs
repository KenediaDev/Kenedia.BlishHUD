using System.ComponentModel;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using System.Collections.Generic;
using System;
using Kenedia.Modules.BuildsManager.Extensions;
using Kenedia.Modules.BuildsManager.DataModels.Stats;
using System.Linq;
using static Kenedia.Modules.BuildsManager.DataModels.Professions.Weapon;
using Kenedia.Modules.Core.Models;
using System.Data;
using System.Diagnostics;
using Kenedia.Modules.Core.Extensions;

namespace Kenedia.Modules.BuildsManager.Models.Templates
{
    public class GearTemplateEntry : INotifyPropertyChanged
    {
        private GearTemplateSlot _slot = (GearTemplateSlot)(-2);
        private int _mappedId = -1;
        private BaseItem _item;

        public GearTemplateEntryType Type { get; set; }

        public GearTemplateSlot Slot { get => _slot; set => Common.SetProperty(ref _slot, value, OnSlotApply); }

        public BaseItem Item { get => _item; set => Common.SetProperty(ref _item, value, OnItemChanged); }

        public int MappedId
        {
            get => _mappedId; set
            {
                if (Common.SetProperty(ref _mappedId, value, OnPropertyChanged))
                {
                    Item = BuildsManager.Data.TryGetItemsFor<BaseItem>(_slot, out var items) ? items.Values.Where(e => e.MappedId == _mappedId).FirstOrDefault() : null;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnSlotApply()
        {
            Type =
                _slot.IsArmor() ? GearTemplateEntryType.Armor :
                _slot.IsWeapon() ? GearTemplateEntryType.Weapon :
                _slot.IsJuwellery() ? GearTemplateEntryType.Equipment :
                GearTemplateEntryType.None;
        }

        public virtual string ToCode()
        {
            return $"[{MappedId}]";
        }

        public virtual void OnItemChanged()
        {
            _mappedId = Item?.MappedId ?? -1;

            OnPropertyChanged(this, new(nameof(Item)));            
        }

        public virtual void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(sender, e);
        }

        public virtual void FromCode(string code)
        {
            MappedId = int.TryParse(code, out int mappedId) ? mappedId : -1;
        }
    }

    public class JuwelleryEntry : GearTemplateEntry
    {
        private Enrichment _enrichment;
        private Infusion _infusion;
        private Infusion _infusion2;
        private Infusion _infusion3;

        private ObservableList<int> _infusionIds = new();

        public JuwelleryEntry()
        {
            _enrichmentIds.PropertyChanged += EnrichmentIds_Changed;
        }

        public EquipmentStat Stat { get; set; }

        public ObservableList<int> InfusionIds
        {
            get => _infusionIds;
            set
            {
                var temp = _infusionIds;
                if (Common.SetProperty(ref _infusionIds, value))
                {
                    if (temp != null) temp.PropertyChanged -= InfusionIds_Changed;
                    if (_infusionIds != null)
                    {
                        _infusionIds.PropertyChanged += InfusionIds_Changed;
                        OnPropertyChanged(this, new(nameof(InfusionIds)));
                    }
                }
            }
        }

        private void InfusionIds_Changed(object sender, PropertyChangedEventArgs e)
        {
            if (_infusionIds != null && _infusionIds.Count > 0) _infusion = BuildsManager.Data.Infusions.Values.Where(e => e.MappedId == _infusionIds[0]).FirstOrDefault();
            if (_infusionIds != null && _infusionIds.Count > 1) _infusion2 = BuildsManager.Data.Infusions.Values.Where(e => e.MappedId == _infusionIds[1]).FirstOrDefault();
            if (_infusionIds != null && _infusionIds.Count > 2) _infusion3 = BuildsManager.Data.Infusions.Values.Where(e => e.MappedId == _infusionIds[2]).FirstOrDefault();

            OnPropertyChanged(sender, e);
        }

        public Infusion Infusion
        {
            get => InfusionIds.Count < 1 ? null : _infusion;
            set
            {
                if (Common.SetProperty(ref _infusion, value))
                {
                    InfusionIds[0] = _infusion?.MappedId ?? -1;
                }
            }
        }

        public Infusion Infusion2
        {
            get => InfusionIds.Count < 2 ? null : _infusion2;
            set
            {
                if (InfusionIds.Count < 2) return;
                if (Common.SetProperty(ref _infusion2, value))
                {
                    InfusionIds[1] = _infusion2?.MappedId ?? -1;
                }
            }
        }

        public Infusion Infusion3
        {
            get => InfusionIds.Count < 3 ? null : _infusion3;
            set
            {
                if (InfusionIds.Count < 3) return;
                if (Common.SetProperty(ref _infusion3, value))
                {
                    InfusionIds[2] = _infusion3?.MappedId ?? -1;
                }
            }
        }

        private ObservableList<int> _enrichmentIds = new();

        public ObservableList<int> EnrichmentIds
        {
            get => _enrichmentIds;
            set
            {
                var temp = _enrichmentIds;
                if (Common.SetProperty(ref _enrichmentIds, value))
                {
                    if (temp != null) temp.PropertyChanged -= EnrichmentIds_Changed;
                    if (_enrichmentIds != null)
                    {
                        _enrichmentIds.PropertyChanged += EnrichmentIds_Changed;
                        OnPropertyChanged(this, new(nameof(EnrichmentIds)));
                    }
                }
            }
        }

        private void EnrichmentIds_Changed(object sender, PropertyChangedEventArgs e)
        {
            if (_enrichmentIds != null && _enrichmentIds.Count > 0)
                _enrichment = BuildsManager.Data.Enrichments.Values.Where(e => e.MappedId == _enrichmentIds[0]).FirstOrDefault();

            OnPropertyChanged(sender, e);
        }

        public Enrichment Enrichment
        {
            get => EnrichmentIds.Count < 1 ? null : _enrichment;
            set
            {
                if (Common.SetProperty(ref _enrichment, value))
                {
                    EnrichmentIds[0] = _enrichment?.MappedId ?? -1;
                }
            }
        }

        public override void OnSlotApply()
        {
            base.OnSlotApply();

            EnrichmentIds = Slot switch
            {
                GearTemplateSlot.Amulet => new() { -1 },
                _ => null,
            };

            InfusionIds = Slot switch
            {
                GearTemplateSlot.Ring_1 or GearTemplateSlot.Ring_2 => new() { -1, -1, -1, },
                GearTemplateSlot.Back or GearTemplateSlot.Aquatic or GearTemplateSlot.AltAquatic => new() { -1, -1 },
                GearTemplateSlot.Amulet => null,
                _ => new() { -1 },
            };
        }

        public override string ToCode()
        {
            string enrichmentsOrInfusions = string.Join("|", EnrichmentIds ?? InfusionIds);
            return $"[{MappedId}|{(int)Stat}|{enrichmentsOrInfusions}]";
        }

        public override void FromCode(string code)
        {
            string[] parts = code.Split('|');

            if (Slot is GearTemplateSlot.Ring_1 or GearTemplateSlot.Ring_2)
            {
                if (parts.Length != 5) return;
            }
            else if (Slot is GearTemplateSlot.Back)
            {
                if (parts.Length != 4) return;
            }
            else
            {
                if (parts.Length != 3) return;
            }

            MappedId = int.TryParse(parts[0], out int mappedId) ? mappedId : -1;
            Stat = (EquipmentStat)(int.TryParse(parts[1], out int stat) ? stat : -1);

            if (Slot == GearTemplateSlot.Back)
            {
                InfusionIds[0] = int.TryParse(parts[2], out int infusion1) ? infusion1 : -1;
                InfusionIds[1] = int.TryParse(parts[3], out int infusion2) ? infusion2 : -1;
            }
            else if (Slot is GearTemplateSlot.Ring_1 or GearTemplateSlot.Ring_2)
            {
                InfusionIds[0] = int.TryParse(parts[2], out int infusion1) ? infusion1 : -1;
                InfusionIds[1] = int.TryParse(parts[3], out int infusion2) ? infusion2 : -1;
                InfusionIds[2] = int.TryParse(parts[4], out int infusion3) ? infusion3 : -1;
            }
            else if (Slot is GearTemplateSlot.Amulet)
            {
                EnrichmentIds[0] = int.TryParse(parts[2], out int enrichment) ? enrichment : -1;
                Enrichment = BuildsManager.Data.Enrichments.Values.Where(e => e.MappedId == enrichment).FirstOrDefault();
            }
            else
            {
                InfusionIds[0] = int.TryParse(parts[2], out int infusion1) ? infusion1 : -1;
            }

            if (MappedId > -1)
            {
                Item = Slot == GearTemplateSlot.Back ?
                    BuildsManager.Data.Backs.Values.Where(e => e.MappedId == mappedId).FirstOrDefault() :
                    BuildsManager.Data.Trinkets.Values.Where(e => e.MappedId == mappedId).FirstOrDefault();
            }
        }
    }

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
            if (_sigilIds != null && _sigilIds.Count > 1 && Slot is not GearTemplateSlot.Aquatic and not GearTemplateSlot.AltAquatic) _pvpSigil = BuildsManager.Data.PvpSigils.Values.Where(e => e.MappedId == _sigilIds[1]).FirstOrDefault();
            if (_sigilIds != null && _sigilIds.Count > 1 && Slot is GearTemplateSlot.Aquatic or GearTemplateSlot.AltAquatic) _sigil2 = BuildsManager.Data.PveSigils.Values.Where(e => e.MappedId == _sigilIds[1]).FirstOrDefault();

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
                GearTemplateSlot.Aquatic or GearTemplateSlot.AltAquatic => new() { -1, -1 },
                _ => new() { -1, -1, },
            };
        }

        public override string ToCode()
        {
            string infusions = string.Join("|", InfusionIds);
            string sigils = string.Join("|", SigilIds);

            return $"[{MappedId}|{(int)Stat}|{infusions}|{sigils}]";
        }

        public override void FromCode(string code)
        {
            string[] parts = code.Split('|');

            if (Slot is GearTemplateSlot.Aquatic or GearTemplateSlot.AltAquatic)
            {
                if (parts.Length == 6)
                {
                    MappedId = int.TryParse(parts[0], out int mappedId) ? mappedId : -1;
                    Stat = (EquipmentStat)(int.TryParse(parts[1], out int stat) ? stat : -1);
                    InfusionIds[0] = int.TryParse(parts[2], out int infusion1) ? infusion1 : -1;
                    InfusionIds[1] = int.TryParse(parts[3], out int infusion2) ? infusion2 : -1;
                    SigilIds[0] = int.TryParse(parts[4], out int sigil1) ? sigil1 : -1;
                    SigilIds[1] = int.TryParse(parts[5], out int sigil2) ? sigil2 : -1;
                }
            }
            else if (parts.Length == 5)
            {
                MappedId = int.TryParse(parts[0], out int mappedId) ? mappedId : -1;
                Stat = (EquipmentStat)(int.TryParse(parts[1], out int stat) ? stat : -1);
                InfusionIds[0] = int.TryParse(parts[2], out int infusionId) ? infusionId : -1;
                SigilIds[0] = int.TryParse(parts[3], out int sigil) ? sigil : -1;
                SigilIds[1] = int.TryParse(parts[4], out int pvpsigil) ? pvpsigil : -1;
            }

            if (MappedId > -1)
            {
                Item = BuildsManager.Data.Weapons.Values.Where(e => e.MappedId == MappedId).FirstOrDefault();
            }
        }
    }

    public class ArmorEntry : JuwelleryEntry
    {
        private Rune _rune;
        private ObservableList<int> _runeIds = new() { -1 };

        public ArmorEntry()
        {
            _runeIds.PropertyChanged += RuneIds_Changed;
        }

        public ObservableList<int> RuneIds
        {
            get => _runeIds;
            set
            {
                var temp = _runeIds;
                if (Common.SetProperty(ref _runeIds, value))
                {
                    if (temp != null) temp.PropertyChanged -= RuneIds_Changed;
                    if (_runeIds != null)
                    {
                        _runeIds.PropertyChanged += RuneIds_Changed;
                    }
                }
            }
        }

        private void RuneIds_Changed(object sender, PropertyChangedEventArgs e)
        {
            if (_runeIds != null && _runeIds.Count > 0) _rune = BuildsManager.Data.PveRunes.Values.Where(e => e.MappedId == _runeIds[0]).FirstOrDefault();

            OnPropertyChanged(sender, e);
        }

        public Rune Rune
        {
            get => _rune; set
            {
                if (Common.SetProperty(ref _rune, value))
                {
                    _runeIds[0] = _rune?.MappedId ?? -1;
                }
            }
        }

        public override void OnSlotApply()
        {
            base.OnSlotApply();
        }

        public override string ToCode()
        {
            string infusions = string.Join("|", InfusionIds);
            string runes = string.Join("|", RuneIds);

            return $"[{MappedId}|{(int)Stat}|{infusions}|{runes}]";
        }

        public override void FromCode(string code)
        {
            string[] parts = code.Split('|');

            if (parts.Length == 4)
            {
                MappedId = int.TryParse(parts[0], out int mappedId) ? mappedId : -1;
                Stat = (EquipmentStat)(int.TryParse(parts[1], out int stat) ? stat : 0);
                InfusionIds[0] = int.TryParse(parts[2], out int infusionId) ? infusionId : 0;
                RuneIds[0] = int.TryParse(parts[3], out int runeId) ? runeId : 0;

                if (MappedId > -1)
                {
                    Item = BuildsManager.Data.Armors.Values.Where(e => e.MappedId == mappedId).FirstOrDefault();
                }
            }
        }
    }

    public enum GearTemplateEntryType
    {
        None,
        Equipment,
        Weapon,
        Armor,
        PvpAmulet,
    }

    public class GearTemplate : Dictionary<GearTemplateEntryType, DeepObservableDictionary<GearTemplateSlot, GearTemplateEntry>>, INotifyPropertyChanged
    {
        private bool _loading = false;
        public GearTemplate()
        {
            foreach (GearTemplateEntryType type in Enum.GetValues(typeof(GearTemplateEntryType)))
            {
                Add(type, new());
            }

            foreach (GearTemplateSlot slot in Enum.GetValues(typeof(GearTemplateSlot)))
            {
                if (slot.IsWeapon())
                {
                    this[GearTemplateEntryType.Weapon].Add(slot, new WeaponEntry() { Slot = slot });
                }
                else if (slot.IsArmor())
                {
                    this[GearTemplateEntryType.Armor].Add(slot, new ArmorEntry() { Slot = slot });
                }
                else if (slot.IsJuwellery())
                {
                    this[GearTemplateEntryType.Equipment].Add(slot, new JuwelleryEntry() { Slot = slot });
                }
                else if (slot is GearTemplateSlot.PvpAmulet)
                {
                    this[GearTemplateEntryType.PvpAmulet].Add(slot, new ArmorEntry() { Slot = slot });
                }
                else if (slot is not GearTemplateSlot.None)
                {
                    this[GearTemplateEntryType.None].Add(slot, new GearTemplateEntry() { Slot = slot });
                }
            }

            this[GearTemplateEntryType.Weapon].ItemChanged += ItemChanged;
            this[GearTemplateEntryType.Armor].ItemChanged += ItemChanged;
            this[GearTemplateEntryType.Equipment].ItemChanged += ItemChanged;
            this[GearTemplateEntryType.None].ItemChanged += ItemChanged;
        }

        private void ItemChanged(object sender, PropertyChangedEventArgs e)
        {
            if(_loading) return;

            if (Weapons[GearTemplateSlot.MainHand].Weapon is WeaponType.Staff or WeaponType.Rifle or WeaponType.Hammer or WeaponType.Greatsword or WeaponType.LongBow or WeaponType.ShortBow)
            {
                Weapons[GearTemplateSlot.OffHand].Item = null;
                Weapons[GearTemplateSlot.OffHand].MappedId = -1;
            }

            if (Weapons[GearTemplateSlot.AltMainHand].Weapon is WeaponType.Staff or WeaponType.Rifle or WeaponType.Hammer or WeaponType.Greatsword or WeaponType.LongBow or WeaponType.ShortBow)
            {
                Weapons[GearTemplateSlot.AltOffHand].Item = null;
                Weapons[GearTemplateSlot.AltOffHand].MappedId = -1;
            }

            PropertyChanged?.Invoke(sender, e);
        }

        public GearTemplate(string code) : this()
        {
            LoadFromCode(code);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Dictionary<GearTemplateSlot, WeaponEntry> Weapons => this[GearTemplateEntryType.Weapon].ToDictionary(e => e.Key, e => (WeaponEntry)e.Value);

        public Dictionary<GearTemplateSlot, ArmorEntry> Armors => this[GearTemplateEntryType.Armor].ToDictionary(e => e.Key, e => (ArmorEntry)e.Value);

        public Dictionary<GearTemplateSlot, JuwelleryEntry> Juwellery => this[GearTemplateEntryType.Equipment].ToDictionary(e => e.Key, e => (JuwelleryEntry)e.Value);

        public Dictionary<GearTemplateSlot, GearTemplateEntry> PvpAmulets => this[GearTemplateEntryType.PvpAmulet];

        public Dictionary<GearTemplateSlot, GearTemplateEntry> Common => this[GearTemplateEntryType.None];

        public string ParseGearCode()
        {
            string code = "";

            foreach (var entry in Armors.Values.OrderBy(e => e.Slot))
            {
                code += entry.ToCode();
            }

            foreach (var entry in Weapons.Values.OrderBy(e => e.Slot))
            {
                code += entry.ToCode();
            }

            foreach (var entry in Juwellery.Values.OrderBy(e => e.Slot))
            {
                code += entry.ToCode();
            }

            foreach (var entry in PvpAmulets.Values.OrderBy(e => e.Slot))
            {
                code += entry.ToCode();
            }

            foreach (var entry in Common.Values.OrderBy(e => e.Slot))
            {
                code += entry.ToCode();
            }

            return code;
        }

        public void LoadFromCode(string code)
        {
            string[] parts = code.Split(']');

            if (parts.Length == 24)
            {
                _loading = true;

                for (int i = (int)GearTemplateSlot.Head; i <= (int)GearTemplateSlot.AquaBreather; i++)
                {
                    Armors[(GearTemplateSlot)i].FromCode(parts[i].Substring(1, parts[i].Length - 1));
                }

                for (int i = (int)GearTemplateSlot.MainHand; i <= (int)GearTemplateSlot.AltAquatic; i++)
                {
                    Weapons[(GearTemplateSlot)i].FromCode(parts[i].Substring(1, parts[i].Length - 1));
                }

                for (int i = (int)GearTemplateSlot.Back; i <= (int)GearTemplateSlot.Ring_2; i++)
                {
                    Juwellery[(GearTemplateSlot)i].FromCode(parts[i].Substring(1, parts[i].Length - 1));
                }

                PvpAmulets[GearTemplateSlot.PvpAmulet].FromCode(parts[(int)GearTemplateSlot.PvpAmulet].Substring(1, parts[(int)GearTemplateSlot.PvpAmulet].Length - 1));
                Common[GearTemplateSlot.Nourishment].FromCode(parts[(int)GearTemplateSlot.Nourishment].Substring(1, parts[(int)GearTemplateSlot.Nourishment].Length - 1));
                Common[GearTemplateSlot.Utility].FromCode(parts[(int)GearTemplateSlot.Utility].Substring(1, parts[(int)GearTemplateSlot.Utility].Length - 1));
                Common[GearTemplateSlot.JadeBotCore].FromCode(parts[(int)GearTemplateSlot.JadeBotCore].Substring(1, parts[(int)GearTemplateSlot.JadeBotCore].Length - 1));

                _loading = false;
            }
        }
    }
}
