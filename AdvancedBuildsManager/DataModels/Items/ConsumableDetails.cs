using Blish_HUD.Content;
using Gw2Sharp.WebApi.V2.Models;
using System.Runtime.Serialization;

namespace Kenedia.Modules.AdvancedBuildsManager.DataModels.Items
{
    [DataContract]
    public class ConsumableDetails
    {
        private AsyncTexture2D _icon;

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
                if (_icon is not null) return _icon;

                _icon = AsyncTexture2D.FromAssetId(AssetId);
                return _icon;
            }
        }
    }
}
