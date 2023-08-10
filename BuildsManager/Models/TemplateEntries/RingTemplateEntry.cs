using Kenedia.Modules.BuildsManager.Models.Templates;
using System.Linq;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.DataModels.Stats;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.BuildsManager.Utility;

namespace Kenedia.Modules.BuildsManager.TemplateEntries
{
    public class 
        RingTemplateEntry : TemplateEntry
    {
        public RingTemplateEntry(TemplateSlot slot) : base(slot)
        {
        }

        public BaseItem Item { get; } = BuildsManager.Data.Trinkets.TryGetValue(80058, out Trinket ring) ? ring : null;

        public Infusion Infusion_1 { get; set; }

        public Infusion Infusion_2 { get; set; }

        public Infusion Infusion_3 { get; set; }

        public Stat Stat { get; set; }

        public override short[] AddToCodeArray(short[] array)
        {
            return array.Concat(new short[]
            {
                (short)(Stat?.MappedId ?? -1),
                (short)(Infusion_1?.MappedId ?? -1),
                (short)(Infusion_2?.MappedId ?? -1),
                (short)(Infusion_3?.MappedId ?? -1),
            }).ToArray();
        }

        public override void FromCode(string code)
        {
            string[] parts = GetCode(code).Split('|');

            if (parts.Length == 4)
            {
                Stat = int.TryParse(parts[0], out int stat) ? BuildsManager.Data.Stats.Where(e => e.Value.MappedId == stat).FirstOrDefault().Value : null;
                Infusion_1 = int.TryParse(parts[1], out int infusion1) ? BuildsManager.Data.Infusions.Where(e => e.Value.MappedId == infusion1).FirstOrDefault().Value : null;
                Infusion_2 = int.TryParse(parts[2], out int infusion2) ? BuildsManager.Data.Infusions.Where(e => e.Value.MappedId == infusion2).FirstOrDefault().Value : null;
                Infusion_3 = int.TryParse(parts[3], out int infusion3) ? BuildsManager.Data.Infusions.Where(e => e.Value.MappedId == infusion3).FirstOrDefault().Value : null;
            }
        }

        public override short[] GetFromCodeArray(short[] array)
        {
            int newStartIndex = 4;

            Stat = int.TryParse($"{array[0]}", out int stat) ? BuildsManager.Data.Stats.Where(e => e.Value.MappedId == stat).FirstOrDefault().Value : null;
            Infusion_1 = int.TryParse($"{array[1]}", out int infusion1) ? BuildsManager.Data.Infusions.Where(e => e.Value.MappedId == infusion1).FirstOrDefault().Value : null;
            Infusion_2 = int.TryParse($"{array[2]}", out int infusion2) ? BuildsManager.Data.Infusions.Where(e => e.Value.MappedId == infusion2).FirstOrDefault().Value : null;
            Infusion_3 = int.TryParse($"{array[3]}", out int infusion3) ? BuildsManager.Data.Infusions.Where(e => e.Value.MappedId == infusion3).FirstOrDefault().Value : null;

            return GearTemplateCode.RemoveFromStart(array, newStartIndex);
        }

        public override string ToCode()
        {
            return $"[{Stat?.MappedId ?? -1}|{Infusion_1?.MappedId ?? -1}|{Infusion_2?.MappedId ?? -1}|{Infusion_3?.MappedId ?? -1}]";
        }
    }
}
