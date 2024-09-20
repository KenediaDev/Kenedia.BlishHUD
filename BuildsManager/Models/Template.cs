using Gw2Sharp.ChatLinks;
using ProfessionType = Gw2Sharp.Models.ProfessionType;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.DataModels;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Kenedia.Modules.BuildsManager.TemplateEntries;
using System.Collections.Generic;
using Blish_HUD.Gw2Mumble;
using Kenedia.Modules.BuildsManager.Utility;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.DataModels.Stats;
using Kenedia.Modules.BuildsManager.Res;
using Kenedia.Modules.BuildsManager.Services;
using Microsoft.Extensions.DependencyInjection;
using Gw2Sharp;
using Kenedia.Modules.BuildsManager.Extensions;
using System.Timers;

namespace Kenedia.Modules.BuildsManager.Models
{

    public delegate void TemplateChangedEventHandler<T>(object sender, TemplateChangedEventArgs<T?> e);

    public delegate void SkillChangedEventHandler(object sender, SkillChangedEventArgs e);

    public delegate void TraitChangedEventHandler(object sender, TraitChangedEventArgs e);

    public delegate void SpecializationChangedEventHandler(object sender, SpecializationChangedEventArgs e);

    public class TraitSlotTrait
    {
        public Trait Trait { get; set; }

        public TraitTierType Slot { get; set; }

        public TraitSlotTrait(Trait trait, TraitTierType slot)
        {
            Trait = trait;
            Slot = slot;
        }
    }


    [DataContract]
    public class Template : IDisposable
    {
        private readonly System.Timers.Timer _timer;

        public Data Data { get; } = BuildsManager.ModuleInstance.ServiceProvider.GetRequiredService<Data>();

#nullable enable
        private bool _loaded = false;
        private bool _isDisposed = false;
        private bool _triggerEvents = true;

        private Races _race = Races.None;
        private ProfessionType _profession = ProfessionType.Guardian;

        private string _name = strings.NewTemplate;
        private string _description = string.Empty;

        private string _savedBuildCode = string.Empty;
        private string _savedGearCode = string.Empty;
        public Specialization? _savedEliteSpecialization = null;

        [JsonProperty("Tags")]
        [DataMember]
        private UniqueObservableCollection<string> _tags;

        private CancellationTokenSource? _cancellationTokenSource;

        private bool _saveRequested;
        private List<BuildSpecialization> _specializations;

        public event EventHandler? GearCodeChanged;

        public event EventHandler? BuildCodeChanged;

        public event EventHandler? LoadedGearFromCode;

        public event EventHandler? LoadedBuildFromCode;

        public event ValueChangedEventHandler<string>? NameChanged;

        public event ValueChangedEventHandler<Races>? RaceChanged;

        public event ValueChangedEventHandler<ProfessionType>? ProfessionChanged;


        public event DictionaryItemChangedEventHandler<SpecializationSlotType, Specialization?>? SpecializationChanged_OLD;

        public event DictionaryItemChangedEventHandler<LegendSlotType, Legend?>? LegendChanged;

        public event DictionaryItemChangedEventHandler<SkillSlotType, Skill?> SkillChanged_OLD;

        //REWORKED
        public event SkillChangedEventHandler SkillChanged;

        public event TraitChangedEventHandler TraitChanged;

        public event SpecializationChangedEventHandler SpecializationChanged;

        public event SpecializationChangedEventHandler EliteSpecializationChanged;

        public Template()
        {
            _timer = new(1000);
            _timer.Elapsed += OnTimerElapsed;

            Weapons = new()
                {
                {TemplateSlotType.MainHand, MainHand },
                {TemplateSlotType.OffHand, OffHand },
                {TemplateSlotType.Aquatic, Aquatic},
                {TemplateSlotType.AltMainHand, AltMainHand},
                {TemplateSlotType.AltOffHand, AltOffHand },
                {TemplateSlotType.AltAquatic, AltAquatic},
                };

            Armors = new()
                {
                {TemplateSlotType.MainHand, MainHand },
                {TemplateSlotType.OffHand, OffHand },
                {TemplateSlotType.Aquatic, Aquatic},
                {TemplateSlotType.AltMainHand, AltMainHand},
                {TemplateSlotType.AltOffHand, AltOffHand },
                {TemplateSlotType.AltAquatic, AltAquatic},
                };

            Jewellery = new()
                {
                {TemplateSlotType.MainHand, MainHand },
                {TemplateSlotType.OffHand, OffHand },
                {TemplateSlotType.Aquatic, Aquatic},
                {TemplateSlotType.AltMainHand, AltMainHand},
                {TemplateSlotType.AltOffHand, AltOffHand },
                {TemplateSlotType.AltAquatic, AltAquatic},
                };

            Legends.ItemChanged += Legends_ItemChanged;
            Pets.ItemChanged += Pets_ItemChanged;
            Skills.ItemChanged += Skills_ItemChanged;

            MainHand.PairedWeapon = OffHand;
            OffHand.PairedWeapon = MainHand;

            AltMainHand.PairedWeapon = AltOffHand;
            AltOffHand.PairedWeapon = AltMainHand;

            RegisterGearListeners();

            PlayerCharacter player = Blish_HUD.GameService.Gw2Mumble.PlayerCharacter;
            Profession = player?.Profession ?? Profession;
            _tags = new();
        }

        private async void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (_saveRequested)
            {
                _timer.Stop();

                await Save();
            }
        }

        private void RequestSave()
        {
            _saveRequested = true;

            _timer.Stop();
            _timer.Start();
        }

        public Template(string? buildCode, string? gearCode) : this()
        {
            LoadFromCode(buildCode, gearCode);
        }

        [JsonConstructor]
        public Template(string name, string buildCode, string gearCode, string description, UniqueObservableCollection<string> tags, Races? race, ProfessionType? profession, int? elitespecId) : this()
        {
            // Disable Events to prevent unnecessary event triggers during the load
            _triggerEvents = false;

            _name = name;
            _race = race ?? Races.None;
            _profession = profession ?? ProfessionType.Guardian;
            _savedEliteSpecialization = Data.Professions[Profession]?.Specializations.FirstOrDefault(e => e.Value.Id == elitespecId).Value;
            _description = description;
            Tags = tags ?? _tags;

            // Enable Events again to become responsive
            _triggerEvents = true;

            _savedBuildCode = buildCode;
            _savedGearCode = gearCode;

            SetArmorItems();
        }

        public event EventHandler<(TemplateSlotType slot, BaseItem item, Stat stat)> TemplateSlotChanged;

        #region General Template 
        public string FilePath => @$"{BuildsManager.ModuleInstance.Paths.TemplatesPath}{Common.MakeValidFileName(Name.Trim())}.json";

        [DataMember]
        public ProfessionType Profession { get => _profession; set => Common.SetProperty(ref _profession, value, OnProfessionChanged); }

        [DataMember]
        public Races Race { get => _race; set => Common.SetProperty(ref _race, value, OnRaceChanged, _triggerEvents); }

        public UniqueObservableCollection<string> Tags { get => _tags; private set => Common.SetProperty(ref _tags, value, OnTagsListChanged); }

        [DataMember]
        public string Name { get => _name; set => Common.SetProperty(ref _name, value, OnNameChanged, _triggerEvents); }

        [DataMember]
        public string Description { get => _description; set => Common.SetProperty(ref _description, value, OnDescriptionChanged, _triggerEvents); }

        [DataMember]
        public string? BuildCode
        {
            set => LoadBuildFromCode(value);
            get => !_loaded ? _savedBuildCode : ParseBuildCode();
        }

        [DataMember]
        public string? GearCode
        {
            set => LoadGearFromCode(value);
            get => !_loaded ? _savedGearCode : ParseGearCode();
        }
        #endregion General Template

        #region Build

        [DataMember]
        public int? EliteSpecializationId => Specializations.Specialization3.Specialization?.Id ?? _savedEliteSpecialization?.Id;

        public Specialization? EliteSpecialization => Specializations.Specialization3?.Specialization ?? _savedEliteSpecialization;

        public PetCollection Pets { get; } = new();

        public SkillCollection Skills { get; } = new();

        public LegendCollection Legends { get; } = new();

        public Specializations Specializations { get; } = new();

        public BuildSpecialization? this[SpecializationSlotType slot] => slot switch
        {
            SpecializationSlotType.Line_1 => Specializations.Specialization1,
            SpecializationSlotType.Line_2 => Specializations.Specialization2,
            SpecializationSlotType.Line_3 => Specializations.Specialization3,
            _ => null,
        };

        private void OnTagsListChanged(object sender, ValueChangedEventArgs<UniqueObservableCollection<string>> e)
        {
            if (e.NewValue is not null)
            {
                e.NewValue.CollectionChanged += NewValue_CollectionChanged;
            }

            if (e.OldValue is not null)
            {
                e.OldValue.CollectionChanged -= NewValue_CollectionChanged;
            }
        }

        private void NewValue_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //if (_triggerEvents)
            //    AutoSave();
        }
        #endregion Build

        #region Gear
        public TemplateEntry? this[TemplateSlotType slot] => slot == TemplateSlotType.None ? null : (TemplateEntry)GetType().GetProperty(slot.ToString()).GetValue(this);

        public ArmorTemplateEntry Head { get; } = new(TemplateSlotType.Head);

        public ArmorTemplateEntry Shoulder { get; } = new(TemplateSlotType.Shoulder);

        public ArmorTemplateEntry Chest { get; } = new(TemplateSlotType.Chest);

        public ArmorTemplateEntry Hand { get; } = new(TemplateSlotType.Hand);

        public ArmorTemplateEntry Leg { get; } = new(TemplateSlotType.Leg);

        public ArmorTemplateEntry Foot { get; } = new(TemplateSlotType.Foot);

        public ArmorTemplateEntry AquaBreather { get; } = new(TemplateSlotType.AquaBreather);

        public WeaponTemplateEntry MainHand { get; } = new(TemplateSlotType.MainHand);

        public WeaponTemplateEntry OffHand { get; } = new(TemplateSlotType.OffHand);

        public AquaticWeaponTemplateEntry Aquatic { get; } = new(TemplateSlotType.Aquatic);

        public WeaponTemplateEntry AltMainHand { get; } = new(TemplateSlotType.AltMainHand);

        public WeaponTemplateEntry AltOffHand { get; } = new(TemplateSlotType.AltOffHand);

        public AquaticWeaponTemplateEntry AltAquatic { get; } = new(TemplateSlotType.AltAquatic);

        public BackTemplateEntry Back { get; } = new(TemplateSlotType.Back);

        public AmuletTemplateEntry Amulet { get; } = new(TemplateSlotType.Amulet);

        public AccessoireTemplateEntry Accessory_1 { get; } = new(TemplateSlotType.Accessory_1);

        public AccessoireTemplateEntry Accessory_2 { get; } = new(TemplateSlotType.Accessory_2);

        public RingTemplateEntry Ring_1 { get; } = new(TemplateSlotType.Ring_1);

        public RingTemplateEntry Ring_2 { get; } = new(TemplateSlotType.Ring_2);

        public PvpAmuletTemplateEntry PvpAmulet { get; } = new(TemplateSlotType.PvpAmulet);

        public NourishmentTemplateEntry Nourishment { get; } = new(TemplateSlotType.Nourishment);

        public EnhancementTemplateEntry Enhancement { get; } = new(TemplateSlotType.Enhancement);

        public PowerCoreTemplateEntry PowerCore { get; } = new(TemplateSlotType.PowerCore);

        public PveRelicTemplateEntry PveRelic { get; } = new(TemplateSlotType.PveRelic);

        public PvpRelicTemplateEntry PvpRelic { get; } = new(TemplateSlotType.PvpRelic);

        public Dictionary<TemplateSlotType, TemplateEntry> Weapons { get; }

        public Dictionary<TemplateSlotType, TemplateEntry> Armors { get; }

        public Dictionary<TemplateSlotType, TemplateEntry> Jewellery { get; }
        #endregion

        private void RegisterGearListeners()
        {
            foreach (TemplateSlotType slot in Enum.GetValues(typeof(TemplateSlotType)))
            {
                switch (slot)
                {
                    case TemplateSlotType.Head:
                    case TemplateSlotType.Shoulder:
                    case TemplateSlotType.Chest:
                    case TemplateSlotType.Hand:
                    case TemplateSlotType.Leg:
                    case TemplateSlotType.Foot:
                    case TemplateSlotType.AquaBreather:
                        if (this[slot] is not ArmorTemplateEntry armor)
                            continue;

                        armor.StatChanged += Armor_StatChanged;
                        armor.RuneChanged += Armor_RuneChanged;
                        armor.InfusionChanged += Armor_InfusionChanged;
                        break;

                    case TemplateSlotType.MainHand:
                    case TemplateSlotType.OffHand:
                    case TemplateSlotType.AltMainHand:
                    case TemplateSlotType.AltOffHand:
                        if (this[slot] is not WeaponTemplateEntry weapon)
                            continue;

                        weapon.StatChanged += Weapon_StatChanged;
                        weapon.SigilChanged += Weapon_SigilChanged;
                        weapon.InfusionChanged += Weapon_InfusionChanged;
                        weapon.WeaponChanged += Weapon_WeaponChanged;
                        weapon.TemplateSlotChanged += Weapon_TemplateSlotChanged;

                        break;

                    case TemplateSlotType.Aquatic:
                    case TemplateSlotType.AltAquatic:
                        if (this[slot] is not AquaticWeaponTemplateEntry aqua)
                            continue;

                        aqua.StatChanged += Aqua_StatChanged;
                        aqua.Sigil1Changed += Aqua_Sigil1Changed;
                        aqua.Sigil2Changed += Aqua_Sigil2Changed;
                        aqua.WeaponChanged += Aqua_WeaponChanged;
                        aqua.Infusion1Changed += Aqua_Infusion1Changed;
                        aqua.Infusion2Changed += Aqua_Infusion2Changed;

                        break;

                    case TemplateSlotType.Amulet:
                        if (this[slot] is not AmuletTemplateEntry amulet)
                            continue;

                        amulet.StatChanged += Amulet_StatChanged;
                        amulet.EnrichmentChanged += Amulet_EnrichmentChanged;
                        break;

                    case TemplateSlotType.Ring_1:
                    case TemplateSlotType.Ring_2:
                        if (this[slot] is not RingTemplateEntry ring)
                            continue;

                        ring.StatChanged += Ring_StatChanged;
                        ring.Infusion1Changed += Ring_Infusion1Changed;
                        ring.Infusion2Changed += Ring_Infusion2Changed;
                        ring.Infusion3Changed += Ring_Infusion3Changed;
                        break;

                    case TemplateSlotType.Accessory_1:
                    case TemplateSlotType.Accessory_2:
                        if (this[slot] is not AccessoireTemplateEntry accessory)
                            continue;

                        accessory.StatChanged += Accessory_StatChanged;
                        accessory.InfusionChanged += Accessory_InfusionChanged;
                        break;
                    case TemplateSlotType.Back:
                        if (this[slot] is not BackTemplateEntry back)
                            continue;

                        back.StatChanged += Back_StatChanged;
                        back.Infusion1Changed += Back_Infusion1Changed;
                        back.Infusion2Changed += Back_Infusion2Changed;
                        break;

                    case TemplateSlotType.PvpAmulet:
                        if (this[slot] is not PvpAmuletTemplateEntry pvpAmulet)
                            continue;

                        pvpAmulet.PvpAmuletChanged += PvpAmulet_PvpAmuletChanged;
                        pvpAmulet.RuneChanged += PvpAmulet_RuneChanged;
                        break;

                    case TemplateSlotType.Nourishment:
                        if (this[slot] is not NourishmentTemplateEntry nourishment)
                            continue;

                        nourishment.NourishmentChanged += Nourishment_NourishmentChanged;
                        break;

                    case TemplateSlotType.Enhancement:
                        if (this[slot] is not EnhancementTemplateEntry utility)
                            continue;

                        utility.UtilityChanged += Utility_UtilityChanged;
                        break;

                    case TemplateSlotType.PowerCore:
                        if (this[slot] is not PowerCoreTemplateEntry powerCore)
                            continue;

                        powerCore.PowerCoreChanged += PowerCore_PowerCoreChanged;
                        break;

                    case TemplateSlotType.PveRelic:
                        if (this[slot] is not PveRelicTemplateEntry pveRelic)
                            continue;

                        pveRelic.RelicChanged += Relic_RelicChanged;
                        break;

                    case TemplateSlotType.PvpRelic:
                        if (this[slot] is not PvpRelicTemplateEntry pvpRelic)
                            continue;

                        pvpRelic.RelicChanged += Relic_RelicChanged;
                        break;
                }
            }
        }

        private void Weapon_TemplateSlotChanged(object sender, (TemplateSlotType slot, BaseItem item, Stat stat) e)
        {
            TemplateSlotChanged?.Invoke(this, e);
        }

        private void UnRegisterGearListeners()
        {
            foreach (TemplateSlotType slot in Enum.GetValues(typeof(TemplateSlotType)))
            {
                switch (slot)
                {
                    case TemplateSlotType.Head:
                    case TemplateSlotType.Shoulder:
                    case TemplateSlotType.Chest:
                    case TemplateSlotType.Hand:
                    case TemplateSlotType.Leg:
                    case TemplateSlotType.Foot:
                    case TemplateSlotType.AquaBreather:
                        if (this[slot] is not ArmorTemplateEntry armor)
                            continue;

                        armor.StatChanged -= Armor_StatChanged;
                        armor.RuneChanged -= Armor_RuneChanged;
                        armor.InfusionChanged -= Armor_InfusionChanged;
                        break;

                    case TemplateSlotType.MainHand:
                    case TemplateSlotType.OffHand:
                    case TemplateSlotType.AltMainHand:
                    case TemplateSlotType.AltOffHand:
                        if (this[slot] is not WeaponTemplateEntry weapon)
                            continue;

                        weapon.StatChanged -= Weapon_StatChanged;
                        weapon.SigilChanged -= Weapon_SigilChanged;
                        weapon.InfusionChanged -= Weapon_InfusionChanged;
                        weapon.WeaponChanged -= Weapon_WeaponChanged;

                        break;

                    case TemplateSlotType.Aquatic:
                    case TemplateSlotType.AltAquatic:
                        if (this[slot] is not AquaticWeaponTemplateEntry aqua)
                            continue;

                        aqua.StatChanged -= Aqua_StatChanged;
                        aqua.Sigil1Changed -= Aqua_Sigil1Changed;
                        aqua.Sigil2Changed -= Aqua_Sigil2Changed;
                        aqua.WeaponChanged -= Aqua_WeaponChanged;
                        aqua.Infusion1Changed -= Aqua_Infusion1Changed;
                        aqua.Infusion2Changed -= Aqua_Infusion2Changed;

                        break;

                    case TemplateSlotType.Amulet:
                        if (this[slot] is not AmuletTemplateEntry amulet)
                            continue;

                        amulet.StatChanged -= Amulet_StatChanged;
                        amulet.EnrichmentChanged -= Amulet_EnrichmentChanged;
                        break;

                    case TemplateSlotType.Ring_1:
                    case TemplateSlotType.Ring_2:
                        if (this[slot] is not RingTemplateEntry ring)
                            continue;

                        ring.StatChanged -= Ring_StatChanged;
                        ring.Infusion1Changed -= Ring_Infusion1Changed;
                        ring.Infusion2Changed -= Ring_Infusion2Changed;
                        ring.Infusion3Changed -= Ring_Infusion3Changed;
                        break;

                    case TemplateSlotType.Accessory_1:
                    case TemplateSlotType.Accessory_2:
                        if (this[slot] is not AccessoireTemplateEntry accessory)
                            continue;

                        accessory.StatChanged -= Accessory_StatChanged;
                        accessory.InfusionChanged -= Accessory_InfusionChanged;
                        break;
                    case TemplateSlotType.Back:
                        if (this[slot] is not BackTemplateEntry back)
                            continue;

                        back.StatChanged -= Back_StatChanged;
                        back.Infusion1Changed -= Back_Infusion1Changed;
                        back.Infusion2Changed -= Back_Infusion2Changed;
                        break;

                    case TemplateSlotType.PvpAmulet:
                        if (this[slot] is not PvpAmuletTemplateEntry pvpAmulet)
                            continue;

                        pvpAmulet.PvpAmuletChanged -= PvpAmulet_PvpAmuletChanged;
                        pvpAmulet.RuneChanged -= PvpAmulet_RuneChanged;
                        break;

                    case TemplateSlotType.Nourishment:
                        if (this[slot] is not NourishmentTemplateEntry nourishment)
                            continue;

                        nourishment.NourishmentChanged -= Nourishment_NourishmentChanged;
                        break;

                    case TemplateSlotType.Enhancement:
                        if (this[slot] is not EnhancementTemplateEntry utility)
                            continue;

                        utility.UtilityChanged -= Utility_UtilityChanged;
                        break;

                    case TemplateSlotType.PowerCore:
                        if (this[slot] is not PowerCoreTemplateEntry powerCore)
                            continue;

                        powerCore.PowerCoreChanged -= PowerCore_PowerCoreChanged;
                        break;

                    case TemplateSlotType.PveRelic:
                        if (this[slot] is not PveRelicTemplateEntry pveRelic)
                            continue;

                        pveRelic.RelicChanged -= Relic_RelicChanged;
                        break;

                    case TemplateSlotType.PvpRelic:
                        if (this[slot] is not PveRelicTemplateEntry pvpRelic)
                            continue;

                        pvpRelic.RelicChanged -= Relic_RelicChanged;
                        break;
                }
            }
        }

        private async void OnNameChanged(object sender, ValueChangedEventArgs<string> e)
        {
            if (!_triggerEvents)
                return;

            RequestSave();
            NameChanged?.Invoke(this, e);
        }

        private async void OnGearChanged(object sender, EventArgs e)
        {
            if (!_triggerEvents)
                return;

            OnGearCodeChanged();

            RequestSave();
        }

        private void Relic_RelicChanged(object sender, ValueChangedEventArgs<Relic> e)
        {
            OnGearChanged(sender, e);
        }

        private void PowerCore_PowerCoreChanged(object sender, ValueChangedEventArgs<PowerCore> e)
        {
            OnGearChanged(sender, e);
        }

        private void Utility_UtilityChanged(object sender, ValueChangedEventArgs<DataModels.Items.Enhancement> e)
        {
            OnGearChanged(sender, e);
        }

        private void Nourishment_NourishmentChanged(object sender, ValueChangedEventArgs<Nourishment> e)
        {
            OnGearChanged(sender, e);
        }

        private void PvpAmulet_RuneChanged(object sender, ValueChangedEventArgs<Rune> e)
        {
            OnGearChanged(sender, e);
        }

        private void PvpAmulet_PvpAmuletChanged(object sender, ValueChangedEventArgs<PvpAmulet> e)
        {
            OnGearChanged(sender, e);
        }

        private void Back_Infusion2Changed(object sender, ValueChangedEventArgs<Infusion> e)
        {
            OnGearChanged(sender, e);
        }

        private void Back_Infusion1Changed(object sender, ValueChangedEventArgs<Infusion> e)
        {
            OnGearChanged(sender, e);
        }

        private void Back_StatChanged(object sender, ValueChangedEventArgs<Stat> e)
        {
            OnGearChanged(sender, e);
        }

        private void Accessory_InfusionChanged(object sender, ValueChangedEventArgs<Infusion> e)
        {
            OnGearChanged(sender, e);
        }

        private void Accessory_StatChanged(object sender, ValueChangedEventArgs<Stat> e)
        {
            OnGearChanged(sender, e);
        }

        private void Ring_Infusion3Changed(object sender, ValueChangedEventArgs<Infusion> e)
        {
            OnGearChanged(sender, e);
        }

        private void Ring_Infusion2Changed(object sender, ValueChangedEventArgs<Infusion> e)
        {
            OnGearChanged(sender, e);
        }

        private void Ring_Infusion1Changed(object sender, ValueChangedEventArgs<Infusion> e)
        {
            OnGearChanged(sender, e);
        }

        private void Ring_StatChanged(object sender, ValueChangedEventArgs<Stat> e)
        {
            OnGearChanged(sender, e);
        }

        private void Amulet_EnrichmentChanged(object sender, ValueChangedEventArgs<Enrichment> e)
        {
            OnGearChanged(sender, e);
        }

        private void Amulet_StatChanged(object sender, ValueChangedEventArgs<Stat> e)
        {
            OnGearChanged(sender, e);
        }

        private void Aqua_Infusion2Changed(object sender, ValueChangedEventArgs<Infusion> e)
        {
            OnGearChanged(sender, e);
        }

        private void Aqua_Infusion1Changed(object sender, ValueChangedEventArgs<Infusion> e)
        {
            OnGearChanged(sender, e);
        }

        private void Aqua_WeaponChanged(object sender, ValueChangedEventArgs<DataModels.Items.Weapon> e)
        {
            OnGearChanged(sender, e);
        }

        private void Aqua_Sigil2Changed(object sender, ValueChangedEventArgs<Sigil> e)
        {
            OnGearChanged(sender, e);
        }

        private void Aqua_Sigil1Changed(object sender, ValueChangedEventArgs<Sigil> e)
        {
            OnGearChanged(sender, e);
        }

        private void Aqua_StatChanged(object sender, ValueChangedEventArgs<Stat> e)
        {
            OnGearChanged(sender, e);
        }

        private void Weapon_WeaponChanged(object sender, ValueChangedEventArgs<DataModels.Items.Weapon> e)
        {
            OnGearChanged(sender, e);
        }

        private void Weapon_InfusionChanged(object sender, ValueChangedEventArgs<Infusion> e)
        {
            OnGearChanged(sender, e);
        }

        private void Weapon_SigilChanged(object sender, ValueChangedEventArgs<Sigil> e)
        {
            OnGearChanged(sender, e);
        }

        private void Weapon_StatChanged(object sender, ValueChangedEventArgs<Stat> e)
        {
            OnGearChanged(sender, e);
        }

        private void Armor_ItemChanged(object sender, ValueChangedEventArgs<Trinket> e)
        {
            OnGearChanged(sender, e);
        }

        private void Armor_InfusionChanged(object sender, ValueChangedEventArgs<Infusion> e)
        {
            OnGearChanged(sender, e);
        }

        private void Armor_RuneChanged(object sender, ValueChangedEventArgs<Rune> e)
        {
            OnGearChanged(sender, e);
        }

        private void Armor_StatChanged(object sender, ValueChangedEventArgs<Stat> e)
        {
            OnGearChanged(sender, e);
        }

        private async void Spec_TraitsChanged(object sender, EventArgs e)
        {
            if (!_triggerEvents) return;

            OnBuildCodeChanged();
            RequestSave();
        }

        private async void Skills_ItemChanged(object sender, DictionaryItemChangedEventArgs<SkillSlotType, Skill?> e)
        {
            //if (!_triggerEvents) return;

            //SkillChanged_OLD?.Invoke(this, e);
            //OnBuildCodeChanged();

            RequestSave();
        }

        private async void Pets_ItemChanged(object sender, DictionaryItemChangedEventArgs<PetSlotType, Pet?> e)
        {
            if (!_triggerEvents) return;
            OnBuildCodeChanged();
            RequestSave();
        }

        private async void Legends_ItemChanged(object sender, DictionaryItemChangedEventArgs<LegendSlotType, Legend?> e)
        {
            if (!_triggerEvents) return;

            OnBuildCodeChanged();
            RequestSave();
        }

        private async void Specializations_ItemPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!_triggerEvents) return;
            OnBuildCodeChanged();
            RequestSave();
        }

        private async void OnProfessionChanged(object sender, ValueChangedEventArgs<ProfessionType> e)
        {
            RemoveInvalidSkillsBasedOnSpec();

            Specializations.Specialization1.Specialization = null;
            Specializations.Specialization2.Specialization = null;
            Specializations.Specialization3.Specialization = null;

            Pets.Wipe();
            Legends.Wipe();

            _savedEliteSpecialization = null;

            SetArmorItems();

            RemoveInvalidSkillsBasedOnSpec();

            ProfessionChanged?.Invoke(this, e);
            OnBuildCodeChanged();

            RequestSave();
        }

        private void SetArmorItems()
        {
            switch (Profession.GetArmorType())
            {
                case Gw2Sharp.WebApi.V2.Models.ItemWeightType.Heavy:
                    AquaBreather.Armor = Data.Armors[79895];
                    Head.Armor = Data.Armors[80384];
                    Shoulder.Armor = Data.Armors[80435];
                    Chest.Armor = Data.Armors[80254];
                    Hand.Armor = Data.Armors[80205];
                    Leg.Armor = Data.Armors[80277];
                    Foot.Armor = Data.Armors[80557];
                    break;
                case Gw2Sharp.WebApi.V2.Models.ItemWeightType.Medium:
                    AquaBreather.Armor = Data.Armors[79838];
                    Head.Armor = Data.Armors[80296];
                    Shoulder.Armor = Data.Armors[80145];
                    Chest.Armor = Data.Armors[80578];
                    Hand.Armor = Data.Armors[80161];
                    Leg.Armor = Data.Armors[80252];
                    Foot.Armor = Data.Armors[80281];
                    break;
                case Gw2Sharp.WebApi.V2.Models.ItemWeightType.Light:
                    AquaBreather.Armor = Data.Armors[79873];
                    Head.Armor = Data.Armors[80248];
                    Shoulder.Armor = Data.Armors[80131];
                    Chest.Armor = Data.Armors[80190];
                    Hand.Armor = Data.Armors[80111];
                    Leg.Armor = Data.Armors[80356];
                    Foot.Armor = Data.Armors[80399];
                    break;
            }
        }

        private void OnRaceChanged(object sender, ValueChangedEventArgs<Races> e)
        {
            RemoveInvalidSkillsBasedOnRace();
            RaceChanged?.Invoke(this, e);

            OnBuildCodeChanged();
        }

        public void LoadFromCode(string? build = null, string? gear = null)
        {
            if (build is not null)
            {
                LoadBuildFromCode(build);
            }

            if (gear is not null)
            {
                LoadGearFromCode(gear);
            }
        }

        public void LoadBuildFromCode(string? code, bool save = false)
        {
            // Disable Events to prevent unnecessary event triggers during the load
            _triggerEvents = false;

            if (code is not null && Gw2ChatLink.TryParse(code, out IGw2ChatLink? chatlink))
            {
                BuildChatLink build = new();
                build.Parse(chatlink.ToArray());

                Profession = build.Profession;

                LoadSpecializationFromCode(build.Profession, SpecializationSlotType.Line_1, build.Specialization1Id, build.Specialization1Trait1Index, build.Specialization1Trait2Index, build.Specialization1Trait3Index);
                LoadSpecializationFromCode(build.Profession, SpecializationSlotType.Line_2, build.Specialization2Id, build.Specialization2Trait1Index, build.Specialization2Trait2Index, build.Specialization2Trait3Index);
                LoadSpecializationFromCode(build.Profession, SpecializationSlotType.Line_3, build.Specialization3Id, build.Specialization3Trait1Index, build.Specialization3Trait2Index, build.Specialization3Trait3Index);

                if (Profession == ProfessionType.Ranger)
                {
                    Pets.LoadFromCode(build.RangerTerrestrialPet1Id, build.RangerTerrestrialPet2Id, build.RangerAquaticPet1Id, build.RangerAquaticPet2Id);
                }

                if (Profession == ProfessionType.Revenant)
                {
                    Legends[LegendSlotType.TerrestrialInactive] = Legend.FromByte(build.RevenantInactiveTerrestrialLegend);

                    Skills[SkillSlotType.Inactive | SkillSlotType.Terrestrial | SkillSlotType.Heal] = Legend.FromByte(build.RevenantInactiveTerrestrialLegend)?.Heal;
                    Skills[SkillSlotType.Inactive | SkillSlotType.Terrestrial | SkillSlotType.Utility_1] = Legend.SkillFromUShort(build.RevenantInactiveTerrestrialUtility1SkillPaletteId, Legends[LegendSlotType.TerrestrialInactive]);
                    Skills[SkillSlotType.Inactive | SkillSlotType.Terrestrial | SkillSlotType.Utility_2] = Legend.SkillFromUShort(build.RevenantInactiveTerrestrialUtility2SkillPaletteId, Legends[LegendSlotType.TerrestrialInactive]);
                    Skills[SkillSlotType.Inactive | SkillSlotType.Terrestrial | SkillSlotType.Utility_3] = Legend.SkillFromUShort(build.RevenantInactiveTerrestrialUtility3SkillPaletteId, Legends[LegendSlotType.TerrestrialInactive]);
                    Skills[SkillSlotType.Inactive | SkillSlotType.Terrestrial | SkillSlotType.Elite] = Legend.FromByte(build.RevenantInactiveTerrestrialLegend)?.Elite;

                    Legends[LegendSlotType.AquaticInactive] = Legend.FromByte(build.RevenantInactiveAquaticLegend);
                    Skills[SkillSlotType.Inactive | SkillSlotType.Aquatic | SkillSlotType.Heal] = Legend.FromByte(build.RevenantInactiveAquaticLegend)?.Heal;
                    Skills[SkillSlotType.Inactive | SkillSlotType.Aquatic | SkillSlotType.Utility_1] = Legend.SkillFromUShort(build.RevenantInactiveAquaticUtility1SkillPaletteId, Legends[LegendSlotType.AquaticInactive]);
                    Skills[SkillSlotType.Inactive | SkillSlotType.Aquatic | SkillSlotType.Utility_2] = Legend.SkillFromUShort(build.RevenantInactiveAquaticUtility2SkillPaletteId, Legends[LegendSlotType.AquaticInactive]);
                    Skills[SkillSlotType.Inactive | SkillSlotType.Aquatic | SkillSlotType.Utility_3] = Legend.SkillFromUShort(build.RevenantInactiveAquaticUtility3SkillPaletteId, Legends[LegendSlotType.AquaticInactive]);
                    Skills[SkillSlotType.Inactive | SkillSlotType.Aquatic | SkillSlotType.Elite] = Legend.FromByte(build.RevenantInactiveAquaticLegend)?.Elite;

                    Legends[LegendSlotType.TerrestrialActive] = Legend.FromByte(build.RevenantActiveTerrestrialLegend);
                    Skills[SkillSlotType.Active | SkillSlotType.Terrestrial | SkillSlotType.Heal] = Legend.SkillFromUShort(build.TerrestrialHealingSkillPaletteId, Legends[LegendSlotType.TerrestrialActive]);
                    Skills[SkillSlotType.Active | SkillSlotType.Terrestrial | SkillSlotType.Utility_1] = Legend.SkillFromUShort(build.TerrestrialUtility1SkillPaletteId, Legends[LegendSlotType.TerrestrialActive]);
                    Skills[SkillSlotType.Active | SkillSlotType.Terrestrial | SkillSlotType.Utility_2] = Legend.SkillFromUShort(build.TerrestrialUtility2SkillPaletteId, Legends[LegendSlotType.TerrestrialActive]);
                    Skills[SkillSlotType.Active | SkillSlotType.Terrestrial | SkillSlotType.Utility_3] = Legend.SkillFromUShort(build.TerrestrialUtility3SkillPaletteId, Legends[LegendSlotType.TerrestrialActive]);
                    Skills[SkillSlotType.Active | SkillSlotType.Terrestrial | SkillSlotType.Elite] = Legend.SkillFromUShort(build.TerrestrialEliteSkillPaletteId, Legends[LegendSlotType.TerrestrialActive]);

                    Legends[LegendSlotType.AquaticActive] = Legend.FromByte(build.RevenantActiveAquaticLegend);
                    Skills[SkillSlotType.Active | SkillSlotType.Aquatic | SkillSlotType.Heal] = Legend.SkillFromUShort(build.AquaticHealingSkillPaletteId, Legends[LegendSlotType.AquaticActive]);
                    Skills[SkillSlotType.Active | SkillSlotType.Aquatic | SkillSlotType.Utility_1] = Legend.SkillFromUShort(build.AquaticUtility1SkillPaletteId, Legends[LegendSlotType.AquaticActive]);
                    Skills[SkillSlotType.Active | SkillSlotType.Aquatic | SkillSlotType.Utility_2] = Legend.SkillFromUShort(build.AquaticUtility2SkillPaletteId, Legends[LegendSlotType.AquaticActive]);
                    Skills[SkillSlotType.Active | SkillSlotType.Aquatic | SkillSlotType.Utility_3] = Legend.SkillFromUShort(build.AquaticUtility3SkillPaletteId, Legends[LegendSlotType.AquaticActive]);
                    Skills[SkillSlotType.Active | SkillSlotType.Aquatic | SkillSlotType.Elite] = Legend.SkillFromUShort(build.AquaticEliteSkillPaletteId, Legends[LegendSlotType.AquaticActive]);
                }
                else
                {
                    Skills[SkillSlotType.Active | SkillSlotType.Terrestrial | SkillSlotType.Heal] = Skill.FromUShort(build.TerrestrialHealingSkillPaletteId, build.Profession);
                    Skills[SkillSlotType.Active | SkillSlotType.Terrestrial | SkillSlotType.Utility_1] = Skill.FromUShort(build.TerrestrialUtility1SkillPaletteId, build.Profession);
                    Skills[SkillSlotType.Active | SkillSlotType.Terrestrial | SkillSlotType.Utility_2] = Skill.FromUShort(build.TerrestrialUtility2SkillPaletteId, build.Profession);
                    Skills[SkillSlotType.Active | SkillSlotType.Terrestrial | SkillSlotType.Utility_3] = Skill.FromUShort(build.TerrestrialUtility3SkillPaletteId, build.Profession);
                    Skills[SkillSlotType.Active | SkillSlotType.Terrestrial | SkillSlotType.Elite] = Skill.FromUShort(build.TerrestrialEliteSkillPaletteId, build.Profession);

                    Skills[SkillSlotType.Active | SkillSlotType.Aquatic | SkillSlotType.Heal] = Skill.FromUShort(build.AquaticHealingSkillPaletteId, build.Profession);
                    Skills[SkillSlotType.Active | SkillSlotType.Aquatic | SkillSlotType.Utility_1] = Skill.FromUShort(build.AquaticUtility1SkillPaletteId, build.Profession);
                    Skills[SkillSlotType.Active | SkillSlotType.Aquatic | SkillSlotType.Utility_2] = Skill.FromUShort(build.AquaticUtility2SkillPaletteId, build.Profession);
                    Skills[SkillSlotType.Active | SkillSlotType.Aquatic | SkillSlotType.Utility_3] = Skill.FromUShort(build.AquaticUtility3SkillPaletteId, build.Profession);
                    Skills[SkillSlotType.Active | SkillSlotType.Aquatic | SkillSlotType.Elite] = Skill.FromUShort(build.AquaticEliteSkillPaletteId, build.Profession);
                }

                SetArmorItems();
            }

            // Enable Events again to become responsive
            _triggerEvents = true;

            OnBuildLoadedFromCode();

            if (save)
            {
                RequestSave();
            }
        }

        private void OnBuildLoadedFromCode()
        {
            LoadedBuildFromCode?.Invoke(this, null);
        }

        public async void LoadGearFromCode(string? code, bool save = false)
        {
            // Disable Events to prevent unnecessary event triggers during the load
            _triggerEvents = false;

            if (code == null) return;

            try
            {
                byte[] codeArray = Convert.FromBase64String(GearTemplateCode.PrepareBase64String(code));

                codeArray = MainHand.GetFromCodeArray(codeArray);
                codeArray = OffHand.GetFromCodeArray(codeArray);
                codeArray = AltMainHand.GetFromCodeArray(codeArray);
                codeArray = AltOffHand.GetFromCodeArray(codeArray);

                codeArray = Head.GetFromCodeArray(codeArray);
                codeArray = Shoulder.GetFromCodeArray(codeArray);
                codeArray = Chest.GetFromCodeArray(codeArray);
                codeArray = Hand.GetFromCodeArray(codeArray);
                codeArray = Leg.GetFromCodeArray(codeArray);
                codeArray = Foot.GetFromCodeArray(codeArray);

                codeArray = Back.GetFromCodeArray(codeArray);
                codeArray = Amulet.GetFromCodeArray(codeArray);
                codeArray = Accessory_1.GetFromCodeArray(codeArray);
                codeArray = Accessory_2.GetFromCodeArray(codeArray);
                codeArray = Ring_1.GetFromCodeArray(codeArray);
                codeArray = Ring_2.GetFromCodeArray(codeArray);

                codeArray = AquaBreather.GetFromCodeArray(codeArray);
                codeArray = Aquatic.GetFromCodeArray(codeArray);
                codeArray = AltAquatic.GetFromCodeArray(codeArray);

                codeArray = PvpAmulet.GetFromCodeArray(codeArray);
                codeArray = Nourishment.GetFromCodeArray(codeArray);
                codeArray = Enhancement.GetFromCodeArray(codeArray);
                codeArray = PowerCore.GetFromCodeArray(codeArray);
                codeArray = PveRelic.GetFromCodeArray(codeArray);
                codeArray = PvpRelic.GetFromCodeArray(codeArray);

                // Enable Events again to become responsive
            }
            catch { }

            RemoveInvalidGearCombinations();
            _triggerEvents = true;

            OnGearLoadedFromCode();

            if (save)
            {
                RequestSave();
            }
        }

        private void OnGearLoadedFromCode()
        {
            LoadedGearFromCode?.Invoke(this, null);
        }

        public string? ParseBuildCode()
        {
            BuildChatLink build = new()
            {
                Profession = Profession,
                RangerAquaticPet1Id = Pets.GetPetByte(PetSlotType.Aquatic_1),
                RangerAquaticPet2Id = Pets.GetPetByte(PetSlotType.Aquatic_2),
                RangerTerrestrialPet1Id = Pets.GetPetByte(PetSlotType.Terrestrial_1),
                RangerTerrestrialPet2Id = Pets.GetPetByte(PetSlotType.Terrestrial_2),

                Specialization1Id = Specializations.Specialization1.GetSpecializationByte(),
                Specialization1Trait1Index = Specializations.Specialization1.GetTraitByte(TraitTierType.Adept),
                Specialization1Trait2Index = Specializations.Specialization1.GetTraitByte(TraitTierType.Master),
                Specialization1Trait3Index = Specializations.Specialization1.GetTraitByte(TraitTierType.GrandMaster),

                Specialization2Id = Specializations.Specialization2.GetSpecializationByte(),
                Specialization2Trait1Index = Specializations.Specialization2.GetTraitByte(TraitTierType.Adept),
                Specialization2Trait2Index = Specializations.Specialization2.GetTraitByte(TraitTierType.Master),
                Specialization2Trait3Index = Specializations.Specialization2.GetTraitByte(TraitTierType.GrandMaster),

                Specialization3Id = Specializations.Specialization3.GetSpecializationByte(),
                Specialization3Trait1Index = Specializations.Specialization3.GetTraitByte(TraitTierType.Adept),
                Specialization3Trait2Index = Specializations.Specialization3.GetTraitByte(TraitTierType.Master),
                Specialization3Trait3Index = Specializations.Specialization3.GetTraitByte(TraitTierType.GrandMaster),
            };

            if (Profession == ProfessionType.Revenant)
            {
                build.RevenantActiveTerrestrialLegend = Legends.GetLegendByte(LegendSlotType.TerrestrialActive);
                build.RevenantInactiveTerrestrialLegend = Legends.GetLegendByte(LegendSlotType.TerrestrialInactive);
                build.RevenantInactiveTerrestrialUtility1SkillPaletteId = Skills.GetPaletteId(SkillSlotType.Inactive | SkillSlotType.Terrestrial | SkillSlotType.Utility_1);
                build.RevenantInactiveTerrestrialUtility2SkillPaletteId = Skills.GetPaletteId(SkillSlotType.Inactive | SkillSlotType.Terrestrial | SkillSlotType.Utility_2);
                build.RevenantInactiveTerrestrialUtility3SkillPaletteId = Skills.GetPaletteId(SkillSlotType.Inactive | SkillSlotType.Terrestrial | SkillSlotType.Utility_3);

                build.RevenantActiveAquaticLegend = Legends.GetLegendByte(LegendSlotType.AquaticActive);
                build.RevenantInactiveAquaticLegend = Legends.GetLegendByte(LegendSlotType.AquaticInactive);

                build.RevenantInactiveAquaticUtility1SkillPaletteId = Skills.GetPaletteId(SkillSlotType.Inactive | SkillSlotType.Aquatic | SkillSlotType.Utility_1);
                build.RevenantInactiveAquaticUtility2SkillPaletteId = Skills.GetPaletteId(SkillSlotType.Inactive | SkillSlotType.Aquatic | SkillSlotType.Utility_2);
                build.RevenantInactiveAquaticUtility3SkillPaletteId = Skills.GetPaletteId(SkillSlotType.Inactive | SkillSlotType.Aquatic | SkillSlotType.Utility_3);
            }

            build.TerrestrialHealingSkillPaletteId = Skills.GetPaletteId(SkillSlotType.Active | SkillSlotType.Terrestrial | SkillSlotType.Heal);
            build.TerrestrialUtility1SkillPaletteId = Skills.GetPaletteId(SkillSlotType.Active | SkillSlotType.Terrestrial | SkillSlotType.Utility_1);
            build.TerrestrialUtility2SkillPaletteId = Skills.GetPaletteId(SkillSlotType.Active | SkillSlotType.Terrestrial | SkillSlotType.Utility_2);
            build.TerrestrialUtility3SkillPaletteId = Skills.GetPaletteId(SkillSlotType.Active | SkillSlotType.Terrestrial | SkillSlotType.Utility_3);
            build.TerrestrialEliteSkillPaletteId = Skills.GetPaletteId(SkillSlotType.Active | SkillSlotType.Terrestrial | SkillSlotType.Elite);

            build.AquaticHealingSkillPaletteId = Skills.GetPaletteId(SkillSlotType.Active | SkillSlotType.Aquatic | SkillSlotType.Heal);
            build.AquaticUtility1SkillPaletteId = Skills.GetPaletteId(SkillSlotType.Active | SkillSlotType.Aquatic | SkillSlotType.Utility_1);
            build.AquaticUtility2SkillPaletteId = Skills.GetPaletteId(SkillSlotType.Active | SkillSlotType.Aquatic | SkillSlotType.Utility_2);
            build.AquaticUtility3SkillPaletteId = Skills.GetPaletteId(SkillSlotType.Active | SkillSlotType.Aquatic | SkillSlotType.Utility_3);
            build.AquaticEliteSkillPaletteId = Skills.GetPaletteId(SkillSlotType.Active | SkillSlotType.Aquatic | SkillSlotType.Elite);

            byte[] bytes = build.ToArray();

            build.Parse(bytes.Concat(new byte[] { 0, 0 }).ToArray());
            string code = build.ToString();

            return code;
        }

        public string? ParseGearCode()
        {
            byte[] codeArray = new byte[0];

            codeArray = MainHand.AddToCodeArray(codeArray);
            codeArray = OffHand.AddToCodeArray(codeArray);
            codeArray = AltMainHand.AddToCodeArray(codeArray);
            codeArray = AltOffHand.AddToCodeArray(codeArray);

            codeArray = Head.AddToCodeArray(codeArray);
            codeArray = Shoulder.AddToCodeArray(codeArray);
            codeArray = Chest.AddToCodeArray(codeArray);
            codeArray = Hand.AddToCodeArray(codeArray);
            codeArray = Leg.AddToCodeArray(codeArray);
            codeArray = Foot.AddToCodeArray(codeArray);

            codeArray = Back.AddToCodeArray(codeArray);
            codeArray = Amulet.AddToCodeArray(codeArray);
            codeArray = Accessory_1.AddToCodeArray(codeArray);
            codeArray = Accessory_2.AddToCodeArray(codeArray);
            codeArray = Ring_1.AddToCodeArray(codeArray);
            codeArray = Ring_2.AddToCodeArray(codeArray);

            codeArray = AquaBreather.AddToCodeArray(codeArray);
            codeArray = Aquatic.AddToCodeArray(codeArray);
            codeArray = AltAquatic.AddToCodeArray(codeArray);

            codeArray = PvpAmulet.AddToCodeArray(codeArray);
            codeArray = Nourishment.AddToCodeArray(codeArray);
            codeArray = Enhancement.AddToCodeArray(codeArray);
            codeArray = PowerCore.AddToCodeArray(codeArray);
            codeArray = PveRelic.AddToCodeArray(codeArray);
            codeArray = PvpRelic.AddToCodeArray(codeArray);

            return $"[&{Convert.ToBase64String(codeArray)}]";
        }

        private async void OnDescriptionChanged()
        {
            RequestSave();
        }

        public async Task Save(int timeToWait = 1000)
        {
            if (!_loaded) return;

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                await Task.Delay(timeToWait, _cancellationTokenSource.Token);
                if (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    string path = BuildsManager.ModuleInstance.Paths.TemplatesPath;
                    if (!Directory.Exists(path)) _ = Directory.CreateDirectory(path);

                    string json = JsonConvert.SerializeObject(this, SerializerSettings.Default);
                    string filePath = $@"{path}\{Common.MakeValidFileName(Name.Trim())}.json";

                    File.WriteAllText(filePath, json);
                    BuildsManager.Logger.Debug($"Saved {Name} in {filePath}");
                    _saveRequested = false;
                    _timer.Stop();
                }
            }
            catch (Exception ex)
            {
                if (!_cancellationTokenSource.Token.IsCancellationRequested) BuildsManager.Logger.Warn(ex.ToString());
            }
        }

        public async Task<bool> ChangeName(string name)
        {
            string path = FilePath;
            bool unlocked = await FileExtension.WaitForFileUnlock(path);

            if (!unlocked)
            {
                return false;
            }

            try
            {
                if (File.Exists(path)) File.Delete(path);
            }
            catch (Exception ex)
            {
                BuildsManager.Logger.Warn(ex.ToString());
            }

            Name = name;

            RequestSave();
            return true;
        }

        public async Task<bool> Delete()
        {
            bool unlocked = await FileExtension.WaitForFileUnlock(FilePath);

            if (!unlocked)
            {
                return false;
            }

            try
            {
                if (File.Exists(FilePath)) File.Delete(FilePath);
            }
            catch (Exception ex)
            {
                BuildsManager.Logger.Warn(ex.ToString());
            }

            return true;
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;
            _triggerEvents = false;

            UnRegisterGearListeners();

            Legends.ItemChanged -= Legends_ItemChanged;
            Pets.ItemChanged -= Pets_ItemChanged;
            Skills.ItemChanged -= Skills_ItemChanged;
        }

        public void Load()
        {
            if (_loaded) return;

            GearCode = _savedGearCode;
            BuildCode = _savedBuildCode;

            _loaded = true;
        }

        private void LoadSpecializationFromCode(ProfessionType profession, SpecializationSlotType slot, byte specId, byte adept, byte master, byte grandMaster)
        {
            var buildSpecialization = slot switch
            {
                SpecializationSlotType.Line_1 => Specializations.Specialization1,
                SpecializationSlotType.Line_2 => Specializations.Specialization2,
                SpecializationSlotType.Line_3 => Specializations.Specialization3,
                _ => null
            };

            if (buildSpecialization is not null)
            {
                buildSpecialization.Specialization = Specialization.FromByte(specId, profession);
                buildSpecialization.Traits.Adept = Trait.FromByte(adept, buildSpecialization.Specialization, TraitTierType.Adept);
                buildSpecialization.Traits.Master = Trait.FromByte(master, buildSpecialization.Specialization, TraitTierType.Master);
                buildSpecialization.Traits.GrandMaster = Trait.FromByte(grandMaster, buildSpecialization.Specialization, TraitTierType.GrandMaster);
            }
        }

        private void OnSpecializationChanged(object sender, SpecializationChangedEventArgs e)
        {
            if (!_triggerEvents) return;

            SpecializationChanged?.Invoke(sender, e);

            if (e.Slot == SpecializationSlotType.Line_3)
            {
                RemoveInvalidSkillsBasedOnSpec();
                EliteSpecializationChanged?.Invoke(sender, e);
            }

            RequestSave();
        }

        public bool HasSpecialization(int? id, out BuildSpecialization slot)
        {
            if (id is not null)
            {
                foreach (var spec in Specializations)
                {
                    if (spec.Specialization?.Id == id)
                    {
                        slot = spec;
                        return true;
                    }
                }
            }

            slot = null!;
            return false;

        }

        public bool HasSpecialization(Specialization specialization, out BuildSpecialization slot)
        {
            return HasSpecialization(specialization?.Id, out slot);
        }

        public void SetSpecialization(SpecializationSlotType slot, Specialization specialization)
        {
            if (this?[slot] is BuildSpecialization newSpecline)
            {
                if (HasSpecialization(specialization, out var currentSpecline))
                {
                    if (currentSpecline.SpecializationSlot != slot)
                    {
                        var currentSpecialization = newSpecline.Specialization?.Elite is true && currentSpecline.SpecializationSlot != SpecializationSlotType.Line_3 ? null : newSpecline.Specialization;
                        var newSpecialization = currentSpecline.Specialization?.Elite is true && newSpecline.SpecializationSlot != SpecializationSlotType.Line_3 ? null : currentSpecline.Specialization;

                        var currentTraits = newSpecline.Traits.ToArray();
                        var newTraits = currentSpecline.Traits.ToArray();

                        newSpecline.Specialization = newSpecialization;
                        SetTrait(newSpecline, newTraits[0], TraitTierType.Adept);
                        SetTrait(newSpecline, newTraits[1], TraitTierType.Master);
                        SetTrait(newSpecline, newTraits[2], TraitTierType.GrandMaster);

                        currentSpecline.Specialization = currentSpecialization;
                        SetTrait(currentSpecline, currentTraits[0], TraitTierType.Adept);
                        SetTrait(currentSpecline, currentTraits[1], TraitTierType.Master);
                        SetTrait(currentSpecline, currentTraits[2], TraitTierType.GrandMaster);

                        OnSpecializationChanged(this, new(currentSpecline.SpecializationSlot, newSpecialization, currentSpecialization));
                        OnSpecializationChanged(this, new(newSpecline.SpecializationSlot, currentSpecialization, newSpecialization));

                        return;
                    }
                }

                var previousSpecialization = newSpecline.Specialization;
                newSpecline.Specialization = specialization;

                if (slot is SpecializationSlotType.Line_3)
                {
                    _savedEliteSpecialization = specialization;
                }

                SetTrait(currentSpecline, null, TraitTierType.Adept);
                SetTrait(currentSpecline, null, TraitTierType.Master);
                SetTrait(currentSpecline, null, TraitTierType.GrandMaster);

                OnSpecializationChanged(this, new(slot, previousSpecialization, specialization));
            }
        }


        public void SetTrait(SpecializationSlotType spec, Trait? trait, TraitTierType tier)
        {
            if (this[spec] is BuildSpecialization buildSpec)
            {
                SetTrait(buildSpec, trait, tier);
            }
        }

        public void SetTrait(BuildSpecialization spec, Trait? trait, TraitTierType tier)
        {
            Trait? previousTrait = null;
            
            switch (tier)
            {
                case TraitTierType.Adept:
                    previousTrait = spec.Traits.Adept;
                    spec.Traits.Adept = trait;
                    break;

                case TraitTierType.Master:
                    previousTrait = spec.Traits.Master;
                    spec.Traits.Master = trait;
                    break;

                case TraitTierType.GrandMaster:
                    previousTrait = spec.Traits.GrandMaster;
                    spec.Traits.GrandMaster = trait;
                    break;
            }

            OnTraitChanged(new(spec.SpecializationSlot, tier, previousTrait, trait));
        }

        private void OnTraitChanged(TraitChangedEventArgs e)
        {
            TraitChanged?.Invoke(this, e);
        }

        public void SetLegend(Legend legend, LegendSlotType slot)
        {
            _triggerEvents = false;
            SkillSlotType state = SkillSlotType.Active;
            SkillSlotType enviroment = SkillSlotType.Terrestrial;

            switch (slot)
            {
                case LegendSlotType.AquaticActive:
                    state = SkillSlotType.Active;
                    enviroment = SkillSlotType.Aquatic;
                    break;

                case LegendSlotType.AquaticInactive:
                    state = SkillSlotType.Inactive;
                    enviroment = SkillSlotType.Aquatic;
                    break;

                case LegendSlotType.TerrestrialActive:
                    state = SkillSlotType.Active;
                    enviroment = SkillSlotType.Terrestrial;
                    break;

                case LegendSlotType.TerrestrialInactive:
                    state = SkillSlotType.Inactive;
                    enviroment = SkillSlotType.Terrestrial;
                    break;
            };

            Skills[state | enviroment | SkillSlotType.Heal] = legend?.Heal;
            Skills[state | enviroment | SkillSlotType.Elite] = legend?.Elite;

            List<int?> paletteIds = new()
            {
                 Skills[state | enviroment | SkillSlotType.Utility_1]?.PaletteId,
                 Skills[state | enviroment | SkillSlotType.Utility_2]?.PaletteId,
                 Skills[state | enviroment | SkillSlotType.Utility_3]?.PaletteId,
            };

            List<int?> ids = new() { 4614, 4651, 4564 };
            int?[] missingIds = ids.Except(paletteIds).ToArray();

            var array = new SkillSlotType[] { SkillSlotType.Utility_1, SkillSlotType.Utility_2, SkillSlotType.Utility_3 };
            for (int i = 0; i < array.Length; i++)
            {
                SkillSlotType skillSlot = state | enviroment | array[i];
                int? paletteId = Skills[skillSlot]?.PaletteId;
                Skills[skillSlot] = paletteId is not null ? Legend.SkillFromUShort((ushort)paletteId.Value, legend) : null;
            }

            for (int j = 0; j < missingIds.Length; j++)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    SkillSlotType skillSlot = state | enviroment | array[i];
                    if (Skills[skillSlot] == null)
                    {
                        Skills[skillSlot] ??= Legend.SkillFromUShort((ushort)missingIds[j], legend);
                        break;
                    }
                }
            }

            _triggerEvents = true;
            var temp = Legends[slot];
            Legends[slot] = legend;
            LegendChanged?.Invoke(this, new(slot, temp, legend));
        }

        private void RemoveInvalidGearCombinations()
        {
            // Check and clean invalid Weapons?
            var wipeWeapons = new List<TemplateSlotType>();
            var professionWeapons = Data.Professions[Profession]?.Weapons.Select(e => e.Value.Type.ToItemWeaponType()).ToList() ?? new();

            foreach (var slot in Weapons)
            {
                if (slot.Key is not TemplateSlotType.Aquatic and not TemplateSlotType.AltAquatic)
                {
                    var weapon = (slot.Value as WeaponTemplateEntry)?.Weapon;
                    if (weapon is not null && !professionWeapons.Contains(weapon.WeaponType))
                    {
                        wipeWeapons.Add(slot.Key);
                    }
                }
                else
                {
                    var weapon = (slot.Value as AquaticWeaponTemplateEntry)?.Weapon;
                    if (weapon is not null && !professionWeapons.Contains(weapon.WeaponType))
                    {
                        wipeWeapons.Add(slot.Key);
                    }
                }
            }

            foreach (var slot in wipeWeapons)
            {
                if (slot is not TemplateSlotType.Aquatic and not TemplateSlotType.AltAquatic)
                {
                    if (Weapons[slot] is WeaponTemplateEntry weapon)
                    {
                        BuildsManager.Logger.Info($"Remove {weapon.Weapon?.Name} because we can not wield it with our current profession.");
                        weapon.Weapon = null;
                    }
                }
                else
                {
                    if (Weapons[slot] is AquaticWeaponTemplateEntry weapon)
                    {
                        BuildsManager.Logger.Info($"Remove {weapon.Weapon?.Name} because we can not wield it with our current profession.");
                        weapon.Weapon = null;
                    }
                }
            }
        }

        private void RemoveInvalidSkillsBasedOnRace()
        {
            //Get all Skill ids from all races which don't match the current race id
            var invalidSkills = Data.Races.Values.Where(x => x.Id != Race).SelectMany(x => x.Skills.Values.Select(e => e.Id)).ToList();
            var slotsToWipe = Skills.Where(x => x.Value is not null && invalidSkills.Contains(x.Value.Id)).Select(x => x.Key).ToList();

            foreach (var slot in slotsToWipe)
            {
                SelectSkill(slot, null);
            }
        }

        private void RemoveInvalidSkillsBasedOnSpec()
        {
            // Check and clean invalid Legends
            if (Profession is ProfessionType.Revenant)
            {
                var wipeLegends = new List<LegendSlotType>();

                foreach (var legend in Legends)
                {
                    if (legend.Value is not null && legend.Value?.Specialization != 0 && legend.Value?.Specialization != EliteSpecialization?.Id)
                    {
                        wipeLegends.Add(legend.Key);
                    }
                }

                foreach (var slot in wipeLegends)
                {
                    var legend = Legends[slot];
                    Legends[slot] = null;

                    OnLegendChanged(this, new(slot, legend, null));
                }
            }
            else
            {

                var specIds = Specializations.Where(x => x != null).Select(x => x?.Specialization?.Id);
                var wipeSlots = new List<SkillSlotType>();

                foreach (var s in Skills)
                {
                    var skill = s.Value;
                    bool profMatch = skill?.Professions.Contains(Profession) is true;
                    bool specMatch = skill?.Specialization is null or 0 || (skill?.Specialization is int specId && specIds.Contains(specId));
                    bool isRacial = Data.Races[Race].Skills.Any(x => x.Value.Id == s.Value?.Id);

                    if ((!profMatch || !specMatch) && !isRacial)
                    {
                        wipeSlots.Add(s.Key);
                    }
                }

                foreach (var slot in wipeSlots)
                {
                    SelectSkill(slot, null);
                }
            }
        }

        private void OnLegendChanged(object sender, DictionaryItemChangedEventArgs<LegendSlotType, Legend?> e)
        {
            if (!_triggerEvents) return;

            LegendChanged?.Invoke(sender, e);
            RequestSave();
        }

        public void SelectSkill(SkillSlotType skillSlot, Skill? skill)
        {
            SkillSlotType enviromentState = skillSlot.GetEnviromentState();
            bool canSelectSkill = skill is null || skillSlot.IsTerrestrial() || !skill?.Flags.HasFlag(SkillFlag.NoUnderwater) is true;

            if (canSelectSkill || Profession == ProfessionType.Revenant)
            {
                if (skill is not null && Skills.HasSkill(skill, enviromentState))
                {
                    var slot = Skills.GetSkillSlot(skill, enviromentState);

                    Skills[slot] = Skills[skillSlot];
                    Skills[skillSlot] = Skills[slot];
                }

                Skills[skillSlot] = skill;
            }

            OnSkillChanged(skillSlot, skill);
        }

        private void OnSkillChanged(SkillSlotType skillSlot, Skill? skill)
        {
            if (!_triggerEvents) return;

            SkillChanged?.Invoke(this, new(skillSlot, skill));
        }

        private void OnBuildCodeChanged()
        {
            if (!_triggerEvents) return;

            BuildCodeChanged?.Invoke(this, EventArgs.Empty);
            RequestSave();
        }

        private void OnGearCodeChanged()
        {
            if (!_triggerEvents) return;

            GearCodeChanged?.Invoke(this, EventArgs.Empty);
            RequestSave();
        }

        public Skill? this[SkillSlotType slot] => Skills[slot];
#nullable disable
    }
}
