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
        private readonly AsyncTexture2D _subWindowEmblem = AsyncTexture2D.FromAssetId(156027);
        private readonly BitmapFont _titleFont = GameService.Content.DefaultFont32;
        protected readonly FlowPanel ContentPanel;
        private Rectangle _subEmblemRectangle;
        private Rectangle _mainEmblemRectangle;
        private Rectangle _titleRectangle;

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

        }

        public SemVer.Version Version { get; set; }

        public string Name { get; set; } = "Module";

        public AsyncTexture2D MainWindowEmblem { get; set; } = AsyncTexture2D.FromAssetId(156015);

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            _subEmblemRectangle = new(-43 + 64, -58 + 64, 64, 64);
            _mainEmblemRectangle = new(-43, -58, 128, 128);

            MonoGame.Extended.RectangleF titleBounds = _titleFont.GetStringRectangle(Name);
            _titleRectangle = new(80, 5, (int)titleBounds.Width, Math.Max(30, (int)titleBounds.Height));
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintAfterChildren(spriteBatch, bounds);

            if (MainWindowEmblem != null)
            {
                spriteBatch.DrawOnCtrl(
                    this,
                    MainWindowEmblem,
                    _mainEmblemRectangle,
                    MainWindowEmblem.Bounds,
                    Color.White,
                    0f,
                    default);
            }

            spriteBatch.DrawOnCtrl(
                this,
                _subWindowEmblem,
                _subEmblemRectangle,
                _subWindowEmblem.Bounds,
                Color.White,
                0f,
                default);

            if (_titleRectangle.Width < bounds.Width - (_subEmblemRectangle.Width - 20))
            {
                spriteBatch.DrawStringOnCtrl(
                    this,
                    Name,
                    _titleFont,
                    _titleRectangle,
                    Colors.ColonialWhite, // new Color(247, 231, 182, 97),
                    false,
                    HorizontalAlignment.Left,
                    VerticalAlignment.Bottom);
            }
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            Children.DisposeAll();
            ContentPanel.Children.DisposeAll();

            _subWindowEmblem.Dispose();
            MainWindowEmblem = null;
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            if (Version != null) spriteBatch.DrawStringOnCtrl(this, $"v. {Version}", Content.DefaultFont16, new(bounds.Right - 150, bounds.Top + 10, 100, 30), Color.White, false, true, 1, HorizontalAlignment.Right, VerticalAlignment.Top);
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
