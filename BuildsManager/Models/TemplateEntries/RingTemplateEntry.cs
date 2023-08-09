using Kenedia.Modules.BuildsManager.Models.Templates;
using System.Linq;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.DataModels.Stats;

namespace Kenedia.Modules.BuildsManager.TemplateEntries
{
    public class RingTemplateEntry : TemplateEntry
    {
        public RingTemplateEntry(TemplateSlot slot) : base(slot)
        {
        }

        public BaseItem Item { get; } = BuildsManager.Data.Trinkets.TryGetValue(80058, out Trinket ring) ? ring : null;

        public Infusion Infusion_1 { get; set; }

        public Infusion Infusion_2 { get; set; }

        public Infusion Infusion_3 { get; set; }

        public Stat Stat { get; set; }

        public override void FromCode(string code)
        {
            string[] parts = GetCode(code).Split('|');

            if (parts.Length == 4)
            {
                Stat = int.TryParse(parts[0], out int stat) ? BuildsManager.Data.Stats.Where(e => e.Value.Id == stat).FirstOrDefault().Value : null;
                Infusion_1 = int.TryParse(parts[1], out int infusion1) ? BuildsManager.Data.Infusions.Where(e => e.Value.Id == infusion1).FirstOrDefault().Value : null;
                Infusion_2 = int.TryParse(parts[2], out int infusion2) ? BuildsManager.Data.Infusions.Where(e => e.Value.Id == infusion2).FirstOrDefault().Value : null;
                Infusion_3 = int.TryParse(parts[3], out int infusion3) ? BuildsManager.Data.Infusions.Where(e => e.Value.Id == infusion3).FirstOrDefault().Value : null;
            }
        }

        public override string ToCode()
        {
            return $"[{Stat?.Id ?? -1}|{Infusion_1?.Id ?? -1}|{Infusion_2?.Id ?? -1}|{Infusion_3?.Id ?? -1}]";
        }
    }
}
