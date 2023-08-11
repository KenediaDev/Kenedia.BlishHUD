using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.Core.Models;
using System;

namespace Kenedia.Modules.BuildsManager.Models.Templates
{
    public class TraitCollection : ObservableDictionary<TraitTierType, Trait>
    {
        public TraitCollection()
        {
            foreach (TraitTierType e in Enum.GetValues(typeof(TraitTierType)))
            {
                Add(e, null);
            }
        }
    }
}
