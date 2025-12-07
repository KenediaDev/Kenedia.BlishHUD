using Blish_HUD;
using Blish_HUD.Input;
using Gw2Sharp.WebApi;
using Kenedia.Modules.Core.Interfaces;
using Kenedia.Modules.Core.Services;
using System;
using static System.Net.Mime.MediaTypeNames;

namespace Kenedia.Modules.Characters.Controls
{
    internal class ImageColorToggle : ImageGrayScaled, ILocalizable
    {
        public Gw2Sharp.Models.ProfessionType Profession { get; set; }

        public Func<string>? SetLocalizedTooltip
        {
            get => field;
            set
            {
                field = value;
                BasicTooltipText = value?.Invoke();
            }
        }

        private readonly Action<bool> _onChanged;

        public ImageColorToggle()
        {
            LocalizingService.LocaleChanged += UserLocale_SettingChanged;
        }

        public ImageColorToggle(Action<bool> onChanged) 
            : this()
        {
            _onChanged= onChanged;
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);
            Active = !Active;
            _onChanged?.Invoke(Active);
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            GameService.Overlay.UserLocale.SettingChanged -= UserLocale_SettingChanged;
        }

        public void UserLocale_SettingChanged(object sender = null, ValueChangedEventArgs<Locale> e = null)
        {
            if (SetLocalizedTooltip is not null) BasicTooltipText = SetLocalizedTooltip?.Invoke();
        }
    }
}
