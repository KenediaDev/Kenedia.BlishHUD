using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Input;
using Gw2Sharp.WebApi;
using Kenedia.Modules.Core.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Kenedia.Modules.Core.Controls
{
    public class ImageButton : Blish_HUD.Controls.Control, ILocalizable
    {
        private Func<string> _setLocalizedTooltip;

        public ImageButton()
        {
            GameService.Overlay.UserLocale.SettingChanged += UserLocale_SettingChanged;
            UserLocale_SettingChanged(null, null);
        }

        private bool Clicked => Input.Mouse.State.LeftButton is ButtonState.Pressed || Input.Mouse.State.RightButton is ButtonState.Pressed;

        public Action<MouseEventArgs> ClickAction { get; set; }

        public Color? ColorHovered { get; set; }

        public Color? ColorClicked { get; set; }

        public Color? Color { get; set; } = Microsoft.Xna.Framework.Color.White;

        public Rectangle? SizeRectangle { get; set; }

        public Rectangle? TextureRectangle { get; set; } 

        public AsyncTexture2D Texture { get; set; }

        public AsyncTexture2D HoveredTexture { get; set; }

        public AsyncTexture2D ClickedTexture { get; set; }

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
            if (SetLocalizedTooltip != null) BasicTooltipText = SetLocalizedTooltip?.Invoke();
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            GameService.Overlay.UserLocale.SettingChanged -= UserLocale_SettingChanged;
        }

        private AsyncTexture2D GetTexture()
        {
            return Clicked && ClickedTexture != null ? ClickedTexture
                : MouseOver && HoveredTexture != null ? HoveredTexture
                : Texture;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            AsyncTexture2D texture = GetTexture();
            var color = ColorHovered != null && MouseOver ? ColorHovered : ColorClicked != null && Clicked ? ColorClicked : Color;

            if (texture != null && color != null)
            {              
                spriteBatch.DrawOnCtrl(
                    this,
                    texture,
                    SizeRectangle ?? bounds,
                    TextureRectangle ?? texture.Bounds,
                    (Color) color,
                    (float)(TextureRotation ?? 0f),
                    default);
            }
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            ClickAction?.Invoke(e);
        }
    }
}
