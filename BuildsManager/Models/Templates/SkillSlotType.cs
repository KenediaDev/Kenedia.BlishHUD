using System;

namespace Kenedia.Modules.BuildsManager.Models.Templates
{
    [Flags]
    public enum SkillSlotType
    {
        None = 0,
        Active = 1 << 0,
        Inactive = 1 << 1,
        Terrestrial = 1 << 2,
        Aquatic = 1 << 3,
        Heal = 1 << 4,
        Utility_1 = 1 << 5,
        Utility_2 = 1 << 6,
        Utility_3 = 1 << 7,
        Elite = 1 << 8,
    }
}
