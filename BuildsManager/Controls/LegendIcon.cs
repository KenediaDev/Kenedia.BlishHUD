using Blish_HUD.Controls;
using Blish_HUD.Input;
using Gw2Sharp;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Kenedia.Modules.BuildsManager.Controls
{
    public class LegendIcon : Control
    {
        private Legend _legend;

        private readonly DetailedTexture _selector = new(157138, 157140);
        private readonly DetailedTexture _fallBackTexture = new(157154);
        private readonly DetailedTexture _hoveredFrameTexture = new(157143) { TextureRegion = new(8, 8, 112, 112), };
        private readonly DetailedTexture _noAquaticFlagTexture = new(157145) { TextureRegion = new(16, 16, 96, 96), };
        private readonly DetailedTexture _texture = new() { TextureRegion = new(14, 14, 100, 100), };

        public LegendIcon()
        {
            Tooltip = SkillTooltip = new SkillTooltip();

            Size = new(48, 62);
        }

        public Legend Legend { get => _legend; set => Common.SetProperty(ref _legend, value, ApplyLegend); }

        public LegendSlotType LegendSlot { get; set; }

        public Action<LegendIcon> LeftClickAction { get; set; }

        public Action<LegendIcon> RightClickAction { get; set; }

        public SkillTooltip SkillTooltip { get; }

        public bool IsActive { get; set; } = false;

        private void ApplyLegend()
        {
            _texture.Texture = Legend?.Swap.Icon;
            SkillTooltip.Skill = Legend?.Swap;
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int selectorHeight = (int)(15 / 79.0 * Height);
            _selector.Bounds = new Rectangle(0, 0, Width, selectorHeight);

            _texture.Bounds = new Rectangle(0, selectorHeight - 1, Width, Height - selectorHeight - 5);
            _hoveredFrameTexture.Bounds = new Rectangle(0, selectorHeight - 1, Width, Height - selectorHeight - 5);
            _noAquaticFlagTexture.Bounds = new Rectangle(0, selectorHeight - 1, Width, Height - selectorHeight - 5);

            Size = new(48, 62);
            ClipsBounds = true;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (Legend is not null)
                _texture.Draw(this, spriteBatch, RelativeMousePosition, IsActive ? Color.White : Color.Gray);
            else
                _fallBackTexture.Draw(this, spriteBatch, RelativeMousePosition, Color.White);

            if (MouseOver)
                _hoveredFrameTexture.Draw(this, spriteBatch, RelativeMousePosition, Color.White);

            if (LegendSlot is LegendSlotType.AquaticActive or LegendSlotType.AquaticInactive && Legend?.Swap.Flags.HasFlag(SkillFlag.NoUnderwater) == true)
                _noAquaticFlagTexture.Draw(this, spriteBatch, RelativeMousePosition, Color.White);

            _selector?.Draw(this, spriteBatch, RelativeMousePosition);
        }

        protected override void OnLeftMouseButtonPressed(MouseEventArgs e)
        {
            base.OnLeftMouseButtonPressed(e);
            LeftClickAction?.Invoke(this);
        }

        protected override void OnRightMouseButtonPressed(MouseEventArgs e)
        {
            base.OnRightMouseButtonPressed(e);
            RightClickAction?.Invoke(this);
        }
    }
}
