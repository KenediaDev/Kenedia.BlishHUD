﻿using Blish_HUD.Input;
using Blish_HUD.Settings;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Structs;
using Microsoft.Xna.Framework.Input;

namespace Kenedia.Modules.ReleaseTheChoya.Services
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
            ShowCornerIcon = s.DefineSetting(nameof(ShowCornerIcon), true);

            SpawnChoyaKey = s.DefineSetting(nameof(SpawnChoyaKey), new KeyBinding(Keys.None));
            ToggleChoyaHunt = s.DefineSetting(nameof(ToggleChoyaHunt), new KeyBinding(Keys.None));

            StaticChoya = s.AddSubCollection(nameof(StaticChoya));
        }

        public SettingCollection StaticChoya;

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

        public SettingEntry<bool> ShowCornerIcon;

        public SettingEntry<KeyBinding> SpawnChoyaKey;

        public SettingEntry<KeyBinding> ToggleChoyaHunt;
    }
}
