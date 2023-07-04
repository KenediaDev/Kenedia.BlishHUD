using Gw2Sharp.WebApi.V2.Models;
using System.Runtime.Serialization;

namespace Kenedia.Modules.AdvancedBuildsManager.DataModels.Items
{
    [DataContract]
    public class StatConversion
    {
        [DataMember]
        public AttributeType SourceAttribute;

        [DataMember]
        public AttributeType TargetAttribute;

        /// <summary>
        /// For each <see cref="SourceAttribute"/> return <see cref="Factor"/>*<see cref="TargetAttribute"/>
        /// </summary>
        [DataMember]
        public double Factor = 0;

        public double Amount(int amount)
        {
            return amount * Factor;
        }
    }
}
