using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.BuildsManager.Models.Templates;
using System.Linq;
using System.Runtime.Serialization;

namespace Kenedia.Modules.BuildsManager.DataModels.Items
{
    [DataContract]
    public class Relic : BaseItem
    {
        public Relic()
        {
            TemplateSlot = TemplateSlotType.PveRelic;
        }

        public override void Apply(Item item)
        {
            base.Apply(item);
        }
    }
}
