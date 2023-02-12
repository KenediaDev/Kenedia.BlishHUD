using Blish_HUD.Content;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.Core.Models;
using System.Runtime.Serialization;

namespace Kenedia.Modules.BuildsManager.DataModels.ItemUpgrades
{
    [DataContract]
    public class Upgrade
    {
        private AsyncTexture2D _icon;

        [DataMember]
        public int Id { get; protected set; }

        [DataMember]
        public ItemRarity Rarity { get; protected set; } = ItemRarity.Legendary;

        [DataMember]
        public LocalizedString Names { get; protected set; } = new();
        public string Name
        {
            get => Names.Text;
            set => Names.Text = value;
        }

        [DataMember]
        public LocalizedString Descriptions { get; protected set; } = new();
        public string Description
        {
            get => Descriptions.Text;
            set => Descriptions.Text = value;
        }

        [DataMember]
        public int AssetId { get; protected set; }

        public AsyncTexture2D Icon
        {
            get
            {
                if (_icon != null) return _icon;

                _icon = AsyncTexture2D.FromAssetId(AssetId);
                return _icon;
            }
        }
    }
}
