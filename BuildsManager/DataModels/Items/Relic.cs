using Kenedia.Modules.BuildsManager.Models.Templates;
using System.Runtime.Serialization;

namespace Kenedia.Modules.BuildsManager.DataModels.Items
{
    [DataContract]
    public class Relic : BaseItem
    {
        public Relic()
        {
            TemplateSlot = TemplateSlotType.Relic;
        }
    }
}
