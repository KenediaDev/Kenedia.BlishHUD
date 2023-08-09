using Kenedia.Modules.BuildsManager.Models.Templates;
using System.Linq;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.DataModels.Stats;

namespace Kenedia.Modules.BuildsManager.TemplateEntries
{
    public class ArmorTemplateEntry : TemplateEntry
    {
        public ArmorTemplateEntry(TemplateSlot slot) : base(slot)
        {
        }

        public BaseItem Item { get; set; }

        public Rune Rune { get; set; }

        public Infusion Infusion { get; set; }

        public Stat Stat { get; set; }

        public override void FromCode(string code)
        {
            string[] parts = GetCode(code).Split('|');

            if (parts.Length == 3)
            {
                Stat = int.TryParse(parts[0], out int stat) ? BuildsManager.Data.Stats.Where(e => e.Value.Id == stat).FirstOrDefault().Value : null;
                Rune = int.TryParse(parts[1], out int rune) ? BuildsManager.Data.PveRunes.Where(e => e.Value.Id == rune).FirstOrDefault().Value : null;
                Infusion = int.TryParse(parts[2], out int infusion) ? BuildsManager.Data.Infusions.Where(e => e.Value.Id == infusion).FirstOrDefault().Value : null;
            }
        }

        public override string ToCode()
        {
            return $"[{Stat?.Id ?? -1}|{Rune?.Id ?? -1}|{Infusion?.Id ?? -1}]";
        }
    }
}
