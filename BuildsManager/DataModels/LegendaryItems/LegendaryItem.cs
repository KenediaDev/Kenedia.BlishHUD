using Blish_HUD.Content;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.BuildsManager.DataModels.Stats;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Models;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Kenedia.Modules.BuildsManager.DataModels.LegendaryItems
{
    [DataContract]
    public class BaseItem
    {
        private AsyncTexture2D _icon;

        public BaseItem()
        {

        }

        [DataMember]
        public GearTemplateSlot Slot { get; protected set; }

        [DataMember]
        public int Id { get; protected set; }

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
        public LocalizedString Descriptions { get; protected set; } = new();
        public string Description
        {
            get => Descriptions.Text;
            set => Descriptions.Text = value;
        }        
    }

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
                if (_icon != null) return _icon;

                _icon = AsyncTexture2D.FromAssetId(AssetId);
                return _icon;
            }
        }
    }

    [DataContract]
    public class StatConversion
    {
        [DataMember]
        public AttributeType SourceAttribute;

        [DataMember]
        public AttributeType TargetAttribute;

        /// <summary>
        /// For each <see cref="SourceAttribute"/> return <see cref="Factor"/>*<see cref="TargetAttribute"/>
        /// </summary>
        [DataMember]
        public double Factor = 0;

        public double Amount(int amount)
        {
            return amount * Factor;
        }
    }

    [DataContract]
    public class Nourishment : BaseItem
    {
        public Nourishment()
        {
            Attributes[AttributeType.Power] = 100;
            Attributes[AttributeType.CritDamage] = 70;
        }

        [DataMember]
        public Dictionary<AttributeType, int> Attributes { get; set; } = new();

        [DataMember]
        public ConsumableDetails Details { get; set; } = new();
    }

    [DataContract]
    public class Utility : BaseItem
    {
        public Utility()
        {
        }

        [DataMember]
        public List<StatConversion> Conversions { get; set; } = new();

        [DataMember]
        public Dictionary<AttributeType, int> Attributes { get; set; } = new();

        [DataMember]
        public ConsumableDetails Details { get; set; } = new();
    }

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
        /// Food (100 / 70) = 70
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
