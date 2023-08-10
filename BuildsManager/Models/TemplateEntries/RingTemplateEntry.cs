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

        public override byte[] AddToCodeArray(byte[] array)
        {
            return array.Concat(new byte[]
            {
                Stat ?.MappedId ?? 0,
                Infusion_1 ?.MappedId ?? 0,
                Infusion_2 ?.MappedId ?? 0,
                Infusion_3 ?.MappedId ?? 0,
            }).ToArray();
        }

        public override byte[] GetFromCodeArray(byte[] array)
        {
            int newStartIndex = 4;

            Stat = BuildsManager.Data.Stats.Where(e => e.Value.MappedId == array[0]).FirstOrDefault().Value;
            Infusion_1 = BuildsManager.Data.Infusions.Where(e => e.Value.MappedId == array[1]).FirstOrDefault().Value;
            Infusion_2 = BuildsManager.Data.Infusions.Where(e => e.Value.MappedId == array[2]).FirstOrDefault().Value;
            Infusion_3 = BuildsManager.Data.Infusions.Where(e => e.Value.MappedId == array[3]).FirstOrDefault().Value;

            return GearTemplateCode.RemoveFromStart(array, newStartIndex);
        }
    }
}
