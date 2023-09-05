using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.Core.DataModels;
using Kenedia.Modules.Core.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kenedia.Modules.BuildsManager.Controls.BuildPage
{
    public class SkillSelectable : Selectable<Skill>
    {
        private readonly DetailedTexture _noAquaticFlagTexture = new(157145) { TextureRegion = new(16, 16, 96, 96) };

        public SkillSelectable()
        {
            Tooltip = new SkillTooltip();
        }

        public Enviroment Enviroment { get; set; }

        protected override void ApplyData(object sender, Core.Models.ValueChangedEventArgs<Skill> e)
        {
            base.ApplyData(sender, e);

            if (Tooltip is SkillTooltip skillTooltip)
            {
                skillTooltip.Skill = e.NewValue;
            }
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            _noAquaticFlagTexture.Bounds = new(0, 0, Width, Height);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.Paint(spriteBatch, bounds);

            if (Data is not null and Skill skill && skill.Flags.HasFlag(Gw2Sharp.SkillFlag.NoUnderwater) && Enviroment is Enviroment.Aquatic)
            {
                _noAquaticFlagTexture.Draw(this, spriteBatch);
            }
        }
    }
}
