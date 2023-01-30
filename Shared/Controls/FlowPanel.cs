using Blish_HUD.Content;
using Blish_HUD;
using Gw2Sharp.WebApi;
using Kenedia.Modules.Core.Interfaces;
using Kenedia.Modules.Core.Structs;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kenedia.Modules.Core.Controls
{

    public class FlowPanel : Blish_HUD.Controls.FlowPanel, ILocalizable
    {
        private Func<string> _setLocalizedTooltip;

        public FlowPanel()
        {
            GameService.Overlay.UserLocale.SettingChanged += UserLocale_SettingChanged;
            UserLocale_SettingChanged(null, null);
        }

        public RectangleDimensions BorderWidth { get; set; } = new(2);

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

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            Color? backgroundImageColor = BackgroundImageHoveredColor != null && MouseOver ? BackgroundImageHoveredColor : BackgroundImageColor;

            if (BackgroundImage != null && backgroundImageColor != null)
            {
                spriteBatch.DrawOnCtrl(
                    this,
                    BackgroundImage,
                    bounds,
                    TextureRectangle ?? BackgroundImage.Bounds,
                    (Color)backgroundImageColor,
                    0f,
                    default);
            }

            Color? backgroundColor = BackgroundHoveredColor != null && MouseOver ? BackgroundHoveredColor : BackgroundColor;
            if (backgroundColor != null)
            {
                spriteBatch.DrawOnCtrl(
                    this,
                    ContentService.Textures.Pixel,
                    bounds,
                    Rectangle.Empty,
                    (Color)backgroundColor);
            }

            Color? borderColor = HoveredBorderColor != null && MouseOver ? HoveredBorderColor : BorderColor;
            if (borderColor != null)
            {
                // Top
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, bounds.Width, BorderWidth.Top), Rectangle.Empty, (Color)borderColor * 0.5f);
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, bounds.Width, BorderWidth.Top / 2), Rectangle.Empty, (Color)borderColor * 0.6f);

                // Bottom
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Bottom - 2, bounds.Width, BorderWidth.Bottom), Rectangle.Empty, (Color)borderColor * 0.5f);
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Bottom - 1, bounds.Width, BorderWidth.Bottom / 2), Rectangle.Empty, (Color)borderColor * 0.6f);

                // Left
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, BorderWidth.Left, bounds.Height), Rectangle.Empty, (Color)borderColor * 0.5f);
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, BorderWidth.Left / 2, bounds.Height), Rectangle.Empty, (Color)borderColor * 0.6f);

                // Right
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Right - 2, bounds.Top, BorderWidth.Right, bounds.Height), Rectangle.Empty, (Color)borderColor * 0.5f);
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Right - 1, bounds.Top, BorderWidth.Right / 2, bounds.Height), Rectangle.Empty, (Color)borderColor * 0.6f);
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
    }
}
