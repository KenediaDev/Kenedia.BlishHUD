using Kenedia.Modules.AdvancedBuildsManager.DataModels.Professions;
using Kenedia.Modules.Core.Models;
using System;

namespace Kenedia.Modules.AdvancedBuildsManager.Models.Templates
{
    public class TraitCollection : ObservableDictionary<TraitTier, Trait>
    {
        public TraitCollection()
        {
            foreach (TraitTier e in Enum.GetValues(typeof(TraitTier)))
            {
                Add(e, null);
            }
        }
    }
}
