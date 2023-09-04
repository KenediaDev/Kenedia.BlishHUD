using Blish_HUD.Content;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Kenedia.Modules.BuildsManager.DataModels.Items
{
    public interface IDataMember
    {
    }

    [DataContract]
    public class BaseItem : IDataMember
    {
        private AsyncTexture2D _icon;

        public BaseItem()
        {

        }

        public BaseItem(Item item)
        {
            Rarity = item.Rarity;
            Chatlink = item.ChatLink;
            AssetId = item.Icon.GetAssetIdFromRenderUrl();
            Id = item.Id;
            Name = item.Name;
            Description = item.Description;
            Type = item.Type;
        }

        public BaseItem(Item item, TemplateSlotType templateSlot) : this(item)
        {
            TemplateSlot = templateSlot;
        }

        [DataMember]
        public ItemType Type { get; set; }

        [DataMember]
        public TemplateSlotType TemplateSlot { get; set; }

        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public byte MappedId { get;  set; }

        [DataMember]
        public ItemRarity Rarity { get; set; }

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

        public virtual void Apply(Item item)
        {
            Id = item.Id;
            Name = item.Name;
            Description = item.Description;
            AssetId = item.Icon.GetAssetIdFromRenderUrl();

            Rarity = item.Rarity;
            Chatlink = item.ChatLink;
            Type = item.Type;
            DisplayText = item.Name;
        }

        public virtual void Apply(Gw2Sharp.WebApi.V2.Models.PvpAmulet amulet)
        {
            Id = amulet.Id;
            Name = amulet.Name;
            AssetId = amulet.Icon.GetAssetIdFromRenderUrl();
            DisplayText = amulet.Name;
            Rarity = ItemRarity.Basic;
            Type = ItemType.Unknown;            
        }

        public void SetAssetId(int id)
        {
            AssetId = id;
        }
    }
}
