using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.Core.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Kenedia.Modules.BuildsManager.DataModels.Converter
{
    public class PowerCoreConverter : JsonConverter<PowerCore>
    {
        public override PowerCore ReadJson(JsonReader reader, Type objectType, PowerCore existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);
            var jadeBotCore = new PowerCore
            {
                Name = (string)jObject["name"],
                Description = (string)jObject["description"],
                Type = Enum.TryParse((string)jObject["type"], out ItemType type) ? type : ItemType.Unknown,
                Rarity = Enum.TryParse((string)jObject["rarity"], out ItemRarity rarity) ? rarity : ItemRarity.Unknown,
                Id = (int)jObject["id"],
                AssetId = ((string)jObject["icon"]).GetAssetIdFromRenderUrl()
            };
            return jadeBotCore;
        }

        public override void WriteJson(JsonWriter writer, PowerCore value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
