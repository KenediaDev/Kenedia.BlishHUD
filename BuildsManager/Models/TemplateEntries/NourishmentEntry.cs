﻿using Kenedia.Modules.BuildsManager.Models.Templates;
using System.Linq;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.Utility;

namespace Kenedia.Modules.BuildsManager.TemplateEntries
{
    public class NourishmentEntry : TemplateEntry
    {
        public NourishmentEntry(TemplateSlot slot) : base(slot)
        {
        }

        public Nourishment Item { get; set; }

        public override short[] AddToCodeArray(short[] array)
        {
            return array.Concat(new short[]
            {
                (short)(Item?.MappedId ?? -1),
            }).ToArray();
        }

        public override void FromCode(string code)
        {
            string[] parts = GetCode(code).Split('|');

            if (parts.Length == 1)
            {
                Item = int.TryParse(parts[0], out int id) ? BuildsManager.Data.Nourishments.Values.Where(e => e.MappedId == id).FirstOrDefault() : null;
            }
        }

        public override short[] GetFromCodeArray(short[] array)
        {
            int newStartIndex = 1;

            Item = int.TryParse($"{array[0]}", out int id) ? BuildsManager.Data.Nourishments.Values.Where(e => e.MappedId == id).FirstOrDefault() : null;

            return GearTemplateCode.RemoveFromStart(array, newStartIndex);
        }

        public override string ToCode()
        {
            return $"[{Item?.MappedId ?? -1}]";
        }
    }
}
