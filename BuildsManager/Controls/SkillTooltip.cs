using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Kenedia.Modules.BuildsManager.DataModels.Professions;

namespace Kenedia.Modules.BuildsManager.Controls
{
    public class SkillTooltip : Tooltip
    {
        private readonly SkillTooltipContentControl _skillContentControl;

        public SkillTooltip()
        {
            WidthSizingMode = SizingMode.AutoSize;
            HeightSizingMode = SizingMode.AutoSize;
            AutoSizePadding = new(5);
            _skillContentControl = new() { Parent = this };
        }

        public SkillTooltip(Skill skill) : this()
        {
            _skillContentControl.Skill = skill;
        }

        public Skill Skill
        {
            get => _skillContentControl.Skill;
            set => _skillContentControl.Skill = value;
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle drawBounds, Rectangle scissor)
        {
            if (string.IsNullOrEmpty(_skillContentControl.Title))
                return;

            base.Draw(spriteBatch, drawBounds, scissor);
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            _skillContentControl?.Dispose();
        }
    }
}
