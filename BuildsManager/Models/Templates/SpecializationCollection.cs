using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.Core.Models;
using System;
using System.ComponentModel;
using System.Diagnostics;
using static Kenedia.Modules.BuildsManager.Models.Templates.BuildSpecialization;

namespace Kenedia.Modules.BuildsManager.Models.Templates
{
    public class SpecializationCollection : NotifyPropertyChangedDictionary<SpecializationSlot, BuildSpecialization>
    {
        public SpecializationCollection()
        {
            ItemChanged += SpecializationCollection_ItemChanged;
            ItemPropertyChanged += SpecializationCollection_ItemPropertyChanged;

            foreach (SpecializationSlot e in Enum.GetValues(typeof(SpecializationSlot)))
            {
                Add(e, new() { SpecializationSlot = e });
            }
        }

        private void SpecializationCollection_ItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            TraitsChanged?.Invoke(sender, e);
        }

        private void SpecializationCollection_ItemChanged(object sender, DictionaryItemChangedEventArgs<SpecializationSlot, BuildSpecialization> e)
        {
            if (e.Key == SpecializationSlot.Line_3)
            {
                if(e.OldValue != null) e.OldValue.SpecChanged -= SpecializationCollection_SpecChanged;
                if (e.NewValue != null) e.NewValue.SpecChanged += SpecializationCollection_SpecChanged;
            }
        }

        private void SpecializationCollection_SpecChanged(object sender, SpecializationChangedEventArgs e)
        {
            if (e.SpecializationSlot == SpecializationSlot.Line_3)
            {
                EliteSpecChanged?.Invoke(this, e);
            }
        }

        public event EventHandler<SpecializationChangedEventArgs> EliteSpecChanged;
        public event EventHandler<PropertyChangedEventArgs> TraitsChanged;

        public byte GetSpecializationByte(SpecializationSlot slot)
        {
            byte id = (byte)(TryGetValue(slot, out BuildSpecialization specialization) && specialization != null && specialization.Specialization != null ? specialization.Specialization?.Id : 0);
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
            foreach (var buildspec in this)
            {
                buildspec.Value.Specialization = null;
                buildspec.Value.Traits.Wipe();
            }
        }
    }
}
