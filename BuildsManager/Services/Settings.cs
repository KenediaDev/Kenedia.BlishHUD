using Blish_HUD.Settings;
using Kenedia.Modules.Core.Models;

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
        }

        public SettingEntry<bool> ShowCornerIcon { get; set; }
    }
}
