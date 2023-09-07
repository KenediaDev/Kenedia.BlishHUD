using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.BuildsManager.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Kenedia.Modules.BuildsManager.DataModels.Stats
{
    public class StatAttributes : Dictionary<AttributeType, StatAttribute>
    {
        public StatAttributes()
        {
        }

        public string ToString(double attributeAdjustment)
        {
            return string.Join(Environment.NewLine, Values.Where(e => e is not null).Select(e => $"+ {Math.Round(e.Value + (e.Multiplier * attributeAdjustment))} {e.Id.GetDisplayName()}"));
        }
    }
}
