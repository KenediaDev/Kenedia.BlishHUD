using Kenedia.Modules.BuildsManager.Models;

namespace Kenedia.Modules.BuildsManager.Extensions
{
    public static class TemplateSlotsExtension
    {
        public static bool IsTerrestrial(this TemplateSlots slot)
        {
            return slot is TemplateSlots.TerrestrialHeal or TemplateSlots.TerrestrialUtility1 or TemplateSlots.TerrestrialUtility2 or TemplateSlots.TerrestrialUtility3 or TemplateSlots.TerrestrialElite;
        }

        public static bool IsAquatic(this TemplateSlots slot)
        {
            return slot is TemplateSlots.AquaticHeal or TemplateSlots.AquaticUtility1 or TemplateSlots.AquaticUtility2 or TemplateSlots.AquaticUtility3 or TemplateSlots.AquaticElite;
        }
    }
}
