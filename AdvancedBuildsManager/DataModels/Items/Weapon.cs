﻿using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.AdvancedBuildsManager.Models.Templates;
using System.Runtime.Serialization;

namespace Kenedia.Modules.AdvancedBuildsManager.DataModels.Items
{
    [DataContract]
    public class Weapon : EquipmentItem
    {
        public Weapon() { }

        public override void Apply(Item item)
        {
            base.Apply(item);

            if (item.Type == ItemType.Weapon)
            {
                var weapon = (ItemWeapon) item;

                AttributeAdjustment = weapon.Details.AttributeAdjustment;
                WeaponType = weapon.Details.Type;
                StatChoices = weapon.Details.StatChoices;
                InfusionSlots = new int[weapon.Details.InfusionSlots.Count];                
                Defense = weapon.Details.Defense;

                TemplateSlot = weapon.Details.Type.Value switch
                {
                    ItemWeaponType.Speargun => GearTemplateSlot.Aquatic,
                    ItemWeaponType.Harpoon => GearTemplateSlot.Aquatic,
                    ItemWeaponType.Trident => GearTemplateSlot.Aquatic,

                    ItemWeaponType.Shield => GearTemplateSlot.OffHand,
                    ItemWeaponType.Focus => GearTemplateSlot.OffHand,
                    ItemWeaponType.Torch => GearTemplateSlot.OffHand,
                    ItemWeaponType.Warhorn => GearTemplateSlot.OffHand,

                    _ => GearTemplateSlot.MainHand,
                };
            }
        }

        [DataMember]
        public ItemWeaponType WeaponType { get; protected set; }

        [DataMember]
        public int Defense { get; protected set; }
    }
}
