using Kenedia.Modules.BuildsManager.DataModels.Professions;
using System;
using System.Collections.Generic;

namespace Kenedia.Modules.BuildsManager.Models.Templates
{
    public class PetCollection : Dictionary<PetSlot, Pet>
    {
        public PetCollection()
        {
            foreach (PetSlot e in Enum.GetValues(typeof(PetSlot)))
            {
                Add(e, null);
            }
        }

        public byte GetPetByte(PetSlot slot)
        {
            return (byte)(TryGetValue(slot, out Pet pet) && pet != null ? pet.Id : 0);
        }
    }
}
