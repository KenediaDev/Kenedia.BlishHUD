using Kenedia.Modules.BuildsManager.DataModels.Professions;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Kenedia.Modules.BuildsManager.Models.Templates
{
    public class RangerPets : IEnumerable<Pet?>
    {
        public Pet Terrestrial_1 { get; private set; }

        public Pet Terrestrial_2 { get; private set; }

        public Pet Aquatic_1 { get; private set; }

        public Pet Aquatic_2 { get; private set; }

        public Pet this[PetSlotType slot] => slot switch
        {
            PetSlotType.Terrestrial_1 => Terrestrial_1,
            PetSlotType.Terrestrial_2 => Terrestrial_2,
            PetSlotType.Aquatic_1 => Aquatic_1,
            PetSlotType.Aquatic_2 => Aquatic_2,
            _ => throw new NotImplementedException(),
        };

        public byte GetPetByte(PetSlotType slot)
        {
            return (byte)(this[slot] is Pet pet && pet is not null ? pet.Id : 0);
        }

        public bool SetPet(PetSlotType slot, Pet pet)
        {
            switch (slot)
            {
                case PetSlotType.Terrestrial_1:
                    if (Terrestrial_1 == pet)
                    {
                        return false;
                    }

                    Terrestrial_1 = pet;
                    return true;
                case PetSlotType.Terrestrial_2:
                    if (Terrestrial_2 == pet)
                    {
                        return false;
                    }

                    Terrestrial_2 = pet;
                    return true;
                case PetSlotType.Aquatic_1:
                    if (Aquatic_1 == pet)
                    {
                        return false;
                    }

                    Aquatic_1 = pet;
                    return true;
                case PetSlotType.Aquatic_2:
                    if (Aquatic_2 == pet)
                    {
                        return false;
                    }

                    Aquatic_2 = pet;
                    return true;
                default:
                    return false;
            }
        }

        public void LoadFromCode(byte terrestrial_1, byte terrestrial_2, byte aquatic_1, byte aquatic_2)
        {
            Terrestrial_1 = Pet.FromByte(terrestrial_1);
            Terrestrial_2 = Pet.FromByte(terrestrial_2);
            Aquatic_1 = Pet.FromByte(aquatic_1);
            Aquatic_2 = Pet.FromByte(aquatic_2);
        }

        public IEnumerator<Pet?> GetEnumerator()
        {
            yield return Terrestrial_1;
            yield return Terrestrial_2;
            yield return Aquatic_1;
            yield return Aquatic_2;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Wipe()
        {
            Terrestrial_1 = null;
            Terrestrial_2 = null;
            Aquatic_1 = null;
            Aquatic_2 = null;
        }
    }
}
