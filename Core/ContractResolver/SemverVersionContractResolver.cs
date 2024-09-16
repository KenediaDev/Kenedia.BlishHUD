using Kenedia.Modules.Core.Attributes;
using Kenedia.Modules.Core.Converter;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace Kenedia.Modules.Core.ContractResolver
{
    public class SemverVersionContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(System.Reflection.MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (Attribute.IsDefined(member, typeof(JsonSemverVersionAttribute)))
            {
                property.Converter = new SemverVersionConverter();
            }

            return property;
        }
    }
}
