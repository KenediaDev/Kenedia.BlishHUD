namespace Kenedia.Modules.BuildsManager.Models.Templates
{
    public static class SkillSlotExtension
    {
        public static SkillSlotType GetEnviromentState(this SkillSlotType slot)
        {
            var tempSlot = slot;
            return tempSlot &= ~(SkillSlotType.Utility_1 | SkillSlotType.Utility_1 | SkillSlotType.Utility_2 | SkillSlotType.Utility_3 | SkillSlotType.Heal | SkillSlotType.Elite);
        }
    }
}
