using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.AdvancedBuildsManager.Models.Templates;
using System.Runtime.Serialization;

namespace Kenedia.Modules.AdvancedBuildsManager.DataModels.Items
{
    [DataContract]
    public class Trinket : EquipmentItem
    {
        public Trinket() { }

        public override void Apply(Item item)
        {
            base.Apply(item);

            if (item.Type == ItemType.Trinket)
            {
                var trinket = (ItemTrinket)item;
                AttributeAdjustment = trinket.Details.AttributeAdjustment;
                StatChoices = trinket.Details.StatChoices;
                InfusionSlots = new int[trinket.Details.InfusionSlots.Count];

                TemplateSlot = trinket.Details.Type.Value switch
                {
                    ItemTrinketType.Amulet => GearTemplateSlot.Amulet,
                    ItemTrinketType.Ring => GearTemplateSlot.Ring_1,
                    ItemTrinketType.Accessory => GearTemplateSlot.Accessory_1,
                    _ => GearTemplateSlot.None,
                };
            }
            else if(item.Type == ItemType.Back)
            {
                var back = (ItemBack)item;

                AttributeAdjustment = back.Details.AttributeAdjustment;
                StatChoices = back.Details.StatChoices;
                InfusionSlots = new int[back.Details.InfusionSlots.Count];
                TemplateSlot = GearTemplateSlot.Back;
            }
        }
    }
}
