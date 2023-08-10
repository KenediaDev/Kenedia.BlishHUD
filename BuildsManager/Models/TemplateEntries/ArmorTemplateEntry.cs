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

        public override short[] AddToCodeArray(short[] array)
        {
            return array.Concat(new short[]
            {
                (short)(Stat?.MappedId ?? -1),
                (short)(Rune?.MappedId ?? -1),
                (short)(Infusion?.MappedId ?? -1),
            }).ToArray();
        }

        public override void FromCode(string code)
        {
            string[] parts = GetCode(code).Split('|');

            if (parts.Length == 3)
            {
                Stat = int.TryParse(parts[0], out int stat) ? BuildsManager.Data.Stats.Where(e => e.Value.MappedId == stat).FirstOrDefault().Value : null;
                Rune = int.TryParse(parts[1], out int rune) ? BuildsManager.Data.PveRunes.Where(e => e.Value.MappedId == rune).FirstOrDefault().Value : null;
                Infusion = int.TryParse(parts[2], out int infusion) ? BuildsManager.Data.Infusions.Where(e => e.Value.MappedId == infusion).FirstOrDefault().Value : null;
            }
        }

        public override short[] GetFromCodeArray(short[] array)
        {
            int newStartIndex = 3;

            Stat = int.TryParse($"{array[0]}", out int stat) ? BuildsManager.Data.Stats.Where(e => e.Value.MappedId == stat).FirstOrDefault().Value : null;
            Rune = int.TryParse($"{array[1]}", out int rune) ? BuildsManager.Data.PveRunes.Where(e => e.Value.MappedId == rune).FirstOrDefault().Value : null;
            Infusion = int.TryParse($"{array[2]}", out int infusion_1) ? BuildsManager.Data.Infusions.Where(e => e.Value.MappedId == infusion_1).FirstOrDefault().Value : null;

            return GearTemplateCode.RemoveFromStart(array, newStartIndex);
        }

        public override string ToCode()
        {
            return $"[{Stat?.MappedId ?? -1}|{Rune?.MappedId ?? -1}|{Infusion?.MappedId ?? -1}]";
        }
    }
}
