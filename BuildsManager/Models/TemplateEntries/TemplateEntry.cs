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

        public abstract short[] AddToCodeArray(short[] array);

        public abstract short[] GetFromCodeArray(short[] array);
    }
}
