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

    public class MajorTraitCollection : Dictionary<MajorTraitSlot, Trait>
    {
        public MajorTraitCollection()
        {
            foreach (MajorTraitSlot e in Enum.GetValues(typeof(MajorTraitSlot)))
            {
                Add(e, null);
            }
        }

        public byte GetTraitByte(MajorTraitSlot slot)
        {
            return (byte)(TryGetValue(slot, out Trait trait) ? trait.Id : 0);
        }
    }
}
