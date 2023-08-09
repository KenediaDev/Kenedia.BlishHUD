using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.BuildsManager.Models.Templates;
using System.Runtime.Serialization;

namespace Kenedia.Modules.BuildsManager.DataModels.Items
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
                    ItemWeaponType.Speargun => TemplateSlot.Aquatic,
                    ItemWeaponType.Harpoon => TemplateSlot.Aquatic,
                    ItemWeaponType.Trident => TemplateSlot.Aquatic,

                    ItemWeaponType.Shield => TemplateSlot.OffHand,
                    ItemWeaponType.Focus => TemplateSlot.OffHand,
                    ItemWeaponType.Torch => TemplateSlot.OffHand,
                    ItemWeaponType.Warhorn => TemplateSlot.OffHand,

                    _ => TemplateSlot.MainHand,
                };
            }
        }

        [DataMember]
        public ItemWeaponType WeaponType { get; protected set; }

        [DataMember]
        public int Defense { get; protected set; }
    }
}
