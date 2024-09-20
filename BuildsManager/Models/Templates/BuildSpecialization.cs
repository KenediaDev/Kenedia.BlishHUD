using Kenedia.Modules.BuildsManager.DataModels.Professions;

namespace Kenedia.Modules.BuildsManager.Models.Templates
{
    public class BuildSpecialization
    {
        public BuildSpecialization(SpecializationSlotType slot)
        {
            SpecializationSlot = slot;
        }

        public SpecializationSlotType SpecializationSlot { get; }

        public Specialization Specialization { get; set; }

        public TraitCollection Traits { get; } = new();

        public byte GetSpecializationByte()
        {
            byte id = (byte)(Specialization is not null ? Specialization.Id : 0);
            return id;
        }

        public byte GetTraitByte(TraitTierType traitSlot)
        {
            if (Traits[traitSlot] is not null)
            {
                int? order = Traits[traitSlot]?.Order;

                return (byte)(order is not null ? order + 1 : 0);
            }

            return 0;
        }
    }    
}
