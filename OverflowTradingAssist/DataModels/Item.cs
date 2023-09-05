using Blish_HUD.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Models;
using ApiItem = Gw2Sharp.WebApi.V2.Models.Item;
using Kenedia.Modules.Core.Utility;
using ItemType = Kenedia.Modules.Core.DataModels.ItemType;

namespace Kenedia.Modules.OverflowTradingAssist.DataModels
{
    [DataContract]
    public class Item
    {
        private AsyncTexture2D _icon;

        public Item()
        {

        }

        public Item(ApiItem item)
        {
            Apply(item);
        }

        [DataMember]
        public ItemType Type { get; set; }

        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public Gw2Sharp.WebApi.V2.Models.ItemRarity Rarity { get; set; }

        [DataMember]
        public string Chatlink { get; set; }

        [DataMember]
        public int AssetId { get; set; }
        public AsyncTexture2D Icon
        {
            get
            {
                if (_icon is not null) return _icon;

                if (AssetId is not 0)
                    _icon = AsyncTexture2D.FromAssetId(AssetId);

                return _icon;
            }
        }

        [DataMember]
        public LocalizedString Names { get; protected set; } = new();
        public string Name
        {
            get => Names.Text;
            set => Names.Text = value;
        }

        [DataMember]
        public LocalizedString DisplayTexts { get; protected set; } = new();
        public string DisplayText
        {
            get => DisplayTexts.Text;
            set => DisplayTexts.Text = value;
        }

        [DataMember]
        public LocalizedString Descriptions { get; protected set; } = new();
        public string Description
        {
            get => Descriptions.Text;
            set => Descriptions.Text = value;
        }

        public virtual void Apply(ApiItem item)
        {
            Id = item.Id;
            Name = item.Name;
            Description = item.Description;
            AssetId = item.Icon.GetAssetIdFromRenderUrl();

            Rarity = item.Rarity;
            Chatlink = item.ChatLink;
            Type = item.Type.ToItemType();
            DisplayText = item.Name;
        }

        public void SetAssetId(int id)
        {
            AssetId = id;
        }
    }
}
