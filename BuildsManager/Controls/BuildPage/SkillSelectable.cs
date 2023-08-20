using Kenedia.Modules.BuildsManager.DataModels.Professions;

namespace Kenedia.Modules.BuildsManager.Controls.BuildPage
{
    public class SkillSelectable : Selectable<Skill>
    {
        public SkillSelectable()
        {
            Tooltip = new SkillTooltip();
        }

        protected override void ApplyData(object sender, Core.Models.ValueChangedEventArgs<Skill> e)
        {
            base.ApplyData(sender, e);

            if (Tooltip is SkillTooltip skillTooltip)
            {
                skillTooltip.Skill = e.NewValue;
            }
        }
    }
}
