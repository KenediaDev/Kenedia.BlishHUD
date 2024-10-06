using Gw2Sharp.WebApi.V2.Models;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Kenedia.Modules.Core.DataModels;
using Newtonsoft.Json.Linq;
using Kenedia.Modules.FashionManager.Models;

namespace Kenedia.Modules.FashionManager.Utility
{
    public class FashionTemplateConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(FashionTemplate);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // Load the JSON for the Result into a JObject
            var jo = JObject.Load(reader);

            // Read the properties which will be used as constructor parameters
            string? name = (string?)jo["Name"];
            string? code = (string?)jo["FashionCode"];
            string? thumbnail = (string?)jo["ThumbnailFileName"];
            int? race = (int?)jo["Race"];
            int? gender = (int?)jo["Gender"];

            var galleryPaths = jo["GalleryPaths"]?.ToObject<List<string>>();

            // Construct the Result object using the non-default constructor
            var result = new FashionTemplate(name, thumbnail, code, (Races)race, (Gender)gender, galleryPaths);

            // (If anything else needs to be populated on the result object, do that here)

            // Return the result
            return result;
        }

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
