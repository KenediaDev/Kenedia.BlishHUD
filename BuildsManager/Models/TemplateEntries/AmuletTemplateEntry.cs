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

        public override byte[] AddToCodeArray(byte[] array)
        {
            return array.Concat(new byte[]
            {
                Stat?.MappedId ?? 0,
                Enrichment ?.MappedId ?? 0,
            }).ToArray();
        }

        public override byte[] GetFromCodeArray(byte[] array)
        {
            int newStartIndex = 2;

            Stat = BuildsManager.Data.Stats.Where(e => e.Value.MappedId == array[0]).FirstOrDefault().Value;
            Enrichment = BuildsManager.Data.Enrichments.Where(e => e.Value.MappedId == array[1]).FirstOrDefault().Value;

            return GearTemplateCode.RemoveFromStart(array, newStartIndex);
        }
    }
}
