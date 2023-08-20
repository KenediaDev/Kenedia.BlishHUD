using Blish_HUD.Controls;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kenedia.Modules.BuildsManager.Controls.BuildPage
{
    public class SkillControl : Control
    {
        private readonly SkillIcon _skillIcon = new() { ShowSelector = true };
        private Skill _skill;

        public SkillControl()
        {
            Tooltip = new SkillTooltip();
        }

        public Skill Skill { get => _skill; set => Common.SetProperty(ref _skill, value, ApplySkill); }

        public SkillSlotType Slot { get; set; } = SkillSlotType.None;

        public bool ShowSelector { get => _skillIcon.ShowSelector; set => _skillIcon.ShowSelector = value; }

        public bool IsSelectorHovered => ShowSelector && _skillIcon.Selector.Hovered;

        private void ApplySkill(object sender, Core.Models.ValueChangedEventArgs<Skill> e)
        {
            _skillIcon.Skill = Skill;

            if(Tooltip is SkillTooltip tooltip)
            {
                tooltip.Skill = Skill;
            }
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            _skillIcon.Draw(this, spriteBatch, Slot.HasFlag(SkillSlotType.Terrestrial), MouseOver ? RelativeMousePosition : null);
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int selectorHeight = 15;
            _skillIcon.Selector.Bounds = new(Point.Zero, new(Width, selectorHeight));
            _skillIcon.Bounds = new(new(0, selectorHeight - 2), new(Width, Height - selectorHeight));
        }
    }
}
