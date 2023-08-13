using Kenedia.Modules.AdvancedBuildsManager.DataModels.Professions;
using Kenedia.Modules.Core.Models;
using System;

namespace Kenedia.Modules.AdvancedBuildsManager.Models.Templates
{
    public class PetCollection : ObservableDictionary<PetSlot, Pet>
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
            return (byte)(TryGetValue(slot, out Pet pet) && pet is not null ? pet.Id : 0);
        }
    }
}
