using Microsoft.Xna.Framework;
using Kenedia.Modules.BuildsManager.Models;
using System;
using Microsoft.Xna.Framework.Graphics;
using Blish_HUD.Input;
using Blish_HUD;
using Blish_HUD.Content;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Services;
using Gw2Sharp.WebApi;
using Kenedia.Modules.Core.Utility;

namespace Kenedia.Modules.BuildsManager.Views
{
    public class TagToggle : Blish_HUD.Controls.Control
    {
        public static int TagHeight = 25;

        private AsyncTexture2D _textureEnabled;
        private AsyncTexture2D _textureDisabled;
        private Func<string> _setLocalizedTooltip;
        private bool _selected;

        public bool Selected { get => _selected; set => Common.SetProperty(ref _selected , value, OnSelected); }

        public TagToggle(TemplateTag tag)
        {
            Size = new(TagHeight);

            if (tag is null)
            {
                Logger.GetLogger(typeof(BuildsManager)).Error("TagToggle created with null tag.");
                throw new ArgumentNullException(nameof(tag));
            }

            Tag = tag;

            Tag.PropertyChanged += Tag_PropertyChanged;
            Tag.Icon.Texture.TextureSwapped += Texture_TextureSwapped;

            BasicTooltipText = Tag.Name;
            _textureEnabled = Tag.Icon.Texture;
            _textureDisabled = Tag.Icon.Texture.Texture.ToGrayScaledPalettable();

            LocalizingService.LocaleChanged += UserLocale_SettingChanged;
            UserLocale_SettingChanged(null, null);
        }

        private void Texture_TextureSwapped(object sender, ValueChangedEventArgs<Texture2D> e)
        {
            _textureEnabled = Tag.Icon.Texture;
            _textureDisabled = Tag.Icon.Texture.Texture.ToGrayScaledPalettable();
        }

        public Func<string> SetLocalizedTooltip
        {
            get => _setLocalizedTooltip;
            set
            {
                _setLocalizedTooltip = value;

                if (value is not null)
                    BasicTooltipText = value?.Invoke();
            }
        }

        public void UserLocale_SettingChanged(object sender, ValueChangedEventArgs<Locale> e)
        {
            if (SetLocalizedTooltip is not null)
                BasicTooltipText = SetLocalizedTooltip?.Invoke();
        }

        private void Tag_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(TemplateTag.Name):
                    BasicTooltipText = Tag.Name;
                    break;

                case nameof(TemplateTag.AssetId):
                    _textureEnabled = Tag.Icon.Texture;
                    _textureDisabled = Tag.Icon.Texture.Texture.ToGrayScaledPalettable();
                    break;
            }
        }

        public TemplateTag Tag { get; private set; }

        public Action<TemplateTag> OnSelectedChanged { get; internal set; }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if ((Selected ? _textureEnabled : _textureDisabled) is AsyncTexture2D texture)
            {
                spriteBatch.DrawOnCtrl(this, texture, bounds, Selected ? Color.White : Color.Gray * 0.5F);
            }
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            Selected = !Selected;
        }

        private void OnSelected(object sender, Core.Models.ValueChangedEventArgs<bool> e)
        {
            if (OnSelectedChanged is not null)
            {
                OnSelectedChanged(Tag);
            }
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            LocalizingService.LocaleChanged -= UserLocale_SettingChanged;
        }
    }
}
