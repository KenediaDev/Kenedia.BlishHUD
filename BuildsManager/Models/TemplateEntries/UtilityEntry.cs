using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.BuildsManager.Utility;
using System.Linq;

namespace Kenedia.Modules.BuildsManager.TemplateEntries
{
    public class UtilityEntry : TemplateEntry
    {
        public UtilityEntry(TemplateSlot slot) : base(slot)
        {
        }

        public DataModels.Items.Utility Item { get; set; }

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

            Item = BuildsManager.Data.Utilities.Values.Where(e => e.MappedId == array[0]).FirstOrDefault();

            return GearTemplateCode.RemoveFromStart(array, newStartIndex);
        }
    }
}
