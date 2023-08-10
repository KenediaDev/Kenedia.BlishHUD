using Kenedia.Modules.BuildsManager.Models.Templates;
using System.Linq;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.DataModels.Stats;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.BuildsManager.Utility;
using System;

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

        public override byte[] AddToCodeArray(byte[] array)
        {
            return array.Concat(new byte[]
            {
                Stat?.MappedId ?? 0,
                Rune ?.MappedId ?? 0,
                Infusion ?.MappedId ?? 0,
            }).ToArray();
        }

        public override byte[] GetFromCodeArray(byte[] array)
        {
            int newStartIndex = 3;

            Stat = BuildsManager.Data.Stats.Where(e => e.Value.MappedId == array[0]).FirstOrDefault().Value;
            Rune = BuildsManager.Data.PveRunes.Where(e => e.Value.MappedId == array[1]).FirstOrDefault().Value;
            Infusion = BuildsManager.Data.Infusions.Where(e => e.Value.MappedId == array[2]).FirstOrDefault().Value;

            return GearTemplateCode.RemoveFromStart(array, newStartIndex);
        }
    }
}
