using Blish_HUD.Content;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.Core.Models;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Kenedia.Modules.BuildsManager.DataModels.LegendaryItems
{
    [DataContract]
    public class LegendaryItem
    {
        private AsyncTexture2D _icon;

        public LegendaryItem()
        {
        }

        [DataMember]
        public int Id { get; protected set; }

        [DataMember]
        public ItemRarity Rarity { get; protected set; } = ItemRarity.Legendary;

        [DataMember]
        public int AssetId { get; protected set; }

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
        public double AttributeAdjustment { get; protected set; }

        [DataMember]
        public string Chatlink { get; protected set; }

        [DataMember]
        public ItemEquipmentSlotType Slot { get; protected set; }

        [DataMember]
        public IReadOnlyList<int> StatChoices { get; protected set; }

        [DataMember]
        public int[] InfusionSlots { get; protected set; }

        public AsyncTexture2D Icon 
        { 
            get
            {
                if(_icon != null) return _icon;

                _icon = AsyncTexture2D.FromAssetId(AssetId);
                return _icon;
            }
        }
    }
}
