using Gw2Sharp.WebApi.V2.Models;
using System.Runtime.Serialization;

namespace Kenedia.Modules.BuildsManager.DataModels.Stats
{
    [DataContract]
    public class StatAttribute
    {
        public StatAttribute() { }

        public StatAttribute(ItemstatAttribute attribute)
        {
            Id = attribute.Attribute.ToEnum();
            Multiplier = attribute.Multiplier;
            Value = attribute.Value;
        }

        [DataMember]
        public double Multiplier { get; set; }

        [DataMember]
        public AttributeType Id { get; set; }

        [DataMember]
        public int Value { get; set; }
    }
}
