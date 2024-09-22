using System;
using System.Collections.Generic;
using System.Collections;

namespace Kenedia.Modules.BuildsManager.Models.Templates
{
    public class Specializations : IEnumerable<BuildSpecialization>
    {
        public BuildSpecialization Specialization1 { get; } = new(SpecializationSlotType.Line_1);

        public BuildSpecialization Specialization2 { get; } = new(SpecializationSlotType.Line_2);

        public BuildSpecialization Specialization3 { get; } = new(SpecializationSlotType.Line_3);

        public BuildSpecialization this[SpecializationSlotType slot] => slot switch
        {
            SpecializationSlotType.Line_1 => Specialization1,
            SpecializationSlotType.Line_2 => Specialization2,
            SpecializationSlotType.Line_3 => Specialization3,
            _ => throw new ArgumentOutOfRangeException(nameof(slot), slot, null)
        };

        public IEnumerator<BuildSpecialization> GetEnumerator()
        {
            yield return Specialization1;
            yield return Specialization2;
            yield return Specialization3;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
