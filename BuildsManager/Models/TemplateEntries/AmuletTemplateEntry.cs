using Kenedia.Modules.BuildsManager.Models.Templates;
using System.Linq;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.DataModels.Stats;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.BuildsManager.Utility;
using System;

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

        public override short[] AddToCodeArray(short[] array)
        {
            return array.Concat(new short[]
            {
                (short)(Stat?.MappedId ?? -1),
                (short)(Enrichment?.MappedId ?? -1),
            }).ToArray();
        }

        public override short[] GetFromCodeArray(short[] array)
        {
            int newStartIndex = 2;

            Stat = byte.TryParse($"{array[0]}", out byte stat) ? BuildsManager.Data.Stats.Where(e => e.Value.MappedId == stat).FirstOrDefault().Value : null;
            Enrichment = byte.TryParse($"{array[1]}", out byte enrichment) ? BuildsManager.Data.Enrichments.Where(e => e.Value.MappedId == enrichment).FirstOrDefault().Value : null;

            return GearTemplateCode.RemoveFromStart(array, newStartIndex);
        }
    }
}
