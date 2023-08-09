using Kenedia.Modules.BuildsManager.Models.Templates;
using System.Linq;
using Kenedia.Modules.BuildsManager.DataModels.Items;

namespace Kenedia.Modules.BuildsManager.TemplateEntries
{
    public class JadeBotTemplateEntry : TemplateEntry
    {
        public JadeBotTemplateEntry(TemplateSlot slot) : base(slot)
        {
        }

        public JadeBotCore Item { get; set; }

        public override void FromCode(string code)
        {
            string[] parts = GetCode(code).Split('|');

            if (parts.Length == 1)
            {
                Item = int.TryParse(parts[0], out int id) ? BuildsManager.Data.JadeBotCores.Values.Where(e => e.Id == id).FirstOrDefault() : null;
            }
        }

        public override string ToCode()
        {
            return $"[{Item?.Id ?? -1}]";
        }
    }
}
