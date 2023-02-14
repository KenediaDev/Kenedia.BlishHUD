using System;
using System.Collections.Generic;

namespace Kenedia.Modules.BuildsManager.Models.Templates
{
    public class GearCollection : Dictionary<GearSlot, TemplateItem>
    {
        public GearCollection()
        {
            foreach (GearSlot e in Enum.GetValues(typeof(GearSlot)))
            {
                Add(e, new());
            }
        }

        public string ToCode(GearSlot slot)
        {
            return this[slot].ToCode(slot);
        }
    }
}
