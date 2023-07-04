using Blish_HUD.Content;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.AdvancedBuildsManager.Models.Templates;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using System.Runtime.Serialization;

namespace Kenedia.Modules.AdvancedBuildsManager.DataModels.Items
{
    [DataContract]
    public class BaseItem
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

        public BaseItem(Item item, GearTemplateSlot templateSlot) : this(item)
        {
            TemplateSlot = templateSlot;
        }

        [DataMember]
        public ItemType Type { get; protected set; }

        [DataMember]
        public GearTemplateSlot TemplateSlot { get; protected set; }

        [DataMember]
        public int Id { get; protected set; }

        [DataMember]
        public int MappedId { get;  set; }

        [DataMember]
        public ItemRarity Rarity { get; protected set; }

        [DataMember]
        public string Chatlink { get; protected set; }

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
    }
}
