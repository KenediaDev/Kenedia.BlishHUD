using Blish_HUD.Settings;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kenedia.Modules.ReleaseTheChoya.Models
{
    public class Settings : BaseSettingsModel
    {
        private readonly SettingCollection _settings;

        public Settings(SettingCollection s)
        {
            _settings = s;

            ChoyaDelay = s.DefineSetting(nameof(ChoyaDelay), new Range(30, 90));
            ChoyaIdleDelay = s.DefineSetting(nameof(ChoyaIdleDelay), new Range(125, 1500));
            ChoyaSpeed = s.DefineSetting(nameof(ChoyaSpeed), new Range(64, 720));
            ChoyaTravelDistance = s.DefineSetting(nameof(ChoyaTravelDistance), new Range(4, 30));
            ChoyaSize = s.DefineSetting(nameof(ChoyaSize), new Range(64, 512));

            IdleDelay = s.DefineSetting(nameof(IdleDelay), 15);
            NoMoveDelay = s.DefineSetting(nameof(NoMoveDelay), 15);

            ShowWhenStandingStill = s.DefineSetting(nameof(ShowWhenStandingStill), true);
            ShowRandomly = s.DefineSetting(nameof(ShowRandomly), true);
            ShowWhenIdle = s.DefineSetting(nameof(ShowWhenIdle), true);
            AvoidCombat = s.DefineSetting(nameof(AvoidCombat), true);
        }

        public SettingEntry<Range> ChoyaIdleDelay;

        public SettingEntry<Range> ChoyaDelay;

        public SettingEntry<Range> ChoyaSpeed;

        public SettingEntry<Range> ChoyaTravelDistance;

        public SettingEntry<Range> ChoyaSize;

        public SettingEntry<int> IdleDelay;

        public SettingEntry<int> NoMoveDelay;

        public SettingEntry<bool> ShowWhenStandingStill;

        public SettingEntry<bool> ShowRandomly;

        public SettingEntry<bool> ShowWhenIdle;

        public SettingEntry<bool> AvoidCombat;
    }
}
