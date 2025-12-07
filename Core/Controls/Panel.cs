using Blish_HUD.Content;
using Blish_HUD;
using Gw2Sharp.WebApi;
using Kenedia.Modules.Core.Interfaces;
using Kenedia.Modules.Core.Structs;
using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Blish_HUD.Controls;
using System.Collections.Generic;
using Kenedia.Modules.Core.Services;
using Blish_HUD.Input;

namespace Kenedia.Modules.Core.Controls
{
    public class Panel : Blish_HUD.Controls.Panel, ILocalizable
    {
        private readonly List<(Rectangle, float)> _leftBorders = [];
        private readonly List<(Rectangle, float)> _topBorders = [];
        private readonly List<(Rectangle, float)> _rightBorders = [];
        private readonly List<(Rectangle, float)> _bottomBorders = [];

        private readonly AsyncTexture2D _texturePanelHeader = AsyncTexture2D.FromAssetId(1032325);
        private readonly AsyncTexture2D _texturePanelHeaderActive = AsyncTexture2D.FromAssetId(1032324);
        private readonly AsyncTexture2D _textureCornerAccent = AsyncTexture2D.FromAssetId(1002144);
        private readonly AsyncTexture2D _textureLeftSideAccent = AsyncTexture2D.FromAssetId(605025);
        private readonly AsyncTexture2D _textureAccordionArrow = AsyncTexture2D.FromAssetId(155953);

        private readonly BasicTooltip _tooltip = new()
        {
            Parent = Graphics.SpriteScreen,
            ZIndex = int.MaxValue / 2,
            Visible = false,
        };
        private Vector2 _layoutAccordionArrowOrigin;
        private Rectangle _layoutTopLeftAccentBounds;
        private Rectangle _layoutBottomRightAccentBounds;
        private Rectangle _layoutCornerAccentSrc;
        private Rectangle _layoutLeftAccentBounds;
        private Rectangle _layoutRightAccentBounds;
        private Rectangle _layoutLeftAccentSrc;
        private Rectangle _layoutHeaderBounds;
        private Rectangle _layoutHeaderTextBounds;
        private Rectangle _layoutHeaderIconBounds;
        private Rectangle _layoutAccordionArrowBounds;

        private RectangleDimensions _contentPadding = new(0);
        private RectangleDimensions _borderWidth = new(0);
        private Rectangle _backgroundBounds;
        private RectangleDimensions _titleIconPadding = new(3, 3, 5, 3);

        public string BasicTitleTooltipText { get; set; }

        public Panel()
        {
            LocalizingService.LocaleChanged += UserLocale_SettingChanged;
            UserLocale_SettingChanged(null, null);
        }

        public bool Hovered => ClipInputToBounds ? MouseOver : AbsoluteBounds.Contains(Input.Mouse.Position);

        public bool ClipInputToBounds { get; set; } = true;

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

        public AsyncTexture2D TitleIcon { get; set; }

        public bool ShowRightBorder { get; set; } = false;

        public Color? BackgroundImageColor { get; set; } = Color.White;

        public Color? BackgroundImageHoveredColor { get; set; }

        public new Color? BackgroundColor { get; set; }

        public Color? BackgroundHoveredColor { get; set; }

        public Rectangle? TextureRectangle { get; set; }

        public RectangleDimensions TitleIconPadding
        {
            get => _titleIconPadding;
            set
            {
                _titleIconPadding = value;
                RecalculateLayout();
            }
        }

        public int TitleBarHeight
        {
            get;
            set
            {
                field = value;
                RecalculateLayout();
            }
        } = 36;

        public string TitleTooltipText
        {
            get => _tooltip.Text;
            set => _tooltip.Text = value;
        }

        public Func<string> SetLocalizedTooltip
        {
            get;
            set
            {
                field = value;
                BasicTooltipText = value?.Invoke();
            }
        }

        public Func<string> SetLocalizedTitleTooltip
        {
            get;
            set
            {
                field = value;
                TitleTooltipText = value?.Invoke();
            }
        }

        public Func<string> SetLocalizedTitle
        {
            get;
            set
            {
                field = value;
                Title = value?.Invoke();
            }
        }

        public bool CaptureInput { get; set; } = true;

        public CaptureType? Capture { get; set; }

        public Action OnCollapse { get; set; }

        public Action OnExpand { get; set; }

        public Rectangle? TitleTextureRegion { get; set; }

        protected override void OnClick(MouseEventArgs e)
        {
            bool collapsed = Collapsed;

            base.OnClick(e);

            if (collapsed != Collapsed)
            {
                if (Collapsed)
                    OnCollapse?.Invoke();
                else
                    OnExpand?.Invoke();
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

            int num = (!string.IsNullOrEmpty(_title)) ? TitleBarHeight : 0;
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
                _layoutRightAccentBounds = new Rectangle(_size.X - 12, Math.Max (0, _size.Y - _layoutLeftAccentBounds.Height),_textureLeftSideAccent.Width, Math.Min(_size.Y - num - num3 - 10, _textureLeftSideAccent.Height - 10));
                _layoutLeftAccentSrc = new Rectangle(0, 0, _textureLeftSideAccent.Width, _layoutLeftAccentBounds.Height);
            }

            ContentRegion = new Rectangle(_contentPadding.Left + num4, _contentPadding.Top + num, _size.X - num4 - num2 - _contentPadding.Horizontal, _size.Y - num - num3 - _contentPadding.Vertical);
            _layoutHeaderBounds = new Rectangle(num4, 0, Width, num);
            _layoutHeaderIconBounds = TitleIcon is not null ? new Rectangle(_layoutHeaderBounds.Left + _titleIconPadding.Left, _titleIconPadding.Top, num - _titleIconPadding.Vertical, num - _titleIconPadding.Vertical) : Rectangle.Empty;

            _layoutHeaderTextBounds = new Rectangle(_layoutHeaderIconBounds.Right + _titleIconPadding.Right, 0, _layoutHeaderBounds.Width - _layoutHeaderIconBounds.Width, num);

            int arrowSize = num - 4;    
            _layoutAccordionArrowOrigin = new Vector2(16f, 16f);
            _layoutAccordionArrowBounds = new Rectangle(_layoutHeaderBounds.Right - arrowSize, (num - arrowSize) / 2, arrowSize, arrowSize).OffsetBy(new(arrowSize / 2, arrowSize / 2));

            CalculateBorders();
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            _tooltip.Visible = false;
            if (_backgroundTexture is not null)
            {
                spriteBatch.DrawOnCtrl(this, _backgroundTexture, bounds);
            }

            if (_showTint)
            {
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, ContentRegion, Color.Black * 0.4f);
            }

            if (!string.IsNullOrEmpty(_title))
            {
                spriteBatch.DrawOnCtrl(this, _texturePanelHeader, _layoutHeaderBounds);
                if (_canCollapse && _mouseOver && RelativeMousePosition.Y <= 36)
                {
                    _tooltip.Visible = true;
                    spriteBatch.DrawOnCtrl(this, _texturePanelHeaderActive, _layoutHeaderBounds);
                }
                else
                {
                    spriteBatch.DrawOnCtrl(this, _texturePanelHeader, _layoutHeaderBounds);
                }

                spriteBatch.DrawStringOnCtrl(this, _title, Content.DefaultFont16, _layoutHeaderTextBounds, Color.White);
                if (TitleIcon is not null) spriteBatch.DrawOnCtrl(this, TitleIcon, _layoutHeaderIconBounds, TitleTextureRegion ?? TitleIcon.Bounds, Color.White);
                if (_canCollapse)
                {
                    spriteBatch.DrawOnCtrl(this, _textureAccordionArrow, _layoutAccordionArrowBounds, null, Color.White, ArrowRotation, _layoutAccordionArrowOrigin);
                }
            }

            if (ShowBorder)
            {
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, ContentRegion, Color.Black * (0.1f * AccentOpacity));
                spriteBatch.DrawOnCtrl(this, _textureLeftSideAccent, _layoutLeftAccentBounds, _layoutLeftAccentSrc, Color.Black * AccentOpacity, 0f, Vector2.Zero, SpriteEffects.FlipVertically);
               if(ShowRightBorder)  spriteBatch.DrawOnCtrl(this, _textureLeftSideAccent, _layoutRightAccentBounds, _layoutLeftAccentSrc, Color.Black * AccentOpacity, 0f, Vector2.Zero);

                spriteBatch.DrawOnCtrl(this, _textureCornerAccent, _layoutTopLeftAccentBounds, _layoutCornerAccentSrc, Color.White * AccentOpacity, 0f, Vector2.Zero, SpriteEffects.FlipHorizontally);
                spriteBatch.DrawOnCtrl(this, _textureCornerAccent, _layoutBottomRightAccentBounds, _layoutCornerAccentSrc, Color.White * AccentOpacity, 0f, Vector2.Zero, SpriteEffects.FlipVertically);
            }

            Color? backgroundColor = BackgroundHoveredColor is not null && Hovered ? BackgroundHoveredColor : BackgroundColor;
            if (backgroundColor is not null)
            {
                spriteBatch.DrawOnCtrl(
                    this,
                    ContentService.Textures.Pixel,
                    _backgroundBounds,
                    Rectangle.Empty,
                    (Color)backgroundColor);
            }

            Color? backgroundImageColor = BackgroundImageHoveredColor is not null && Hovered ? BackgroundImageHoveredColor : BackgroundImageColor;
            if (BackgroundImage is not null && backgroundImageColor is not null)
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

            Color? borderColor = HoveredBorderColor is not null && Hovered ? HoveredBorderColor : BorderColor;
            if (borderColor is not null)
            {
                DrawBorders(spriteBatch);
            }
        }

        public virtual void UserLocale_SettingChanged(object sender, ValueChangedEventArgs<Locale> e)
        {
            if (SetLocalizedTooltip is not null) BasicTooltipText = SetLocalizedTooltip?.Invoke();
            if (SetLocalizedTitle is not null) Title = SetLocalizedTitle?.Invoke();
            if (SetLocalizedTitleTooltip is not null) TitleTooltipText = SetLocalizedTitleTooltip?.Invoke();
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            GameService.Overlay.UserLocale.SettingChanged -= UserLocale_SettingChanged;
            _tooltip?.Dispose();
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
            Color? borderColor = HoveredBorderColor is not null && Hovered ? HoveredBorderColor : BorderColor;
            if (borderColor is not null)
            {
                foreach (var r in new List<(Rectangle, float)>(_topBorders))
                {
                    spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, r.Item1, Rectangle.Empty, (Color)borderColor * r.Item2);
                }

                foreach (var r in new List<(Rectangle, float)>(_leftBorders))
                {
                    spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, r.Item1, Rectangle.Empty, (Color)borderColor * r.Item2);
                }

                foreach (var r in new List<(Rectangle, float)>(_bottomBorders))
                {
                    spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, r.Item1, Rectangle.Empty, (Color)borderColor * r.Item2);
                }

                foreach (var r in new List<(Rectangle, float)>(_rightBorders))
                {
                    spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, r.Item1, Rectangle.Empty, (Color)borderColor * r.Item2);
                }
            }
        }

        protected override CaptureType CapturesInput()
        {
            return Capture ?? (CaptureInput ? base.CapturesInput() : CaptureType.None);
        }
    }
}
