using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Kenedia.Modules.BuildsManager.Utility
{
    public class TemplateConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(VTemplate);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // Load the JSON for the Result into a JObject
            JObject jo = JObject.Load(reader);

            // Read the properties which will be used as constructor parameters
            int? tags = (int?)jo["Tags"];
            int? encounters = (int?)jo["Encounters"];
            string? name = (string?)jo["Name"];
            string? buildCode = (string?)jo["BuildCode"];
            string? gearCode = (string?)jo["GearCode"];

            // Construct the Result object using the non-default constructor
            VTemplate result = new VTemplate(name, (EncounterFlag)encounters, (TemplateFlag)tags, buildCode, gearCode);

            // (If anything else needs to be populated on the result object, do that here)

            // Return the result
            return result;
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
