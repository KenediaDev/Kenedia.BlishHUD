using Kenedia.Modules.BuildsManager.Models.Templates;
using System.Linq;

namespace Kenedia.Modules.BuildsManager.TemplateEntries
{
    public class UtilityEntry : TemplateEntry
    {
        public UtilityEntry(TemplateSlot slot) : base(slot)
        {
        }

        public DataModels.Items.Utility Item { get; set; }

        public override void FromCode(string code)
        {
            string[] parts = GetCode(code).Split('|');

            if (parts.Length == 1)
            {
                Item = int.TryParse(parts[0], out int id) ? BuildsManager.Data.Utilities.Values.Where(e => e.Id == id).FirstOrDefault() : null;
            }
        }

        public override string ToCode()
        {
            return $"[{Item?.Id ?? -1}]";
        }
    }
}
