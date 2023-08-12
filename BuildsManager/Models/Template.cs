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
using Kenedia.Modules.BuildsManager.Extensions;

namespace Kenedia.Modules.BuildsManager.Models
{
    [DataContract]
    public class Template : IDisposable
    {
#nullable enable
        private bool _loaded = false;
        private bool _disposed = false;
        private bool _triggerEvents = true;

        private Races _race = Races.None;
        private TemplateFlag _tags = TemplateFlag.None;
        private EncounterFlag _encounters = EncounterFlag.None;
        private ProfessionType _profession = ProfessionType.Guardian;

        private string _name = "New Template";
        private string _description = string.Empty;

        private string _savedBuildCode = string.Empty;
        private string _savedGearCode = string.Empty;
        public Specialization? _savedEliteSpecialization = null;

        private CancellationTokenSource? _cancellationTokenSource;

        public event EventHandler? GearChanged;

        public event EventHandler? BuildChanged;

        public event EventHandler? LoadedGearFromCode;

        public event EventHandler? LoadedBuildFromCode;

        public event ValueChangedEventHandler<TemplateFlag>? TagsChanged;

        public event ValueChangedEventHandler<EncounterFlag>? EncountersChanged;

        public event ValueChangedEventHandler<Races>? RaceChanged;

        public event ValueChangedEventHandler<ProfessionType>? ProfessionChanged;

        public event ValueChangedEventHandler<Specialization>? EliteSpecializationChanged;

        public event DictionaryItemChangedEventHandler<SpecializationSlotType, Specialization>? SpecializationChanged;

        public event DictionaryItemChangedEventHandler<LegendSlotType, Legend>? LegendChanged;

        public Template()
        {
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

            Juwellery = new()
                {
                {TemplateSlotType.MainHand, MainHand },
                {TemplateSlotType.OffHand, OffHand },
                {TemplateSlotType.Aquatic, Aquatic},
                {TemplateSlotType.AltMainHand, AltMainHand},
                {TemplateSlotType.AltOffHand, AltOffHand },
                {TemplateSlotType.AltAquatic, AltAquatic},
                };

            Specializations.EliteSpecChanged += Specializations_EliteSpecChanged;
            Specializations.ItemPropertyChanged += Specializations_ItemPropertyChanged;
            Legends.ItemChanged += Legends_ItemChanged;
            Pets.ItemChanged += Pets_ItemChanged;
            Skills.ItemChanged += Skills_ItemChanged;

            foreach (var spec in Specializations.Values)
            {
                spec.TraitsChanged += Spec_TraitsChanged;
            }

            RegisterGearListeners();

            PlayerCharacter player = Blish_HUD.GameService.Gw2Mumble.PlayerCharacter;
            Profession = player?.Profession ?? Profession;
        }

        private void Specializations_EliteSpecChanged(object sender, BuildSpecialization.SpecializationChangedEventArgs e)
        {
            EliteSpecializationChanged?.Invoke(this, new ValueChangedEventArgs<Specialization>(e.OldSpecialization, e.NewSpecialization));
        }

        public Template(string? buildCode, string? gearCode) : this()
        {
            LoadFromCode(buildCode, gearCode);
        }

        [JsonConstructor]
        public Template(string name, EncounterFlag encounters, TemplateFlag tags, string buildCode, string gearCode, string description, Races? race, ProfessionType? profession, int? elitespecId) : this()
        {
            // Disable Events to prevent unnecessary event triggers during the load
            _triggerEvents = false;

            _name = name;
            _encounters = encounters;
            _tags = tags;
            _race = race ?? Races.None;
            _profession = profession ?? ProfessionType.Guardian;
            _savedEliteSpecialization = BuildsManager.Data.Professions[Profession]?.Specializations.FirstOrDefault(e => e.Value.Id == elitespecId).Value;
            _description = description;

            // Enable Events again to become responsive
            _triggerEvents = true;

            _savedBuildCode = buildCode;
            _savedGearCode = gearCode;
        }

        #region General Template 
        public string FilePath => @$"{BuildsManager.ModuleInstance.Paths.TemplatesPath}{Common.MakeValidFileName(Name.Trim())}.json";

        [DataMember]
        public ProfessionType Profession { get => _profession; set => Common.SetProperty(ref _profession, value, OnProfessionChanged, _triggerEvents); }

        [DataMember]
        public Races Race { get => _race; set => Common.SetProperty(ref _race, value, OnRaceChanged, _triggerEvents); }

        [DataMember]
        public TemplateFlag Tags { get => _tags; set => Common.SetProperty(ref _tags, value, OnTagsChanged, _triggerEvents); }

        [DataMember]
        public EncounterFlag Encounters { get => _encounters; set => Common.SetProperty(ref _encounters, value, OnEncountersChanged, _triggerEvents); }

        [DataMember]
        public string Name { get => _name; set => Common.SetProperty(ref _name, value, AutoSave, _triggerEvents); }

        [DataMember]
        public string Description { get => _description; set => Common.SetProperty(ref _description, value, AutoSave, _triggerEvents); }

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
        public int? EliteSpecializationId => Specializations?[SpecializationSlotType.Line_3]?.Specialization?.Id ?? _savedEliteSpecialization?.Id;

        public Specialization? EliteSpecialization => Specializations?[SpecializationSlotType.Line_3]?.Specialization ?? _savedEliteSpecialization;

        public PetCollection Pets { get; } = new();

        public SkillCollection Skills { get; } = new();

        public LegendCollection Legends { get; } = new();

        public SpecializationCollection Specializations { get; } = new();
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

        public UtilityTemplateEntry Utility { get; } = new(TemplateSlotType.Utility);

        public JadeBotTemplateEntry JadeBotCore { get; } = new(TemplateSlotType.JadeBotCore);

        public RelicTemplateEntry Relic { get; } = new(TemplateSlotType.Relic);

        public Dictionary<TemplateSlotType, TemplateEntry> Weapons { get; }

        public Dictionary<TemplateSlotType, TemplateEntry> Armors { get; }

        public Dictionary<TemplateSlotType, TemplateEntry> Juwellery { get; }
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

                    case TemplateSlotType.Utility:
                        if (this[slot] is not UtilityTemplateEntry utility)
                            continue;

                        utility.UtilityChanged += Utility_UtilityChanged;
                        break;

                    case TemplateSlotType.JadeBotCore:
                        if (this[slot] is not JadeBotTemplateEntry jadeBotCore)
                            continue;

                        jadeBotCore.JadeBotCoreChanged += JadeBotCore_JadeBotCoreChanged;
                        break;

                    case TemplateSlotType.Relic:
                        if (this[slot] is not RelicTemplateEntry relic)
                            continue;

                        relic.RelicChanged += Relic_RelicChanged;
                        break;
                }
            }
        }

        private async void OnGearChanged(object sender, EventArgs e)
        {
            GearChanged?.Invoke(this, e);
            await Save();
        }

        private void Relic_RelicChanged(object sender, ValueChangedEventArgs<Relic> e)
        {
            OnGearChanged(sender, e);
        }

        private void JadeBotCore_JadeBotCoreChanged(object sender, ValueChangedEventArgs<JadeBotCore> e)
        {
            OnGearChanged(sender, e);
        }

        private void Utility_UtilityChanged(object sender, ValueChangedEventArgs<DataModels.Items.Utility> e)
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

        private void OnTagsChanged(object sender, ValueChangedEventArgs<TemplateFlag> e)
        {
            TagsChanged?.Invoke(this, e);
        }
        private void OnEncountersChanged(object sender, ValueChangedEventArgs<EncounterFlag> e)
        {
            EncountersChanged?.Invoke(this, e);
        }

        private async void Spec_TraitsChanged(object sender, EventArgs e)
        {
            BuildChanged?.Invoke(this, e);
            await Save();
        }

        private async void Skills_ItemChanged(object sender, DictionaryItemChangedEventArgs<SkillSlotType, Skill?> e)
        {
            BuildChanged?.Invoke(this, e);
            await Save();
        }

        private async void Pets_ItemChanged(object sender, DictionaryItemChangedEventArgs<PetSlotType, Pet?> e)
        {
            BuildChanged?.Invoke(this, e);
            await Save();
        }

        private async void Legends_ItemChanged(object sender, DictionaryItemChangedEventArgs<LegendSlotType, Legend?> e)
        {
            BuildChanged?.Invoke(this, e);
            await Save();
        }

        private async void Specializations_ItemPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            BuildChanged?.Invoke(this, e);
            await Save();
        }

        private async void OnProfessionChanged(object sender, ValueChangedEventArgs<ProfessionType> e)
        {
            if (Skills.WipeSkills(Race))
            {
                BuildChanged?.Invoke(this, e);
            }

            Specializations.Wipe();
            Pets.Wipe();
            Legends.Wipe();

            _savedEliteSpecialization = null;

            SetArmorItems();

            RemoveInvalidBuildCombinations();

            if (!_triggerEvents) return;
            ProfessionChanged?.Invoke(this, e);

            await Save();
        }

        private void SetArmorItems()
        {
            switch (Profession.GetArmorType())
            {
                case Gw2Sharp.WebApi.V2.Models.ItemWeightType.Heavy:
                    AquaBreather.Armor = BuildsManager.Data.Armors[79895];
                    Head.Armor = BuildsManager.Data.Armors[85193];
                    Shoulder.Armor = BuildsManager.Data.Armors[84875];
                    Chest.Armor  = BuildsManager.Data.Armors[85084];
                    Hand.Armor = BuildsManager.Data.Armors[85140];
                    Leg.Armor = BuildsManager.Data.Armors[84887];
                    Foot.Armor = BuildsManager.Data.Armors[85055];
                    break;
                case Gw2Sharp.WebApi.V2.Models.ItemWeightType.Medium:
                    AquaBreather.Armor = BuildsManager.Data.Armors[79838];
                    Head.Armor = BuildsManager.Data.Armors[80701];
                    Shoulder.Armor = BuildsManager.Data.Armors[80825];
                    Chest.Armor = BuildsManager.Data.Armors[84977];
                    Hand.Armor = BuildsManager.Data.Armors[85169];
                    Leg.Armor = BuildsManager.Data.Armors[85264];
                    Foot.Armor = BuildsManager.Data.Armors[80836];
                    break;
                case Gw2Sharp.WebApi.V2.Models.ItemWeightType.Light:
                    AquaBreather.Armor = BuildsManager.Data.Armors[79873];
                    Head.Armor = BuildsManager.Data.Armors[85128];
                    Shoulder.Armor = BuildsManager.Data.Armors[84918];
                    Chest.Armor = BuildsManager.Data.Armors[85333];
                    Hand.Armor = BuildsManager.Data.Armors[85070];
                    Leg.Armor = BuildsManager.Data.Armors[85362];
                    Foot.Armor = BuildsManager.Data.Armors[80815];
                    break;
            }
        }

        private async void OnRaceChanged(object sender, ValueChangedEventArgs<Races> e)
        {
            if (Skills.WipeInvalidRacialSkills(Race))
            {
                BuildChanged?.Invoke(this, e);
            }

            RemoveInvalidBuildCombinations();

            if (!_triggerEvents) return;
            RaceChanged?.Invoke(this, e);
            await Save();
        }

        public void LoadFromCode(string? build = null, string? gear = null)
        {
            if (build != null)
            {
                LoadBuildFromCode(build);
            }

            if (gear != null)
            {
                LoadGearFromCode(gear);
            }
        }

        public async void LoadBuildFromCode(string? code, bool save = false)
        {
            // Disable Events to prevent unnecessary event triggers during the load
            _triggerEvents = false;

            if (code != null && Gw2ChatLink.TryParse(code, out IGw2ChatLink? chatlink))
            {
                BuildChatLink build = new();
                build.Parse(chatlink.ToArray());

                Profession = build.Profession;

                Specializations.LoadFromCode(build.Profession, SpecializationSlotType.Line_1, build.Specialization1Id, build.Specialization1Trait1Index, build.Specialization1Trait2Index, build.Specialization1Trait3Index);
                Specializations.LoadFromCode(build.Profession, SpecializationSlotType.Line_2, build.Specialization2Id, build.Specialization2Trait1Index, build.Specialization2Trait2Index, build.Specialization2Trait3Index);
                Specializations.LoadFromCode(build.Profession, SpecializationSlotType.Line_3, build.Specialization3Id, build.Specialization3Trait1Index, build.Specialization3Trait2Index, build.Specialization3Trait3Index);

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
            }

            // Enable Events again to become responsive
            _triggerEvents = true;

            LoadedBuildFromCode?.Invoke(this, null);

            if (save)
            {
                await Save();
            }
        }

        public async void LoadGearFromCode(string? code, bool save = false)
        {
            // Disable Events to prevent unnecessary event triggers during the load
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
                codeArray = Utility.GetFromCodeArray(codeArray);
                codeArray = Relic.GetFromCodeArray(codeArray);
                codeArray = JadeBotCore.GetFromCodeArray(codeArray);

                // Enable Events again to become responsive
            }
            catch { }

            RemoveInvalidGearCombinations();
            _triggerEvents = true;

            LoadedGearFromCode?.Invoke(this, null);

            if (save)
            {
                await Save();
            }
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

                Specialization1Id = Specializations.GetSpecializationByte(SpecializationSlotType.Line_1),
                Specialization1Trait1Index = Specializations.GetTraitByte(TraitTierType.Adept, Specializations[SpecializationSlotType.Line_1]),
                Specialization1Trait2Index = Specializations.GetTraitByte(TraitTierType.Master, Specializations[SpecializationSlotType.Line_1]),
                Specialization1Trait3Index = Specializations.GetTraitByte(TraitTierType.GrandMaster, Specializations[SpecializationSlotType.Line_1]),

                Specialization2Id = Specializations.GetSpecializationByte(SpecializationSlotType.Line_2),
                Specialization2Trait1Index = Specializations.GetTraitByte(TraitTierType.Adept, Specializations[SpecializationSlotType.Line_2]),
                Specialization2Trait2Index = Specializations.GetTraitByte(TraitTierType.Master, Specializations[SpecializationSlotType.Line_2]),
                Specialization2Trait3Index = Specializations.GetTraitByte(TraitTierType.GrandMaster, Specializations[SpecializationSlotType.Line_2]),

                Specialization3Id = Specializations.GetSpecializationByte(SpecializationSlotType.Line_3),
                Specialization3Trait1Index = Specializations.GetTraitByte(TraitTierType.Adept, Specializations[SpecializationSlotType.Line_3]),
                Specialization3Trait2Index = Specializations.GetTraitByte(TraitTierType.Master, Specializations[SpecializationSlotType.Line_3]),
                Specialization3Trait3Index = Specializations.GetTraitByte(TraitTierType.GrandMaster, Specializations[SpecializationSlotType.Line_3]),
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
            codeArray = Utility.AddToCodeArray(codeArray);
            codeArray = Relic.AddToCodeArray(codeArray);
            codeArray = JadeBotCore.AddToCodeArray(codeArray);

            return $"[&{Convert.ToBase64String(codeArray)}]";
        }

        private async void AutoSave()
        {
            if (!_triggerEvents) return;

            await Save();
        }

        public async Task Save(int timeToWait = 250)
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

                    string json = JsonConvert.SerializeObject(this, Formatting.Indented);
                    File.WriteAllText($@"{path}\{Common.MakeValidFileName(Name.Trim())}.json", json);
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

            await Save();
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
            if (_disposed) return;
            _disposed = true;
        }

        public bool HasSpecialization(Specialization specialization)
        {
            foreach (var spec in Specializations)
            {
                if (spec.Value?.Specialization == specialization) return true;
            }

            return false;
        }

        public bool HasSpecialization(int specializationId)
        {
            foreach (var spec in Specializations)
            {
                if (spec.Value?.Specialization?.Id == specializationId) return true;
            }

            return false;
        }

        public SpecializationSlotType? GetSpecializationSlot(Specialization specialization)
        {
            foreach (var spec in Specializations)
            {
                if (spec.Value?.Specialization == specialization) return spec.Key;
            }

            return null;
        }

        public void SetSpecialization(SpecializationSlotType slot, Specialization? specialization = null, Trait? adept = null, Trait? master = null, Trait? grandmaster = null)
        {
            if (Specializations.TryGetValue(slot, out var spec) && spec != null)
            {
                var prev = spec.Specialization;
                spec.Specialization = specialization;
                spec.Traits[TraitTierType.Adept] = adept;
                spec.Traits[TraitTierType.Master] = master;
                spec.Traits[TraitTierType.GrandMaster] = grandmaster;

                RemoveInvalidBuildCombinations();
                OnSpecializationChanged(this, new(slot, prev, specialization));
            }
        }

        public void Load()
        {
            if (_loaded) return;

            GearCode = _savedGearCode;
            BuildCode = _savedBuildCode;

            _loaded = true;
        }

        private async void OnSpecializationChanged(object sender, DictionaryItemChangedEventArgs<SpecializationSlotType, Specialization> e)
        {
            if (!_triggerEvents) return;

            SpecializationChanged?.Invoke(sender, e);
            await Save();
        }

        public void SwapSpecializations(SpecializationSlotType slot1, SpecializationSlotType slot2)
        {
            _triggerEvents = false;

            if (Specializations != null)
            {
                var prevSlot1 = new BuildSpecialization() { Specialization = Specializations?[slot1]?.Specialization?.Elite == true && slot2 != SpecializationSlotType.Line_3 ? null : Specializations?[slot1]?.Specialization };
                prevSlot1.Traits[TraitTierType.Adept] = prevSlot1.Specialization != null ? Specializations?[slot1]?.Traits[TraitTierType.Adept] : null;
                prevSlot1.Traits[TraitTierType.Master] = prevSlot1.Specialization != null ? Specializations?[slot1]?.Traits[TraitTierType.Master] : null;
                prevSlot1.Traits[TraitTierType.GrandMaster] = prevSlot1.Specialization != null ? Specializations?[slot1]?.Traits[TraitTierType.GrandMaster] : null;

                var prevSlot2 = new BuildSpecialization() { Specialization = Specializations?[slot2]?.Specialization?.Elite == true && slot1 != SpecializationSlotType.Line_3 ? null : Specializations?[slot2]?.Specialization };
                prevSlot2.Traits[TraitTierType.Adept] = prevSlot2.Specialization != null ? Specializations?[slot2]?.Traits[TraitTierType.Adept] : null;
                prevSlot2.Traits[TraitTierType.Master] = prevSlot2.Specialization != null ? Specializations?[slot2]?.Traits[TraitTierType.Master] : null;
                prevSlot2.Traits[TraitTierType.GrandMaster] = prevSlot2.Specialization != null ? Specializations?[slot2]?.Traits[TraitTierType.GrandMaster] : null;

                SetSpecialization(slot2, prevSlot1.Specialization, prevSlot1.Traits[TraitTierType.Adept], prevSlot1.Traits[TraitTierType.Master], prevSlot1.Traits[TraitTierType.GrandMaster]);
                SetSpecialization(slot1, prevSlot2.Specialization, prevSlot2.Traits[TraitTierType.Adept], prevSlot2.Traits[TraitTierType.Master], prevSlot2.Traits[TraitTierType.GrandMaster]);

                RemoveInvalidBuildCombinations();
                SpecializationChanged?.Invoke(this, new(slot1, prevSlot1.Specialization, prevSlot2.Specialization));
            }

            _triggerEvents = true;
        }

        public void SetLegend(Legend legend, LegendSlotType slot)
        {
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
                Skills[skillSlot] = paletteId != null ? Legend.SkillFromUShort((ushort)paletteId.Value, legend) : null;
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

            var temp = Legends[slot];
            Legends[slot] = legend;
            LegendChanged?.Invoke(this, new(slot, temp, legend));
        }

        private void RemoveInvalidGearCombinations()
        {
            // Check and clean invalid Weapons?
            var wipeWeapons = new List<TemplateSlotType>();
            var professionWeapons = BuildsManager.Data.Professions[Profession].Weapons.Select(e => e.Value.Type.ToItemWeaponType()).ToList() ?? new();

            foreach (var slot in Weapons)
            {
                if (slot.Key is not TemplateSlotType.Aquatic and not TemplateSlotType.AltAquatic)
                {
                    var weapon = (slot.Value as WeaponTemplateEntry)?.Weapon;
                    if (weapon != null && !professionWeapons.Contains(weapon.WeaponType))
                    {
                        wipeWeapons.Add(slot.Key);
                    }
                }
                else
                {
                    var weapon = (slot.Value as AquaticWeaponTemplateEntry)?.Weapon;
                    if(weapon != null && !professionWeapons.Contains(weapon.WeaponType))
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

        private void RemoveInvalidBuildCombinations()
        {
            // Check and clean invalid Legends
            if (Profession is ProfessionType.Revenant)
            {
                var wipeLegends = new List<LegendSlotType>();

                foreach (var legend in Legends)
                {
                    if (legend.Value != null && legend.Value?.Specialization != 0 && legend.Value?.Specialization != EliteSpecialization?.Id)
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
        }

        private async void OnLegendChanged(object sender, DictionaryItemChangedEventArgs<LegendSlotType, Legend> e)
        {
            if (!_triggerEvents) return;

            LegendChanged?.Invoke(sender, e);
            await Save();
        }
#nullable disable
    }
}
