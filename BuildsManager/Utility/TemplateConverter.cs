using Gw2Sharp.Models;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.Services;
using Kenedia.Modules.Core.DataModels;
using Kenedia.Modules.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Kenedia.Modules.BuildsManager.Utility
{
    public class TemplateConverter : JsonConverter
    {
        public TemplateConverter(Data data)
        {
            Data = data;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Template);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // Load the JSON for the Result into a JObject
            JObject jo = JObject.Load(reader);
            UniqueObservableCollection<string> tags;

            // Read the properties which will be used as constructor parameters
            try
            {
               tags = jo["Tags"].ToObject<UniqueObservableCollection<string>>(serializer);
            }
            catch
            {
                tags = [];
            }

            string? name = (string?)jo["Name"];
            string? buildCode = (string?)jo["BuildCode"];
            string? gearCode = (string?)jo["GearCode"];
            string? description = (string?)jo["Description"];
            int? race = (int?)jo["Race"];
            int? profession = (int?)jo["Profession"];
            int? elitespecId = (int?)jo["EliteSpecializationId"];

            // Construct the Result object using the non-default constructor
            var result = new Template(name, buildCode, gearCode, description, tags, (Races)(race ?? -1), (ProfessionType)(profession ?? 1), elitespecId ?? 0, Data);

            // (If anything else needs to be populated on the result object, do that here)

            // Return the result
            return result;
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public Data Data { get; }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
