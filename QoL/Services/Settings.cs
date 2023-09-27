using Blish_HUD.Settings;
using Kenedia.Modules.Core.Controls;
using Microsoft.Xna.Framework;
using Kenedia.Modules.Core.Models;

namespace Kenedia.Modules.QoL.Services
{
    public class Settings : BaseSettingsModel
    {
        private readonly SettingCollection _settings;
        private readonly SettingCollection _internal_settings;

        public Settings(SettingCollection settings)
        {
            _settings = settings;

            _internal_settings = _settings.AddSubCollection("Internal");
            HotbarPosition = _internal_settings.DefineSetting(nameof(HotbarPosition), new Point(0, 32));

            HotbarExpandDirection = _settings.DefineSetting(nameof(HotbarExpandDirection), ExpandType.LeftToRight);
            HotbarButtonSorting = _settings.DefineSetting(nameof(HotbarButtonSorting), SortType.ActivesFirst);
            KeyboardLayout = _settings.DefineSetting(nameof(KeyboardLayout), KeyboardLayoutType.QWERTZ);
        }

        public SettingEntry<ExpandType> HotbarExpandDirection { get; }

        public SettingEntry<SortType> HotbarButtonSorting{ get; }

        public SettingEntry<KeyboardLayoutType> KeyboardLayout { get; }

        public SettingEntry<Point> HotbarPosition { get; }
    }
}
