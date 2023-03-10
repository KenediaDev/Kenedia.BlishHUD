using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.Core.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Kenedia.Modules.BuildsManager.Models.Templates
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
