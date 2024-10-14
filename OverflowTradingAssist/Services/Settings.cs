using Blish_HUD.Input;
using Blish_HUD.Settings;
using Kenedia.Modules.Core.Res;
using Microsoft.Xna.Framework.Input;
using Kenedia.Modules.Core.Models;

namespace Kenedia.Modules.OverflowTradingAssist.Services
{
    public class Settings : BaseSettingsModel
    {
        
        public Settings(SettingCollection settings) : base(settings)
        {
        }

        protected override void InitializeSettings(SettingCollection settings)
        {
            base.InitializeSettings(settings);

            SettingCollection internalSettings = settings.AddSubCollection("Internal", false, false);

            ShowCornerIcon = internalSettings.DefineSetting(nameof(ShowCornerIcon), true);
            SheetInitialized = internalSettings.DefineSetting(nameof(SheetInitialized), false);
            TradesInitialized = internalSettings.DefineSetting(nameof(TradesInitialized), false);

            //Temporary
            ToggleWindowKey = settings.DefineSetting(nameof(ToggleWindowKey), new KeyBinding(ModifierKeys.Shift, Keys.B),
                () => string.Format(strings_common.ToggleItem, OverflowTradingAssist.ModuleName),
                () => string.Format(strings_common.ToggleItem, OverflowTradingAssist.ModuleName));
        }

        public SettingEntry<bool> ShowCornerIcon { get; set; }

        public SettingEntry<bool> SheetInitialized { get; set; }

        public SettingEntry<bool> TradesInitialized { get; set; }

        public SettingEntry<KeyBinding> ToggleWindowKey { get; set; }
    }
}