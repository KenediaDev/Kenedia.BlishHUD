using Kenedia.Modules.BuildsManager.Models.Templates;
using System.Linq;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.DataModels.Stats;

namespace Kenedia.Modules.BuildsManager.TemplateEntries
{
    public class AmuletTemplateEntry : TemplateEntry
    {
        public AmuletTemplateEntry(TemplateSlot slot) : base(slot)
        {
        }

        public Stat Stat { get; set; }

        public BaseItem Item { get; } = BuildsManager.Data.Trinkets.TryGetValue(79980, out Trinket accessoire) ? accessoire : null;

        public Enrichment Enrichment { get; set; }

        public override void FromCode(string code)
        {
            string[] parts = GetCode(code).Split('|');

            if (parts.Length == 2)
            {
                Stat = int.TryParse(parts[0], out int stat) ? BuildsManager.Data.Stats.Where(e => e.Value.Id == stat).FirstOrDefault().Value : null;
                Enrichment = int.TryParse(parts[1], out int enrichment) ? BuildsManager.Data.Enrichments.Where(e => e.Value.Id == enrichment).FirstOrDefault().Value : null;
            }
        }

        public override string ToCode()
        {
            return $"[{Stat?.Id ?? -1}|{Enrichment?.Id ?? -1}]";
        }
    }
}
