using Kenedia.Modules.BuildsManager.Models.Templates;
using System.Linq;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.DataModels.Stats;

namespace Kenedia.Modules.BuildsManager.TemplateEntries
{
    public class BackTemplateEntry : TemplateEntry
    {
        public BackTemplateEntry(TemplateSlot slot) : base(slot)
        {
        }

        public Stat Stat { get; set; }

        public BaseItem Item { get; } = BuildsManager.Data.Trinkets.TryGetValue(94947, out Trinket accessoire) ? accessoire : null;

        public Infusion Infusion_1 { get; set; }

        public Infusion Infusion_2 { get; set; }

        public override void FromCode(string code)
        {
            string[] parts = GetCode(code).Split('|');

            if (parts.Length == 3)
            {
                Stat = int.TryParse(parts[0], out int stat) ? BuildsManager.Data.Stats.Where(e => e.Value.Id == stat).FirstOrDefault().Value : null;
                Infusion_1 = int.TryParse(parts[1], out int infusion1) ? BuildsManager.Data.Infusions.Where(e => e.Value.Id == infusion1).FirstOrDefault().Value : null;
                Infusion_2 = int.TryParse(parts[2], out int infusion2) ? BuildsManager.Data.Infusions.Where(e => e.Value.Id == infusion2).FirstOrDefault().Value : null;
            }
        }

        public override string ToCode()
        {
            return $"[{Stat?.Id ?? -1}|{Infusion_1?.Id ?? -1}|{Infusion_2?.Id ?? -1}]";
        }
    }
}
