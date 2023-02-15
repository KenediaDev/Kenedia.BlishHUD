using Kenedia.Modules.BuildsManager.DataModels.Professions;
using System;
using System.Collections.Generic;

namespace Kenedia.Modules.BuildsManager.Models.Templates
{
    public class TraitCollection : Dictionary<TraitTier, Trait>
    {


        public TraitCollection()
        {
            foreach (TraitTier e in Enum.GetValues(typeof(TraitTier)))
            {
                Add(e, null);
            }
        }

        public byte GetTraitByte(TraitTier slot)
        {
            return (byte)(TryGetValue(slot, out Trait trait) ? trait.Id : 0);
        }
    }
}
