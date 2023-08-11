using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.Core.Models;
using System;
using System.ComponentModel;
using ProfessionType = Gw2Sharp.Models.ProfessionType;
using static Kenedia.Modules.BuildsManager.Models.Templates.BuildSpecialization;

namespace Kenedia.Modules.BuildsManager.Models.Templates
{
    public class SpecializationCollection : NotifyPropertyChangedDictionary<SpecializationSlotType, BuildSpecialization>
    {
        public SpecializationCollection()
        {
            ItemChanged += SpecializationCollection_ItemChanged;
            ItemPropertyChanged += SpecializationCollection_ItemPropertyChanged;

            foreach (SpecializationSlotType e in Enum.GetValues(typeof(SpecializationSlotType)))
            {
                Add(e, new() { SpecializationSlot = e });
            }
        }

        private void SpecializationCollection_ItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            TraitsChanged?.Invoke(sender, e);
        }

        private void SpecializationCollection_ItemChanged(object sender, DictionaryItemChangedEventArgs<SpecializationSlotType, BuildSpecialization> e)
        {
            if (e.Key == SpecializationSlotType.Line_3)
            {
                if(e.OldValue != null) e.OldValue.SpecChanged -= SpecializationCollection_SpecChanged;
                if (e.NewValue != null) e.NewValue.SpecChanged += SpecializationCollection_SpecChanged;
            }
        }

        private void SpecializationCollection_SpecChanged(object sender, SpecializationChangedEventArgs e)
        {
            if (e.SpecializationSlot == SpecializationSlotType.Line_3)
            {
                EliteSpecChanged?.Invoke(this, e);
            }
        }

        public event EventHandler<SpecializationChangedEventArgs> EliteSpecChanged;
        public event EventHandler<PropertyChangedEventArgs> TraitsChanged;

        public byte GetSpecializationByte(SpecializationSlotType slot)
        {
            byte id = (byte)(TryGetValue(slot, out BuildSpecialization specialization) && specialization != null && specialization.Specialization != null ? specialization.Specialization?.Id : 0);
            return id;
        }

        public byte GetTraitByte(TraitTierType traitSlot, BuildSpecialization buildSpecialization)
        {
            if (buildSpecialization != null)
            {
                int? order = buildSpecialization.Traits[traitSlot]?.Order;

                return (byte)(order != null ? order + 1 : 0);
            }

            return 0;
        }

        public override void Wipe()
        {
            foreach (var buildspec in this)
            {
                buildspec.Value.Specialization = null;
                buildspec.Value.Traits.Wipe();
            }
        }

        public void LoadFromCode(ProfessionType profession, SpecializationSlotType slot, byte specId, byte adept, byte master, byte grandMaster)
        {
            var specialization = this[slot].Specialization = Specialization.FromByte(specId, profession);
            this[slot].Traits[TraitTierType.Adept] = Trait.FromByte(adept, specialization, TraitTierType.Adept);
            this[slot].Traits[TraitTierType.Master] = Trait.FromByte(master, specialization, TraitTierType.Master);
            this[slot].Traits[TraitTierType.GrandMaster] = Trait.FromByte(grandMaster, specialization, TraitTierType.GrandMaster);
        }
    }
}
