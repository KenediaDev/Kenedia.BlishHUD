using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.BuildsManager.Res;

namespace Kenedia.Modules.BuildsManager.Extensions
{
    public static class AttributeTypeExtension
    {
        public static string GetDisplayName(this AttributeType attribute)
        {
            return attribute switch
            {
                AttributeType.BoonDuration => strings.BoonDuration,
                AttributeType.ConditionDamage => strings.ConditionDamage,
                AttributeType.ConditionDuration => strings.ConditionDuration,
                AttributeType.CritDamage => strings.CritDamage,
                AttributeType.Healing => strings.Healing,
                AttributeType.Power => strings.Power,
                AttributeType.Precision => strings.Precision,
                AttributeType.Toughness => strings.Toughness,
                AttributeType.Vitality => strings.Vitality,
                AttributeType.AgonyResistance => strings.AgonyResistance,
                _ => attribute.ToString(),
            };
        }
    }
}
