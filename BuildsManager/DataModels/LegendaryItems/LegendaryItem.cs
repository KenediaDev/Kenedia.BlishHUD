using Blish_HUD.Content;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Kenedia.Modules.BuildsManager.DataModels.LegendaryItems
{
    [DataContract]
    public class LegendaryUpgrade : LegendaryItem
    {
        public LegendaryUpgrade()
        {

        }

        public LegendaryUpgrade(ItemUpgradeComponent upgrade)
        {
            Apply(upgrade);
        }

        public void Apply(ItemUpgradeComponent upgrade)
        {
            Id = upgrade.Id;
            Names.Text = upgrade.Name;
            AssetId = Common.GetAssetIdFromRenderUrl(upgrade.Icon);
        }
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

        public string Name
        {
            get => Names.Text;
            set => Names.Text = value;
        }

        [DataMember]
        public LocalizedString Names { get; protected set; } = new();

        [DataMember]
        public LocalizedString Description { get; protected set; } = new();

        [DataMember]
        public double AttributeAdjustment { get; protected set; }

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
