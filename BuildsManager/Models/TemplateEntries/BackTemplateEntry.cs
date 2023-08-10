using Kenedia.Modules.BuildsManager.Models.Templates;
using System.Linq;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.DataModels.Stats;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.BuildsManager.Utility;

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

        public override short[] AddToCodeArray(short[] array)
        {
            return array.Concat(new short[]
            {
                (short)(Stat?.MappedId ?? -1),
                (short)(Infusion_1?.MappedId ?? -1),
                (short)(Infusion_2?.MappedId ?? -1),
            }).ToArray();
        }

        public override short[] GetFromCodeArray(short[] array)
        {
            int newStartIndex = 3;

            Stat = byte.TryParse($"{array[0]}", out byte stat) ? BuildsManager.Data.Stats.Where(e => e.Value.MappedId == stat).FirstOrDefault().Value : null;
            Infusion_1 = byte.TryParse($"{array[1]}", out byte infusion1) ? BuildsManager.Data.Infusions.Where(e => e.Value.MappedId == infusion1).FirstOrDefault().Value : null;
            Infusion_2 = byte.TryParse($"{array[2]}", out byte infusion2) ? BuildsManager.Data.Infusions.Where(e => e.Value.MappedId == infusion2).FirstOrDefault().Value : null;

            return GearTemplateCode.RemoveFromStart(array, newStartIndex);
        }
    }
}
