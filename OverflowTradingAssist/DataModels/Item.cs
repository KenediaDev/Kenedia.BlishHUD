using Blish_HUD.Content;
using System.Runtime.Serialization;
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
        public static Item UnkownItem = new()
        {
            Id = 0,
            Name = "Unknown",
            Description = "Unknown",
            AssetId = 960286,
            Rarity = Gw2Sharp.WebApi.V2.Models.ItemRarity.Unknown,
            Type = ItemType.Unknown,
        };

        public static Item Coin = new()
        {
            Id = -1,
            Name = "Raw Gold",
            Description = "Raw Gold",
            AssetId = 156904,
            Rarity = Gw2Sharp.WebApi.V2.Models.ItemRarity.Unknown,
            Type = ItemType.Unknown,
        };

        public static Item T6Set = new()
        {
            Id = -2,
            Name = "T6 Set",
            Description = "250 of each T6 Material",
            AssetId = 66950,
            Rarity = Gw2Sharp.WebApi.V2.Models.ItemRarity.Rare,
            Type = ItemType.Unknown,
        };

        public static Item Guild = new()
        {
            Id = -3,
            Name = "Guild",
            Description = "A GW2 Guild.",
            AssetId = 156744,
            Rarity = Gw2Sharp.WebApi.V2.Models.ItemRarity.Masterwork,
            Type = ItemType.Unknown,
        };

        public static Item GuildBank50 = new()
        {
            Id = -4,
            Name = "Guild Bank 50 Slot",
            Description = "Guild Bank with 50 Slot vault.",
            AssetId = 240682,
            Rarity = Gw2Sharp.WebApi.V2.Models.ItemRarity.Rare,
            Type = ItemType.Unknown,
        };

        public static Item GuildBank150 = new()
        {
            Id = -5,
            Name = "Guild Bank 150 Slot",
            Description = "Guild Bank with 150 Slot vault.",
            AssetId = 240682,
            Rarity = Gw2Sharp.WebApi.V2.Models.ItemRarity.Exotic,
            Type = ItemType.Unknown,
        };

        public static Item GuildBank250 = new()
        {
            Id = -6,
            Name = "Guild Bank 250 Slot",
            Description = "Guild Bank with 250 Slot vault.",
            AssetId = 240682,
            Rarity = Gw2Sharp.WebApi.V2.Models.ItemRarity.Ascended,
            Type = ItemType.Unknown,
        };

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
                if (field is not null) return field;

                if (AssetId is not 0)
                    field = AsyncTexture2D.FromAssetId(AssetId);

                return field;
            }
        }

        [DataMember]
        public LocalizedString Names { get; protected set; } = [];
        public string Name
        {
            get => Names.Text;
            set => Names.Text = value;
        }

        [DataMember]
        public LocalizedString Descriptions { get; protected set; } = [];
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
        }

        public void SetAssetId(int id)
        {
            AssetId = id;
        }
    }
}
