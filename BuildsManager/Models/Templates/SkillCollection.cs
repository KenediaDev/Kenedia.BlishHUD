using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.Core.Models;
using System;

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
    }
}
