using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using Kenedia.Modules.Core.Utility;
using ItemType = Kenedia.Modules.Core.DataModels.ItemType;

namespace Kenedia.Modules.BuildsManager.DataModels.Converter
{
    public class RelicConverter : JsonConverter<Relic>
    {
        public override Relic ReadJson(JsonReader reader, Type objectType, Relic existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);
            var relic = new Relic
            {
                Name = (string)jObject["name"],
                Description = (string)jObject["description"],
                Type = Enum.TryParse((string)jObject["type"], out ItemType type) ? type : ItemType.Unknown,
                Rarity = Enum.TryParse((string)jObject["rarity"], out ItemRarity rarity) ? rarity : ItemRarity.Unknown,
                Id = (int)jObject["id"],
                AssetId = ((string)jObject["icon"]).GetAssetIdFromRenderUrl()
            };
            return relic;
        }

        public override void WriteJson(JsonWriter writer, Relic value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
