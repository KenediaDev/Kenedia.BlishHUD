using Kenedia.Modules.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;

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
        }

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
    }
}
