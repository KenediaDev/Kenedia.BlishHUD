using Blish_HUD.Content;
using Gw2Sharp.WebApi.V2.Models;
using System.Runtime.Serialization;

namespace Kenedia.Modules.AdvancedBuildsManager.DataModels.Items
{
    [DataContract]
    public class ConsumableDetails
    {
        [DataMember]
        public string? Name { get; set; }

        [DataMember]
        public ItemConsumableType Type { get; set; }

        [DataMember]
        public string Description { get; set; } = string.Empty;

        [DataMember]
        public int? DurationMs { get; set; }

        [DataMember]
        public int AssetId { get; protected set; }
        public AsyncTexture2D Icon
        {
            get
            {
                if (field is not null) return field;

                field = AsyncTexture2D.FromAssetId(AssetId);
                return field;
            }
        }
    }
}
