using Blish_HUD.Input;
using Blish_HUD.Settings;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Res;
using Microsoft.Xna.Framework.Input;

namespace Kenedia.Modules.AdvancedBuildsManager.Services
{
    public class Settings : BaseSettingsModel
    {
        private readonly SettingCollection _settings;

        public Settings(SettingCollection settings)
        {
            _settings = settings;
            SettingCollection internalSettings = settings.AddSubCollection("Internal", false, false);

            ShowCornerIcon = internalSettings.DefineSetting(nameof(ShowCornerIcon), true);

            //Temporary
            ToggleWindowKey = settings.DefineSetting(nameof(ToggleWindowKey), new KeyBinding(ModifierKeys.Shift, Keys.B), 
                () => string.Format(strings_common.ToggleItem, AdvancedBuildsManager.ModuleName),
                () => string.Format(strings_common.ToggleItem, AdvancedBuildsManager.ModuleName));
        }

        public SettingEntry<bool> ShowCornerIcon { get; set; }

        public SettingEntry<KeyBinding> ToggleWindowKey { get; set; }
    }
}
