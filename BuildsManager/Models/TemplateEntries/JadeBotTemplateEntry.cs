using Kenedia.Modules.BuildsManager.Models.Templates;
using System.Linq;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.BuildsManager.Utility;

namespace Kenedia.Modules.BuildsManager.TemplateEntries
{
    public class JadeBotTemplateEntry : TemplateEntry
    {
        public JadeBotTemplateEntry(TemplateSlot slot) : base(slot)
        {
        }

        public JadeBotCore Item { get; set; }

        public override byte[] AddToCodeArray(byte[] array)
        {
            return array.Concat(new byte[]
            {
                Item ?.MappedId ?? 0,
            }).ToArray();
        }

        public override byte[] GetFromCodeArray(byte[] array)
        {
            int newStartIndex = 1;

            Item = BuildsManager.Data.JadeBotCores.Values.Where(e => e.MappedId == array[0]).FirstOrDefault();

            return GearTemplateCode.RemoveFromStart(array, newStartIndex);
        }
    }
}
