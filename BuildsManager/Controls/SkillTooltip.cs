using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.BuildsManager.Models;
using System;
using System.Diagnostics;
using MonoGame.Extended.BitmapFonts;

namespace Kenedia.Modules.BuildsManager.Controls
{
    public class SkillTooltip : Tooltip
    {
        private readonly SkillTooltipContentControl _skillContentControl;

        public SkillTooltip(SkillSlotType skillSlot, TemplatePresenter templatePresenter)
        {
            WidthSizingMode = SizingMode.AutoSize;
            HeightSizingMode = SizingMode.AutoSize;
            AutoSizePadding = new(5);
            _skillContentControl = new(skillSlot, templatePresenter) { Parent = this };
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
