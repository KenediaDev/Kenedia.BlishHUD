using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.Core.Utility;
using System.Runtime.Serialization;

namespace Kenedia.Modules.BuildsManager.DataModels.LegendaryItems
{
    [DataContract]
    public class LegendaryWeapon : LegendaryItem
    {
        public LegendaryWeapon() { }

        public LegendaryWeapon(ItemWeapon weapon)
        {
            Apply(weapon);
        }

        public void Apply(ItemWeapon weapon)
        {
            Id = weapon.Id;
            AttributeAdjustment = weapon.Details.AttributeAdjustment;
            Description = weapon.Description;
            Name = weapon.Name;
            WeaponType = weapon.Details.Type;
            StatChoices = weapon.Details.StatChoices;
            InfusionSlots = new int[weapon.Details.InfusionSlots.Count];
            AssetId = Common.GetAssetIdFromRenderUrl(weapon.Icon);
            Chatlink = weapon.ChatLink;
        }

        [DataMember]
        public LegendaryItemType ItemType { get; } = LegendaryItemType.Weapon;

        [DataMember]
        public ItemWeaponType WeaponType { get; protected set; }
    }
}
