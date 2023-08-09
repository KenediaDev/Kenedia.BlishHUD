using Gw2Sharp.WebApi;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.BuildsManager.Models.Templates;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Kenedia.Modules.BuildsManager.DataModels.Items
{
    [DataContract]
    public class Rune : BaseItem
    {
        public Rune()
        {
            TemplateSlot = TemplateSlot.None;

        }

        [DataMember]
        public List<BonusStat> Bonuses { get; set; } = new();

        [DataMember]
        public RuneBonuses BonusDescriptions { get; set; } = new();

        public override void Apply(Item item)
        {
            base.Apply(item);

            if (item.Type == ItemType.UpgradeComponent)
            {
                var upgrade = (ItemUpgradeComponent)item;
                if (upgrade.Details.Bonuses != null)
                {
                    BonusDescriptions.AddBonuses(upgrade.Details.Bonuses);
                }
            }

            string[] rune_strings = Blish_HUD.GameService.Overlay.UserLocale.Value switch
            {
                Locale.German => new string[] { "Überlegene Rune des",  "Überlegene Rune der" },
                Locale.French => new string[] { "Rune des", "supérieure" },
                Locale.Spanish => new string[] { "Runa superior de" },
                _ => new string[] { "Superior Rune of the", "Superior Rune of" },
            };

            string displayText = item.Name;
            foreach (string s in rune_strings)
            {
                displayText = displayText.Replace(s, string.Empty);
            }

            DisplayText = displayText.Trim();
        }
    }
}
