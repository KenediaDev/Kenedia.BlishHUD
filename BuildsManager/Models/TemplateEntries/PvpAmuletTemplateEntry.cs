using Kenedia.Modules.BuildsManager.Models.Templates;
using System.Linq;
using Kenedia.Modules.BuildsManager.DataModels.Items;

namespace Kenedia.Modules.BuildsManager.TemplateEntries
{
    public class PvpAmuletTemplateEntry : TemplateEntry
    {
        public PvpAmuletTemplateEntry(TemplateSlot slot) : base(slot)
        {
        }

        public PvpAmulet Item { get; set; }

        public Rune Rune{ get; set; }

        public override void FromCode(string code)
        {
            string[] parts = GetCode(code).Split('|');

            if (parts.Length == 2)
            {
                Item = int.TryParse(parts[0], out int id) ? BuildsManager.Data.PvpAmulets.Values.Where(e => e.Id == id).FirstOrDefault() : null;
                Rune = int.TryParse(parts[1], out int runeId) ? BuildsManager.Data.PvpRunes.Values.Where(e => e.Id == runeId).FirstOrDefault() : null;
            }
        }

        public override string ToCode()
        {
            return $"[{Item?.Id ?? -1}|{Rune?.Id ?? -1}]";
        }
    }
}
