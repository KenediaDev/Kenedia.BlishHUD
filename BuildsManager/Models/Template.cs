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
using Gw2Sharp;
using Kenedia.Modules.BuildsManager.Extensions;
using System.Timers;
using Kenedia.Modules.BuildsManager.Controls_Old.GearPage.GearSlots;
using Kenedia.Modules.BuildsManager.Interfaces;
using System.IdentityModel.Protocols.WSTrust;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Kenedia.Modules.BuildsManager.Models
{
    [DataContract]
    public class Template : IDisposable
    {
        private readonly System.Timers.Timer _timer;

        public Data Data { get; }

        private bool _isDisposed = false;

        private Races _race = Races.None;
        private ProfessionType _profession = ProfessionType.Guardian;

        private string _name = strings.NewTemplate;
        private string _description = string.Empty;

        private string _savedBuildCode = string.Empty;
        private string _savedGearCode = string.Empty;

        [JsonProperty("Tags")]
        [DataMember]
        private UniqueObservableCollection<string> _tags;

        private CancellationTokenSource? _cancellationTokenSource;

        private bool _saveRequested;
        private List<BuildSpecialization> _specializations;

        public event EventHandler? GearCodeChanged;

        public event EventHandler? BuildCodeChanged;

        public event ValueChangedEventHandler<Races>? RaceChanged;

        public event ValueChangedEventHandler<string>? NameChanged;

        public event ValueChangedEventHandler<string>? LastModifiedChanged;

        public event ValueChangedEventHandler<ProfessionType>? ProfessionChanged;

        public event PetChangedEventHandler? PetChanged;

        public event SkillChangedEventHandler? SkillChanged;

        public event TraitChangedEventHandler? TraitChanged;

        public event SpecializationChangedEventHandler? SpecializationChanged;

        public event SpecializationChangedEventHandler? EliteSpecializationChanged;

        public event LegendChangedEventHandler? LegendChanged;

        public event TemplateSlotChangedEventHandler? TemplateSlotChanged;

        [JsonProperty("LastModified")]
        [DataMember]
        private string _lastModified = DateTime.Now.ToString("d");

        public Template(Data data)
        {
            Data = data;

            _timer = new(1000);
            _timer.Elapsed += OnTimerElapsed;

            Head = new(TemplateSlotType.Head, Data);
            Shoulder = new(TemplateSlotType.Shoulder, Data);
            Chest = new(TemplateSlotType.Chest, Data);
            Hand = new(TemplateSlotType.Hand, Data);
            Leg = new(TemplateSlotType.Leg, Data);
            Foot = new(TemplateSlotType.Foot, Data);
            AquaBreather = new(TemplateSlotType.AquaBreather, Data);
            MainHand = new(TemplateSlotType.MainHand, Data);
            OffHand = new(TemplateSlotType.OffHand, Data);
            Aquatic = new(TemplateSlotType.Aquatic, Data);
            AltMainHand = new(TemplateSlotType.AltMainHand, Data);
            AltOffHand = new(TemplateSlotType.AltOffHand, Data);
            AltAquatic = new(TemplateSlotType.AltAquatic, Data);
            Back = new(TemplateSlotType.Back, Data);
            Amulet = new(TemplateSlotType.Amulet, Data);
            Accessory_1 = new(TemplateSlotType.Accessory_1, Data);
            Accessory_2 = new(TemplateSlotType.Accessory_2, Data);
            Ring_1 = new(TemplateSlotType.Ring_1, Data);
            Ring_2 = new(TemplateSlotType.Ring_2, Data);
            PvpAmulet = new(TemplateSlotType.PvpAmulet, Data);
            Nourishment = new(TemplateSlotType.Nourishment, Data);
            Enhancement = new(TemplateSlotType.Enhancement, Data);
            PowerCore = new(TemplateSlotType.PowerCore, Data);
            PveRelic = new(TemplateSlotType.PveRelic, Data);
            PvpRelic = new(TemplateSlotType.PvpRelic, Data);

            Weapons = new()
                {
                {TemplateSlotType.MainHand, MainHand },
                {TemplateSlotType.OffHand, OffHand },
                {TemplateSlotType.AltMainHand, AltMainHand},
                {TemplateSlotType.AltOffHand, AltOffHand },
                {TemplateSlotType.Aquatic, Aquatic},
                {TemplateSlotType.AltAquatic, AltAquatic},
                };

            //Pets.ItemChanged += Pets_ItemChanged;
            //Skills.ItemChanged += Skills_ItemChanged;

            MainHand.PairedWeapon = OffHand;
            OffHand.PairedWeapon = MainHand;

            AltMainHand.PairedWeapon = AltOffHand;
            AltOffHand.PairedWeapon = AltMainHand;

            PlayerCharacter player = Blish_HUD.GameService.Gw2Mumble.PlayerCharacter;
            Profession = player?.Profession ?? Profession;
            Tags = [];

            Tags.CollectionChanged += Tags_CollectionChanged;
        }

        public Template(Data data, string? buildCode, string? gearCode) : this(data)
        {
            LoadFromCode(buildCode, gearCode);
        }

        [JsonConstructor]
        public Template(Data data, string name, string buildCode, string gearCode, string description, UniqueObservableCollection<string> tags, Races? race, ProfessionType? profession, int? elitespecId) : this(data)
        {
            _name = name;
            _race = race ?? Races.None;
            _profession = profession ?? ProfessionType.Guardian;
            SavedEliteSpecialization = Data.Professions[Profession]?.Specializations.FirstOrDefault(e => e.Value.Id == elitespecId).Value;
            _description = description;
            Tags = tags ?? _tags;

            _savedBuildCode = buildCode;
            _savedGearCode = gearCode;

            SetArmorItems();
        }

        public Specialization? SavedEliteSpecialization { get; private set; } = null;

        public string FilePath => @$"{BuildsManager.ModuleInstance.Paths.TemplatesPath}{Common.MakeValidFileName(Name.Trim())}.json";

        [DataMember]
        public ProfessionType Profession { get => _profession; private set => Common.SetProperty(ref _profession, value, OnProfessionChanged); }

        [DataMember]
        public Races Race { get => _race; private set => Common.SetProperty(ref _race, value, OnRaceChanged); }

        public UniqueObservableCollection<string> Tags { get => _tags; private set => Common.SetProperty(ref _tags, value, OnTagsListChanged); }

        [DataMember]
        public string Name { get => _name; set => Common.SetProperty(ref _name, value, OnNameChanged); }

        [DataMember]
        public string Description { get => _description; set => Common.SetProperty(ref _description, value, OnDescriptionChanged); }

        [DataMember]
        public string? BuildCode
        {
            set => LoadBuildFromCode(value);
            get => !Loaded ? _savedBuildCode : ParseBuildCode();
        }

        [DataMember]
        public string? GearCode
        {
            set => LoadGearFromCode(value);
            get => !Loaded ? _savedGearCode : ParseGearCode();
        }

        [DataMember]
        public int? EliteSpecializationId => Specializations.Specialization3.Specialization?.Id ?? SavedEliteSpecialization?.Id;

        public Specialization? EliteSpecialization => Specializations.Specialization3?.Specialization ?? SavedEliteSpecialization;

        public Templates.RangerPets Pets { get; } = [];

        public SkillCollection Skills { get; } = [];

        public LegendCollection Legends { get; } = new();

        public Specializations Specializations { get; } = new();

        public BuildSpecialization? this[SpecializationSlotType slot] => slot switch
        {
            SpecializationSlotType.Line_1 => Specializations.Specialization1,
            SpecializationSlotType.Line_2 => Specializations.Specialization2,
            SpecializationSlotType.Line_3 => Specializations.Specialization3,
            _ => null,
        };

        public TemplateEntry? this[TemplateSlotType slot] => slot == TemplateSlotType.None ? null : (TemplateEntry)GetType().GetProperty(slot.ToString()).GetValue(this);

        public ArmorTemplateEntry Head { get; }

        public ArmorTemplateEntry Shoulder { get; }

        public ArmorTemplateEntry Chest { get; }

        public ArmorTemplateEntry Hand { get; }

        public ArmorTemplateEntry Leg { get; } 

        public ArmorTemplateEntry Foot { get; }

        public ArmorTemplateEntry AquaBreather { get; }

        public WeaponTemplateEntry MainHand { get; }

        public WeaponTemplateEntry OffHand { get; }

        public AquaticWeaponTemplateEntry Aquatic { get; }

        public WeaponTemplateEntry AltMainHand { get; }

        public WeaponTemplateEntry AltOffHand { get; }

        public AquaticWeaponTemplateEntry AltAquatic { get; }

        public BackTemplateEntry Back { get; }

        public AmuletTemplateEntry Amulet { get; }

        public AccessoireTemplateEntry Accessory_1 { get; }

        public AccessoireTemplateEntry Accessory_2 { get; }

        public RingTemplateEntry Ring_1 { get; }

        public RingTemplateEntry Ring_2 { get; }

        public PvpAmuletTemplateEntry PvpAmulet { get; }

        public NourishmentTemplateEntry Nourishment { get; }

        public EnhancementTemplateEntry Enhancement { get; }

        public PowerCoreTemplateEntry PowerCore { get; }

        public PveRelicTemplateEntry PveRelic { get; }

        public PvpRelicTemplateEntry PvpRelic { get; }

        public Dictionary<TemplateSlotType, TemplateEntry> Weapons { get; }

        public bool TriggerAutoSave { get; set; } = false;

        public string LastModified { get => _lastModified; set => Common.SetProperty(ref _lastModified, value, OnLastModifiedChanged); }

        public bool Loaded { get; set; }

        private void OnLastModifiedChanged(object sender, ValueChangedEventArgs<string> e)
        {
            LastModifiedChanged?.Invoke(this, e);
            RequestSave();
        }

        private async void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (_saveRequested)
            {
                _timer.Stop();

                await Save();
            }
        }

        public void RequestSave([CallerMemberName] string name = "unkown")
        {
            _saveRequested = !string.IsNullOrEmpty(Name) && TriggerAutoSave && Loaded;

            if (_saveRequested)
            {
                _timer.Stop();
                _timer.Start();
            }
        }

        private void OnTagsListChanged(object sender, ValueChangedEventArgs<UniqueObservableCollection<string>> e)
        {
            if (e.NewValue is not null)
            {
                e.NewValue.CollectionChanged += Tags_CollectionChanged;
            }

            if (e.OldValue is not null)
            {
                e.OldValue.CollectionChanged -= Tags_CollectionChanged;
            }
        }

        private void Tags_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RequestSave();
        }

        private void OnNameChanged(object sender, ValueChangedEventArgs<string> e)
        {
            RequestSave();
            NameChanged?.Invoke(this, e);
        }

        private void OnGearChanged(object sender, TemplateSlotChangedEventArgs e)
        {
            TemplateSlotChanged?.Invoke(sender, e);
            OnGearCodeChanged();

            RequestSave();
        }

        public void SetItem<T>(TemplateSlotType slot, TemplateSubSlotType subSlot, T obj)
        {
            TemplateEntry entry = this[slot];

            if (entry.SetValue(slot, subSlot, obj))
            {
                if (slot.IsWeapon())
                {
                    SetWeapon(slot, subSlot, obj);
                }

                OnGearChanged(entry, new TemplateSlotChangedEventArgs(slot, subSlot, obj));
            }
        }

        private void SetWeapon(TemplateSlotType slot, TemplateSubSlotType subSlot, object? obj)
        {
            if (subSlot is not TemplateSubSlotType.Item and not TemplateSubSlotType.Stat)
            {
                return;
            }

            if (slot.GetOffhand() is TemplateSlotType offhand && slot.IsMainHand())
            {
                if (this[slot] is WeaponTemplateEntry mainHand)
                {
                    if (this[offhand] is WeaponTemplateEntry offhandEntry)
                    {
                        if (obj is DataModels.Items.Weapon weapon && weapon?.WeaponType.IsTwoHanded() is true)
                        {
                            if (offhandEntry.SetValue(offhand, TemplateSubSlotType.Item, mainHand.Weapon))
                            {
                                OnGearChanged(offhandEntry, new TemplateSlotChangedEventArgs(offhand, subSlot, obj));
                            }

                            if (offhandEntry.SetValue(offhand, TemplateSubSlotType.Stat, mainHand.Stat))
                            {
                                OnGearChanged(offhandEntry, new TemplateSlotChangedEventArgs(offhand, subSlot, obj));
                            }
                        }
                        else if (obj is Stat && mainHand.Weapon?.WeaponType.IsTwoHanded() is true)
                        {
                            if (offhandEntry.SetValue(offhand, subSlot, obj))
                            {
                                OnGearChanged(offhandEntry, new TemplateSlotChangedEventArgs(offhand, subSlot, obj));
                            }
                        }
                        else if (offhandEntry.Weapon?.WeaponType.IsTwoHanded() is true)
                        {
                            if (obj is null || (obj is DataModels.Items.Weapon newWeapon && newWeapon?.WeaponType.IsOneHanded() is true))
                            {
                                if (offhandEntry.SetValue(offhand, subSlot, null))
                                {
                                    OnGearChanged(offhandEntry, new TemplateSlotChangedEventArgs(offhand, subSlot, obj));
                                }
                            }
                        }
                    }
                }
            }
            else if (slot.IsOffhand() && slot.GetMainHand() is TemplateSlotType mainHand)
            {
                if (this[mainHand] is WeaponTemplateEntry mainHandEntry && mainHandEntry.Weapon?.WeaponType.IsTwoHanded() is true)
                {
                    if (obj is DataModels.Items.Weapon newWeapon && (newWeapon?.WeaponType.IsOneHanded() is true || newWeapon?.WeaponType.IsOffHand() is true))
                    {
                        if (mainHandEntry.SetValue(mainHand, subSlot, null))
                        {
                            OnGearChanged(mainHandEntry, new TemplateSlotChangedEventArgs(mainHand, subSlot, obj));
                        }
                    }
                    else if (obj is Stat stat)
                    {
                        if (mainHandEntry.SetValue(mainHand, subSlot, stat))
                        {
                            OnGearChanged(mainHandEntry, new TemplateSlotChangedEventArgs(mainHand, subSlot, obj));
                        }
                    }
                }
            }
        }

        public void SetProfession(ProfessionType profession)
        {
            Profession = profession;

            SetSpecialization(SpecializationSlotType.Line_1, null);
            SetSpecialization(SpecializationSlotType.Line_2, null);
            SetSpecialization(SpecializationSlotType.Line_3, null);

            Pets.Wipe();
            Legends.Wipe();

            SavedEliteSpecialization = null;
            RemoveInvalidSkillsBasedOnSpec();
        }

        private void OnProfessionChanged(object sender, ValueChangedEventArgs<ProfessionType> e)
        {
            SetArmorItems();
            RemoveInvalidGearCombinations();

            ProfessionChanged?.Invoke(this, e);
            OnBuildCodeChanged();
        }

        private void SetArmorItems()
        {
            if (Data is null) return;

            switch (Profession.GetArmorType())
            {
                case Gw2Sharp.WebApi.V2.Models.ItemWeightType.Heavy:
                    AquaBreather.SetValue(TemplateSlotType.AquaBreather, TemplateSubSlotType.Item, Data.Armors[79895]);
                    Head.SetValue(TemplateSlotType.Head, TemplateSubSlotType.Item, Data.Armors[80384]);
                    Shoulder.SetValue(TemplateSlotType.Shoulder, TemplateSubSlotType.Item, Data.Armors[80435]);
                    Chest.SetValue(TemplateSlotType.Chest, TemplateSubSlotType.Item, Data.Armors[80254]);
                    Hand.SetValue(TemplateSlotType.Hand, TemplateSubSlotType.Item, Data.Armors[80205]);
                    Leg.SetValue(TemplateSlotType.Leg, TemplateSubSlotType.Item, Data.Armors[80277]);
                    Foot.SetValue(TemplateSlotType.Foot, TemplateSubSlotType.Item, Data.Armors[80557]);
                    break;

                case Gw2Sharp.WebApi.V2.Models.ItemWeightType.Medium:
                    AquaBreather.SetValue(TemplateSlotType.AquaBreather, TemplateSubSlotType.Item, Data.Armors[79838]);
                    Head.SetValue(TemplateSlotType.Head, TemplateSubSlotType.Item, Data.Armors[80296]);
                    Shoulder.SetValue(TemplateSlotType.Shoulder, TemplateSubSlotType.Item, Data.Armors[80145]);
                    Chest.SetValue(TemplateSlotType.Chest, TemplateSubSlotType.Item, Data.Armors[80578]);
                    Hand.SetValue(TemplateSlotType.Hand, TemplateSubSlotType.Item, Data.Armors[80161]);
                    Leg.SetValue(TemplateSlotType.Leg, TemplateSubSlotType.Item, Data.Armors[80252]);
                    Foot.SetValue(TemplateSlotType.Foot, TemplateSubSlotType.Item, Data.Armors[80281]);
                    break;

                case Gw2Sharp.WebApi.V2.Models.ItemWeightType.Light:
                    AquaBreather.SetValue(TemplateSlotType.AquaBreather, TemplateSubSlotType.Item, Data.Armors[79873]);
                    Head.SetValue(TemplateSlotType.Head, TemplateSubSlotType.Item, Data.Armors[80248]);
                    Shoulder.SetValue(TemplateSlotType.Shoulder, TemplateSubSlotType.Item, Data.Armors[80131]);
                    Chest.SetValue(TemplateSlotType.Chest, TemplateSubSlotType.Item, Data.Armors[80190]);
                    Hand.SetValue(TemplateSlotType.Hand, TemplateSubSlotType.Item, Data.Armors[80111]);
                    Leg.SetValue(TemplateSlotType.Leg, TemplateSubSlotType.Item, Data.Armors[80356]);
                    Foot.SetValue(TemplateSlotType.Foot, TemplateSubSlotType.Item, Data.Armors[80399]);
                    break;
            }
        }

        private void OnRaceChanged(object sender, ValueChangedEventArgs<Races> e)
        {
            RemoveInvalidSkillsBasedOnRace();

            RaceChanged?.Invoke(this, e);
            OnBuildCodeChanged();
        }

        public void LoadFromCode(string? build = null, string? gear = null, bool save = false)
        {
            if (build is not null)
            {
                LoadBuildFromCode(build, save);
            }

            if (gear is not null)
            {
                LoadGearFromCode(gear, save);
            }
        }

        public void LoadBuildFromCode(string? code, bool save = false)
        {
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
                    SetLegend(LegendSlotType.TerrestrialInactive, Legend.FromByte(build.RevenantInactiveTerrestrialLegend));
                    SetSkill(SkillSlotType.Inactive | SkillSlotType.Terrestrial | SkillSlotType.Heal, Legend.FromByte(build.RevenantInactiveTerrestrialLegend)?.Heal);
                    SetSkill(SkillSlotType.Inactive | SkillSlotType.Terrestrial | SkillSlotType.Utility_1, Legend.SkillFromUShort(build.RevenantInactiveTerrestrialUtility1SkillPaletteId, Legends[LegendSlotType.TerrestrialInactive]));
                    SetSkill(SkillSlotType.Inactive | SkillSlotType.Terrestrial | SkillSlotType.Utility_2, Legend.SkillFromUShort(build.RevenantInactiveTerrestrialUtility2SkillPaletteId, Legends[LegendSlotType.TerrestrialInactive]));
                    SetSkill(SkillSlotType.Inactive | SkillSlotType.Terrestrial | SkillSlotType.Utility_3, Legend.SkillFromUShort(build.RevenantInactiveTerrestrialUtility3SkillPaletteId, Legends[LegendSlotType.TerrestrialInactive]));
                    SetSkill(SkillSlotType.Inactive | SkillSlotType.Terrestrial | SkillSlotType.Elite, Legend.FromByte(build.RevenantInactiveTerrestrialLegend)?.Elite);

                    SetLegend(LegendSlotType.AquaticInactive, Legend.FromByte(build.RevenantInactiveAquaticLegend));
                    SetSkill(SkillSlotType.Inactive | SkillSlotType.Aquatic | SkillSlotType.Heal, Legend.FromByte(build.RevenantInactiveAquaticLegend)?.Heal);
                    SetSkill(SkillSlotType.Inactive | SkillSlotType.Aquatic | SkillSlotType.Utility_1, Legend.SkillFromUShort(build.RevenantInactiveAquaticUtility1SkillPaletteId, Legends[LegendSlotType.AquaticInactive]));
                    SetSkill(SkillSlotType.Inactive | SkillSlotType.Aquatic | SkillSlotType.Utility_2, Legend.SkillFromUShort(build.RevenantInactiveAquaticUtility2SkillPaletteId, Legends[LegendSlotType.AquaticInactive]));
                    SetSkill(SkillSlotType.Inactive | SkillSlotType.Aquatic | SkillSlotType.Utility_3, Legend.SkillFromUShort(build.RevenantInactiveAquaticUtility3SkillPaletteId, Legends[LegendSlotType.AquaticInactive]));
                    SetSkill(SkillSlotType.Inactive | SkillSlotType.Aquatic | SkillSlotType.Elite, Legend.FromByte(build.RevenantInactiveAquaticLegend)?.Elite);

                    SetLegend(LegendSlotType.TerrestrialActive, Legend.FromByte(build.RevenantActiveTerrestrialLegend));
                    SetSkill(SkillSlotType.Active | SkillSlotType.Terrestrial | SkillSlotType.Heal, Legend.SkillFromUShort(build.TerrestrialHealingSkillPaletteId, Legends[LegendSlotType.TerrestrialActive]));
                    SetSkill(SkillSlotType.Active | SkillSlotType.Terrestrial | SkillSlotType.Utility_1, Legend.SkillFromUShort(build.TerrestrialUtility1SkillPaletteId, Legends[LegendSlotType.TerrestrialActive]));
                    SetSkill(SkillSlotType.Active | SkillSlotType.Terrestrial | SkillSlotType.Utility_2, Legend.SkillFromUShort(build.TerrestrialUtility2SkillPaletteId, Legends[LegendSlotType.TerrestrialActive]));
                    SetSkill(SkillSlotType.Active | SkillSlotType.Terrestrial | SkillSlotType.Utility_3, Legend.SkillFromUShort(build.TerrestrialUtility3SkillPaletteId, Legends[LegendSlotType.TerrestrialActive]));
                    SetSkill(SkillSlotType.Active | SkillSlotType.Terrestrial | SkillSlotType.Elite, Legend.SkillFromUShort(build.TerrestrialEliteSkillPaletteId, Legends[LegendSlotType.TerrestrialActive]));

                    SetLegend(LegendSlotType.AquaticActive, Legend.FromByte(build.RevenantActiveAquaticLegend));
                    SetSkill(SkillSlotType.Active | SkillSlotType.Aquatic | SkillSlotType.Heal, Legend.SkillFromUShort(build.AquaticHealingSkillPaletteId, Legends[LegendSlotType.AquaticActive]));
                    SetSkill(SkillSlotType.Active | SkillSlotType.Aquatic | SkillSlotType.Utility_1, Legend.SkillFromUShort(build.AquaticUtility1SkillPaletteId, Legends[LegendSlotType.AquaticActive]));
                    SetSkill(SkillSlotType.Active | SkillSlotType.Aquatic | SkillSlotType.Utility_2, Legend.SkillFromUShort(build.AquaticUtility2SkillPaletteId, Legends[LegendSlotType.AquaticActive]));
                    SetSkill(SkillSlotType.Active | SkillSlotType.Aquatic | SkillSlotType.Utility_3, Legend.SkillFromUShort(build.AquaticUtility3SkillPaletteId, Legends[LegendSlotType.AquaticActive]));
                    SetSkill(SkillSlotType.Active | SkillSlotType.Aquatic | SkillSlotType.Elite, Legend.SkillFromUShort(build.AquaticEliteSkillPaletteId, Legends[LegendSlotType.AquaticActive]));
                }
                else
                {
                    SetSkill(SkillSlotType.Active | SkillSlotType.Terrestrial | SkillSlotType.Heal, Skill.FromUShort(build.TerrestrialHealingSkillPaletteId, build.Profession, Data));
                    SetSkill(SkillSlotType.Active | SkillSlotType.Terrestrial | SkillSlotType.Utility_1, Skill.FromUShort(build.TerrestrialUtility1SkillPaletteId, build.Profession, Data));
                    SetSkill(SkillSlotType.Active | SkillSlotType.Terrestrial | SkillSlotType.Utility_2, Skill.FromUShort(build.TerrestrialUtility2SkillPaletteId, build.Profession, Data));
                    SetSkill(SkillSlotType.Active | SkillSlotType.Terrestrial | SkillSlotType.Utility_3, Skill.FromUShort(build.TerrestrialUtility3SkillPaletteId, build.Profession, Data));
                    SetSkill(SkillSlotType.Active | SkillSlotType.Terrestrial | SkillSlotType.Elite, Skill.FromUShort(build.TerrestrialEliteSkillPaletteId, build.Profession, Data));

                    SetSkill(SkillSlotType.Active | SkillSlotType.Aquatic | SkillSlotType.Heal, Skill.FromUShort(build.AquaticHealingSkillPaletteId, build.Profession, Data));
                    SetSkill(SkillSlotType.Active | SkillSlotType.Aquatic | SkillSlotType.Utility_1, Skill.FromUShort(build.AquaticUtility1SkillPaletteId, build.Profession, Data));
                    SetSkill(SkillSlotType.Active | SkillSlotType.Aquatic | SkillSlotType.Utility_2, Skill.FromUShort(build.AquaticUtility2SkillPaletteId, build.Profession, Data));
                    SetSkill(SkillSlotType.Active | SkillSlotType.Aquatic | SkillSlotType.Utility_3, Skill.FromUShort(build.AquaticUtility3SkillPaletteId, build.Profession, Data));
                    SetSkill(SkillSlotType.Active | SkillSlotType.Aquatic | SkillSlotType.Elite, Skill.FromUShort(build.AquaticEliteSkillPaletteId, build.Profession, Data));
                }

                SetArmorItems();
            }

            if (save)
            {
                RequestSave();
            }
        }

        public void LoadGearFromCode(string? code, bool save = false)
        {
            GearChatCode.LoadTemplateFromChatCode(this, code, Data);

            RemoveInvalidGearCombinations();
            OnGearCodeChanged();

            if (save)
            {
                RequestSave();
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
            return GearChatCode.GetGearChatCode(this);
        }

        private void OnDescriptionChanged()
        {
            RequestSave();
        }

        public async Task Save(int timeToWait = 1000)
        {
            if (!Loaded) return;

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
        }

        public void Load()
        {
            if (Loaded) return;

            GearCode = _savedGearCode;
            BuildCode = _savedBuildCode;

            Loaded = true;
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
                SetSpecialization(slot, Specialization.FromByte(specId, profession, Data));
                SetTrait(buildSpecialization, Trait.FromByte(adept, buildSpecialization.Specialization, TraitTierType.Adept), TraitTierType.Adept);
                SetTrait(buildSpecialization, Trait.FromByte(master, buildSpecialization.Specialization, TraitTierType.Master), TraitTierType.Master);
                SetTrait(buildSpecialization, Trait.FromByte(grandMaster, buildSpecialization.Specialization, TraitTierType.GrandMaster), TraitTierType.GrandMaster);
            }
        }

        private void OnSpecializationChanged(object sender, SpecializationChangedEventArgs e)
        {
            SpecializationChanged?.Invoke(sender, e);

            if (e.Slot == SpecializationSlotType.Line_3)
            {
                RemoveInvalidSkillsBasedOnSpec();
                EliteSpecializationChanged?.Invoke(sender, e);
            }

            OnBuildCodeChanged();
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

        public bool HasSpecialization(Specialization? specialization, out BuildSpecialization slot)
        {
            return HasSpecialization(specialization?.Id, out slot);
        }

        public void SetSpecialization(SpecializationSlotType slot, Specialization? specialization)
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
                    SavedEliteSpecialization = specialization;
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
            if (spec is null) return;

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
            OnBuildCodeChanged();
        }

        public void SetLegend(LegendSlotType slot, Legend? legend)
        {
            SkillSlotType state = SkillSlotType.Active;
            SkillSlotType otherState = SkillSlotType.Active;

            SkillSlotType enviroment = SkillSlotType.Terrestrial;

            switch (slot)
            {
                case LegendSlotType.AquaticActive:
                    state = SkillSlotType.Active;
                    otherState = SkillSlotType.Inactive;
                    enviroment = SkillSlotType.Aquatic;
                    break;

                case LegendSlotType.AquaticInactive:
                    state = SkillSlotType.Inactive;
                    otherState = SkillSlotType.Active;
                    enviroment = SkillSlotType.Aquatic;
                    break;

                case LegendSlotType.TerrestrialActive:
                    state = SkillSlotType.Active;
                    otherState = SkillSlotType.Inactive;
                    enviroment = SkillSlotType.Terrestrial;
                    break;

                case LegendSlotType.TerrestrialInactive:
                    state = SkillSlotType.Inactive;
                    otherState = SkillSlotType.Active;
                    enviroment = SkillSlotType.Terrestrial;
                    break;
            };

            LegendSlotType otherSlot = slot switch
            {
                LegendSlotType.AquaticActive => LegendSlotType.AquaticInactive,
                LegendSlotType.AquaticInactive => LegendSlotType.AquaticActive,
                LegendSlotType.TerrestrialActive => LegendSlotType.TerrestrialInactive,
                LegendSlotType.TerrestrialInactive => LegendSlotType.TerrestrialActive,
                _ => LegendSlotType.TerrestrialActive
            };

            Legend otherLegend = Legends[otherSlot];

            if (otherLegend?.Id == legend?.Id)
            {
                Legends.SetLegend(otherSlot, Legends[slot]);
                SetLegendSkills(otherState, enviroment, Legends[otherSlot]);
            }

            var temp = Legends[slot];
            Legends.SetLegend(slot, legend);
            SetLegendSkills(state, enviroment, legend);

            LegendChanged?.Invoke(this, new(slot, temp, legend));
        }

        private void SetLegendSkills(SkillSlotType state, SkillSlotType enviroment, Legend? legend)
        {
            SetSkill(state | enviroment | SkillSlotType.Heal, legend?.Heal);
            SetSkill(state | enviroment | SkillSlotType.Elite, legend?.Elite);

            List<int?> paletteIds =
            [
                 Skills[state | enviroment | SkillSlotType.Utility_1]?.PaletteId,
                 Skills[state | enviroment | SkillSlotType.Utility_2]?.PaletteId,
                 Skills[state | enviroment | SkillSlotType.Utility_3]?.PaletteId,
            ];

            List<int?> ids = [4614, 4651, 4564];
            int?[] missingIds = ids.Except(paletteIds).ToArray();

            var array = new SkillSlotType[] { SkillSlotType.Utility_1, SkillSlotType.Utility_2, SkillSlotType.Utility_3 };
            for (int i = 0; i < array.Length; i++)
            {
                SkillSlotType skillSlot = state | enviroment | array[i];
                int? paletteId = Skills[skillSlot]?.PaletteId;

                SetSkill(skillSlot, paletteId is not null ? Legend.SkillFromUShort((ushort)paletteId.Value, legend) : null);
            }

            for (int j = 0; j < missingIds.Length; j++)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    SkillSlotType skillSlot = state | enviroment | array[i];
                    if (Skills[skillSlot] == null)
                    {
                        SetSkill(skillSlot, Skills[skillSlot] ?? Legend.SkillFromUShort((ushort)missingIds[j], legend));
                        break;
                    }
                }
            }
        }

        private void OnLegendChanged(object sender, LegendChangedEventArgs e)
        {
            LegendChanged?.Invoke(sender, e);
            OnBuildCodeChanged();
        }

        private void RemoveInvalidGearCombinations()
        {
            // Check and clean invalid Weapons?
            var wipeWeapons = new List<TemplateSlotType>();
            var professionWeapons = Data.Professions[Profession]?.Weapons.Select(e => e.Value.Type.ToItemWeaponType()).ToList() ?? [];

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
                        BuildsManager.Logger.Info($"Remove {weapon.Weapon?.Name} because we can not wield it with our current profession '{Profession}'.");
                        SetItem<DataModels.Items.Weapon>(slot, TemplateSubSlotType.Item, null);
                    }
                }
                else
                {
                    if (Weapons[slot] is AquaticWeaponTemplateEntry weapon)
                    {
                        BuildsManager.Logger.Info($"Remove {weapon.Weapon?.Name} because we can not wield it with our current profession '{Profession}'.");
                        SetItem<DataModels.Items.Weapon>(slot, TemplateSubSlotType.Item, null);
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
                SetSkill(slot, null);
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
                    if (legend is not null && legend?.Specialization != 0 && legend?.Specialization != EliteSpecialization?.Id)
                    {
                        wipeLegends.Add(Legends[legend]);
                    }
                }

                foreach (var slot in wipeLegends)
                {
                    var legend = Legends[slot];
                    SetLegend(slot, null);

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
                    bool isRacial = Data?.Races[Race].Skills.Any(x => x.Value.Id == s.Value?.Id) is true;

                    if ((!profMatch || !specMatch) && !isRacial)
                    {
                        wipeSlots.Add(s.Key);
                    }
                }

                foreach (var slot in wipeSlots)
                {
                    SetSkill(slot, null);
                }
            }
        }

        public void SetSkill(SkillSlotType skillSlot, Skill? skill)
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
            SkillChanged?.Invoke(this, new(skillSlot, skill));
            OnBuildCodeChanged();
        }

        public void SetPet(PetSlotType slot, Pet pet)
        {
            var oldpet = Pets[slot];

            if (Pets.SetPet(slot, pet))
            {
                OnPetChanged(new(slot, pet, oldpet));
            }
        }

        private void OnPetChanged(PetChangedEventArgs e)
        {
            PetChanged?.Invoke(this, e);
        }

        private void OnBuildCodeChanged()
        {
            BuildCodeChanged?.Invoke(this, EventArgs.Empty);
            RequestSave();
        }

        private void OnGearCodeChanged()
        {
            GearCodeChanged?.Invoke(this, EventArgs.Empty);
            RequestSave();
        }

        public void SetRace(Races race)
        {
            Race = race;
        }

        public void SetGroup<T>(TemplateSlotType templateSlot, TemplateSubSlotType subSlot, T obj, bool overrideExisting)
        {
            var slots = templateSlot.GetGroup();

            switch (subSlot)
            {
                case TemplateSubSlotType.Item:
                    if (obj is null)
                    {
                        SetWeapons(null, overrideExisting, slots);
                        break;
                    }
                    else if (obj is DataModels.Items.Weapon weapon)
                    {
                        SetWeapons(weapon, overrideExisting, templateSlot.GetWeaponGroup());
                        break;
                    }
                    else if (obj is BaseItem item)
                    {
                        foreach (var slot in slots)
                        {
                            if (this[slot] is IItemTemplateEntry entry)
                            {
                                SetItem(slot, subSlot, overrideExisting ? item : entry?.Item ?? item);
                            }
                        }
                    }

                    break;

                case TemplateSubSlotType.Stat:
                    if (obj is null)
                    {
                        foreach (var slot in slots)
                        {
                            if (this[slot] is IStatTemplateEntry entry)
                            {
                                SetItem(slot, subSlot, overrideExisting ? null : entry?.Stat ?? null);
                            }
                        }
                    }
                    else if (obj is Stat stat)
                    {
                        foreach (var slot in slots)
                        {
                            if (this[slot] is IStatTemplateEntry entry)
                            {
                                SetItem(slot, subSlot, overrideExisting ? stat : entry?.Stat ?? stat);
                            }
                        }
                    }

                    break;

                case TemplateSubSlotType.Rune:
                    if (obj is null)
                    {
                        foreach (var slot in slots)
                        {
                            if (this[slot] is IRuneTemplateEntry entry)
                            {
                                SetItem(slot, subSlot, overrideExisting ? null : entry?.Rune ?? null);
                            }
                        }
                    }
                    else if (obj is Rune rune)
                    {
                        foreach (var slot in slots)
                        {
                            if (this[slot] is IRuneTemplateEntry entry)
                            {
                                SetItem(slot, subSlot, overrideExisting ? rune : entry?.Rune ?? rune);
                            }
                        }
                    }

                    break;

                case TemplateSubSlotType.Sigil1:
                case TemplateSubSlotType.Sigil2:

                    SetSigils(templateSlot, overrideExisting, obj);

                    break;

                case TemplateSubSlotType.PvpSigil:
                    if (obj is null)
                    {
                        foreach (var slot in slots)
                        {
                            if (this[slot] is IPvpSigilTemplateEntry entry)
                            {
                                SetItem(slot, subSlot, overrideExisting ? null : entry?.PvpSigil ?? null);
                            }
                        }
                    }
                    else if (obj is Sigil pvpSigil)
                    {
                        foreach (var slot in slots)
                        {
                            if (this[slot] is IPvpSigilTemplateEntry entry)
                            {
                                SetItem(slot, subSlot, overrideExisting ? pvpSigil : entry?.PvpSigil ?? pvpSigil);
                            }
                        }
                    }

                    break;

                case TemplateSubSlotType.Infusion1:
                    if (obj is null)
                    {
                        SetAllInfusions(subSlot, null, overrideExisting, slots);
                    }
                    else if (obj is Infusion infusion1)
                    {
                        SetAllInfusions(subSlot, infusion1, overrideExisting, slots);
                    }

                    break;

                case TemplateSubSlotType.Infusion2:
                    if (obj is null)
                    {
                        SetAllInfusions(subSlot, null, overrideExisting, slots);
                    }
                    else if (obj is Infusion infusion2)
                    {
                        SetAllInfusions(subSlot, infusion2, overrideExisting, slots);
                    }

                    break;

                case TemplateSubSlotType.Infusion3:
                    if (obj is null)
                    {
                        SetAllInfusions(subSlot, null, overrideExisting, slots);
                    }
                    else if (obj is Infusion infusion3)
                    {
                        SetAllInfusions(subSlot, infusion3, overrideExisting, slots);
                    }

                    break;

                case TemplateSubSlotType.Enrichment:
                    foreach (var slot in slots)
                    {
                        if (this[slot] is IEnrichmentTemplateEntry entry)
                        {
                            if (obj is null)
                            {
                                SetItem(slot, subSlot, overrideExisting ? null : entry?.Enrichment ?? null);
                            }
                            else if (obj is Enrichment enrichment)
                            {
                                SetItem(slot, subSlot, overrideExisting ? enrichment : entry?.Enrichment ?? enrichment);
                            }
                        }
                    }
                    break;
            }

            void SetWeapons(DataModels.Items.Weapon? weapon, bool overrideExisting, TemplateSlotType[] slots)
            {
                if (templateSlot.IsAquatic())
                {
                    foreach (var slot in slots)
                    {
                        if (this[slot] is IWeaponTemplateEntry entry)
                        {
                            SetItem(slot, TemplateSubSlotType.Item, overrideExisting ? weapon : entry?.Weapon ?? weapon);
                        }
                    }
                }
                else if (weapon?.WeaponType.IsTwoHanded() is true)
                {
                    slots = [TemplateSlotType.MainHand, TemplateSlotType.AltMainHand];

                    foreach (var slot in slots)
                    {
                        if (this[slot] is IWeaponTemplateEntry entry)
                        {
                            SetItem(slot, TemplateSubSlotType.Item, overrideExisting ? weapon : entry?.Weapon ?? weapon);
                        }
                    }
                }
                else
                {
                    foreach (var slot in slots)
                    {
                        if (this[slot] is IWeaponTemplateEntry entry)
                        {
                            SetItem(slot, TemplateSubSlotType.Item, overrideExisting ? weapon : entry?.Weapon ?? weapon);
                        }
                    }
                }
            }

            void SetAllSigils(TemplateSubSlotType subSlot, Sigil? inputSigil, bool overrideExisting, TemplateSlotType[] slots)
            {
                foreach (var slot in slots)
                {
                    switch (this[slot])
                    {
                        case IDoubleSigilTemplateEntry entry:
                            var sigil = overrideExisting ? inputSigil : (subSlot is TemplateSubSlotType.Sigil1 ? entry?.Sigil1 : entry.Sigil2) ?? inputSigil;
                            SetItem(slot, subSlot, sigil);
                            break;

                        case ISingleSigilTemplateEntry entry:
                            SetItem(slot, subSlot, overrideExisting ? inputSigil : entry?.Sigil1 ?? inputSigil);
                            break;
                    }
                }
            }

            void SetAllInfusions(TemplateSubSlotType subSlot, Infusion? infusion, bool overrideExisting, TemplateSlotType[] slots)
            {
                foreach (var slot in slots)
                {
                    switch (this[slot])
                    {
                        case ITripleInfusionTemplateEntry entry:
                            SetItem(slot, TemplateSubSlotType.Infusion1, overrideExisting ? infusion : entry?.Infusion1 ?? infusion);
                            SetItem(slot, TemplateSubSlotType.Infusion2, overrideExisting ? infusion : entry?.Infusion2 ?? infusion);
                            SetItem(slot, TemplateSubSlotType.Infusion3, overrideExisting ? infusion : entry?.Infusion3 ?? infusion);
                            break;

                        case IDoubleInfusionTemplateEntry entry:
                            SetItem(slot, TemplateSubSlotType.Infusion1, overrideExisting ? infusion : entry?.Infusion1 ?? infusion);
                            SetItem(slot, TemplateSubSlotType.Infusion2, overrideExisting ? infusion : entry?.Infusion2 ?? infusion);
                            break;

                        case ISingleInfusionTemplateEntry entry:
                            SetItem(slot, TemplateSubSlotType.Infusion1, overrideExisting ? infusion : entry?.Infusion1 ?? infusion);
                            break;
                    }
                }
            }
        }

        void SetSigils(TemplateSlotType templateSlot, bool overrideExisting, object obj)
        {
            var sigilSlots = templateSlot.GetGroup();
            Sigil sigil1 = obj is null ? null :
                templateSlot switch
                {
                    TemplateSlotType.MainHand => (this[TemplateSlotType.MainHand] as ISingleSigilTemplateEntry).Sigil1,
                    TemplateSlotType.OffHand => (this[TemplateSlotType.MainHand] as ISingleSigilTemplateEntry).Sigil1,
                    TemplateSlotType.AltMainHand => (this[TemplateSlotType.AltMainHand] as ISingleSigilTemplateEntry).Sigil1,
                    TemplateSlotType.AltOffHand => (this[TemplateSlotType.AltMainHand] as ISingleSigilTemplateEntry).Sigil1,
                    TemplateSlotType.Aquatic => (this[TemplateSlotType.Aquatic] as IDoubleSigilTemplateEntry).Sigil1,
                    TemplateSlotType.AltAquatic => (this[TemplateSlotType.AltAquatic] as IDoubleSigilTemplateEntry).Sigil1,
                    _ => null
                };

            Sigil sigil2 = obj is null ? null :
                templateSlot switch
                {
                    TemplateSlotType.MainHand => (this[TemplateSlotType.OffHand] as ISingleSigilTemplateEntry).Sigil1,
                    TemplateSlotType.OffHand => (this[TemplateSlotType.OffHand] as ISingleSigilTemplateEntry).Sigil1,
                    TemplateSlotType.AltMainHand => (this[TemplateSlotType.AltOffHand] as ISingleSigilTemplateEntry).Sigil1,
                    TemplateSlotType.AltOffHand => (this[TemplateSlotType.AltOffHand] as ISingleSigilTemplateEntry).Sigil1,
                    TemplateSlotType.Aquatic => (this[TemplateSlotType.Aquatic] as IDoubleSigilTemplateEntry).Sigil2,
                    TemplateSlotType.AltAquatic => (this[TemplateSlotType.AltAquatic] as IDoubleSigilTemplateEntry).Sigil2,
                    _ => null
                };

            SetSigils(overrideExisting, sigil1, sigil2);

            void SetSigils(bool overrideExisting, Sigil sigil1, Sigil sigil2)
            {
                SetItem(TemplateSlotType.MainHand, TemplateSubSlotType.Sigil1, overrideExisting ? sigil1 : (this[TemplateSlotType.MainHand] as ISingleSigilTemplateEntry).Sigil1 ?? sigil1);
                SetItem(TemplateSlotType.OffHand, TemplateSubSlotType.Sigil1, overrideExisting ? sigil2 : (this[TemplateSlotType.OffHand] as ISingleSigilTemplateEntry).Sigil1 ?? sigil2);
                SetItem(TemplateSlotType.AltMainHand, TemplateSubSlotType.Sigil1, overrideExisting ? sigil1 : (this[TemplateSlotType.AltMainHand] as ISingleSigilTemplateEntry).Sigil1 ?? sigil1);
                SetItem(TemplateSlotType.AltOffHand, TemplateSubSlotType.Sigil1, overrideExisting ? sigil2 : (this[TemplateSlotType.AltOffHand] as ISingleSigilTemplateEntry).Sigil1 ?? sigil2);

                SetItem(TemplateSlotType.Aquatic, TemplateSubSlotType.Sigil1, overrideExisting ? sigil1 : (this[TemplateSlotType.Aquatic] as ISingleSigilTemplateEntry).Sigil1 ?? sigil1);
                SetItem(TemplateSlotType.Aquatic, TemplateSubSlotType.Sigil2, overrideExisting ? sigil2 : (this[TemplateSlotType.Aquatic] as IDoubleSigilTemplateEntry).Sigil2 ?? sigil2);

                SetItem(TemplateSlotType.AltAquatic, TemplateSubSlotType.Sigil1, overrideExisting ? sigil1 : (this[TemplateSlotType.AltAquatic] as ISingleSigilTemplateEntry).Sigil1 ?? sigil1);
                SetItem(TemplateSlotType.AltAquatic, TemplateSubSlotType.Sigil2, overrideExisting ? sigil2 : (this[TemplateSlotType.AltAquatic] as IDoubleSigilTemplateEntry).Sigil2 ?? sigil2);
            }
        }

        public Skill? this[SkillSlotType slot] => Skills[slot];
    }
}
