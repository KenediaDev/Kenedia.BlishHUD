using Control = Blish_HUD.Controls.Control;
using Kenedia.Modules.BuildsManager.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Blish_HUD;
using Kenedia.Modules.Core.Utility;
using Blish_HUD.Content;
using MonoGame.Extended.BitmapFonts;
using Blish_HUD.Input;

namespace Kenedia.Modules.BuildsManager.Controls.AboutPage
{
    public class TagControl : Control
    {
        private AsyncTexture2D _editIcon = AsyncTexture2D.FromAssetId(157109);
        private TemplateTag _tag;
        private string _displayText = string.Empty;
        private Rectangle _bounds;
        private Rectangle _iconBounds;
        private Rectangle _editIconBounds;
        private Rectangle _editIconTextureRegion;
        private Rectangle _textBounds;
        private BitmapFont _font = Content.DefaultFont14;

        public TagControl()
        {
            Height = Font.LineHeight + (FontPadding * 2);
        }

        public TemplateTag Tag { get => _tag; set => Common.SetProperty(ref _tag, value, OnTagChanged); }

        public BitmapFont Font { get => _font; set => Common.SetProperty(ref _font, value, OnFontChanged); }

        public bool Selected { get; set; }

        public Action<bool> OnClicked { get; set; }

        public Color HoverColor { get; set; } = Color.White * 0.2F;

        public Color DisabledColor { get; set; } = Color.Transparent;

        public Color ActiveColor { get; set; } = Color.Lime * 0.2F;

        public int FontPadding { get; set; } = 4;

        private void OnFontChanged(object sender, Core.Models.ValueChangedEventArgs<BitmapFont> e)
        {
            _font ??= Content.DefaultFont14;
            Height = Font.LineHeight + (FontPadding * 2);

            ApplyTag();
        }

        private void OnTagChanged(object sender, Core.Models.ValueChangedEventArgs<TemplateTag> e)
        {
            if (e.NewValue is not null)
            {
                e.NewValue.OnTagChanged = ApplyTag;
                ApplyTag();
            }
        }

        private void ApplyTag()
        {
            int height = Font.LineHeight;
            _iconBounds = new(FontPadding, FontPadding, height, height);
            _editIconBounds = new(Width - height - FontPadding, FontPadding, height, height);
            _editIconTextureRegion = new(2, 2, 28, 28);
            _textBounds = new(_iconBounds.Right + 5, _iconBounds.Top, Width - _iconBounds.Width - 5 - _editIconBounds.Width - 5, height);

            _displayText = UI.GetDisplayText(Font, Tag?.Name ?? string.Empty, _textBounds.Width);
            BasicTooltipText = Tag?.Name ?? string.Empty;

            ActiveColor = Color.Lime * 0.2F;
            HoverColor = Color.Lime * 0.3F;
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            Height = Font.LineHeight + (FontPadding * 2);
            ApplyTag();
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, bounds, MouseOver ? HoverColor : Selected ? ActiveColor : DisabledColor);

            if (Tag?.Icon?.Texture is AsyncTexture2D texture)
                spriteBatch.DrawOnCtrl(this, texture, _iconBounds, Tag.Icon.TextureRegion, Color.White);

            if (MouseOver)
                spriteBatch.DrawOnCtrl(this, _editIcon, _editIconBounds, _editIconTextureRegion, Color.White);

            spriteBatch.DrawStringOnCtrl(this, _displayText, Font, _textBounds, Color.White);
        }

        public void SetSelected(bool selected)
        {
            Selected = selected;
            OnClicked?.Invoke(Selected);
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            if (_editIconBounds.Contains(RelativeMousePosition))
            {

            }

            Selected = !Selected;
            OnClicked?.Invoke(Selected);
        }
    }
}
