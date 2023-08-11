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
                Defense = armor.Details.Defense;

                TemplateSlot = armor.Details.Type.Value switch
                {
                    ItemArmorSlotType.HelmAquatic=> TemplateSlotType.AquaBreather,
                    ItemArmorSlotType.Helm=> TemplateSlotType.Head,
                    ItemArmorSlotType.Shoulders=> TemplateSlotType.Shoulder,
                    ItemArmorSlotType.Coat => TemplateSlotType.Chest,
                    ItemArmorSlotType.Gloves=> TemplateSlotType.Hand,
                    ItemArmorSlotType.Leggings => TemplateSlotType.Leg,
                    ItemArmorSlotType.Boots=> TemplateSlotType.Foot,
                    _ => TemplateSlotType.None,
                };
            }
        }

        [DataMember]
        public ItemWeightType Weight { get; protected set; }

        [DataMember]
        public int Defense { get; protected set; }
    }
}
