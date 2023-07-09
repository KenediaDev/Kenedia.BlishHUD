using Blish_HUD.Gw2Mumble;
using Gw2Sharp.ChatLinks;
using Gw2Sharp.Models;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.Core.DataModels;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using SharpDX.WIC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Kenedia.Modules.BuildsManager.Models.Templates
{
    [Flags]
    public enum SkillSlot
    {
        None = 0,
        Active = 1 << 0,
        Inactive = 1 << 1,
        Terrestrial = 1 << 2,
        Aquatic = 1 << 3,
        Heal = 1 << 4,
        Utility_1 = 1 << 5,
        Utility_2 = 1 << 6,
        Utility_3 = 1 << 7,
        Elite = 1 << 8,
    }

    public static class SkillSlotExtension
    {
        public static SkillSlot GetEnviromentState(this SkillSlot slot)
        {
            var tempSlot = slot;
            return tempSlot &= ~(SkillSlot.Utility_1 | SkillSlot.Utility_1 | SkillSlot.Utility_2 | SkillSlot.Utility_3 | SkillSlot.Heal | SkillSlot.Elite);
        }
    }

    public class SkillCollection : ObservableDictionary<SkillSlot, Skill>
    {
        /// <summary>
        /// <para>Generates a Dictionary with entries for all Skill Slot combinations that are valid in a build </para>
        ///  <br>Active | Terrestrial | Heal : 21</br>
        ///  <br>Active | Aquatic | Heal : 25</br>
        ///  <br>Inactive | Terrestrial | Heal : 22</br>
        ///  <br>Inactive | Aquatic | Heal : 26</br>
        ///  <br>Active | Terrestrial | Utility_1 : 37</br>
        ///  <br>... </br>
        /// </summary>
        public SkillCollection()
        {
            foreach (SkillSlot slot in Enum.GetValues(typeof(SkillSlot)))
            {
                if (slot >= SkillSlot.Heal)
                {
                    foreach (SkillSlot state in new SkillSlot[] { SkillSlot.Active, SkillSlot.Inactive })
                    {
                        foreach (SkillSlot enviroment in new SkillSlot[] { SkillSlot.Terrestrial, SkillSlot.Aquatic })
                        {
                            Add(state | enviroment | slot, new());
                        }
                    }
                }
            }
        }

        public ushort GetPaletteId(SkillSlot slot)
        {
            return (ushort)(TryGetValue(slot, out Skill skill) && skill != null ? skill.PaletteId : 0);
        }

        public bool HasSkill(Skill skill, SkillSlot state_enviroment)
        {
            foreach (var s in this)
            {
                if (s.Key.HasFlag(state_enviroment))
                {
                    if (s.Value == skill) return true;
                }
            }

            return false;
        }

        public bool HasSkill(int skillid, SkillSlot state_enviroment)
        {
            foreach (var s in this)
            {
                if (s.Key.HasFlag(state_enviroment))
                {
                    if (s.Value?.Id == skillid) return true;
                }
            }

            return false;
        }

        public SkillSlot GetSkillSlot(int skillid, SkillSlot state_enviroment)
        {
            foreach (var s in this)
            {
                if (s.Key.HasFlag(state_enviroment))
                {
                    if (s.Value?.Id == skillid) return s.Key;
                }
            }

            return SkillSlot.Utility_1;
        }

        public SkillSlot GetSkillSlot(Skill skill, SkillSlot state_enviroment)
        {
            foreach (var s in this)
            {
                if (s.Key.HasFlag(state_enviroment))
                {
                    if (s.Value == skill) return s.Key;
                }
            }

            return SkillSlot.Utility_1;
        }

        public void SelectSkill(Skill skill, SkillSlot targetSlot, Skill previousSkill = null)
        {
            SkillSlot enviromentState = targetSlot.GetEnviromentState();

            if (HasSkill(skill, enviromentState))
            {
                var slot = GetSkillSlot(skill, enviromentState);
                this[slot] = previousSkill;
            }

            this[targetSlot] = skill;
        }
    }

    public class BuildTemplate : IDisposable
    {
        private bool _disposed = false;
        private bool _loading = false;
        private bool _busy = false;

        private ProfessionType _profession;

        public BuildTemplate()
        {
            PlayerCharacter player = Blish_HUD.GameService.Gw2Mumble.PlayerCharacter;
            Profession = player != null ? player.Profession : ProfessionType.Guardian;

            Specializations.ItemPropertyChanged += Specializations_ItemPropertyChanged;
            Legends.ItemChanged += Legends_ItemChanged;
            Pets.ItemChanged += Pets_ItemChanged;
            Skills.ItemChanged += Skills_ItemChanged;

            foreach (var spec in Specializations.Values)
            {
                spec.TraitsChanged += Spec_TraitsChanged;
            }
        }

        private void Skills_ItemChanged(object sender, DictionaryItemChangedEventArgs<SkillSlot, Skill> e)
        {
            if (_busy) return;
            SkillChanged?.Invoke(sender, e);
            BuildCodeChanged?.Invoke(sender, e);
        }

        private void Pets_ItemChanged(object sender, DictionaryItemChangedEventArgs<PetSlot, Pet> e)
        {
            if (_busy) return;
            BuildCodeChanged?.Invoke(sender, e);
        }

        private void Legends_ItemChanged(object sender, DictionaryItemChangedEventArgs<LegendSlot, Legend> e)
        {
            if (_busy) return;
            BuildCodeChanged?.Invoke(sender, e);
        }

        public BuildTemplate(string buildCode) : this()
        {
            LoadFromCode(buildCode);
        }

        public event EventHandler BuildCodeChanged;

        public event EventHandler Loaded;

        public event DictionaryItemChangedEventHandler<PetSlot, Pet> PetChanged;

        public event ValueChangedEventHandler<ProfessionType> ProfessionChanged;

        public event DictionaryItemChangedEventHandler<SkillSlot, Skill> SkillChanged;

        public event ValueChangedEventHandler<Specialization> EliteSpecializationChanged;

        public event DictionaryItemChangedEventHandler<LegendSlot, Legend> LegendChanged;

        public event DictionaryItemChangedEventHandler<SpecializationSlot, Specialization> SpecializationChanged;

        public ProfessionType Profession { get => _profession; set => Common.SetProperty(ref _profession, value, OnProfessionChanged); }

        public Specialization EliteSpecialization => Specializations[SpecializationSlot.Line_3]?.Specialization;

        public SkillCollection Skills { get; } = new();

        public LegendCollection Legends { get; } = new();

        public PetCollection Pets { get; } = new();

        public SpecializationCollection Specializations { get; } = new();

        private void Spec_TraitsChanged(object sender, EventArgs e)
        {
            if (_busy) return;
            BuildCodeChanged?.Invoke(sender, e);
        }

        private void Specializations_ItemPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (_busy) return;
            if (e.PropertyName == nameof(Specialization))
            {
                SpecializationChanged?.Invoke(this, null);
            }
        }

        private void OnProfessionChanged(object sender, ValueChangedEventArgs<ProfessionType> e)
        {
            if (_busy) return;

            ProfessionChanged?.Invoke(this, e);
            BuildCodeChanged?.Invoke(this, e);
        }

        private void OnBuildCodeChanged(object sender, EventArgs e)
        {
            if (_busy) return;
            BuildCodeChanged?.Invoke(this, e);
        }

        private void OnLegendChanged(object sender, DictionaryItemChangedEventArgs<LegendSlot, Legend> e)
        {
            if (_busy) return;
            LegendChanged?.Invoke(this, e);

            OnBuildCodeChanged(this, e);
        }

        private void OnSkillChanged(object sender, DictionaryItemChangedEventArgs<SkillSlot, Skill> e)
        {
            if (_busy) return;
            SkillChanged?.Invoke(this, e);

            OnBuildCodeChanged(this, e);
        }

        private void OnSpecializationChanged(object sender, DictionaryItemChangedEventArgs<SpecializationSlot, Specialization> e)
        {
            if (_busy) return;
            SpecializationChanged?.Invoke(this, e);

            if (e.Key == SpecializationSlot.Line_3)
            {
                EliteSpecializationChanged(sender, new(e.OldValue, e.NewValue));
            }

            OnBuildCodeChanged(this, e);
        }

        private void OnPetChanged(object sender, DictionaryItemChangedEventArgs<PetSlot, Pet> e)
        {
            if (_busy) return;
            PetChanged?.Invoke(this, e);

            OnBuildCodeChanged(this, e);
        }

        public string ParseBuildCode()
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

            //Debug.WriteLine($"{Specializations[SpecializationSlot.Line_1].Traits.Aggregate($"{Specializations[SpecializationSlot.Line_1].Specialization?.Name}: ", (c, n) => c + n.Value?.Name + $"[{Specializations.GetTraitByte(n.Value.Tier, Specializations[SpecializationSlot.Line_1])}]"+",")}");

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

        public void LoadFromCode(string code)
        {
            BuildChatLink build = new();
            _loading = true;
            _busy = true;

            if (Gw2ChatLink.TryParse(code, out IGw2ChatLink chatlink))
            {
                build.Parse(chatlink.ToArray());
                Profession = build.Profession;

                Specializations[SpecializationSlot.Line_1].Specialization = Specialization.FromByte(build.Specialization1Id, build.Profession);
                if (Specializations[SpecializationSlot.Line_1].Specialization != null)
                {
                    Specializations[SpecializationSlot.Line_1].Traits[TraitTier.Adept] = Trait.FromByte(build.Specialization1Trait1Index, Specializations[SpecializationSlot.Line_1]?.Specialization, TraitTier.Adept);
                    Specializations[SpecializationSlot.Line_1].Traits[TraitTier.Master] = Trait.FromByte(build.Specialization1Trait2Index, Specializations[SpecializationSlot.Line_1]?.Specialization, TraitTier.Master);
                    Specializations[SpecializationSlot.Line_1].Traits[TraitTier.GrandMaster] = Trait.FromByte(build.Specialization1Trait3Index, Specializations[SpecializationSlot.Line_1]?.Specialization, TraitTier.GrandMaster);
                }

                Specializations[SpecializationSlot.Line_2].Specialization = Specialization.FromByte(build.Specialization2Id, build.Profession);
                if (Specializations[SpecializationSlot.Line_2] != null)
                {
                    Specializations[SpecializationSlot.Line_2].Traits[TraitTier.Adept] = Trait.FromByte(build.Specialization2Trait1Index, Specializations[SpecializationSlot.Line_2]?.Specialization, TraitTier.Adept);
                    Specializations[SpecializationSlot.Line_2].Traits[TraitTier.Master] = Trait.FromByte(build.Specialization2Trait2Index, Specializations[SpecializationSlot.Line_2]?.Specialization, TraitTier.Master);
                    Specializations[SpecializationSlot.Line_2].Traits[TraitTier.GrandMaster] = Trait.FromByte(build.Specialization2Trait3Index, Specializations[SpecializationSlot.Line_2]?.Specialization, TraitTier.GrandMaster);
                }

                Specializations[SpecializationSlot.Line_3].Specialization = Specialization.FromByte(build.Specialization3Id, build.Profession);
                if (Specializations[SpecializationSlot.Line_3] != null)
                {
                    Specializations[SpecializationSlot.Line_3].Traits[TraitTier.Adept] = Trait.FromByte(build.Specialization3Trait1Index, Specializations[SpecializationSlot.Line_3]?.Specialization, TraitTier.Adept);
                    Specializations[SpecializationSlot.Line_3].Traits[TraitTier.Master] = Trait.FromByte(build.Specialization3Trait2Index, Specializations[SpecializationSlot.Line_3]?.Specialization, TraitTier.Master);
                    Specializations[SpecializationSlot.Line_3].Traits[TraitTier.GrandMaster] = Trait.FromByte(build.Specialization3Trait3Index, Specializations[SpecializationSlot.Line_3]?.Specialization, TraitTier.GrandMaster);
                }

                if (Profession == ProfessionType.Ranger)
                {
                    Pets[PetSlot.Terrestrial_1] = Pet.FromByte(build.RangerTerrestrialPet1Id);
                    Pets[PetSlot.Terrestrial_2] = Pet.FromByte(build.RangerTerrestrialPet2Id);
                    Pets[PetSlot.Aquatic_1] = Pet.FromByte(build.RangerAquaticPet1Id);
                    Pets[PetSlot.Aquatic_2] = Pet.FromByte(build.RangerAquaticPet2Id);
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

            _loading = false;
            _busy = false;
            Loaded?.Invoke(this, null);
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

        public void SwapSpecializations(SpecializationSlot slot1, SpecializationSlot slot2)
        {
            _busy = true;

            var prevSlot1 = new BuildSpecialization() { Specialization = Specializations[slot1].Specialization?.Elite == true && slot2 != SpecializationSlot.Line_3 ? null : Specializations[slot1].Specialization };
            prevSlot1.Traits[TraitTier.Adept] = prevSlot1.Specialization != null ? Specializations[slot1].Traits[TraitTier.Adept] : null;
            prevSlot1.Traits[TraitTier.Master] = prevSlot1.Specialization != null ? Specializations[slot1].Traits[TraitTier.Master] : null;
            prevSlot1.Traits[TraitTier.GrandMaster] = prevSlot1.Specialization != null ? Specializations[slot1].Traits[TraitTier.GrandMaster] : null;

            var prevSlot2 = new BuildSpecialization() { Specialization = Specializations[slot2].Specialization?.Elite == true && slot1 != SpecializationSlot.Line_3 ? null : Specializations[slot2].Specialization };
            prevSlot2.Traits[TraitTier.Adept] = prevSlot2.Specialization != null ? Specializations[slot2].Traits[TraitTier.Adept] : null;
            prevSlot2.Traits[TraitTier.Master] = prevSlot2.Specialization != null ? Specializations[slot2].Traits[TraitTier.Master] : null;
            prevSlot2.Traits[TraitTier.GrandMaster] = prevSlot2.Specialization != null ? Specializations[slot2].Traits[TraitTier.GrandMaster] : null;

            Specializations[slot2].Specialization = prevSlot1.Specialization;
            Specializations[slot2].Traits[TraitTier.Adept] = prevSlot1.Traits[TraitTier.Adept];
            Specializations[slot2].Traits[TraitTier.Master] = prevSlot1.Traits[TraitTier.Master];
            Specializations[slot2].Traits[TraitTier.GrandMaster] = prevSlot1.Traits[TraitTier.GrandMaster];

            Specializations[slot1].Specialization = prevSlot2.Specialization;
            Specializations[slot1].Traits[TraitTier.Adept] = prevSlot2.Traits[TraitTier.Adept];
            Specializations[slot1].Traits[TraitTier.Master] = prevSlot2.Traits[TraitTier.Master];
            Specializations[slot1].Traits[TraitTier.GrandMaster] = prevSlot2.Traits[TraitTier.GrandMaster];

            _busy = false;

            OnSpecializationChanged(this, new(slot1, prevSlot1.Specialization, prevSlot2.Specialization));
        }

        public bool HasSpecialization(int specializationId)
        {
            foreach (var spec in Specializations)
            {
                if (spec.Value?.Specialization?.Id == specializationId) return true;
            }

            return false;
        }

        public bool HasSpecialization(Specialization specialization)
        {
            foreach (var spec in Specializations)
            {
                if (spec.Value?.Specialization == specialization) return true;
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

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

        }
    }
}
