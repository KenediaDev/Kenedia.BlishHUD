using Kenedia.Modules.Core.Models;
using System;

namespace Kenedia.Modules.BuildsManager.Models.Templates
{
    public class SpecializationCollection : DeepObservableDictionary<SpecializationSlot, BuildSpecialization>
    {
        public SpecializationCollection()
        {
            foreach (SpecializationSlot e in Enum.GetValues(typeof(SpecializationSlot)))
            {
                Add(e, new());
            }

            ValueChanged += SpecializationCollection_ValueChanged;
        }

        private void SpecializationCollection_ValueChanged(object sender, DictionaryItemChanged<SpecializationSlot, BuildSpecialization> e)
        {
            if(e.Key == SpecializationSlot.Line_3)
            {
                EliteSpecChanged?.Invoke(this, e);
            }
        }

        private void SpecializationCollection_ItemChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if((SpecializationSlot) sender == SpecializationSlot.Line_3)
            {
                this[SpecializationSlot.Line_3].SpecChanged += SpecializationCollection_SpecChanged;

            }
        }

        private void SpecializationCollection_SpecChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {

        }

        public event EventHandler EliteSpecChanged;

        public byte GetSpecializationByte(SpecializationSlot slot)
        {
            byte id = (byte)(TryGetValue(slot, out BuildSpecialization specialization) && specialization != null  && specialization.Specialization  != null ? specialization.Specialization?.Id : 0);
            return id;
        }

        public byte GetTraitByte(TraitTier traitSlot, BuildSpecialization buildSpecialization)
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
            foreach(var buildspec in this)
            {
                buildspec.Value.Specialization = null;
                buildspec.Value.Traits.Wipe();
            }
        }
    }
}
