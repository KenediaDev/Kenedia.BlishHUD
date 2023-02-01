using Blish_HUD.Content;
using Blish_HUD;
using Gw2Sharp.WebApi;
using Kenedia.Modules.Core.Interfaces;
using Kenedia.Modules.Core.Structs;
using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Blish_HUD.Controls;
using Blish_HUD.Input;

namespace Kenedia.Modules.Core.Controls
{

    public class Panel : Blish_HUD.Controls.Panel, ILocalizable
    {

        private readonly AsyncTexture2D _texturePanelHeader = AsyncTexture2D.FromAssetId(1032325);
        private readonly AsyncTexture2D _texturePanelHeaderActive = AsyncTexture2D.FromAssetId(1032324);
        private readonly AsyncTexture2D _textureCornerAccent = AsyncTexture2D.FromAssetId(1002144);
        private readonly AsyncTexture2D _textureLeftSideAccent = AsyncTexture2D.FromAssetId(605025);
        private readonly AsyncTexture2D _textureAccordionArrow = AsyncTexture2D.FromAssetId(155953);

        private readonly BasicTooltip _tooltip = new()
        {
            Parent = Graphics.SpriteScreen,
            ZIndex = int.MaxValue - 1,
            Visible = false,
        };

        private Func<string> _setLocalizedTitleTooltip;
        private Func<string> _setLocalizedTooltip;
        private Func<string> _setLocalizedTitle;
        private Rectangle _layoutTopLeftAccentBounds;
        private Rectangle _layoutBottomRightAccentBounds;
        private Rectangle _layoutCornerAccentSrc;
        private Rectangle _layoutLeftAccentBounds;
        private Rectangle _layoutLeftAccentSrc;
        private Rectangle _layoutHeaderBounds;
        private Rectangle _layoutHeaderTextBounds;
        private Rectangle _layoutHeaderIconBounds;
        private Vector2 _layoutAccordionArrowOrigin;
        private Rectangle _layoutAccordionArrowBounds;

        public string BasicTitleTooltipText { get; set; }

        public Panel()
        {
            GameService.Overlay.UserLocale.SettingChanged += UserLocale_SettingChanged;
            UserLocale_SettingChanged(null, null);
        }

        public RectangleDimensions BorderWidth { get; set; } = new(2);

        public Color? BorderColor { get; set; }

        public Color? HoveredBorderColor { get; set; }

        public AsyncTexture2D BackgroundImage { get; set; }

        public AsyncTexture2D TitleIcon { get; set; }

        public Color? BackgroundImageColor { get; set; } = Color.White;

        public Color? BackgroundImageHoveredColor { get; set; }

        public new Color? BackgroundColor { get; set; }

        public Color? BackgroundHoveredColor { get; set; }

        public Rectangle? TextureRectangle { get; set; }

        public string TitleTooltipText
        {
            get => _tooltip.Text;
            set => _tooltip.Text = value;
        }

        public Func<string> SetLocalizedTooltip
        {
            get => _setLocalizedTooltip;
            set
            {
                _setLocalizedTooltip = value;
                BasicTooltipText = value?.Invoke();
            }
        }

        public Func<string> SetLocalizedTitleTooltip
        {
            get => _setLocalizedTitleTooltip;
            set
            {
                _setLocalizedTitleTooltip = value;
                TitleTooltipText = value?.Invoke();
            }
        }

        public Func<string> SetLocalizedTitle
        {
            get => _setLocalizedTitle;
            set
            {
                _setLocalizedTitle = value;
                Title = value?.Invoke();
            }
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int num = (!string.IsNullOrEmpty(_title)) ? 36 : 0;
            int num2 = 0;
            int num3 = 0;
            int num4 = 0;
            if (ShowBorder)
            {
                num = Math.Max(7, num);
                num2 = 4;
                num3 = 7;
                num4 = 4;
                int num5 = Math.Min(_size.X, 256);
                _layoutTopLeftAccentBounds = new Rectangle(-2, num - 12, num5, _textureCornerAccent.Height);
                _layoutBottomRightAccentBounds = new Rectangle(_size.X - num5 + 2, _size.Y - 59, num5, _textureCornerAccent.Height);
                _layoutCornerAccentSrc = new Rectangle(256 - num5, 0, num5, _textureCornerAccent.Height);
                _layoutLeftAccentBounds = new Rectangle(num4 - 7, num, _textureLeftSideAccent.Width, Math.Min(_size.Y - num - num3, _textureLeftSideAccent.Height));
                _layoutLeftAccentSrc = new Rectangle(0, 0, _textureLeftSideAccent.Width, _layoutLeftAccentBounds.Height);
            }

            base.ContentRegion = new Rectangle(num4, num, _size.X - num4 - num2, _size.Y - num - num3);
            _layoutHeaderBounds = new Rectangle(base.ContentRegion.Left, 0, base.ContentRegion.Width, 36);
            _layoutHeaderTextBounds = new Rectangle(_layoutHeaderBounds.Left + 10 + (TitleIcon == null ? 0 : 36), 0, _layoutHeaderBounds.Width - 10, 36);
            _layoutHeaderIconBounds = new Rectangle(_layoutHeaderBounds.Left + 5, 0, 36, 36);
            _layoutAccordionArrowOrigin = new Vector2(16f, 16f);
            _layoutAccordionArrowBounds = new Rectangle(_layoutHeaderBounds.Right - 32, (num - 32) / 2, 32, 32).OffsetBy(_layoutAccordionArrowOrigin.ToPoint());
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            _tooltip.Visible = false;
            if (_backgroundTexture != null)
            {
                spriteBatch.DrawOnCtrl(this, _backgroundTexture, bounds);
            }

            if (_showTint)
            {
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, base.ContentRegion, Color.Black * 0.4f);
            }

            if (!string.IsNullOrEmpty(_title))
            {
                spriteBatch.DrawOnCtrl(this, _texturePanelHeader, _layoutHeaderBounds);
                if (_canCollapse && _mouseOver && base.RelativeMousePosition.Y <= 36)
                {
                    _tooltip.Visible = true;
                    spriteBatch.DrawOnCtrl(this, _texturePanelHeaderActive, _layoutHeaderBounds);
                }
                else
                {
                    spriteBatch.DrawOnCtrl(this, _texturePanelHeader, _layoutHeaderBounds);
                }

                spriteBatch.DrawStringOnCtrl(this, _title, Control.Content.DefaultFont16, _layoutHeaderTextBounds, Color.White);
                if(TitleIcon != null) spriteBatch.DrawOnCtrl(this, TitleIcon, _layoutHeaderIconBounds, TitleIcon.Bounds, Color.White);
                if (_canCollapse)
                {
                    spriteBatch.DrawOnCtrl(this, _textureAccordionArrow, _layoutAccordionArrowBounds, null, Color.White, ArrowRotation, _layoutAccordionArrowOrigin);
                }
            }

            if (ShowBorder)
            {
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, base.ContentRegion, Color.Black * (0.1f * AccentOpacity));
                spriteBatch.DrawOnCtrl(this, _textureCornerAccent, _layoutTopLeftAccentBounds, _layoutCornerAccentSrc, Color.White * AccentOpacity, 0f, Vector2.Zero, SpriteEffects.FlipHorizontally);
                spriteBatch.DrawOnCtrl(this, _textureCornerAccent, _layoutBottomRightAccentBounds, _layoutCornerAccentSrc, Color.White * AccentOpacity, 0f, Vector2.Zero, SpriteEffects.FlipVertically);
                spriteBatch.DrawOnCtrl(this, _textureLeftSideAccent, _layoutLeftAccentBounds, _layoutLeftAccentSrc, Color.Black * AccentOpacity, 0f, Vector2.Zero, SpriteEffects.FlipVertically);
            }

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
            if (SetLocalizedTitle != null) Title = SetLocalizedTitle?.Invoke();
            if (SetLocalizedTitleTooltip != null) TitleTooltipText = SetLocalizedTitleTooltip?.Invoke();
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            GameService.Overlay.UserLocale.SettingChanged -= UserLocale_SettingChanged;
            _tooltip?.Dispose();
        }
    }
}
