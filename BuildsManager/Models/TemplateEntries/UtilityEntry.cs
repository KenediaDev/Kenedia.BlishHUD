using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.BuildsManager.Utility;
using System.Linq;

namespace Kenedia.Modules.BuildsManager.TemplateEntries
{
    public class UtilityEntry : TemplateEntry
    {
        public UtilityEntry(TemplateSlot slot) : base(slot)
        {
        }

        public DataModels.Items.Utility Item { get; set; }

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
                Item = int.TryParse(parts[0], out int id) ? BuildsManager.Data.Utilities.Values.Where(e => e.MappedId == id).FirstOrDefault() : null;
            }
        }

        public override short[] GetFromCodeArray(short[] array)
        {
            int newStartIndex = 1;

            Item = int.TryParse($"{array[0]}", out int id) ? BuildsManager.Data.Utilities.Values.Where(e => e.MappedId == id).FirstOrDefault() : null;

            return GearTemplateCode.RemoveFromStart(array, newStartIndex);
        }

        public override string ToCode()
        {
            return $"[{Item?.MappedId ?? -1}]";
        }
    }
}
