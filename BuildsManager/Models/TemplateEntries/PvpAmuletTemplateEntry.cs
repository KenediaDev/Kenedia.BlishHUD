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

        public override short[] AddToCodeArray(short[] array)
        {
            return array.Concat(new short[]
            {
                (short)(Item?.MappedId ?? -1),
                (short)(Rune?.MappedId ?? -1),
            }).ToArray();
        }

        public override void FromCode(string code)
        {
            string[] parts = GetCode(code).Split('|');

            if (parts.Length == 2)
            {
                Item = int.TryParse(parts[0], out int id) ? BuildsManager.Data.PvpAmulets.Values.Where(e => e.MappedId == id).FirstOrDefault() : null;
                Rune = int.TryParse(parts[1], out int runeId) ? BuildsManager.Data.PvpRunes.Values.Where(e => e.MappedId == runeId).FirstOrDefault() : null;
            }
        }

        public override short[] GetFromCodeArray(short[] array)
        {
            int newStartIndex = 2;

            Item = int.TryParse($"{array[0]}", out int id) ? BuildsManager.Data.PvpAmulets.Values.Where(e => e.MappedId == id).FirstOrDefault() : null;
            Rune = int.TryParse($"{array[1]}", out int rune) ? BuildsManager.Data.PvpRunes.Where(e => e.Value.MappedId == rune).FirstOrDefault().Value : null;

            return GearTemplateCode.RemoveFromStart(array, newStartIndex);
        }

        public override string ToCode()
        {
            return $"[{Item?.MappedId ?? -1}|{Rune?.MappedId ?? -1}]";
        }
    }
}
