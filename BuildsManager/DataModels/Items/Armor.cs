using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.BuildsManager.Models.Templates;
using System.Runtime.Serialization;

namespace Kenedia.Modules.BuildsManager.DataModels.Items
{
    [DataContract]
    public class Armor : EquipmentItem
    {
        public Armor() { }

        public override void Apply(Item item)
        {
            base.Apply(item);

            if (item.Type == ItemType.Armor)
            {
                var armor = (ItemArmor)item;
                AttributeAdjustment = armor.Details.AttributeAdjustment;
                Weight = armor.Details.WeightClass;
                StatChoices = armor.Details.StatChoices;
                InfusionSlots = new int[armor.Details.InfusionSlots.Count];

                TemplateSlot = armor.Details.Type.Value switch
                {
                    ItemArmorSlotType.HelmAquatic=> GearTemplateSlot.AquaBreather,
                    ItemArmorSlotType.Helm=> GearTemplateSlot.Head,
                    ItemArmorSlotType.Shoulders=> GearTemplateSlot.Shoulder,
                    ItemArmorSlotType.Coat => GearTemplateSlot.Chest,
                    ItemArmorSlotType.Gloves=> GearTemplateSlot.Hand,
                    ItemArmorSlotType.Leggings => GearTemplateSlot.Leg,
                    ItemArmorSlotType.Boots=> GearTemplateSlot.Foot,
                    _ => GearTemplateSlot.None,
                };
            }
        }

        [DataMember]
        public ItemWeightType Weight { get; protected set; }
    }
}
