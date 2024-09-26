using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.DataModels;
using System;

namespace Kenedia.Modules.BuildsManager.Models
{

    public delegate void AttunementChangedEventHandler(object sender, AttunementChangedEventArgs e);

    public delegate void PetChangedEventHandler(object sender, PetChangedEventArgs e);

    public delegate void SkillChangedEventHandler(object sender, SkillChangedEventArgs e);

    public delegate void TraitChangedEventHandler(object sender, TraitChangedEventArgs e);

    public delegate void SpecializationChangedEventHandler(object sender, SpecializationChangedEventArgs e);

    public delegate void TemplateSlotChangedEventHandler(object sender, TemplateSlotChangedEventArgs e);

    public delegate void LegendChangedEventHandler(object sender, LegendChangedEventArgs e);

    public  class AttunementChangedEventArgs : EventArgs
    {
        public AttunementChangedEventArgs(AttunementSlotType slot, AttunementType? attunement, AttunementType? oldAttunement = default)
        {
            Slot = slot;
            Attunement = attunement;
            OldAttunement = oldAttunement;
        }

        public AttunementSlotType Slot { get; set; }

        public AttunementType? Attunement { get; set; }

        public AttunementType? OldAttunement { get; set; }
    }

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

    public class PetChangedEventArgs : EventArgs
    {
        public PetChangedEventArgs(PetSlotType slot, Pet? pet, Pet? oldPet = default)
        {
            Slot = slot;
            Pet = pet;
            OldPet = oldPet;
        }

        public PetSlotType Slot { get; set; }

        public Pet? Pet { get; set; }

        public Pet? OldPet { get; set; }
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
