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
using System.Diagnostics;
using Kenedia.Modules.BuildsManager.Utility;
using SharpDX;

namespace Kenedia.Modules.BuildsManager.Models
{
    [DataContract]
    public class VTemplate : IDisposable
    {
#nullable enable
        private bool _disposed = false;
        private bool _triggerEvents = true;

        private Races _race = Races.None;
        private TemplateFlag _tags = TemplateFlag.None;
        private EncounterFlag _encounters = EncounterFlag.None;
        private ProfessionType _profession = ProfessionType.Guardian;

        private string _name = "New Template";
        private string _description = string.Empty;

        private CancellationTokenSource? _cancellationTokenSource;

        public event EventHandler? GearChanged;

        public event EventHandler? BuildChanged;

        public event EventHandler? LoadedGearFromCode;

        public event EventHandler? LoadedBuildFromCode;

        public event ValueChangedEventHandler<Races>? RaceChanged;

        public event ValueChangedEventHandler<ProfessionType>? ProfessionChanged;

        public event ValueChangedEventHandler<Specialization>? EliteSpecializationChanged;

        public event DictionaryItemChangedEventHandler<SpecializationSlot, Specialization> SpecializationChanged;

        public event DictionaryItemChangedEventHandler<LegendSlot, Legend>? LegendChanged;

        public VTemplate()
        {
            Weapons = new()
                {
                {TemplateSlot.MainHand, MainHand },
                {TemplateSlot.OffHand, OffHand },
                {TemplateSlot.Aquatic, Aquatic},
                {TemplateSlot.AltMainHand, AltMainHand},
                {TemplateSlot.AltOffHand, AltOffHand },
                {TemplateSlot.AltAquatic, AltAquatic},
                };

            Armors = new()
                {
                {TemplateSlot.MainHand, MainHand },
                {TemplateSlot.OffHand, OffHand },
                {TemplateSlot.Aquatic, Aquatic},
                {TemplateSlot.AltMainHand, AltMainHand},
                {TemplateSlot.AltOffHand, AltOffHand },
                {TemplateSlot.AltAquatic, AltAquatic},
                };

            Juwellery = new()
                {
                {TemplateSlot.MainHand, MainHand },
                {TemplateSlot.OffHand, OffHand },
                {TemplateSlot.Aquatic, Aquatic},
                {TemplateSlot.AltMainHand, AltMainHand},
                {TemplateSlot.AltOffHand, AltOffHand },
                {TemplateSlot.AltAquatic, AltAquatic},
                };

            Specializations.ItemPropertyChanged += Specializations_ItemPropertyChanged;
            Legends.ItemChanged += Legends_ItemChanged;
            Pets.ItemChanged += Pets_ItemChanged;
            Skills.ItemChanged += Skills_ItemChanged;

            foreach (var spec in Specializations.Values)
            {
                spec.TraitsChanged += Spec_TraitsChanged;
            }

            MainHand.Item = BuildsManager.Data.Weapons[84888];
            AltMainHand.Item = BuildsManager.Data.Weapons[85052];

            PlayerCharacter player = Blish_HUD.GameService.Gw2Mumble.PlayerCharacter;
            Profession = player?.Profession ?? Profession;
        }

        private void Spec_TraitsChanged(object sender, EventArgs e)
        {
            BuildChanged?.Invoke(this, e);
        }

        private void Skills_ItemChanged(object sender, DictionaryItemChangedEventArgs<SkillSlot, Skill?> e)
        {
            BuildChanged?.Invoke(this, e);
        }

        private void Pets_ItemChanged(object sender, DictionaryItemChangedEventArgs<PetSlot, Pet?> e)
        {
            BuildChanged?.Invoke(this, e);
        }

        private void Legends_ItemChanged(object sender, DictionaryItemChangedEventArgs<LegendSlot, Legend?> e)
        {
            BuildChanged?.Invoke(this, e);
        }

        private void Specializations_ItemPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            BuildChanged?.Invoke(this, e);
        }

        public VTemplate(string? buildCode, string? gearCode) : this()
        {
            LoadFromCode(buildCode, gearCode);
        }

        [JsonConstructor]
        public VTemplate(string name, EncounterFlag encounters, TemplateFlag tags, string buildCode, string gearCode) : this()
        {
            // Disable Events to prevent unnecessary event triggers during the load
            _triggerEvents = false;

            _name = name;
            _encounters = encounters;
            _tags = tags;

            // Enable Events again to become responsive
            _triggerEvents = true;

            BuildCode = buildCode;
            GearCode = gearCode;
        }

        #region General Template 
        public string FilePath => @$"{BuildsManager.ModuleInstance.Paths.TemplatesPath}{Common.MakeValidFileName(Name.Trim())}.json";

        public ProfessionType Profession { get => _profession; set => Common.SetProperty(ref _profession, value, OnProfessionChanged, _triggerEvents); }

        public Races Race { get => _race; set => Common.SetProperty(ref _race, value, OnRaceChanged, _triggerEvents); }

        [DataMember]
        public TemplateFlag Tags { get => _tags; set => Common.SetProperty(ref _tags, value, AutoSave, _triggerEvents); }

        [DataMember]
        public EncounterFlag Encounters { get => _encounters; set => Common.SetProperty(ref _encounters, value, AutoSave, _triggerEvents); }

        [DataMember]
        public string Name { get => _name; set => Common.SetProperty(ref _name, value, AutoSave, _triggerEvents); }

        [DataMember]
        public string Description { get => _description; set => Common.SetProperty(ref _description, value, AutoSave, _triggerEvents); }

        [DataMember]
        public string? BuildCode
        {
            set => LoadBuildFromCode(value);
            get => ParseBuildCode();
        }

        [DataMember]
        public string? GearCode
        {
            set => LoadGearFromCode(value);
            get => ParseGearCode();
        }
        #endregion General Template

        #region Build
        public Specialization? EliteSpecialization => Specializations?[SpecializationSlot.Line_3]?.Specialization;

        public PetCollection Pets { get; } = new();

        public SkillCollection Skills { get; } = new();

        public LegendCollection Legends { get; } = new();

        public SpecializationCollection Specializations { get; } = new();
        #endregion Build

        #region Gear
        public TemplateEntry? this[TemplateSlot slot]
        {
            get => slot == TemplateSlot.None ? null : (TemplateEntry)GetType().GetProperty(slot.ToString()).GetValue(this);
            set
            {
                if (slot == TemplateSlot.None) return;

                GetType().GetProperty(slot.ToString())?.SetValue(this, value);
            }
        }

        public ArmorTemplateEntry Head { get; } = new(TemplateSlot.Head);

        public ArmorTemplateEntry Shoulder { get; } = new(TemplateSlot.Shoulder);

        public ArmorTemplateEntry Chest { get; } = new(TemplateSlot.Chest);

        public ArmorTemplateEntry Hand { get; } = new(TemplateSlot.Hand);

        public ArmorTemplateEntry Leg { get; } = new(TemplateSlot.Leg);

        public ArmorTemplateEntry Foot { get; } = new(TemplateSlot.Foot);

        public ArmorTemplateEntry AquaBreather { get; } = new(TemplateSlot.AquaBreather);

        public WeaponTemplateEntry MainHand { get; } = new(TemplateSlot.MainHand);

        public WeaponTemplateEntry OffHand { get; } = new(TemplateSlot.OffHand);

        public AquaticWeaponTemplateEntry Aquatic { get; } = new(TemplateSlot.Aquatic);

        public WeaponTemplateEntry AltMainHand { get; } = new(TemplateSlot.AltMainHand);

        public WeaponTemplateEntry AltOffHand { get; } = new(TemplateSlot.AltOffHand);

        public AquaticWeaponTemplateEntry AltAquatic { get; } = new(TemplateSlot.AltAquatic);

        public BackTemplateEntry Back { get; } = new(TemplateSlot.Back);

        public AmuletTemplateEntry Amulet { get; } = new(TemplateSlot.Amulet);

        public AccessoireTemplateEntry Accessory_1 { get; } = new(TemplateSlot.Accessory_1);

        public AccessoireTemplateEntry Accessory_2 { get; } = new(TemplateSlot.Accessory_2);

        public RingTemplateEntry Ring_1 { get; } = new(TemplateSlot.Ring_1);

        public RingTemplateEntry Ring_2 { get; } = new(TemplateSlot.Ring_2);

        public PvpAmuletTemplateEntry PvpAmulet { get; } = new(TemplateSlot.PvpAmulet);

        public NourishmentEntry Nourishment { get; } = new(TemplateSlot.Nourishment);

        public UtilityEntry Utility { get; } = new(TemplateSlot.Utility);

        public JadeBotTemplateEntry JadeBotCore { get; } = new(TemplateSlot.JadeBotCore);

        public RelicTemplateEntry Relic { get; } = new(TemplateSlot.Relic);

        public Dictionary<TemplateSlot, TemplateEntry> Weapons { get; }

        public Dictionary<TemplateSlot, TemplateEntry> Armors { get; }

        public Dictionary<TemplateSlot, TemplateEntry> Juwellery { get; }
        #endregion

        private async void OnProfessionChanged(object sender, ValueChangedEventArgs<ProfessionType> e)
        {
            if (Skills.WipeSkills(Race))
            {
                BuildChanged?.Invoke(this, e);
            }

            Specializations.Wipe();
            Pets.Wipe();
            Legends.Wipe();

            RemoveInvalidCombinations();
            ProfessionChanged?.Invoke(this, e);
            await Save();
        }

        private async void OnRaceChanged(object sender, ValueChangedEventArgs<Races> e)
        {
            if (Skills.WipeInvalidRacialSkills(Race))
            {
                BuildChanged?.Invoke(this, e);
            }

            RemoveInvalidCombinations();
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

                Specializations.LoadFromCode(build.Profession, SpecializationSlot.Line_1, build.Specialization1Id, build.Specialization1Trait1Index, build.Specialization1Trait2Index, build.Specialization1Trait3Index);
                Specializations.LoadFromCode(build.Profession, SpecializationSlot.Line_2, build.Specialization2Id, build.Specialization2Trait1Index, build.Specialization2Trait2Index, build.Specialization2Trait3Index);
                Specializations.LoadFromCode(build.Profession, SpecializationSlot.Line_3, build.Specialization3Id, build.Specialization3Trait1Index, build.Specialization3Trait2Index, build.Specialization3Trait3Index);

                if (Profession == ProfessionType.Ranger)
                {
                    Pets.LoadFromCode(build.RangerTerrestrialPet1Id, build.RangerTerrestrialPet2Id, build.RangerAquaticPet1Id, build.RangerAquaticPet2Id);
                }

                if (Profession == ProfessionType.Revenant)
                {
                    Legends[LegendSlot.TerrestrialInactive] = Legend.FromByte(build.RevenantInactiveTerrestrialLegend);

                    Skills[SkillSlot.Inactive | SkillSlot.Terrestrial | SkillSlot.Heal] = Legend.FromByte(build.RevenantInactiveTerrestrialLegend)?.Heal;
                    Skills[SkillSlot.Inactive | SkillSlot.Terrestrial | SkillSlot.Utility_1] = Legend.SkillFromUShort(build.RevenantInactiveTerrestrialUtility1SkillPaletteId, Legends[LegendSlot.TerrestrialInactive]);
                    Skills[SkillSlot.Inactive | SkillSlot.Terrestrial | SkillSlot.Utility_2] = Legend.SkillFromUShort(build.RevenantInactiveTerrestrialUtility2SkillPaletteId, Legends[LegendSlot.TerrestrialInactive]);
                    Skills[SkillSlot.Inactive | SkillSlot.Terrestrial | SkillSlot.Utility_3] = Legend.SkillFromUShort(build.RevenantInactiveTerrestrialUtility3SkillPaletteId, Legends[LegendSlot.TerrestrialInactive]);
                    Skills[SkillSlot.Inactive | SkillSlot.Terrestrial | SkillSlot.Elite] = Legend.FromByte(build.RevenantInactiveTerrestrialLegend)?.Elite;

                    Legends[LegendSlot.AquaticInactive] = Legend.FromByte(build.RevenantInactiveAquaticLegend);
                    Skills[SkillSlot.Inactive | SkillSlot.Aquatic | SkillSlot.Heal] = Legend.FromByte(build.RevenantInactiveAquaticLegend)?.Heal;
                    Skills[SkillSlot.Inactive | SkillSlot.Aquatic | SkillSlot.Utility_1] = Legend.SkillFromUShort(build.RevenantInactiveAquaticUtility1SkillPaletteId, Legends[LegendSlot.AquaticInactive]);
                    Skills[SkillSlot.Inactive | SkillSlot.Aquatic | SkillSlot.Utility_2] = Legend.SkillFromUShort(build.RevenantInactiveAquaticUtility2SkillPaletteId, Legends[LegendSlot.AquaticInactive]);
                    Skills[SkillSlot.Inactive | SkillSlot.Aquatic | SkillSlot.Utility_3] = Legend.SkillFromUShort(build.RevenantInactiveAquaticUtility3SkillPaletteId, Legends[LegendSlot.AquaticInactive]);
                    Skills[SkillSlot.Inactive | SkillSlot.Aquatic | SkillSlot.Elite] = Legend.FromByte(build.RevenantInactiveAquaticLegend)?.Elite;

                    Legends[LegendSlot.TerrestrialActive] = Legend.FromByte(build.RevenantActiveTerrestrialLegend);
                    Skills[SkillSlot.Active | SkillSlot.Terrestrial | SkillSlot.Heal] = Legend.SkillFromUShort(build.TerrestrialHealingSkillPaletteId, Legends[LegendSlot.TerrestrialActive]);
                    Skills[SkillSlot.Active | SkillSlot.Terrestrial | SkillSlot.Utility_1] = Legend.SkillFromUShort(build.TerrestrialUtility1SkillPaletteId, Legends[LegendSlot.TerrestrialActive]);
                    Skills[SkillSlot.Active | SkillSlot.Terrestrial | SkillSlot.Utility_2] = Legend.SkillFromUShort(build.TerrestrialUtility2SkillPaletteId, Legends[LegendSlot.TerrestrialActive]);
                    Skills[SkillSlot.Active | SkillSlot.Terrestrial | SkillSlot.Utility_3] = Legend.SkillFromUShort(build.TerrestrialUtility3SkillPaletteId, Legends[LegendSlot.TerrestrialActive]);
                    Skills[SkillSlot.Active | SkillSlot.Terrestrial | SkillSlot.Elite] = Legend.SkillFromUShort(build.TerrestrialEliteSkillPaletteId, Legends[LegendSlot.TerrestrialActive]);

                    Legends[LegendSlot.AquaticActive] = Legend.FromByte(build.RevenantActiveAquaticLegend);
                    Skills[SkillSlot.Active | SkillSlot.Aquatic | SkillSlot.Heal] = Legend.SkillFromUShort(build.AquaticHealingSkillPaletteId, Legends[LegendSlot.AquaticActive]);
                    Skills[SkillSlot.Active | SkillSlot.Aquatic | SkillSlot.Utility_1] = Legend.SkillFromUShort(build.AquaticUtility1SkillPaletteId, Legends[LegendSlot.AquaticActive]);
                    Skills[SkillSlot.Active | SkillSlot.Aquatic | SkillSlot.Utility_2] = Legend.SkillFromUShort(build.AquaticUtility2SkillPaletteId, Legends[LegendSlot.AquaticActive]);
                    Skills[SkillSlot.Active | SkillSlot.Aquatic | SkillSlot.Utility_3] = Legend.SkillFromUShort(build.AquaticUtility3SkillPaletteId, Legends[LegendSlot.AquaticActive]);
                    Skills[SkillSlot.Active | SkillSlot.Aquatic | SkillSlot.Elite] = Legend.SkillFromUShort(build.AquaticEliteSkillPaletteId, Legends[LegendSlot.AquaticActive]);
                }
                else
                {
                    Skills[SkillSlot.Active | SkillSlot.Terrestrial | SkillSlot.Heal] = Skill.FromUShort(build.TerrestrialHealingSkillPaletteId, build.Profession);
                    Skills[SkillSlot.Active | SkillSlot.Terrestrial | SkillSlot.Utility_1] = Skill.FromUShort(build.TerrestrialUtility1SkillPaletteId, build.Profession);
                    Skills[SkillSlot.Active | SkillSlot.Terrestrial | SkillSlot.Utility_2] = Skill.FromUShort(build.TerrestrialUtility2SkillPaletteId, build.Profession);
                    Skills[SkillSlot.Active | SkillSlot.Terrestrial | SkillSlot.Utility_3] = Skill.FromUShort(build.TerrestrialUtility3SkillPaletteId, build.Profession);
                    Skills[SkillSlot.Active | SkillSlot.Terrestrial | SkillSlot.Elite] = Skill.FromUShort(build.TerrestrialEliteSkillPaletteId, build.Profession);

                    Skills[SkillSlot.Active | SkillSlot.Aquatic | SkillSlot.Heal] = Skill.FromUShort(build.AquaticHealingSkillPaletteId, build.Profession);
                    Skills[SkillSlot.Active | SkillSlot.Aquatic | SkillSlot.Utility_1] = Skill.FromUShort(build.AquaticUtility1SkillPaletteId, build.Profession);
                    Skills[SkillSlot.Active | SkillSlot.Aquatic | SkillSlot.Utility_2] = Skill.FromUShort(build.AquaticUtility2SkillPaletteId, build.Profession);
                    Skills[SkillSlot.Active | SkillSlot.Aquatic | SkillSlot.Utility_3] = Skill.FromUShort(build.AquaticUtility3SkillPaletteId, build.Profession);
                    Skills[SkillSlot.Active | SkillSlot.Aquatic | SkillSlot.Elite] = Skill.FromUShort(build.AquaticEliteSkillPaletteId, build.Profession);
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

            short[] codeArray = GearTemplateCode.DecodeBase64ToShorts(code);

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
                RangerAquaticPet1Id = Pets.GetPetByte(PetSlot.Aquatic_1),
                RangerAquaticPet2Id = Pets.GetPetByte(PetSlot.Aquatic_2),
                RangerTerrestrialPet1Id = Pets.GetPetByte(PetSlot.Terrestrial_1),
                RangerTerrestrialPet2Id = Pets.GetPetByte(PetSlot.Terrestrial_2),

                Specialization1Id = Specializations.GetSpecializationByte(SpecializationSlot.Line_1),
                Specialization1Trait1Index = Specializations.GetTraitByte(TraitTier.Adept, Specializations[SpecializationSlot.Line_1]),
                Specialization1Trait2Index = Specializations.GetTraitByte(TraitTier.Master, Specializations[SpecializationSlot.Line_1]),
                Specialization1Trait3Index = Specializations.GetTraitByte(TraitTier.GrandMaster, Specializations[SpecializationSlot.Line_1]),

                Specialization2Id = Specializations.GetSpecializationByte(SpecializationSlot.Line_2),
                Specialization2Trait1Index = Specializations.GetTraitByte(TraitTier.Adept, Specializations[SpecializationSlot.Line_2]),
                Specialization2Trait2Index = Specializations.GetTraitByte(TraitTier.Master, Specializations[SpecializationSlot.Line_2]),
                Specialization2Trait3Index = Specializations.GetTraitByte(TraitTier.GrandMaster, Specializations[SpecializationSlot.Line_2]),

                Specialization3Id = Specializations.GetSpecializationByte(SpecializationSlot.Line_3),
                Specialization3Trait1Index = Specializations.GetTraitByte(TraitTier.Adept, Specializations[SpecializationSlot.Line_3]),
                Specialization3Trait2Index = Specializations.GetTraitByte(TraitTier.Master, Specializations[SpecializationSlot.Line_3]),
                Specialization3Trait3Index = Specializations.GetTraitByte(TraitTier.GrandMaster, Specializations[SpecializationSlot.Line_3]),
            };

            if (Profession == ProfessionType.Revenant)
            {
                build.RevenantActiveTerrestrialLegend = Legends.GetLegendByte(LegendSlot.TerrestrialActive);
                build.RevenantInactiveTerrestrialLegend = Legends.GetLegendByte(LegendSlot.TerrestrialInactive);
                build.RevenantInactiveTerrestrialUtility1SkillPaletteId = Skills.GetPaletteId(SkillSlot.Inactive | SkillSlot.Terrestrial | SkillSlot.Utility_1);
                build.RevenantInactiveTerrestrialUtility2SkillPaletteId = Skills.GetPaletteId(SkillSlot.Inactive | SkillSlot.Terrestrial | SkillSlot.Utility_2);
                build.RevenantInactiveTerrestrialUtility3SkillPaletteId = Skills.GetPaletteId(SkillSlot.Inactive | SkillSlot.Terrestrial | SkillSlot.Utility_3);

                build.RevenantActiveAquaticLegend = Legends.GetLegendByte(LegendSlot.AquaticActive);
                build.RevenantInactiveAquaticLegend = Legends.GetLegendByte(LegendSlot.AquaticInactive);

                build.RevenantInactiveAquaticUtility1SkillPaletteId = Skills.GetPaletteId(SkillSlot.Inactive | SkillSlot.Aquatic | SkillSlot.Utility_1);
                build.RevenantInactiveAquaticUtility2SkillPaletteId = Skills.GetPaletteId(SkillSlot.Inactive | SkillSlot.Aquatic | SkillSlot.Utility_2);
                build.RevenantInactiveAquaticUtility3SkillPaletteId = Skills.GetPaletteId(SkillSlot.Inactive | SkillSlot.Aquatic | SkillSlot.Utility_3);
            }

            build.TerrestrialHealingSkillPaletteId = Skills.GetPaletteId(SkillSlot.Active | SkillSlot.Terrestrial | SkillSlot.Heal);
            build.TerrestrialUtility1SkillPaletteId = Skills.GetPaletteId(SkillSlot.Active | SkillSlot.Terrestrial | SkillSlot.Utility_1);
            build.TerrestrialUtility2SkillPaletteId = Skills.GetPaletteId(SkillSlot.Active | SkillSlot.Terrestrial | SkillSlot.Utility_2);
            build.TerrestrialUtility3SkillPaletteId = Skills.GetPaletteId(SkillSlot.Active | SkillSlot.Terrestrial | SkillSlot.Utility_3);
            build.TerrestrialEliteSkillPaletteId = Skills.GetPaletteId(SkillSlot.Active | SkillSlot.Terrestrial | SkillSlot.Elite);

            build.AquaticHealingSkillPaletteId = Skills.GetPaletteId(SkillSlot.Active | SkillSlot.Aquatic | SkillSlot.Heal);
            build.AquaticUtility1SkillPaletteId = Skills.GetPaletteId(SkillSlot.Active | SkillSlot.Aquatic | SkillSlot.Utility_1);
            build.AquaticUtility2SkillPaletteId = Skills.GetPaletteId(SkillSlot.Active | SkillSlot.Aquatic | SkillSlot.Utility_2);
            build.AquaticUtility3SkillPaletteId = Skills.GetPaletteId(SkillSlot.Active | SkillSlot.Aquatic | SkillSlot.Utility_3);
            build.AquaticEliteSkillPaletteId = Skills.GetPaletteId(SkillSlot.Active | SkillSlot.Aquatic | SkillSlot.Elite);

            byte[] bytes = build.ToArray();

            build.Parse(bytes.Concat(new byte[] { 0, 0 }).ToArray());
            string code = build.ToString();

            return code;
        }

        public string? ParseGearCode()
        {
            short[] codeArray = new short[0];

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

            return GearTemplateCode.EncodeShortsToBase64(codeArray);
        }

        private async void AutoSave()
        {
            await Save();
        }

        public async Task Save()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                await Task.Delay(1000, _cancellationTokenSource.Token);
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

        public SpecializationSlot? GetSpecializationSlot(Specialization specialization)
        {
            foreach (var spec in Specializations)
            {
                if (spec.Value?.Specialization == specialization) return spec.Key;
            }

            return null;
        }

        public void SetSpecialization(SpecializationSlot slot, Specialization? specialization = null, Trait? adept = null, Trait? master = null, Trait? grandmaster = null)
        {
            if (Specializations.TryGetValue(slot, out var spec) && spec != null)
            {
                var prev = spec.Specialization;
                spec.Specialization = specialization;
                spec.Traits[TraitTier.Adept] = adept;
                spec.Traits[TraitTier.Master] = master;
                spec.Traits[TraitTier.GrandMaster] = grandmaster;

                RemoveInvalidCombinations();
                OnSpecializationChanged(this, new(slot, prev, specialization));
            }
        }

        private async void OnSpecializationChanged(object sender, DictionaryItemChangedEventArgs<SpecializationSlot, Specialization> e)
        {
            SpecializationChanged?.Invoke(sender, e);
            await Save();
        }

        public void SwapSpecializations(SpecializationSlot slot1, SpecializationSlot slot2)
        {
            _triggerEvents = false;

            if (Specializations != null)
            {
                var prevSlot1 = new BuildSpecialization() { Specialization = Specializations?[slot1]?.Specialization?.Elite == true && slot2 != SpecializationSlot.Line_3 ? null : Specializations?[slot1]?.Specialization };
                prevSlot1.Traits[TraitTier.Adept] = prevSlot1.Specialization != null ? Specializations?[slot1]?.Traits[TraitTier.Adept] : null;
                prevSlot1.Traits[TraitTier.Master] = prevSlot1.Specialization != null ? Specializations?[slot1]?.Traits[TraitTier.Master] : null;
                prevSlot1.Traits[TraitTier.GrandMaster] = prevSlot1.Specialization != null ? Specializations?[slot1]?.Traits[TraitTier.GrandMaster] : null;

                var prevSlot2 = new BuildSpecialization() { Specialization = Specializations?[slot2]?.Specialization?.Elite == true && slot1 != SpecializationSlot.Line_3 ? null : Specializations?[slot2]?.Specialization };
                prevSlot2.Traits[TraitTier.Adept] = prevSlot2.Specialization != null ? Specializations?[slot2]?.Traits[TraitTier.Adept] : null;
                prevSlot2.Traits[TraitTier.Master] = prevSlot2.Specialization != null ? Specializations?[slot2]?.Traits[TraitTier.Master] : null;
                prevSlot2.Traits[TraitTier.GrandMaster] = prevSlot2.Specialization != null ? Specializations?[slot2]?.Traits[TraitTier.GrandMaster] : null;

                SetSpecialization(slot2, prevSlot1.Specialization, prevSlot1.Traits[TraitTier.Adept], prevSlot1.Traits[TraitTier.Master], prevSlot1.Traits[TraitTier.GrandMaster]);
                SetSpecialization(slot1, prevSlot2.Specialization, prevSlot2.Traits[TraitTier.Adept], prevSlot2.Traits[TraitTier.Master], prevSlot2.Traits[TraitTier.GrandMaster]);

                RemoveInvalidCombinations();
                SpecializationChanged?.Invoke(this, new(slot1, prevSlot1.Specialization, prevSlot2.Specialization));
            }

            _triggerEvents = true;
        }

        public void SetLegend(Legend legend, LegendSlot slot)
        {
            SkillSlot state = SkillSlot.Active;
            SkillSlot enviroment = SkillSlot.Terrestrial;

            switch (slot)
            {
                case LegendSlot.AquaticActive:
                    state = SkillSlot.Active;
                    enviroment = SkillSlot.Aquatic;
                    break;

                case LegendSlot.AquaticInactive:
                    state = SkillSlot.Inactive;
                    enviroment = SkillSlot.Aquatic;
                    break;

                case LegendSlot.TerrestrialActive:
                    state = SkillSlot.Active;
                    enviroment = SkillSlot.Terrestrial;
                    break;

                case LegendSlot.TerrestrialInactive:
                    state = SkillSlot.Inactive;
                    enviroment = SkillSlot.Terrestrial;
                    break;
            };

            Skills[state | enviroment | SkillSlot.Heal] = legend?.Heal;
            Skills[state | enviroment | SkillSlot.Elite] = legend?.Elite;

            List<int?> paletteIds = new()
            {
                 Skills[state | enviroment | SkillSlot.Utility_1]?.PaletteId,
                 Skills[state | enviroment | SkillSlot.Utility_2]?.PaletteId,
                 Skills[state | enviroment | SkillSlot.Utility_3]?.PaletteId,
            };

            List<int?> ids = new() { 4614, 4651, 4564 };
            int?[] missingIds = ids.Except(paletteIds).ToArray();

            var array = new SkillSlot[] { SkillSlot.Utility_1, SkillSlot.Utility_2, SkillSlot.Utility_3 };
            for (int i = 0; i < array.Length; i++)
            {
                SkillSlot skillSlot = state | enviroment | array[i];
                int? paletteId = Skills[skillSlot]?.PaletteId;
                Skills[skillSlot] = paletteId != null ? Legend.SkillFromUShort((ushort)paletteId.Value, legend) : null;
            }

            for (int j = 0; j < missingIds.Length; j++)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    SkillSlot skillSlot = state | enviroment | array[i];
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

        private void RemoveInvalidCombinations()
        {
            // Check and clean invalid Legends
            if (Profession is ProfessionType.Revenant)
            {
                var wipeLegends = new List<LegendSlot>();

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

            // Check and clean invalid Weapons?
            var wipeWeapons = new List<TemplateSlot>();
            foreach (var slot in Weapons)
            {
                if (slot.Key is not TemplateSlot.Aquatic and not TemplateSlot.AltAquatic)
                {
                    var weapon = slot.Value as WeaponTemplateEntry;
                    if (weapon?.Item?.WeaponType != null)
                    {
                        var weaponType = weapon.Item.WeaponType;
                        var profWeapon = BuildsManager.Data.Professions[Profession].Weapons.FirstOrDefault(e => e.Value.Type.ToItemWeaponType() == weaponType);

                        if (profWeapon.Value == null || profWeapon.Value.Specialization != EliteSpecialization?.Id)
                        {
                            wipeWeapons.Add(slot.Key);
                        }
                    }
                }
            }

            foreach (var slot in wipeWeapons)
            {
                var legend = Weapons[slot];
                if (Weapons[slot] != null)
                {
                    if (Weapons[slot] is WeaponTemplateEntry weapon)
                    {
                        BuildsManager.Logger.Info($"Remove {weapon.Item?.Name} because we can not wield it with our current specs.");
                        weapon.Item = null;
                    }
                }
            }
        }

        private async void OnLegendChanged(object sender, DictionaryItemChangedEventArgs<LegendSlot, Legend> e)
        {
            LegendChanged?.Invoke(sender, e);
            await Save();
        }
#nullable disable
    }
}
