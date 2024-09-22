using Kenedia.Modules.BuildsManager.DataModels;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.Core.DataModels;
using Kenedia.Modules.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kenedia.Modules.BuildsManager.Models.Templates
{
    public class SkillCollection : ObservableDictionary<SkillSlotType, Skill>
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
            foreach (SkillSlotType slot in Enum.GetValues(typeof(SkillSlotType)))
            {
                if (slot >= SkillSlotType.Heal)
                {
                    foreach (SkillSlotType state in new SkillSlotType[] { SkillSlotType.Active, SkillSlotType.Inactive })
                    {
                        foreach (SkillSlotType enviroment in new SkillSlotType[] { SkillSlotType.Terrestrial, SkillSlotType.Aquatic })
                        {
                            Add(state | enviroment | slot, new());
                        }
                    }
                }
            }
        }

        public ushort GetPaletteId(SkillSlotType slot)
        {
            return (ushort)(TryGetValue(slot, out Skill skill) && skill is not null ? skill.PaletteId : 0);
        }

        public bool HasSkill(Skill skill, SkillSlotType state_enviroment)
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

        public bool HasSkill(int skillid, SkillSlotType state_enviroment)
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

        public SkillSlotType GetSkillSlot(int skillid, SkillSlotType state_enviroment)
        {
            foreach (var s in this)
            {
                if (s.Key.HasFlag(state_enviroment))
                {
                    if (s.Value?.Id == skillid) return s.Key;
                }
            }

            return SkillSlotType.Utility_1;
        }

        public SkillSlotType GetSkillSlot(Skill skill, SkillSlotType state_enviroment)
        {
            foreach (var s in this)
            {
                if (s.Key.HasFlag(state_enviroment))
                {
                    if (s.Value == skill) return s.Key;
                }
            }

            return SkillSlotType.Utility_1;
        }

        public void SelectSkill(Skill skill, SkillSlotType targetSlot, Skill previousSkill = null)
        {
            SkillSlotType enviromentState = targetSlot.GetEnviromentState();

            if (HasSkill(skill, enviromentState))
            {
                var slot = GetSkillSlot(skill, enviromentState);
                this[slot] = previousSkill;
            }

            this[targetSlot] = skill;
        }

        public bool WipeInvalidRacialSkills(Races race)
        {
            bool wiped = false;
            List<int> invalidIds = new();
            foreach (Races r in Enum.GetValues(typeof(Races)))
            {
                if (r is not Races.None && r != race && BuildsManager.Data.Races.TryGetValue(r, out Race skillRace))
                {
                    invalidIds.AddRange(skillRace?.Skills.Select(e => e.Value.Id));
                }
            }

            foreach (var key in Keys.ToList())
            {
                if (invalidIds.Contains(this[key]?.Id ?? 0))
                {
                    wiped = true;
                    this[key] = default;
                }
            }

            return wiped;
        }

        public bool WipeSkills(Races? race)
        {
            bool wiped = false;
            var racials = race == null ? null : BuildsManager.Data?.Races?[(Races)race]?.Skills;

            foreach (var key in Keys.ToList())
            {
                if (race == null || (racials is not null && !racials.ContainsKey(this[key]?.Id ?? 0)))
                {
                    wiped = true;
                    this[key] = default;
                }
            }

            return wiped;
        }

        internal void SetSkill(TemplateSlots templateSlots, Skill skill)
        {
            this[FromTemplateSlot(templateSlots)] = skill;
        }

        private SkillSlotType FromTemplateSlot(TemplateSlots slot)
        {
            return slot switch
            {
                TemplateSlots.TerrestrialHeal => SkillSlotType.Active | SkillSlotType.Terrestrial | SkillSlotType.Heal,
                TemplateSlots.TerrestrialUtility1 => SkillSlotType.Active | SkillSlotType.Terrestrial | SkillSlotType.Utility_1,
                TemplateSlots.TerrestrialUtility2 => SkillSlotType.Active | SkillSlotType.Terrestrial | SkillSlotType.Utility_2,
                TemplateSlots.TerrestrialUtility3 => SkillSlotType.Active | SkillSlotType.Terrestrial | SkillSlotType.Utility_3,
                TemplateSlots.TerrestrialElite => SkillSlotType.Active | SkillSlotType.Terrestrial | SkillSlotType.Elite,

                TemplateSlots.AquaticHeal => SkillSlotType.Active | SkillSlotType.Aquatic | SkillSlotType.Heal,
                TemplateSlots.AquaticUtility1 => SkillSlotType.Active | SkillSlotType.Aquatic | SkillSlotType.Utility_1,
                TemplateSlots.AquaticUtility2 => SkillSlotType.Active | SkillSlotType.Aquatic | SkillSlotType.Utility_2,
                TemplateSlots.AquaticUtility3 => SkillSlotType.Active | SkillSlotType.Aquatic | SkillSlotType.Utility_3,
                TemplateSlots.AquaticElite => SkillSlotType.Active | SkillSlotType.Aquatic | SkillSlotType.Elite,

                TemplateSlots.InactiveTerrestrialHeal => SkillSlotType.Inactive | SkillSlotType.Aquatic | SkillSlotType.Heal,
                TemplateSlots.InactiveTerrestrialUtility1 => SkillSlotType.Inactive | SkillSlotType.Aquatic | SkillSlotType.Utility_1,
                TemplateSlots.InactiveTerrestrialUtility2 => SkillSlotType.Inactive | SkillSlotType.Aquatic | SkillSlotType.Utility_2,
                TemplateSlots.InactiveTerrestrialUtility3 => SkillSlotType.Inactive | SkillSlotType.Aquatic | SkillSlotType.Utility_3,
                TemplateSlots.InactiveTerrestrialElite => SkillSlotType.Inactive | SkillSlotType.Aquatic | SkillSlotType.Elite,

                TemplateSlots.InactiveAquaticHeal => SkillSlotType.Inactive | SkillSlotType.Terrestrial | SkillSlotType.Heal,
                TemplateSlots.InactiveAquaticUtility1 => SkillSlotType.Inactive | SkillSlotType.Terrestrial | SkillSlotType.Utility_1,
                TemplateSlots.InactiveAquaticUtility2 => SkillSlotType.Inactive | SkillSlotType.Terrestrial | SkillSlotType.Utility_2,
                TemplateSlots.InactiveAquaticUtility3 => SkillSlotType.Inactive | SkillSlotType.Terrestrial | SkillSlotType.Utility_3,
                TemplateSlots.InactiveAquaticElite => SkillSlotType.Inactive | SkillSlotType.Terrestrial | SkillSlotType.Elite,
            };
        }
    }
}
