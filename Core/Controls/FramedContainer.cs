using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Gw2Sharp.WebApi;
using Kenedia.Modules.Core.Interfaces;
using Kenedia.Modules.Core.Structs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.Core.Controls
{
    public class FramedContainer : Container, ILocalizable
    {
        private Func<string> _setLocalizedTooltip;
        protected DateTime LastInteraction;
        private bool _fadeOut = false;
        private double _fadeTickDuration = 0;
        private double _fadeTick = 0;
        private double _fadeDelay = 2500;
        private double _fadeDuration = 500;
        private double _fadePerMs = 0;
        private int _fadeSteps = 200;

        public FramedContainer()
        {
            GameService.Overlay.UserLocale.SettingChanged += UserLocale_SettingChanged;
            UserLocale_SettingChanged(null, null);
            RecalculateFading();
        }

        public bool FadeOut
        {
            get => _fadeOut;
            set
            {
                _fadeOut = value;
                Opacity = 1F;
            }
        }

        public double FadeDelay
        {
            get => _fadeDelay;
            set
            {
                _fadeDelay = value;
                RecalculateFading();
            }
        }

        public double FadeDuration
        {
            get => _fadeDuration;
            set
            {
                _fadeDuration = value;
                RecalculateFading();
            }
        }

        public int FadeSteps
        {
            get => _fadeSteps;
            set
            {
                _fadeSteps = value;
                RecalculateFading();
            }
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

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

            if (FadeOut && Visible && DateTime.Now.Subtract(LastInteraction).TotalMilliseconds >= FadeDelay)
            {
                double timeSinceTick = gameTime.TotalGameTime.TotalMilliseconds - _fadeTick;

                if (timeSinceTick >= _fadeTickDuration)
                {
                    Opacity -= (float)(_fadePerMs * (_fadeTick == 0 ? _fadeTickDuration : timeSinceTick));

                    _fadeTick = gameTime.TotalGameTime.TotalMilliseconds;

                    if (Opacity <= 0F)
                    {
                        Hide();
                        _fadeTick = 0;
                    }
                }
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

        protected override void OnMouseMoved(MouseEventArgs e)
        {
            base.OnMouseMoved(e);
            SetInteracted();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            SetInteracted();
        }

        protected override void OnHidden(EventArgs e)
        {
            base.OnHidden(e);
           if(FadeOut) Opacity = 1F;
        }

        protected void SetInteracted()
        {
            LastInteraction = DateTime.Now;
            Opacity = 1f;
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            GameService.Overlay.UserLocale.SettingChanged -= UserLocale_SettingChanged;
        }

        private void RecalculateFading()
        {
            _fadeTickDuration = FadeDuration / FadeSteps;
            _fadePerMs = (double)1 / FadeSteps / _fadeTickDuration;
        }
    }
}
