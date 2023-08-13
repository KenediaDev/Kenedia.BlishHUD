using Gw2Sharp.WebApi.V2.Models;
using System.Collections.Generic;

namespace Kenedia.Modules.BuildsManager.DataModels.Stats
{
    public class StatAttributes : Dictionary<AttributeType, StatAttribute>
    {
        public StatAttributes()
        {
            //foreach(AttributeType attribute in Enum.GetValues(typeof(AttributeType)))
            //{
            //    if (attribute is AttributeType.Unknown or AttributeType.None) continue;

            //    this[attribute] = null;
            //}
        }
    }
}
