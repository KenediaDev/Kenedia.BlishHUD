using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Kenedia.Modules.BuildsManager.Models.Templates
{
    public class SpecializationCollection : Dictionary<SpecializationSlot, BuildSpecialization>
    {
        public SpecializationCollection()
        {
            foreach (SpecializationSlot e in Enum.GetValues(typeof(SpecializationSlot)))
            {
                Add(e, null);
            }
        }

        public byte GetSpecializationByte(SpecializationSlot slot)
        {
            byte id = (byte)(TryGetValue(slot, out BuildSpecialization specialization) && specialization != null ? specialization.Specialization?.Id : 0);
            return id;
        }

        public byte GetTraitByte(TraitTier traitSlot, BuildSpecialization buildSpecialization)
        {
            int? order = buildSpecialization.Traits[traitSlot]?.Order;

            return (byte)(order != null ? order + 1 : 0);
        }
    }
}
