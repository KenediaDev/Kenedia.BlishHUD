using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Settings;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.QoL.Res;
using Microsoft.Xna.Framework;

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

            HotbarExpandDirection = _settings.DefineSetting(nameof(HotbarExpandDirection), ExpandType.LeftToRight, () => strings.HotbarExpandDirection_Name, () => strings.HotbarExpandDirection_Tooltip );
        }

        public SettingEntry<ExpandType> HotbarExpandDirection { get; }

        public SettingEntry<Point> HotbarPosition { get; }
    }
}
