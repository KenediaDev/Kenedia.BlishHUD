using Newtonsoft.Json;
using System;
using Version = SemVer.Version;

namespace Kenedia.Modules.Core.Converter
{
    public class SemverVersionConverter : JsonConverter<Version>
    {
        public override void WriteJson(JsonWriter writer, Version value, JsonSerializer serializer)
        {
            writer.WriteValue(value?.ToString());
        }

        public override Version ReadJson(JsonReader reader, Type objectType, Version existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            if (reader.TokenType == JsonToken.String)
            {
                string s = (string)reader.Value;

                // Handle empty / invalid values safely
                return string.IsNullOrWhiteSpace(s) ? new Version(0, 0, 0) : new Version(s);
            }

            throw new JsonSerializationException($"Unexpected token {reader.TokenType} when parsing Version.");
        }
    }
}
