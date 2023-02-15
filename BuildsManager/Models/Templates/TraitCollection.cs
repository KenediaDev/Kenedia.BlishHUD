using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Kenedia.Modules.BuildsManager.Models.Templates
{
    public class TraitCollection : Dictionary<TraitTier, Trait>
    {
        public new Trait this[TraitTier key] 
        {
            get => this[key];
            set => this[key] = value;
        }

        public TraitCollection()
        {

        }
    }
}
