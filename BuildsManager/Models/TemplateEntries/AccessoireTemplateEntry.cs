using Kenedia.Modules.BuildsManager.Models.Templates;
using System.Linq;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.DataModels.Stats;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.BuildsManager.Utility;
using System;

namespace Kenedia.Modules.BuildsManager.TemplateEntries
{
    public class AccessoireTemplateEntry : TemplateEntry
    {
        public AccessoireTemplateEntry(TemplateSlot slot) : base(slot)
        {
        }

        public BaseItem Item { get; } = BuildsManager.Data.Trinkets.TryGetValue(80002, out Trinket accessoire) ? accessoire : null;

        public Stat Stat { get; set; }

        public Infusion Infusion { get; set; }

        public override short[] AddToCodeArray(short[] array)
        {
            return array.Concat(new short[]
            {
                (short)(Stat?.MappedId ?? -1),
                (short)(Infusion?.MappedId ?? -1),
            }).ToArray();
        }

        public override short[] GetFromCodeArray(short[] array)
        {
            int newStartIndex = 2;

            Stat = byte.TryParse($"{array[0]}", out byte stat) ? BuildsManager.Data.Stats.Where(e => e.Value.MappedId == stat).FirstOrDefault().Value : null;
            Infusion = byte.TryParse($"{array[1]}", out byte infusion) ? BuildsManager.Data.Infusions.Where(e => e.Value.MappedId == infusion).FirstOrDefault().Value : null;

            return GearTemplateCode.RemoveFromStart(array, newStartIndex);
        }
    }
}
