using Kenedia.Modules.BuildsManager.Models.Templates;

namespace Kenedia.Modules.BuildsManager.Extensions
{
    public static class SkillSlotTypeExtension
    {
        public static bool IsTerrestrial(this SkillSlotType slot)
        {
            return slot.HasFlag(SkillSlotType.Terrestrial);
        }

        public static bool IsAquatic(this SkillSlotType slot)
        {
            return slot.HasFlag(SkillSlotType.Aquatic);
        }

        public static bool IsActive(this SkillSlotType slot)
        {
            return slot.HasFlag(SkillSlotType.Active);
        }

        public static bool IsInactive(this SkillSlotType slot)
        {
            return slot.HasFlag(SkillSlotType.Inactive);
        }

        public static int GetSlotPosition(this SkillSlotType slot)
        {
            var slots = new[]
            {
                SkillSlotType.Heal,
                SkillSlotType.Utility_1,
                SkillSlotType.Utility_2,
                SkillSlotType.Utility_3,
                SkillSlotType.Elite,
            };

            for (int i = 0; i < slots.Length; i++)
            {
                if (slot.HasFlag(slots[i]))
                {
                    return i;
                }
            }

            return 0;
        }
    }
}
