using Blish_HUD.Gw2Mumble;
using Blish_HUD;
using Gw2Sharp.ChatLinks;
using Gw2Sharp.Models;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using System.Linq;
using System;
using System.ComponentModel;
using Kenedia.Modules.Core.Utility;
using System.Collections.Generic;
using System.Diagnostics;
using Kenedia.Modules.Core.Models;
using System.Threading.Tasks;
using System.Threading;

namespace Kenedia.Modules.BuildsManager.Models.Templates
{
    public class BuildTemplate : IDisposable
    {
        private bool _disposed = false;
        private bool _loading = false;
        private ProfessionType _profession;
        private CancellationTokenSource _eventCancellationTokenSource;

        public BuildTemplate()
        {
            PlayerCharacter player = GameService.Gw2Mumble.PlayerCharacter;
            Profession = player != null ? player.Profession : ProfessionType.Guardian;

            TerrestrialSkills.CollectionChanged += CollectionChanged;
            InactiveTerrestrialSkills.CollectionChanged += CollectionChanged;
            AquaticSkills.CollectionChanged += CollectionChanged;
            InactiveAquaticSkills.CollectionChanged += CollectionChanged;
            Legends.CollectionChanged += CollectionChanged;
            Pets.CollectionChanged += CollectionChanged;
            Specializations.CollectionChanged += CollectionChanged;
            Specializations.ItemChanged += CollectionChanged;
        }

        public BuildTemplate(string buildCode) : this()
        {
            LoadFromCode(buildCode);
        }

        public event PropertyChangedEventHandler Changed;

        public ProfessionType Profession { get => _profession; set => Common.SetProperty(ref _profession, value, OnChanged); }

        public Dictionary<BuildSkillSlot, Skill> Test { get; } = new();

        public ObservableDictionary<BuildSkillSlot, Skill> ObservableTest{ get; } = new();

        public SkillCollection SkillCollectionTest { get; } = new();

        public SkillCollection TerrestrialSkills { get; } = new();

        public SkillCollection InactiveTerrestrialSkills { get; } = new();

        public SkillCollection AquaticSkills { get; } = new();

        public SkillCollection InactiveAquaticSkills { get; } = new();

        public LegendCollection Legends { get; } = new();

        public PetCollection Pets { get; } = new();

        public SpecializationCollection Specializations { get; } = new();

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

            if (Profession == ProfessionType.Revenant)
            {
                build.RevenantActiveTerrestrialLegend = Legends.GetLegendByte(LegendSlot.TerrestrialActive);
                build.RevenantInactiveTerrestrialLegend = Legends.GetLegendByte(LegendSlot.TerrestrialInactive);
                build.RevenantInactiveTerrestrialUtility1SkillPaletteId = InactiveTerrestrialSkills.GetPaletteId(BuildSkillSlot.Utility_1);
                build.RevenantInactiveTerrestrialUtility2SkillPaletteId = InactiveTerrestrialSkills.GetPaletteId(BuildSkillSlot.Utility_2);
                build.RevenantInactiveTerrestrialUtility3SkillPaletteId = InactiveTerrestrialSkills.GetPaletteId(BuildSkillSlot.Utility_3);

                build.RevenantActiveAquaticLegend = Legends.GetLegendByte(LegendSlot.AquaticActive);
                build.RevenantInactiveAquaticLegend = Legends.GetLegendByte(LegendSlot.AquaticInactive);
                build.RevenantInactiveAquaticUtility1SkillPaletteId = InactiveAquaticSkills.GetPaletteId(BuildSkillSlot.Utility_1);
                build.RevenantInactiveAquaticUtility2SkillPaletteId = InactiveAquaticSkills.GetPaletteId(BuildSkillSlot.Utility_2);
                build.RevenantInactiveAquaticUtility3SkillPaletteId = InactiveAquaticSkills.GetPaletteId(BuildSkillSlot.Utility_3);

                build.TerrestrialHealingSkillPaletteId = TerrestrialSkills.GetPaletteId(BuildSkillSlot.Heal);
                build.TerrestrialUtility1SkillPaletteId = TerrestrialSkills.GetPaletteId(BuildSkillSlot.Utility_1);
                build.TerrestrialUtility2SkillPaletteId = TerrestrialSkills.GetPaletteId(BuildSkillSlot.Utility_2);
                build.TerrestrialUtility3SkillPaletteId = TerrestrialSkills.GetPaletteId(BuildSkillSlot.Utility_3);
                build.TerrestrialEliteSkillPaletteId = TerrestrialSkills.GetPaletteId(BuildSkillSlot.Elite);

                build.AquaticHealingSkillPaletteId = AquaticSkills.GetPaletteId(BuildSkillSlot.Heal);
                build.AquaticUtility1SkillPaletteId = AquaticSkills.GetPaletteId(BuildSkillSlot.Utility_1);
                build.AquaticUtility2SkillPaletteId = AquaticSkills.GetPaletteId(BuildSkillSlot.Utility_2);
                build.AquaticUtility3SkillPaletteId = AquaticSkills.GetPaletteId(BuildSkillSlot.Utility_3);
                build.AquaticEliteSkillPaletteId = AquaticSkills.GetPaletteId(BuildSkillSlot.Elite);
            }
            else
            {
                build.TerrestrialHealingSkillPaletteId = TerrestrialSkills.GetPaletteId(BuildSkillSlot.Heal);
                build.TerrestrialUtility1SkillPaletteId = TerrestrialSkills.GetPaletteId(BuildSkillSlot.Utility_1);
                build.TerrestrialUtility2SkillPaletteId = TerrestrialSkills.GetPaletteId(BuildSkillSlot.Utility_2);
                build.TerrestrialUtility3SkillPaletteId = TerrestrialSkills.GetPaletteId(BuildSkillSlot.Utility_3);
                build.TerrestrialEliteSkillPaletteId = TerrestrialSkills.GetPaletteId(BuildSkillSlot.Elite);

                build.AquaticHealingSkillPaletteId = AquaticSkills.GetPaletteId(BuildSkillSlot.Heal);
                build.AquaticUtility1SkillPaletteId = AquaticSkills.GetPaletteId(BuildSkillSlot.Utility_1);
                build.AquaticUtility2SkillPaletteId = AquaticSkills.GetPaletteId(BuildSkillSlot.Utility_2);
                build.AquaticUtility3SkillPaletteId = AquaticSkills.GetPaletteId(BuildSkillSlot.Utility_3);
                build.AquaticEliteSkillPaletteId = AquaticSkills.GetPaletteId(BuildSkillSlot.Elite);
            }

            byte[] bytes = build.ToArray();
            build.Parse(bytes);

            return build.ToString(); ;
        }

        public void LoadFromCode(string code)
        {
            BuildChatLink build = new();
            _loading = true;

            if (Gw2ChatLink.TryParse(code, out IGw2ChatLink chatlink))
            {
                build.Parse(chatlink.ToArray());
                Profession = build.Profession;

                Specializations[SpecializationSlot.Line_1] = BuildSpecialization.FromByte(build.Specialization1Id, build.Profession);
                if (Specializations[SpecializationSlot.Line_1] != null)
                {
                    Specializations[SpecializationSlot.Line_1].Traits[TraitTier.Adept] = Trait.FromByte(build.Specialization1Trait1Index, Specializations[SpecializationSlot.Line_1]?.Specialization, TraitTier.Adept);
                    Specializations[SpecializationSlot.Line_1].Traits[TraitTier.Master] = Trait.FromByte(build.Specialization1Trait2Index, Specializations[SpecializationSlot.Line_1]?.Specialization, TraitTier.Master);
                    Specializations[SpecializationSlot.Line_1].Traits[TraitTier.GrandMaster] = Trait.FromByte(build.Specialization1Trait3Index, Specializations[SpecializationSlot.Line_1]?.Specialization, TraitTier.GrandMaster);
                }

                Specializations[SpecializationSlot.Line_2] = BuildSpecialization.FromByte(build.Specialization2Id, build.Profession);
                if (Specializations[SpecializationSlot.Line_2] != null)
                {
                    Specializations[SpecializationSlot.Line_2].Traits[TraitTier.Adept] = Trait.FromByte(build.Specialization2Trait1Index, Specializations[SpecializationSlot.Line_2]?.Specialization, TraitTier.Adept);
                    Specializations[SpecializationSlot.Line_2].Traits[TraitTier.Master] = Trait.FromByte(build.Specialization2Trait2Index, Specializations[SpecializationSlot.Line_2]?.Specialization, TraitTier.Master);
                    Specializations[SpecializationSlot.Line_2].Traits[TraitTier.GrandMaster] = Trait.FromByte(build.Specialization2Trait3Index, Specializations[SpecializationSlot.Line_2]?.Specialization, TraitTier.GrandMaster);
                }

                Specializations[SpecializationSlot.Line_3] = BuildSpecialization.FromByte(build.Specialization3Id, build.Profession);
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
                    InactiveTerrestrialSkills[BuildSkillSlot.Heal] = Legend.FromByte(build.RevenantInactiveTerrestrialLegend)?.Heal;
                    InactiveTerrestrialSkills[BuildSkillSlot.Utility_1] = Legend.SkillFromUShort(build.RevenantInactiveTerrestrialUtility1SkillPaletteId, Legends[LegendSlot.TerrestrialInactive]);
                    InactiveTerrestrialSkills[BuildSkillSlot.Utility_2] = Legend.SkillFromUShort(build.RevenantInactiveTerrestrialUtility2SkillPaletteId, Legends[LegendSlot.TerrestrialInactive]);
                    InactiveTerrestrialSkills[BuildSkillSlot.Utility_3] = Legend.SkillFromUShort(build.RevenantInactiveTerrestrialUtility3SkillPaletteId, Legends[LegendSlot.TerrestrialInactive]);
                    InactiveTerrestrialSkills[BuildSkillSlot.Elite] = Legend.FromByte(build.RevenantInactiveTerrestrialLegend)?.Elite;

                    Legends[LegendSlot.AquaticInactive] = Legend.FromByte(build.RevenantInactiveAquaticLegend);
                    InactiveAquaticSkills[BuildSkillSlot.Heal] = Legend.FromByte(build.RevenantInactiveAquaticLegend)?.Heal;
                    InactiveAquaticSkills[BuildSkillSlot.Utility_1] = Legend.SkillFromUShort(build.RevenantInactiveAquaticUtility1SkillPaletteId, Legends[LegendSlot.AquaticInactive]);
                    InactiveAquaticSkills[BuildSkillSlot.Utility_2] = Legend.SkillFromUShort(build.RevenantInactiveAquaticUtility2SkillPaletteId, Legends[LegendSlot.AquaticInactive]);
                    InactiveAquaticSkills[BuildSkillSlot.Utility_3] = Legend.SkillFromUShort(build.RevenantInactiveAquaticUtility3SkillPaletteId, Legends[LegendSlot.AquaticInactive]);
                    InactiveAquaticSkills[BuildSkillSlot.Elite] = Legend.FromByte(build.RevenantInactiveAquaticLegend)?.Elite;

                    Legends[LegendSlot.TerrestrialActive] = Legend.FromByte(build.RevenantActiveTerrestrialLegend);
                    TerrestrialSkills[BuildSkillSlot.Heal] = Legend.SkillFromUShort(build.TerrestrialHealingSkillPaletteId, Legends[LegendSlot.TerrestrialActive]);
                    TerrestrialSkills[BuildSkillSlot.Utility_1] = Legend.SkillFromUShort(build.TerrestrialUtility1SkillPaletteId, Legends[LegendSlot.TerrestrialActive]);
                    TerrestrialSkills[BuildSkillSlot.Utility_2] = Legend.SkillFromUShort(build.TerrestrialUtility2SkillPaletteId, Legends[LegendSlot.TerrestrialActive]);
                    TerrestrialSkills[BuildSkillSlot.Utility_3] = Legend.SkillFromUShort(build.TerrestrialUtility3SkillPaletteId, Legends[LegendSlot.TerrestrialActive]);
                    TerrestrialSkills[BuildSkillSlot.Elite] = Legend.SkillFromUShort(build.TerrestrialEliteSkillPaletteId, Legends[LegendSlot.TerrestrialActive]);

                    Legends[LegendSlot.AquaticActive] = Legend.FromByte(build.RevenantActiveAquaticLegend);
                    AquaticSkills[BuildSkillSlot.Heal] = Legend.SkillFromUShort(build.AquaticHealingSkillPaletteId, Legends[LegendSlot.AquaticActive]);
                    AquaticSkills[BuildSkillSlot.Utility_1] = Legend.SkillFromUShort(build.AquaticUtility1SkillPaletteId, Legends[LegendSlot.AquaticActive]);
                    AquaticSkills[BuildSkillSlot.Utility_2] = Legend.SkillFromUShort(build.AquaticUtility2SkillPaletteId, Legends[LegendSlot.AquaticActive]);
                    AquaticSkills[BuildSkillSlot.Utility_3] = Legend.SkillFromUShort(build.AquaticUtility3SkillPaletteId, Legends[LegendSlot.AquaticActive]);
                    AquaticSkills[BuildSkillSlot.Elite] = Legend.SkillFromUShort(build.AquaticEliteSkillPaletteId, Legends[LegendSlot.AquaticActive]);
                }
                else
                {
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
                }
            }

            _loading = false;
            OnChanged(this, null);
        }

        public void SwapLegends()
        {
            if (Profession == ProfessionType.Revenant)
            {
                var newActiveTerrestrialLegend = Legends[LegendSlot.TerrestrialInactive];
                var newActiveTerrestrialSkills = InactiveTerrestrialSkills.ToDictionary(e => e.Key, e => e.Value);

                Legends[LegendSlot.TerrestrialInactive] = Legends[LegendSlot.TerrestrialActive];
                Legends[LegendSlot.TerrestrialActive] = newActiveTerrestrialLegend;

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

                var newActiveAquaticLegend = Legends[LegendSlot.AquaticInactive];
                var newActiveAquaticSkills = InactiveAquaticSkills.ToDictionary(e => e.Key, e => e.Value);

                Legends[LegendSlot.AquaticInactive] = Legends[LegendSlot.AquaticActive];
                Legends[LegendSlot.AquaticActive] = newActiveAquaticLegend;

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

        public void SetLegend(Legend legend, LegendSlot slot)
        {
            Legends[slot] = legend;
            var skills = TerrestrialSkills;

            switch (slot)
            {
                case LegendSlot.AquaticActive:
                    skills = AquaticSkills;
                    break;
                case LegendSlot.TerrestrialActive:
                    skills = TerrestrialSkills;
                    break;
                case LegendSlot.TerrestrialInactive:
                    skills = InactiveTerrestrialSkills;
                    break;
                case LegendSlot.AquaticInactive:
                    skills = InactiveAquaticSkills;
                    break;
            };

            skills[BuildSkillSlot.Heal] = legend?.Heal;
            skills[BuildSkillSlot.Elite] = legend?.Elite;

            List<int?> paletteIds = new()
            {
                 skills[BuildSkillSlot.Utility_1]?.PaletteId,
                 skills[BuildSkillSlot.Utility_2]?.PaletteId,
                 skills[BuildSkillSlot.Utility_3]?.PaletteId,
            };

            List<int?> ids = new() { 4614, 4651, 4564 };

            for (int i = 1; i < 3 + 1; i++)
            {
                int? paletteId = paletteIds[i - 1];
                skills[(BuildSkillSlot)i] = paletteId != null ? Legend.SkillFromUShort((ushort)paletteId.Value, legend) : null;
            }

            var missingIds = ids.Except(paletteIds);
            for (int i = 1; i < 3 + 1; i++)
            {
                if (missingIds.Count() > i - 1)
                {
                    int? paletteId = missingIds.ElementAt(i - 1);
                    skills[(BuildSkillSlot)i] = paletteId != null ? Legend.SkillFromUShort((ushort)paletteId.Value, legend) : null;
                }
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
                if (spec.Value != null && spec.Value.Specialization == specialization) return spec.Key;
            }

            return null;
        }

        private async void OnChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_loading) return;
            Changed?.Invoke(sender, e);
        }

        private void CollectionChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_loading) return;

            Changed?.Invoke(sender, e);
        }

        private async Task TriggerChanged(object sender, PropertyChangedEventArgs e)
        {
            _eventCancellationTokenSource?.Cancel();
            _eventCancellationTokenSource = new();

            try
            {
                await Task.Delay(1, _eventCancellationTokenSource.Token);
                if (!_eventCancellationTokenSource.IsCancellationRequested)
                {
                    Debug.WriteLine($"{nameof(BuildTemplate)} Trigger Changed Event now.");
                    Changed?.Invoke(sender, e);
                }
            }
            catch (Exception)
            {

            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            TerrestrialSkills.CollectionChanged -= CollectionChanged;
            InactiveTerrestrialSkills.CollectionChanged -= CollectionChanged;
            AquaticSkills.CollectionChanged -= CollectionChanged;
            InactiveAquaticSkills.CollectionChanged -= CollectionChanged;
            Legends.CollectionChanged -= CollectionChanged;
            Pets.CollectionChanged -= CollectionChanged;
            Specializations.ItemChanged -= CollectionChanged;
        }
    }
}
