using Blish_HUD.Input;
using Blish_HUD.Settings;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Res;
using Microsoft.Xna.Framework.Input;
using Kenedia.Modules.BuildsManager.Res;

namespace Kenedia.Modules.BuildsManager.Services
{
    public class Settings : BaseSettingsModel
    {
        private readonly SettingCollection _settings;

        public Settings(SettingCollection settings)
        {
            _settings = settings;
            SettingCollection internalSettings = settings.AddSubCollection("Internal", false, false);

            ShowCornerIcon = internalSettings.DefineSetting(nameof(ShowCornerIcon), true);
            AutoSetFilterProfession = settings.DefineSetting(nameof(AutoSetFilterProfession), false, () => strings.AutoSetProfession_Name, () => strings.AutoSetProfession_Tooltip);

            //Temporary
            ToggleWindowKey = settings.DefineSetting(nameof(ToggleWindowKey), new KeyBinding(ModifierKeys.Shift, Keys.B), 
                () => string.Format(strings_common.ToggleItem, BuildsManager.ModuleName),
                () => string.Format(strings_common.ToggleItem, BuildsManager.ModuleName));
        }

        public SettingEntry<bool> ShowCornerIcon { get; set; }

        public SettingEntry<bool> AutoSetFilterProfession { get; set; }

        public SettingEntry<KeyBinding> ToggleWindowKey { get; set; }
    }
}
