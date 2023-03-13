using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Models;
using System.Runtime.Serialization;

namespace Kenedia.Modules.BuildsManager.DataModels.Items
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
            }
        }
    }
}
