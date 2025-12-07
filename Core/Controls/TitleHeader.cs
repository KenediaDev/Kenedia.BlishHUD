using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Gw2Sharp.WebApi;
using Kenedia.Modules.Core.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using System;

namespace Kenedia.Modules.Core.Controls
{
    public class TitleHeader : Control, ILocalizable
    {
        private readonly AsyncTexture2D _texturePanelHeader = AsyncTexture2D.FromAssetId(1032325);
        private Rectangle _titleBounds = Rectangle.Empty;

        public BitmapFont Font { get; set; } = Content.DefaultFont16;

        public string Title { get; set; }

        public Func<string> SetLocalizedTooltip
        {
            get;
            set
            {
                field = value;
                BasicTooltipText = value?.Invoke();
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

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            _titleBounds = new(5, 0, Width - 10, Height);
        }

        public void UserLocale_SettingChanged(object sender, ValueChangedEventArgs<Locale> e)
        {
            if (SetLocalizedTooltip is not null) BasicTooltipText = SetLocalizedTooltip?.Invoke();
            if (SetLocalizedTitle is not null) Title = SetLocalizedTitle?.Invoke();
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            spriteBatch.DrawOnCtrl(this, _texturePanelHeader, bounds, _texturePanelHeader.Bounds);
            if (Title is not null) spriteBatch.DrawStringOnCtrl(this, Title, Font, _titleBounds, Color.White);
        }
    }
}
