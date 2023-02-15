using Blish_HUD.Gw2Mumble;
using Blish_HUD;
using Gw2Sharp.ChatLinks;
using Gw2Sharp.Models;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using System.Diagnostics;
using System.Linq;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Kenedia.Modules.Core.Utility;

namespace Kenedia.Modules.BuildsManager.Models.Templates
{
    public class BuildTemplate : IDisposable
    {
        private bool _disposed = false;
        private ProfessionType _profession;

        public BuildTemplate()
        {
            PlayerCharacter player = GameService.Gw2Mumble.PlayerCharacter;
            Profession = player != null ? player.Profession : ProfessionType.Guardian;

            TerrestrialSkills.CollectionChanged += CollectionChanged;
            InactiveTerrestrialSkills.CollectionChanged += CollectionChanged;
            AquaticSkills.CollectionChanged += CollectionChanged;
            InactiveAquaticSkills.CollectionChanged += CollectionChanged;
            TerrestrialLegends.CollectionChanged += CollectionChanged;
            AquaticLegends.CollectionChanged += CollectionChanged;
            Pets.CollectionChanged += CollectionChanged;
            Specializations.CollectionChanged += CollectionChanged;
            Specializations.ItemChanged += CollectionChanged;
        }

        public BuildTemplate(string buildCode) : this()
        {
            LoadFromCode(buildCode);
        }

        public event PropertyChangedEventHandler Changed;

        public ProfessionType Profession { get => _profession; set => Common.SetProperty(ref _profession, value, Changed); }

        public SkillCollection TerrestrialSkills { get; } = new();

        public SkillCollection InactiveTerrestrialSkills { get; } = new();

        public SkillCollection AquaticSkills { get; } = new();

        public SkillCollection InactiveAquaticSkills { get; } = new();

        public LegendCollection TerrestrialLegends { get; } = new();

        public LegendCollection AquaticLegends { get; } = new();

        public PetCollection Pets { get; } = new();

        public SpecializationCollection Specializations { get; } = new();

        public string ParseBuildCode()
        {
            BuildChatLink build = new()
            {
                Profession = Profession,
                RevenantActiveTerrestrialLegend = TerrestrialLegends.GetLegendByte(LegendSlot.Active),
                RevenantInactiveTerrestrialLegend = TerrestrialLegends.GetLegendByte(LegendSlot.Inactive),
                RevenantInactiveTerrestrialUtility1SkillPaletteId = InactiveTerrestrialSkills.GetPaletteId(BuildSkillSlot.Utility_1),
                RevenantInactiveTerrestrialUtility2SkillPaletteId = InactiveTerrestrialSkills.GetPaletteId(BuildSkillSlot.Utility_2),
                RevenantInactiveTerrestrialUtility3SkillPaletteId = InactiveTerrestrialSkills.GetPaletteId(BuildSkillSlot.Utility_3),

                RevenantActiveAquaticLegend = AquaticLegends.GetLegendByte(LegendSlot.Active),
                RevenantInactiveAquaticLegend = AquaticLegends.GetLegendByte(LegendSlot.Inactive),
                RevenantInactiveAquaticUtility1SkillPaletteId = InactiveAquaticSkills.GetPaletteId(BuildSkillSlot.Utility_1),
                RevenantInactiveAquaticUtility2SkillPaletteId = InactiveAquaticSkills.GetPaletteId(BuildSkillSlot.Utility_2),
                RevenantInactiveAquaticUtility3SkillPaletteId = InactiveAquaticSkills.GetPaletteId(BuildSkillSlot.Utility_3),

                TerrestrialHealingSkillPaletteId = TerrestrialSkills.GetPaletteId(BuildSkillSlot.Heal),
                TerrestrialUtility1SkillPaletteId = TerrestrialSkills.GetPaletteId(BuildSkillSlot.Utility_1),
                TerrestrialUtility2SkillPaletteId = TerrestrialSkills.GetPaletteId(BuildSkillSlot.Utility_2),
                TerrestrialUtility3SkillPaletteId = TerrestrialSkills.GetPaletteId(BuildSkillSlot.Utility_3),
                TerrestrialEliteSkillPaletteId = TerrestrialSkills.GetPaletteId(BuildSkillSlot.Elite),

                AquaticHealingSkillPaletteId = AquaticSkills.GetPaletteId(BuildSkillSlot.Heal),
                AquaticUtility1SkillPaletteId = AquaticSkills.GetPaletteId(BuildSkillSlot.Utility_1),
                AquaticUtility2SkillPaletteId = AquaticSkills.GetPaletteId(BuildSkillSlot.Utility_2),
                AquaticUtility3SkillPaletteId = AquaticSkills.GetPaletteId(BuildSkillSlot.Utility_3),
                AquaticEliteSkillPaletteId = AquaticSkills.GetPaletteId(BuildSkillSlot.Elite),

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

            byte[] bytes = build.ToArray();
            build.Parse(bytes);

            return build.ToString(); ;
        }

        public void LoadFromCode(string code)
        {
            BuildChatLink build = new();

            if (Gw2ChatLink.TryParse(code, out IGw2ChatLink chatlink))
            {
                build.Parse(chatlink.ToArray());

                Profession = build.Profession;

                if (Profession == ProfessionType.Revenant)
                {
                    TerrestrialLegends[LegendSlot.Active] = Legend.FromByte(build.RevenantActiveTerrestrialLegend);
                    TerrestrialLegends[LegendSlot.Inactive] = Legend.FromByte(build.RevenantInactiveTerrestrialLegend);
                    InactiveTerrestrialSkills[BuildSkillSlot.Heal] = Legend.FromByte(build.RevenantActiveAquaticLegend)?.Heal;
                    InactiveTerrestrialSkills[BuildSkillSlot.Utility_1] = Skill.FromUShort(build.RevenantInactiveAquaticUtility1SkillPaletteId, build.Profession);
                    InactiveTerrestrialSkills[BuildSkillSlot.Utility_2] = Skill.FromUShort(build.RevenantInactiveAquaticUtility2SkillPaletteId, build.Profession);
                    InactiveTerrestrialSkills[BuildSkillSlot.Utility_3] = Skill.FromUShort(build.RevenantInactiveAquaticUtility3SkillPaletteId, build.Profession);
                    InactiveTerrestrialSkills[BuildSkillSlot.Elite] = Legend.FromByte(build.RevenantActiveAquaticLegend)?.Elite;

                    AquaticLegends[LegendSlot.Active] = Legend.FromByte(build.RevenantActiveAquaticLegend);
                    AquaticLegends[LegendSlot.Inactive] = Legend.FromByte(build.RevenantInactiveAquaticLegend);
                    InactiveAquaticSkills[BuildSkillSlot.Heal] = Legend.FromByte(build.RevenantActiveAquaticLegend)?.Heal;
                    InactiveAquaticSkills[BuildSkillSlot.Utility_1] = Skill.FromUShort(build.RevenantInactiveAquaticUtility1SkillPaletteId, build.Profession);
                    InactiveAquaticSkills[BuildSkillSlot.Utility_2] = Skill.FromUShort(build.RevenantInactiveAquaticUtility2SkillPaletteId, build.Profession);
                    InactiveAquaticSkills[BuildSkillSlot.Utility_3] = Skill.FromUShort(build.RevenantInactiveAquaticUtility3SkillPaletteId, build.Profession);
                    InactiveAquaticSkills[BuildSkillSlot.Elite] = Legend.FromByte(build.RevenantActiveAquaticLegend)?.Elite;
                }

                TerrestrialSkills[BuildSkillSlot.Heal] = Skill.FromUShort(build.TerrestrialHealingSkillPaletteId, build.Profession);
                TerrestrialSkills[BuildSkillSlot.Utility_1] = Skill.FromUShort(build.TerrestrialUtility1SkillPaletteId, build.Profession);
                TerrestrialSkills[BuildSkillSlot.Utility_2] = Skill.FromUShort(build.TerrestrialUtility2SkillPaletteId, build.Profession);
                TerrestrialSkills[BuildSkillSlot.Utility_3] = Skill.FromUShort(build.TerrestrialUtility3SkillPaletteId, build.Profession);
                TerrestrialSkills[BuildSkillSlot.Elite] = Skill.FromUShort(build.TerrestrialEliteSkillPaletteId, build.Profession);

                AquaticSkills[BuildSkillSlot.Heal] = Skill.FromUShort(build.AquaticHealingSkillPaletteId, build.Profession);
                AquaticSkills[BuildSkillSlot.Utility_1] = Skill.FromUShort(build.AquaticUtility1SkillPaletteId, build.Profession);
                AquaticSkills[BuildSkillSlot.Utility_2] = Skill.FromUShort(build.AquaticUtility2SkillPaletteId, build.Profession);
                AquaticSkills[BuildSkillSlot.Utility_3] = Skill.FromUShort(build.AquaticUtility3SkillPaletteId, build.Profession);
                AquaticSkills[BuildSkillSlot.Elite] = Skill.FromUShort(build.AquaticEliteSkillPaletteId, build.Profession);

                Specializations[SpecializationSlot.Line_1] = BuildSpecialization.FromByte(build.Specialization1Id, build.Profession);
                Specializations[SpecializationSlot.Line_1].Traits[TraitTier.Adept] = Trait.FromByte(build.Specialization1Trait1Index, Specializations[SpecializationSlot.Line_1]?.Specialization, TraitTier.Adept);
                Specializations[SpecializationSlot.Line_1].Traits[TraitTier.Master] = Trait.FromByte(build.Specialization1Trait2Index, Specializations[SpecializationSlot.Line_1]?.Specialization, TraitTier.Master);
                Specializations[SpecializationSlot.Line_1].Traits[TraitTier.GrandMaster] = Trait.FromByte(build.Specialization1Trait3Index, Specializations[SpecializationSlot.Line_1]?.Specialization, TraitTier.GrandMaster);

                Specializations[SpecializationSlot.Line_2] = BuildSpecialization.FromByte(build.Specialization2Id, build.Profession);
                Specializations[SpecializationSlot.Line_2].Traits[TraitTier.Adept] = Trait.FromByte(build.Specialization2Trait1Index, Specializations[SpecializationSlot.Line_2]?.Specialization, TraitTier.Adept);
                Specializations[SpecializationSlot.Line_2].Traits[TraitTier.Master] = Trait.FromByte(build.Specialization2Trait2Index, Specializations[SpecializationSlot.Line_2]?.Specialization, TraitTier.Master);
                Specializations[SpecializationSlot.Line_2].Traits[TraitTier.GrandMaster] = Trait.FromByte(build.Specialization2Trait3Index, Specializations[SpecializationSlot.Line_2]?.Specialization, TraitTier.GrandMaster);

                Specializations[SpecializationSlot.Line_3] = BuildSpecialization.FromByte(build.Specialization3Id, build.Profession);
                Specializations[SpecializationSlot.Line_3].Traits[TraitTier.Adept] = Trait.FromByte(build.Specialization3Trait1Index, Specializations[SpecializationSlot.Line_3]?.Specialization, TraitTier.Adept);
                Specializations[SpecializationSlot.Line_3].Traits[TraitTier.Master] = Trait.FromByte(build.Specialization3Trait2Index, Specializations[SpecializationSlot.Line_3]?.Specialization, TraitTier.Master);
                Specializations[SpecializationSlot.Line_3].Traits[TraitTier.GrandMaster] = Trait.FromByte(build.Specialization3Trait3Index, Specializations[SpecializationSlot.Line_3]?.Specialization, TraitTier.GrandMaster);

                if (Profession == ProfessionType.Ranger)
                {
                    Pets[PetSlot.Terrestrial_1] = Pet.FromByte(build.RangerTerrestrialPet1Id);
                    Pets[PetSlot.Terrestrial_2] = Pet.FromByte(build.RangerTerrestrialPet2Id);
                    Pets[PetSlot.Aquatic_1] = Pet.FromByte(build.RangerAquaticPet1Id);
                    Pets[PetSlot.Aquatic_2] = Pet.FromByte(build.RangerAquaticPet2Id);
                }
            }
        }

        public void SwapLegends()
        {
            if (Profession == ProfessionType.Revenant)
            {
                var newActiveTerrestrialLegend = TerrestrialLegends[LegendSlot.Inactive];
                var newActiveTerrestrialSkills = InactiveTerrestrialSkills.ToDictionary(e => e.Key, e => e.Value);

                TerrestrialLegends[LegendSlot.Inactive] = TerrestrialLegends[LegendSlot.Active];
                TerrestrialLegends[LegendSlot.Active] = newActiveTerrestrialLegend;

                InactiveTerrestrialSkills[BuildSkillSlot.Heal] = TerrestrialSkills[BuildSkillSlot.Heal];
                InactiveTerrestrialSkills[BuildSkillSlot.Utility_1] = TerrestrialSkills[BuildSkillSlot.Utility_1];
                InactiveTerrestrialSkills[BuildSkillSlot.Utility_2] = TerrestrialSkills[BuildSkillSlot.Utility_2];
                InactiveTerrestrialSkills[BuildSkillSlot.Utility_3] = TerrestrialSkills[BuildSkillSlot.Utility_3];
                InactiveTerrestrialSkills[BuildSkillSlot.Elite] = TerrestrialSkills[BuildSkillSlot.Elite];

                TerrestrialSkills[BuildSkillSlot.Heal] = newActiveTerrestrialSkills[BuildSkillSlot.Heal];
                TerrestrialSkills[BuildSkillSlot.Utility_1] = newActiveTerrestrialSkills[BuildSkillSlot.Utility_1];
                TerrestrialSkills[BuildSkillSlot.Utility_2] = newActiveTerrestrialSkills[BuildSkillSlot.Utility_2];
                TerrestrialSkills[BuildSkillSlot.Utility_3] = newActiveTerrestrialSkills[BuildSkillSlot.Utility_3];
                TerrestrialSkills[BuildSkillSlot.Elite] = newActiveTerrestrialSkills[BuildSkillSlot.Elite];

                var newActiveAquaticLegend = AquaticLegends[LegendSlot.Inactive];
                var newActiveAquaticSkills = InactiveAquaticSkills.ToDictionary(e => e.Key, e => e.Value);

                AquaticLegends[LegendSlot.Inactive] = AquaticLegends[LegendSlot.Active];
                AquaticLegends[LegendSlot.Active] = newActiveAquaticLegend;

                InactiveAquaticSkills[BuildSkillSlot.Heal] = AquaticSkills[BuildSkillSlot.Heal];
                InactiveAquaticSkills[BuildSkillSlot.Utility_1] = AquaticSkills[BuildSkillSlot.Utility_1];
                InactiveAquaticSkills[BuildSkillSlot.Utility_2] = AquaticSkills[BuildSkillSlot.Utility_2];
                InactiveAquaticSkills[BuildSkillSlot.Utility_3] = AquaticSkills[BuildSkillSlot.Utility_3];
                InactiveAquaticSkills[BuildSkillSlot.Elite] = AquaticSkills[BuildSkillSlot.Elite];

                AquaticSkills[BuildSkillSlot.Heal] = newActiveAquaticSkills[BuildSkillSlot.Heal];
                AquaticSkills[BuildSkillSlot.Utility_1] = newActiveAquaticSkills[BuildSkillSlot.Utility_1];
                AquaticSkills[BuildSkillSlot.Utility_2] = newActiveAquaticSkills[BuildSkillSlot.Utility_2];
                AquaticSkills[BuildSkillSlot.Utility_3] = newActiveAquaticSkills[BuildSkillSlot.Utility_3];
                AquaticSkills[BuildSkillSlot.Elite] = newActiveAquaticSkills[BuildSkillSlot.Elite];
            }
        }

        public bool HasSpecialization(int specializationId)
        {
            foreach (var spec in Specializations)
            {
                if (spec.Value.Specialization != null && spec.Value.Specialization.Id == specializationId) return true;
            }

            return false;
        }
                
        public bool HasSpecialization(Specialization specialization)
        {
            foreach (var spec in Specializations)
            {
                if (spec.Value != null && spec.Value.Specialization == specialization) return true;
            }

            return false;
        }

        public SpecializationSlot? GetSpecializationSlot(Specialization specialization)
        {
            foreach (var spec in Specializations)
            {
                if (spec.Value.Specialization == specialization) return spec.Key;
            }

            return null;
        }

        private void CollectionChanged(object sender, PropertyChangedEventArgs e)
        {
            Changed?.Invoke(sender, e);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            TerrestrialSkills.CollectionChanged -= CollectionChanged;
            InactiveTerrestrialSkills.CollectionChanged -= CollectionChanged;
            AquaticSkills.CollectionChanged -= CollectionChanged;
            InactiveAquaticSkills.CollectionChanged -= CollectionChanged;
            TerrestrialLegends.CollectionChanged -= CollectionChanged;
            AquaticLegends.CollectionChanged -= CollectionChanged;
            Pets.CollectionChanged -= CollectionChanged;
            Specializations.ItemChanged -= CollectionChanged;
        }
    }
}
