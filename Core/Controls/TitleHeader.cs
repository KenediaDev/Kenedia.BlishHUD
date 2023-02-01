using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Gw2Sharp.WebApi;
using Kenedia.Modules.Core.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kenedia.Modules.Core.Controls
{
    public class TitleHeader : Control, ILocalizable
    {
        private readonly AsyncTexture2D _texturePanelHeader = AsyncTexture2D.FromAssetId(1032325);
        private Rectangle _titleBounds = Rectangle.Empty;
        private Func<string> _setLocalizedTitle;
        private Func<string> _setLocalizedTooltip;

        public BitmapFont Font { get; set; } = Control.Content.DefaultFont16;

        public string Title { get; set; }

        public Func<string> SetLocalizedTooltip
        {
            get => _setLocalizedTooltip;
            set
            {
                _setLocalizedTooltip = value;
                BasicTooltipText = value?.Invoke();
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

            _titleBounds = new(5, 0, Width - 10, Height);
        }

        public void UserLocale_SettingChanged(object sender, ValueChangedEventArgs<Locale> e)
        {
            if (SetLocalizedTooltip != null) BasicTooltipText = SetLocalizedTooltip?.Invoke();
            if (SetLocalizedTitle != null) Title = SetLocalizedTitle?.Invoke();
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            spriteBatch.DrawOnCtrl(this, _texturePanelHeader, bounds, _texturePanelHeader.Bounds);
            if (Title != null) spriteBatch.DrawStringOnCtrl(this, Title, Font, _titleBounds, Color.White);
        }
    }
}
