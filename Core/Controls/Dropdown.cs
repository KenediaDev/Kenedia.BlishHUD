using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Gw2Sharp.WebApi;
using Kenedia.Modules.Core.Interfaces;
using Kenedia.Modules.Core.Services;
using Kenedia.Modules.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kenedia.Modules.Core.Controls
{
    public class Dropdown : Blish_HUD.Controls.Dropdown, ILocalizable
    {
        private Func<List<string>> _setLocalizedItems;
        private Func<string> _setLocalizedTooltip;
        private double _lastShown;

        public Dropdown()
        {
            LocalizingService.LocaleChanged  += UserLocale_SettingChanged;
            UserLocale_SettingChanged(null, null);
        }

        public Func<List<string>> SetLocalizedItems
        {
            get => _setLocalizedItems;
            set
            {
                _setLocalizedItems = value;
                UserLocale_SettingChanged(null, null);
            }
        }

        public Func<string> SetLocalizedTooltip
        {
            get => _setLocalizedTooltip;
            set
            {
                _setLocalizedTooltip = value;
                BasicTooltipText = value?.Invoke();
            }
        }

        public Action<string> ValueChangedAction { get; set; }

        public void UserLocale_SettingChanged(object sender, ValueChangedEventArgs<Locale> e)
        {
            if (SetLocalizedItems != null)
            {
                int? index = Items?.ToList().FindIndex(a => a == SelectedItem);

                Items.Clear();

                List<string> items = SetLocalizedItems?.Invoke();
                string selected = string.Empty;
                for (int i = 0; i < items.Count; i++)
                {
                    string item = items[i];
                    Items.Add(item);
                    if (index == i) selected = item;
                }

                SelectedItem = selected ?? SelectedItem;
            }

            if (SetLocalizedTooltip != null) BasicTooltipText = SetLocalizedTooltip?.Invoke();
        }

        protected override void OnValueChanged(ValueChangedEventArgs e)
        {
            base.OnValueChanged(e);

            ValueChangedAction?.Invoke(SelectedItem);
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            GameService.Overlay.UserLocale.SettingChanged -= UserLocale_SettingChanged;
        }
    }
}
