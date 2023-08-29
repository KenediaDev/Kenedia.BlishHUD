using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.BuildsManager.Models.Templates;
using System.Runtime.Serialization;

namespace Kenedia.Modules.BuildsManager.DataModels.Items
{
    [DataContract]
    public class PowerCore : BaseItem
    {
        public PowerCore()
        {
            TemplateSlot = TemplateSlotType.PowerCore;
        }

        public PowerCore(Item item) : this()
        {
            Apply(item);
        }

        //[JsonConstructor]
        //public JadeBotCore(string name) : this()
        //{
        //    Name = name;
        //}
    }
}
