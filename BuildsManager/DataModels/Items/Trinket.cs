using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.BuildsManager.Models.Templates;
using System.Runtime.Serialization;

namespace Kenedia.Modules.BuildsManager.DataModels.Items
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
                    ItemTrinketType.Amulet => TemplateSlotType.Amulet,
                    ItemTrinketType.Ring => TemplateSlotType.Ring_1,
                    ItemTrinketType.Accessory => TemplateSlotType.Accessory_1,
                    _ => TemplateSlotType.None,
                };
            }
            else if(item.Type == ItemType.Back)
            {
                var back = (ItemBack)item;

                AttributeAdjustment = back.Details.AttributeAdjustment;
                StatChoices = back.Details.StatChoices;
                InfusionSlots = new int[back.Details.InfusionSlots.Count];
                TemplateSlot = TemplateSlotType.Back;
            }
        }
    }
}
