using Blish_HUD;
using Blish_HUD.Input;
using Gw2Sharp.WebApi;
using Kenedia.Modules.Core.Interfaces;
using Kenedia.Modules.Core.Services;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using System;
using System.ComponentModel;

namespace Kenedia.Modules.Core.Controls
{
    public class Image : Blish_HUD.Controls.Image, ILocalizable
    {
        private Color? _defaultBackgroundColor;
        private Color? _hoveredBackgroundColor;

        public Color? HoveredBackgroundColor
        {
            set => Common.SetProperty(ref _hoveredBackgroundColor, value);
            get => _hoveredBackgroundColor;
        }

        public Image()
        {
            LocalizingService.LocaleChanged  += UserLocale_SettingChanged;
            UserLocale_SettingChanged(null, null);

            PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(BackgroundColor) && BackgroundColor != _hoveredBackgroundColor)
            {
                _defaultBackgroundColor = BackgroundColor;
            }
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

        public void UserLocale_SettingChanged(object sender, ValueChangedEventArgs<Locale> e)
        {
            if (SetLocalizedTooltip is not null) BasicTooltipText = SetLocalizedTooltip?.Invoke();
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            GameService.Overlay.UserLocale.SettingChanged -= UserLocale_SettingChanged;
        }

        protected override void OnMouseEntered(MouseEventArgs e)
        {
            base.OnMouseEntered(e);

            BackgroundColor = _hoveredBackgroundColor ?? BackgroundColor;
        }

        protected override void OnMouseLeft(MouseEventArgs e)
        {
            base.OnMouseLeft(e);

            BackgroundColor = _defaultBackgroundColor ?? BackgroundColor;
        }
    }
}
