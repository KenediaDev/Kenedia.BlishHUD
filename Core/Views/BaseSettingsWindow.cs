using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Kenedia.Modules.Core.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using System;
using static Blish_HUD.ContentService;
using FlowPanel = Kenedia.Modules.Core.Controls.FlowPanel;
using Label = Kenedia.Modules.Core.Controls.Label;
using Panel = Kenedia.Modules.Core.Controls.Panel;
using TrackBar = Kenedia.Modules.Core.Controls.TrackBar;

namespace Kenedia.Modules.Core.Views
{

    public class BaseSettingsWindow : StandardWindow
    {
        protected readonly FlowPanel ContentPanel;

        public BaseSettingsWindow(AsyncTexture2D background, Rectangle windowRegion, Rectangle contentRegion) : base(background, windowRegion, contentRegion)
        {
            ContentPanel = new()
            {
                Parent = this,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                Width = ContentRegion.Width,
                Height = ContentRegion.Height,
                ControlPadding = new(0, 10),
                CanScroll = true,
            };

            MainWindowEmblem = AsyncTexture2D.FromAssetId(156015);
            SubWindowEmblem =  AsyncTexture2D.FromAssetId(156027);
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintAfterChildren(spriteBatch, bounds);

        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            Children.DisposeAll();
            ContentPanel.Children.DisposeAll();

            SubWindowEmblem = null;
            MainWindowEmblem = null;
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

        }

        protected (Panel, Label, TrackBar) LabeledTrackbar(Container parent, Func<string> setLocalizedText, Func<string> setLocalizedTooltip)
        {
            var subP = new Panel()
            {
                Parent = parent,
                Width = ContentRegion.Width - 20,
                HeightSizingMode = SizingMode.AutoSize,
            };

            var scaleLabel = new Label()
            {
                Parent = subP,
                AutoSizeWidth = true,
                SetLocalizedText = setLocalizedText,
                SetLocalizedTooltip = setLocalizedTooltip,
            };
            var trackBar = new TrackBar()
            {
                Parent = subP,
                SetLocalizedTooltip = setLocalizedTooltip,
                Location = new Point(250, 0),
            };

            return (subP, scaleLabel, trackBar);
        }
    }
}
