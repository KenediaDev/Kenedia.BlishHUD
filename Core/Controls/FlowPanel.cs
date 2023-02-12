using Blish_HUD.Content;
using Blish_HUD;
using Gw2Sharp.WebApi;
using Kenedia.Modules.Core.Interfaces;
using Kenedia.Modules.Core.Structs;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Blish_HUD.Controls;
using Kenedia.Modules.Core.Services;

namespace Kenedia.Modules.Core.Controls
{

    public class FlowPanel : Blish_HUD.Controls.FlowPanel, ILocalizable
    {
        private readonly List<(Rectangle, float)> _leftBorders = new();
        private readonly List<(Rectangle, float)> _topBorders = new();
        private readonly List<(Rectangle, float)> _rightBorders = new();
        private readonly List<(Rectangle, float)> _bottomBorders = new();
        private Func<string> _setLocalizedTooltip;

        private RectangleDimensions _contentPadding = new(0);
        private RectangleDimensions _borderWidth = new(0);
        private Rectangle _backgroundBounds;

        public FlowPanel()
        {
            LocalizingService.LocaleChanged  += UserLocale_SettingChanged;
            UserLocale_SettingChanged(null, null);
        }

        public RectangleDimensions BorderWidth
        {
            get => _borderWidth;
            set
            {
                _borderWidth = value;
                RecalculateLayout();
            }
        }

        public RectangleDimensions ContentPadding
        {
            get => _contentPadding;
            set
            {
                _contentPadding = value;
                RecalculateLayout();
            }
        }

        public Color? BorderColor { get; set; }

        public Color? HoveredBorderColor { get; set; }

        public AsyncTexture2D BackgroundImage { get; set; }

        public Color? BackgroundImageColor { get; set; } = Color.White;

        public Color? BackgroundImageHoveredColor { get; set; }

        public new Color? BackgroundColor { get; set; }

        public Color? BackgroundHoveredColor { get; set; }

        public Rectangle? TextureRectangle { get; set; }

        public Func<string> SetLocalizedTooltip
        {
            get => _setLocalizedTooltip;
            set
            {
                _setLocalizedTooltip = value;
                BasicTooltipText = value?.Invoke();
            }
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            _contentRegion = new(
                _contentPadding.Left + BorderWidth.Left,
                _contentPadding.Top + BorderWidth.Top,
                Width - _contentPadding.Horizontal - BorderWidth.Horizontal - (WidthSizingMode == SizingMode.AutoSize ? AutoSizePadding.X : 0),
                Height - _contentPadding.Vertical - BorderWidth.Vertical - (HeightSizingMode == SizingMode.AutoSize ? AutoSizePadding.Y : 0));

            _backgroundBounds = new(
                Math.Max(BorderWidth.Left - 2, 0),
                Math.Max(BorderWidth.Top - 2, 0),
                Width - Math.Max(BorderWidth.Horizontal - 4, 0),
                Height - Math.Max(BorderWidth.Vertical - 4, 0));

            CalculateBorders();
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            Color? backgroundColor = BackgroundHoveredColor != null && MouseOver ? BackgroundHoveredColor : BackgroundColor;
            if (backgroundColor != null)
            {
                spriteBatch.DrawOnCtrl(
                    this,
                    ContentService.Textures.Pixel,
                    _backgroundBounds,
                    Rectangle.Empty,
                    (Color)backgroundColor);
            }

            Color? backgroundImageColor = BackgroundImageHoveredColor != null && MouseOver ? BackgroundImageHoveredColor : BackgroundImageColor;
            if (BackgroundImage != null && backgroundImageColor != null)
            {
                spriteBatch.DrawOnCtrl(
                    this,
                    BackgroundImage,
                    _backgroundBounds,
                    TextureRectangle ?? BackgroundImage.Bounds,
                    (Color)backgroundImageColor,
                    0f,
                    default);
            }

            Color? borderColor = HoveredBorderColor != null && MouseOver ? HoveredBorderColor : BorderColor;
            if (borderColor != null)
            {
                DrawBorders(spriteBatch);
            }
        }

        public void UserLocale_SettingChanged(object sender, ValueChangedEventArgs<Locale> e)
        {
            if (SetLocalizedTooltip != null) BasicTooltipText = SetLocalizedTooltip?.Invoke();
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            GameService.Overlay.UserLocale.SettingChanged -= UserLocale_SettingChanged;
        }

        private void CalculateBorders()
        {
            _topBorders.Clear();
            _leftBorders.Clear();
            _bottomBorders.Clear();
            _rightBorders.Clear();

            var r = new Rectangle(-1, 0, Width + 2, 0);
            int strength = BorderWidth.Top;
            int fadeLines = Math.Max(0, Math.Min(strength - 1, 4));
            if (fadeLines >= 1) _topBorders.Add(new(r = new(0, 0, Width, 1), 0.5f));
            if (fadeLines >= 3) _topBorders.Add(new(r = new(r.Left + 1, r.Bottom, r.Width - 2, 1), 0.7f));
            _topBorders.Add(new(r = new(r.Left + 1, r.Bottom, r.Width - 1, strength - fadeLines), 1f));
            if (fadeLines >= 4) _topBorders.Add(new(r = new(r.Left + 1, r.Bottom, r.Width - 2, 1), 0.7f));
            if (fadeLines >= 2) _topBorders.Add(new(new(r.Left + 1, r.Bottom, r.Width - 2, 1), 0.5f));

            r = new Rectangle(-1, -1, 0, Height + 2);
            strength = BorderWidth.Left;
            fadeLines = Math.Max(0, Math.Min(strength - 1, 4));
            if (fadeLines >= 1) _leftBorders.Add(new(r = new(0, 0, 1, Height), 0.5f));
            if (fadeLines >= 3) _leftBorders.Add(new(r = new(r.Right, r.Top + 1, 1, r.Height - 2), 0.7f));
            _leftBorders.Add(new(r = new(r.Right, r.Top + 1, strength - fadeLines, r.Height - 2), 1f));
            if (fadeLines >= 4) _leftBorders.Add(new(r = new(r.Right, r.Top + 1, 1, r.Height - 2), 0.7f));
            if (fadeLines >= 2) _leftBorders.Add(new(new(r.Right, r.Top + 1, 1, r.Height - 2), 0.5f));

            r = new Rectangle(Width, -1, 0, Height + 2);
            strength = BorderWidth.Right;
            fadeLines = Math.Max(0, Math.Min(strength - 1, 4));
            if (fadeLines >= 1) _rightBorders.Add(new(r = new(Width - 1, 0, 1, Height), 0.5f));
            if (fadeLines >= 3) _rightBorders.Add(new(r = new(r.Left - 1, r.Top + 1, 1, r.Height - 2), 0.7f));
            _rightBorders.Add(new(r = new(r.Left - (strength - fadeLines), r.Top + 1, strength - fadeLines, r.Height - 2), 1f));
            if (fadeLines >= 4) _rightBorders.Add(new(r = new(r.Left - 1, r.Top + 1, 1, r.Height - 2), 0.7f));
            if (fadeLines >= 2) _rightBorders.Add(new(new(r.Left - 1, r.Top + 1, 1, r.Height - 2), 0.5f));

            r = new Rectangle(-1, Height, Width + 2, 2);
            strength = BorderWidth.Bottom;
            fadeLines = Math.Max(0, Math.Min(strength - 1, 4));
            if (fadeLines >= 1) _bottomBorders.Add(new(r = new(0, Height - 1, Width, 1), 0.5f));
            if (fadeLines >= 3) _bottomBorders.Add(new(r = new(r.Left + 1, r.Top - 1, r.Width - 2, 1), 0.7f));
            _bottomBorders.Add(new(r = new(r.Left + 1, r.Top - (strength - fadeLines), r.Width - 2, strength - fadeLines), 1f));
            if (fadeLines >= 4) _bottomBorders.Add(new(r = new(r.Left + 1, r.Top - 1, r.Width - 2, 1), 0.7f));
            if (fadeLines >= 2) _bottomBorders.Add(new(new(r.Left + 1, r.Top - 1, r.Width - 2, 1), 0.5f));
        }

        private void DrawBorders(SpriteBatch spriteBatch)
        {
            Color? borderColor = HoveredBorderColor != null && MouseOver ? HoveredBorderColor : BorderColor;
            if (borderColor != null)
            {
                foreach (var r in _topBorders)
                {
                    spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, r.Item1, Rectangle.Empty, (Color)borderColor * r.Item2);
                }

                foreach (var r in _leftBorders)
                {
                    spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, r.Item1, Rectangle.Empty, (Color)borderColor * r.Item2);
                }

                foreach (var r in _bottomBorders)
                {
                    spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, r.Item1, Rectangle.Empty, (Color)borderColor * r.Item2);
                }

                foreach (var r in _rightBorders)
                {
                    spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, r.Item1, Rectangle.Empty, (Color)borderColor * r.Item2);
                }
            }
        }
    }
}
