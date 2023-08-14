using System.ComponentModel;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.AdvancedBuildsManager.DataModels.Items;
using System.Collections.Generic;
using System;
using Kenedia.Modules.AdvancedBuildsManager.Extensions;
using Kenedia.Modules.AdvancedBuildsManager.DataModels.Stats;
using System.Linq;
using static Kenedia.Modules.AdvancedBuildsManager.DataModels.Professions.Weapon;
using Kenedia.Modules.Core.Models;
using System.Data;
using Kenedia.Modules.Core.Extensions;
using Gw2Sharp.WebApi.V2.Models;

namespace Kenedia.Modules.AdvancedBuildsManager.Models.Templates
{
    public class BaseTemplateEntry : INotifyPropertyChanged
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
                    Item = AdvancedBuildsManager.Data.TryGetItemsFor<BaseItem>(_slot, out var items) ? items.Values.Where(e => e.MappedId == _mappedId).FirstOrDefault() : null;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnSlotApply()
        {
            Type =
                _slot.IsArmor() ? GearTemplateEntryType.Armor :
                _slot.IsWeapon() ? GearTemplateEntryType.Weapon :
                _slot.IsJewellery() ? GearTemplateEntryType.Equipment :
                GearTemplateEntryType.None;
        }

        public virtual void Reset()
        {
            ResetItem();
        }

        public void ResetItem()
        {
            Item = null;
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

    public class GearTemplateEntry : BaseTemplateEntry
    {
        private Stat _stat;
        private ItemRarity _itemRarity;

        public Stat Stat { get => _stat; set => Common.SetProperty(ref _stat, value, OnPropertyChanged); }

        public ItemRarity ItemRarity { get => _itemRarity; set => Common.SetProperty(ref _itemRarity, value, OnRarityChanged); }

        private void OnRarityChanged(object sender, PropertyChangedEventArgs e)
        {
            int? currentStatId = Stat?.Id;

            if (Stat is not null && (Item == null || !(Item as EquipmentItem).StatChoices.Contains(Stat.Id)))
            {
                if (AdvancedBuildsManager.Data.StatMap.TryFind(e => e.Stat == Stat.EquipmentStatType, out var statMap))
                {
                    var choices = (Item as EquipmentItem)?.StatChoices;

                    if (choices is not null && choices.TryFind(statMap.Ids.Contains, out int newStatId))
                    {
                        Stat = AdvancedBuildsManager.Data.Stats[newStatId];
                        return;
                    }
                }

                Stat = null;
            }
            else
            {
                OnPropertyChanged(sender, e);
            }
        }

        public override void OnItemChanged()
        {
            base.OnItemChanged();

            ItemRarity = Item?.Rarity ?? ItemRarity.Unknown;
        }

        public override void Reset()
        {
            ResetStat();
            ResetUpgrades();
        }

        public void ResetStat()
        {
            Stat = null;
        }

        public virtual void ResetUpgrades()
        {

        }
    }

    public class JewelleryEntry : GearTemplateEntry
    {
        private Enrichment _enrichment;
        private Infusion _infusion;
        private Infusion _infusion2;
        private Infusion _infusion3;

        private ObservableList<int> _infusionIds = new();

        public JewelleryEntry()
        {
            _enrichmentIds.PropertyChanged += EnrichmentIds_Changed;
        }

        public ObservableList<int> InfusionIds
        {
            get => _infusionIds;
            set
            {
                var temp = _infusionIds;
                if (Common.SetProperty(ref _infusionIds, value))
                {
                    if (temp is not null) temp.PropertyChanged -= InfusionIds_Changed;
                    if (_infusionIds is not null)
                    {
                        _infusionIds.PropertyChanged += InfusionIds_Changed;
                        OnPropertyChanged(this, new(nameof(InfusionIds)));
                    }
                }
            }
        }

        private void InfusionIds_Changed(object sender, PropertyChangedEventArgs e)
        {
            if (_infusionIds is not null && _infusionIds.Count > 0) _infusion = AdvancedBuildsManager.Data.Infusions.Values.Where(e => e.MappedId == _infusionIds[0]).FirstOrDefault();
            if (_infusionIds is not null && _infusionIds.Count > 1) _infusion2 = AdvancedBuildsManager.Data.Infusions.Values.Where(e => e.MappedId == _infusionIds[1]).FirstOrDefault();
            if (_infusionIds is not null && _infusionIds.Count > 2) _infusion3 = AdvancedBuildsManager.Data.Infusions.Values.Where(e => e.MappedId == _infusionIds[2]).FirstOrDefault();

            OnPropertyChanged(sender, e);
        }

        public Infusion Infusion
        {
            get => InfusionIds == null || InfusionIds.Count < 1 ? null : _infusion;
            set
            {
                if (Common.SetProperty(ref _infusion, value) && InfusionIds is not null)
                {
                    InfusionIds[0] = _infusion?.MappedId ?? -1;
                }
            }
        }

        public Infusion Infusion2
        {
            get => InfusionIds == null || InfusionIds.Count < 2 ? null : _infusion2;
            set
            {
                if (InfusionIds == null || InfusionIds.Count < 2) return;
                if (Common.SetProperty(ref _infusion2, value) && InfusionIds is not null)
                {
                    InfusionIds[1] = _infusion2?.MappedId ?? -1;
                }
            }
        }

        public Infusion Infusion3
        {
            get => InfusionIds == null || InfusionIds.Count < 3 ? null : _infusion3;
            set
            {
                if (InfusionIds == null || InfusionIds.Count < 3) return;
                if (Common.SetProperty(ref _infusion3, value) && InfusionIds is not null)
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
                    if (temp is not null) temp.PropertyChanged -= EnrichmentIds_Changed;
                    if (_enrichmentIds is not null)
                    {
                        _enrichmentIds.PropertyChanged += EnrichmentIds_Changed;
                        OnPropertyChanged(this, new(nameof(EnrichmentIds)));
                    }
                }
            }
        }

        private void EnrichmentIds_Changed(object sender, PropertyChangedEventArgs e)
        {
            if (_enrichmentIds is not null && _enrichmentIds.Count > 0)
                _enrichment = AdvancedBuildsManager.Data.Enrichments.Values.Where(e => e.MappedId == _enrichmentIds[0]).FirstOrDefault();

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
            return $"[{MappedId}|{Stat?.MappedId ?? -1}|{enrichmentsOrInfusions}]";
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
            Stat = int.TryParse(parts[1], out int stat) ? AdvancedBuildsManager.Data.Stats.Where(e => e.Value.MappedId == stat).FirstOrDefault().Value : null;

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
                Enrichment = AdvancedBuildsManager.Data.Enrichments.Values.Where(e => e.MappedId == enrichment).FirstOrDefault();
            }
            else
            {
                InfusionIds[0] = int.TryParse(parts[2], out int infusion1) ? infusion1 : -1;
            }

            if (MappedId > -1)
            {
                Item = Slot == GearTemplateSlot.Back ?
                    AdvancedBuildsManager.Data.Backs.Values.Where(e => e.MappedId == mappedId).FirstOrDefault() :
                    AdvancedBuildsManager.Data.Trinkets.Values.Where(e => e.MappedId == mappedId).FirstOrDefault();
            }
        }

        public override void Reset()
        {
            base.Reset();

            ResetItem();
            ResetInfusion();
            ResetEnrichment();
        }

        public void ResetInfusion(int? index = null)
        {
            switch (index)
            {
                case null:
                    Infusion = null;
                    Infusion2 = null;
                    Infusion3 = null;
                    break;

                case 0:
                    Infusion = null;
                    break;

                case 1:
                    Infusion2 = null;
                    break;

                case 2:
                    Infusion3 = null;
                    break;
            }
        }

        public void ResetEnrichment()
        {
            Enrichment = null;
        }
    }

    public class WeaponEntry : JewelleryEntry
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
            if (_sigilIds is not null && _sigilIds.Count > 0) _sigil = AdvancedBuildsManager.Data.PveSigils.Values.Where(e => e.MappedId == _sigilIds[0]).FirstOrDefault();
            if (_sigilIds is not null && _sigilIds.Count > 1 && Slot is not GearTemplateSlot.Aquatic and not GearTemplateSlot.AltAquatic) _pvpSigil = AdvancedBuildsManager.Data.PvpSigils.Values.Where(e => e.MappedId == _sigilIds[1]).FirstOrDefault();
            if (_sigilIds is not null && _sigilIds.Count > 1 && Slot is GearTemplateSlot.Aquatic or GearTemplateSlot.AltAquatic) _sigil2 = AdvancedBuildsManager.Data.PveSigils.Values.Where(e => e.MappedId == _sigilIds[1]).FirstOrDefault();

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
                    if (temp is not null) temp.PropertyChanged -= SigilIds_Changed;
                    if (_sigilIds is not null)
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

            return $"[{MappedId}|{Stat?.MappedId ?? -1}|{infusions}|{sigils}]";
        }

        public override void FromCode(string code)
        {
            string[] parts = code.Split('|');

            if (Slot is GearTemplateSlot.Aquatic or GearTemplateSlot.AltAquatic)
            {
                if (parts.Length == 6)
                {
                    MappedId = int.TryParse(parts[0], out int mappedId) ? mappedId : -1;
                    Stat = int.TryParse(parts[1], out int stat) ? AdvancedBuildsManager.Data.Stats.Where(e => e.Value.MappedId == stat).FirstOrDefault().Value : null;
                    InfusionIds[0] = int.TryParse(parts[2], out int infusion1) ? infusion1 : -1;
                    InfusionIds[1] = int.TryParse(parts[3], out int infusion2) ? infusion2 : -1;
                    SigilIds[0] = int.TryParse(parts[4], out int sigil1) ? sigil1 : -1;
                    SigilIds[1] = int.TryParse(parts[5], out int sigil2) ? sigil2 : -1;
                }
            }
            else if (parts.Length == 5)
            {
                MappedId = int.TryParse(parts[0], out int mappedId) ? mappedId : -1;
                Stat = int.TryParse(parts[1], out int stat) ? AdvancedBuildsManager.Data.Stats.Where(e => e.Value.MappedId == stat).FirstOrDefault().Value : null;
                InfusionIds[0] = int.TryParse(parts[2], out int infusionId) ? infusionId : -1;
                SigilIds[0] = int.TryParse(parts[3], out int sigil) ? sigil : -1;
                SigilIds[1] = int.TryParse(parts[4], out int pvpsigil) ? pvpsigil : -1;
            }

            if (MappedId > -1)
            {
                Item = AdvancedBuildsManager.Data.Weapons.Values.Where(e => e.MappedId == MappedId).FirstOrDefault();
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

    public class ArmorEntry : JewelleryEntry
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
                    if (temp is not null) temp.PropertyChanged -= RuneIds_Changed;
                    if (_runeIds is not null)
                    {
                        _runeIds.PropertyChanged += RuneIds_Changed;
                    }
                }
            }
        }

        private void RuneIds_Changed(object sender, PropertyChangedEventArgs e)
        {
            if (_runeIds is not null && _runeIds.Count > 0) _rune = AdvancedBuildsManager.Data.PveRunes.Values.Where(e => e.MappedId == _runeIds[0]).FirstOrDefault();

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

            return $"[{MappedId}|{Stat?.MappedId ?? -1}|{infusions}|{runes}]";
        }

        public override void FromCode(string code)
        {
            string[] parts = code.Split('|');

            if (parts.Length == 4)
            {
                MappedId = int.TryParse(parts[0], out int mappedId) ? mappedId : -1;
                Stat = int.TryParse(parts[1], out int stat) ? AdvancedBuildsManager.Data.Stats.Where(e => e.Value.MappedId == stat).FirstOrDefault().Value : null;
                InfusionIds[0] = int.TryParse(parts[2], out int infusionId) ? infusionId : 0;
                RuneIds[0] = int.TryParse(parts[3], out int runeId) ? runeId : 0;

                if (MappedId > -1)
                {
                    Item = AdvancedBuildsManager.Data.Armors.Values.Where(e => e.MappedId == mappedId).FirstOrDefault();
                }
            }
        }

        public override void ResetUpgrades()
        {
            base.ResetUpgrades();
            ResetRune();
        }

        public void ResetRune()
        {
            Rune = null;
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

    public class GearTemplate : Dictionary<GearTemplateEntryType, DeepObservableDictionary<GearTemplateSlot, BaseTemplateEntry>>, INotifyPropertyChanged
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
                else if (slot.IsJewellery())
                {
                    this[GearTemplateEntryType.Equipment].Add(slot, new JewelleryEntry() { Slot = slot });
                }
                else if (slot is GearTemplateSlot.PvpAmulet)
                {
                    this[GearTemplateEntryType.PvpAmulet].Add(slot, new ArmorEntry() { Slot = slot });
                }
                else if (slot is not GearTemplateSlot.None)
                {
                    this[GearTemplateEntryType.None].Add(slot, new BaseTemplateEntry() { Slot = slot });
                }
            }

            this[GearTemplateEntryType.Weapon].ItemChanged += ItemChanged;
            this[GearTemplateEntryType.Armor].ItemChanged += ItemChanged;
            this[GearTemplateEntryType.Equipment].ItemChanged += ItemChanged;
            this[GearTemplateEntryType.None].ItemChanged += ItemChanged;
        }

        private void ItemChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_loading) return;

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

        public Dictionary<GearTemplateSlot, JewelleryEntry> Jewellery => this[GearTemplateEntryType.Equipment].ToDictionary(e => e.Key, e => (JewelleryEntry)e.Value);

        public Dictionary<GearTemplateSlot, BaseTemplateEntry> PvpAmulets => this[GearTemplateEntryType.PvpAmulet];

        public Dictionary<GearTemplateSlot, BaseTemplateEntry> Common => this[GearTemplateEntryType.None];

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

            foreach (var entry in Jewellery.Values.OrderBy(e => e.Slot))
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
                    Jewellery[(GearTemplateSlot)i].FromCode(parts[i].Substring(1, parts[i].Length - 1));
                }

                PvpAmulets[GearTemplateSlot.PvpAmulet].FromCode(parts[(int)GearTemplateSlot.PvpAmulet].Substring(1, parts[(int)GearTemplateSlot.PvpAmulet].Length - 1));
                Common[GearTemplateSlot.Nourishment].FromCode(parts[(int)GearTemplateSlot.Nourishment].Substring(1, parts[(int)GearTemplateSlot.Nourishment].Length - 1));
                Common[GearTemplateSlot.Utility].FromCode(parts[(int)GearTemplateSlot.Utility].Substring(1, parts[(int)GearTemplateSlot.Utility].Length - 1));
                Common[GearTemplateSlot.JadeBotCore].FromCode(parts[(int)GearTemplateSlot.JadeBotCore].Substring(1, parts[(int)GearTemplateSlot.JadeBotCore].Length - 1));

                PropertyChanged?.Invoke(this, null);

                _loading = false;
            }
        }
    }
}
