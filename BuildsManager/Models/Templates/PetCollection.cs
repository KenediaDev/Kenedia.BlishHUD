using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.Core.Models;
using System;

namespace Kenedia.Modules.BuildsManager.Models.Templates
{
    public class PetCollection : ObservableDictionary<PetSlotType, Pet>
    {
        public PetCollection()
        {
            foreach (PetSlotType e in Enum.GetValues(typeof(PetSlotType)))
            {
                Add(e, null);
            }
        }

        public byte GetPetByte(PetSlotType slot)
        {
            return (byte)(TryGetValue(slot, out Pet pet) && pet != null ? pet.Id : 0);
        }

        public void LoadFromCode(byte terrestrial_1, byte terrestrial_2, byte aquatic_1, byte aquatic_2)
        {
            this[PetSlotType.Terrestrial_1] = Pet.FromByte(terrestrial_1);
            this[PetSlotType.Terrestrial_2] = Pet.FromByte(terrestrial_2);
            this[PetSlotType.Aquatic_1] = Pet.FromByte(aquatic_1);
            this[PetSlotType.Aquatic_2] = Pet.FromByte(aquatic_2);
        }
    }
}
