using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Input;
using Gw2Sharp.WebApi;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Interfaces;
using Kenedia.Modules.Core.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Kenedia.Modules.Core.Controls
{
    public class ImageButton : Blish_HUD.Controls.Control, ILocalizable
    {
        private Func<string> _setLocalizedTooltip;
        private readonly AsyncTexture2D _background = AsyncTexture2D.FromAssetId(155983);

        public AsyncTexture2D IconFrame { get; set; } = AsyncTexture2D.FromAssetId(1414041);

        public ImageButton()
        {
            LocalizingService.LocaleChanged += UserLocale_SettingChanged;
            UserLocale_SettingChanged(null, null);
        }

        private bool Clicked => MouseOver && (Input.Mouse.State.LeftButton is ButtonState.Pressed || Input.Mouse.State.RightButton is ButtonState.Pressed);

        public Action<MouseEventArgs> ClickAction { get; set; }

        public Color? ColorHovered { get; set; }

        public Color? ColorClicked { get; set; }

        public Color? ImageColor { get; set; } = Microsoft.Xna.Framework.Color.White;

        public Rectangle? SizeRectangle { get; set; }

        public Rectangle? TextureRectangle { get; set; }

        public AsyncTexture2D Texture { get; set; }

        public AsyncTexture2D DisabledTexture { get; set; }

        public AsyncTexture2D HoveredTexture { get; set; }

        public AsyncTexture2D ClickedTexture { get; set; }

        public bool ShowButton { get; set; } = false;

        public bool ShowImageFrame { get; set; } = false;

        public float? TextureRotation { get; set; }

        public Func<string> SetLocalizedTooltip
        {
            get => _setLocalizedTooltip;
            set
            {
                _setLocalizedTooltip = value;
                BasicTooltipText = value?.Invoke();
            }
        }

        public void UserLocale_SettingChanged(object sender, ValueChangedEventArgs<Locale> e)
        {
            if (SetLocalizedTooltip is not null) BasicTooltipText = SetLocalizedTooltip?.Invoke();
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            GameService.Overlay.UserLocale.SettingChanged -= UserLocale_SettingChanged;
        }

        private AsyncTexture2D GetTexture()
        {
            return !Enabled && DisabledTexture is not null ? DisabledTexture :
                Clicked && ClickedTexture is not null ? ClickedTexture
                : MouseOver && HoveredTexture is not null ? HoveredTexture
                : Texture;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            //if (ShowButton)
            //{
            //    spriteBatch.DrawOnCtrl(this, _background, bounds, Color.White);
            //    spriteBatch.DrawFrame(this, bounds.Add(new(2, 2, -4, -4)), Color.White * 0.3F, 2);
            //    spriteBatch.DrawFrame(this, bounds, Color.Black, 2);
            //}

            AsyncTexture2D texture = GetTexture();
            Color? color = ColorHovered is not null && MouseOver ? ColorHovered : ColorClicked is not null && Clicked ? ColorClicked : ImageColor;

            if (texture is not null && color is not null)
            {
                spriteBatch.DrawOnCtrl(
                    this,
                    texture,
                    SizeRectangle ?? bounds,
                    TextureRectangle ?? texture.Bounds,
                    (Color)color,
                    (float)(TextureRotation ?? 0f),
                    default);
            }
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            if (Enabled) ClickAction?.Invoke(e);
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

        }
    }
}
