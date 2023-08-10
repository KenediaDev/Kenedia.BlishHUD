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

        public abstract byte[] AddToCodeArray(byte[] array);

        public abstract byte[] GetFromCodeArray(byte[] array);
    }
}
