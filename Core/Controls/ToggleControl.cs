using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Res;
using Kenedia.Modules.Core.Services;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using static Blish_HUD.ContentService;

namespace Kenedia.Modules.Core.Controls
{
    public class ToggleControl : Control
    {
        protected AsyncTexture2D ToggleDot { get; set; } = AsyncTexture2D.FromAssetId(157336);

        protected Texture2D ToggleAreaLeft { get; set; } = TexturesService.GetTextureFromRef(textures_common.ToggleAreaLeft, nameof(textures_common.ToggleAreaLeft));

        protected Texture2D ToggleAreaMid { get; set; } = TexturesService.GetTextureFromRef(textures_common.ToggleAreaMid, nameof(textures_common.ToggleAreaMid));

        protected Texture2D ToggleAreaRight { get; set; } = TexturesService.GetTextureFromRef(textures_common.ToggleAreaRight, nameof(textures_common.ToggleAreaRight));

        protected Rectangle ToggleDotBounds { get; set; }

        protected Rectangle ToggleDotDrawBounds { get; set; } = new(4, 4, 24, 24);

        protected Rectangle ToggleBounds { get; set; }

        protected Rectangle ToggleBoundsLeft { get; set; }

        protected Rectangle ToggleBoundsRight { get; set; }

        public string TextLeft { get; set; } = "Option 1";

        protected Rectangle TextLeftBounds { get; set; }

        public string TextRight { get; set; } = "Option 2";

        protected Rectangle TextRightBounds { get; set; }

        public BitmapFont Font { get; set; } = Content.DefaultFont16;

        public Color FontColor { get; set; } = Colors.OldLace;

        public ToggleState State { get; set; } = ToggleState.Option1;

        public int TogglePadding { get; set; } = 2;

        public Point ToggleDotSize { get; private set; } = new Point(25, 25);

        public Point ToggleSize { get; private set; }

        public enum ToggleState
        {
            OptionNone,
            Option1,
            Option2,
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            RecalculateLayout();

            spriteBatch.DrawStringOnCtrl(this, TextLeft, Font, TextLeftBounds, FontColor * (State is ToggleState.Option1 ? 1 : 0.6F), false, true, 1, HorizontalAlignment.Left, VerticalAlignment.Middle);
            spriteBatch.DrawStringOnCtrl(this, TextRight, Font, TextRightBounds, FontColor * (State is ToggleState.Option2 ? 1 : 0.6F), false, true, 1, HorizontalAlignment.Right, VerticalAlignment.Middle);

            //spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, ToggleBounds, Color.Black * 0.6F);

            spriteBatch.DrawOnCtrl(this, ToggleAreaLeft, ToggleBoundsLeft, Color.Black * 0.6F);
            spriteBatch.DrawOnCtrl(this, ToggleAreaMid, ToggleBounds, Color.Black * 0.6F);
            spriteBatch.DrawOnCtrl(this, ToggleAreaRight, ToggleBoundsRight, Color.Black * 0.6F);

            spriteBatch.DrawOnCtrl(this, ToggleDot, ToggleDotBounds, ToggleDotDrawBounds);

        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            ToggleDotDrawBounds = new(6,6,26,26);
            ToggleSize = new Point(ToggleDotSize.X * 2, ToggleDotSize.Y + (TogglePadding * 2));

            int textWidth = ((Width - ToggleSize.X) / 2) - 10;

            ToggleBoundsLeft = new(textWidth + 10, 0, Height / 2, Height);
            ToggleBoundsRight = new(Right - textWidth - 10, 0, Height / 2, Height);

            ToggleBounds = new(ToggleBoundsLeft.Right, ToggleBoundsLeft.Top, ToggleBoundsRight.Left - ToggleBoundsLeft.Right, ToggleBoundsLeft.Height);

            SetToggleButtonBounds();

            TextLeftBounds = new(0, 0, textWidth, Height);
            TextRightBounds = new(Width - textWidth, 0, textWidth, Height);
        }

        private void SetToggleButtonBounds()
        {
            ToggleDotBounds =
                State is ToggleState.Option1 ? new(ToggleBounds.X + TogglePadding, ToggleBounds.Y + TogglePadding, ToggleDotSize.X, ToggleDotSize.Y) :
                State is ToggleState.Option2 ? new(ToggleBounds.X + ToggleSize.X - ToggleDotSize.X - TogglePadding, ToggleBounds.Y + TogglePadding, ToggleDotSize.X, ToggleDotSize.Y) :
                new(ToggleBounds.Center.X - (ToggleSize.X / 2), ToggleBounds.Y + TogglePadding, ToggleDotSize.X, ToggleDotSize.Y);
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            State = State is ToggleState.Option1 ? ToggleState.Option2 : ToggleState.Option1;
            SetToggleButtonBounds();
        }
    }
}
