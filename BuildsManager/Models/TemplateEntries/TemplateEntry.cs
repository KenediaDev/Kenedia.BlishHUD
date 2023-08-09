using Kenedia.Modules.BuildsManager.Models.Templates;

namespace Kenedia.Modules.BuildsManager.TemplateEntries
{
    public abstract class TemplateEntry
    {
        public TemplateEntry(TemplateSlot slot)
        {
            Slot = slot;
        }

        public TemplateSlot Slot { get; }

        public abstract string ToCode();

        public abstract void FromCode(string code);

        public string GetCode(string code)
        {
            if (code.StartsWith("[")) code = code.Substring(1, code.Length - 1);
            if (code.EndsWith("]")) code = code.Substring(0, code.Length - 1);
            return code;
        }
    }
}
