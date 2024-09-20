using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.BuildsManager.Models.Templates;
using System;

namespace Kenedia.Modules.BuildsManager.Models
{
    public class SpecializationChangedEventArgs : EventArgs
    {
        public SpecializationChangedEventArgs(SpecializationSlotType slot, Specialization? specialization, Specialization? oldSpecialization = default)
        {
            Slot = slot;
            Specialization = specialization;
            OldSpecialization = oldSpecialization;
        }

        public SpecializationSlotType Slot { get; set; }

        public Specialization? Specialization { get; set; }

        public Specialization? OldSpecialization { get; set; }
    }

    public class SkillChangedEventArgs : EventArgs
    {
        public SkillChangedEventArgs(SkillSlotType slot, Skill? skill, Skill? oldSkill = default)
        {
            Slot = slot;
            Skill = skill;
            OldSkill = oldSkill;
        }

        public SkillSlotType Slot { get; set; }

        public Skill? Skill { get; set; }

        public Skill? OldSkill { get; set; }
    }

    public class TraitChangedEventArgs : EventArgs
    {
        public TraitChangedEventArgs(SpecializationSlotType specSlot, TraitTierType slot, Trait? trait, Trait? oldTrait = default)
        {
            SpecSlot = specSlot;
            Slot = slot;
            Trait = trait;
            OldTrait = oldTrait;
        }

        public SpecializationSlotType SpecSlot { get; set; }

        public TraitTierType Slot { get; set; }

        public Trait? Trait { get; set; }

        public Trait? OldTrait { get; set; }
    }
}
