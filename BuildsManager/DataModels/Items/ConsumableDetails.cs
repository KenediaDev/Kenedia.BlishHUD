using Blish_HUD.Content;
using Kenedia.Modules.Core.Extensions;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.Core.Models;
using System.Runtime.Serialization;

namespace Kenedia.Modules.BuildsManager.DataModels.Items
{
    [DataContract]
    public class ConsumableDetails
    {
        [DataMember]
        public ItemConsumableType Type { get; set; }

        [DataMember]
        public LocalizedString Names { get; protected set; } = [];
        public string Name
        {
            get => Names.Text;
            set => Names.Text = value;
        }

        [DataMember]
        public LocalizedString DisplayTexts { get; protected set; } = [];
        public string DisplayText
        {
            get => DisplayTexts.Text;
            set => DisplayTexts.Text = value;
        }

        [DataMember]
        public LocalizedString Descriptions { get; protected set; } = [];
        public string Description
        {
            get => Descriptions.Text.InterpretItemDescription();
            set => Descriptions.Text = value;
        }

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
