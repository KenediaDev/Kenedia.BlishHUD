using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Kenedia.Modules.Core.Models;
using Gw2Sharp;
using Kenedia.Modules.BuildsManager.Models.Templates;

namespace Kenedia.Modules.BuildsManager.Controls.Selectables
{
    public class LegendSelectable : Selectable<Legend>
    {
        private readonly DetailedTexture _noAquaticFlagTexture = new(157145) { TextureRegion = new(16, 16, 96, 96), };

        public SkillTooltip SkillTooltip { get; }

        public LegendSelectable()
        {
            Tooltip = SkillTooltip = new SkillTooltip();
        }

        public LegendSlotType LegendSlot { get; set; } = LegendSlotType.TerrestrialActive;

        protected override void ApplyData(object sender, ValueChangedEventArgs<Legend?> e)
        {
            base.ApplyData(sender, e);

            SkillTooltip.Skill = e.NewValue.Swap;
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            _noAquaticFlagTexture.Bounds = new(0, 0, Width, Height);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.Paint(spriteBatch, bounds);

            if (LegendSlot is LegendSlotType.AquaticActive or LegendSlotType.AquaticInactive && Data?.Swap.Flags.HasFlag(SkillFlag.NoUnderwater) == true)
                _noAquaticFlagTexture.Draw(this, spriteBatch, RelativeMousePosition, Color.White);
        }
    }
}
