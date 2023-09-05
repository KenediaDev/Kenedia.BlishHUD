using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Version = SemVer.Version;

namespace Kenedia.Modules.Core.Converter
{
    public class SemverVersionConverter : JsonConverter<Version>
    {
        public override Version ReadJson(JsonReader reader, Type objectType, Version existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return reader.Value == null ? null : new((string)reader.Value);
        }

        public override void WriteJson(JsonWriter writer, Version value, JsonSerializer serializer)
        {
            if (value != null)
            {
                writer.WriteValue(value.ToString());
            }
        }
    }
}
