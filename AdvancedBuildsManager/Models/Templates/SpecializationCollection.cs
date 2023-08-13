using Kenedia.Modules.Core.Models;
using System;

namespace Kenedia.Modules.AdvancedBuildsManager.Models.Templates
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
            byte id = (byte)(TryGetValue(slot, out BuildSpecialization specialization) && specialization is not null  && specialization.Specialization  is not null ? specialization.Specialization?.Id : 0);
            return id;
        }

        public byte GetTraitByte(TraitTier traitSlot, BuildSpecialization buildSpecialization)
        {
            if (buildSpecialization is not null)
            {
                int? order = buildSpecialization.Traits[traitSlot]?.Order;

                return (byte)(order is not null ? order + 1 : 0);
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
