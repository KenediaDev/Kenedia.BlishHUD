using Gw2Sharp.WebApi;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.AdvancedBuildsManager.Models.Templates;
using Kenedia.Modules.Core.Models;
using System.Runtime.Serialization;

namespace Kenedia.Modules.AdvancedBuildsManager.DataModels.Items
{
    [DataContract]
    public class Sigil : BaseItem
    {
        public Sigil()
        {
            TemplateSlot = GearTemplateSlot.None;

        }

        [DataMember]
        public LocalizedString Buffs { get; protected set; } = new();
        public string Buff
        {
            get => Buffs.Text;
            set => Buffs.Text = value;
        }

        public override void Apply(Item item)
        {
            base.Apply(item);

            if (item.Type == ItemType.UpgradeComponent)
            {
                var upgrade = (ItemUpgradeComponent)item;
                if (upgrade.Details.InfixUpgrade != null && upgrade.Details.InfixUpgrade.Buff != null)
                {
                    Buff = upgrade.Details.InfixUpgrade.Buff.Description;
                }

                string[] sigil_strings = Blish_HUD.GameService.Overlay.UserLocale.Value switch
                {
                    Locale.German => new string[] { "Überlegenes Sigill des" , "Überlegenes Sigill der" },
                    Locale.French => new string[] { "Cachet d'", "supérieur" },
                    Locale.Spanish => new string[] { "Sello superior de" },
                    _ => new string[] { "Superior Sigil of the", "Superior Sigil of" },
                };

                string displayText = item.Name;
                foreach(string s in sigil_strings)
                {
                    displayText = displayText.Replace(s, string.Empty);
                }

                DisplayText = displayText.Trim();
            }
        }
    }
}
