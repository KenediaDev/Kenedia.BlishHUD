using Kenedia.Modules.BuildsManager.Models.Templates;
using System.Linq;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.Utility;

namespace Kenedia.Modules.BuildsManager.TemplateEntries
{
    public class PvpAmuletTemplateEntry : TemplateEntry
    {
        public PvpAmuletTemplateEntry(TemplateSlot slot) : base(slot)
        {
        }

        public PvpAmulet Item { get; set; }

        public Rune Rune{ get; set; }

        public override byte[] AddToCodeArray(byte[] array)
        {
            return array.Concat(new byte[]
            {
                Item ?.MappedId ?? 0,
                Rune ?.MappedId ?? 0,
            }).ToArray();
        }

        public override byte[] GetFromCodeArray(byte[] array)
        {
            int newStartIndex = 2;

            Item = BuildsManager.Data.PvpAmulets.Values.Where(e => e.MappedId == array[0]).FirstOrDefault();
            Rune = BuildsManager.Data.PvpRunes.Where(e => e.Value.MappedId == array[1]).FirstOrDefault().Value;

            return GearTemplateCode.RemoveFromStart(array, newStartIndex);
        }
    }
}
