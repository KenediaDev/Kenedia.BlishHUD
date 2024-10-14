using Blish_HUD.Input;
using Blish_HUD.Settings;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Structs;
using Microsoft.Xna.Framework.Input;

namespace Kenedia.Modules.ReleaseTheChoya.Services
{
    public class Settings : BaseSettingsModel
    {
        public Settings(SettingCollection s) : base(s)
        {
        }
        protected override void InitializeSettings(SettingCollection settings)
        {
            base.InitializeSettings(settings);

            ChoyaDelay = settings.DefineSetting(nameof(ChoyaDelay), new Range(30, 90));
            ChoyaIdleDelay = settings.DefineSetting(nameof(ChoyaIdleDelay), new Range(125, 1500));
            ChoyaSpeed = settings.DefineSetting(nameof(ChoyaSpeed), new Range(64, 720));
            ChoyaTravelDistance = settings.DefineSetting(nameof(ChoyaTravelDistance), new Range(4, 30));
            ChoyaSize = settings.DefineSetting(nameof(ChoyaSize), new Range(64, 512));

            IdleDelay = settings.DefineSetting(nameof(IdleDelay), 15);
            NoMoveDelay = settings.DefineSetting(nameof(NoMoveDelay), 15);

            ShowWhenStandingStill = settings.DefineSetting(nameof(ShowWhenStandingStill), true);
            ShowRandomly = settings.DefineSetting(nameof(ShowRandomly), true);
            ShowWhenIdle = settings.DefineSetting(nameof(ShowWhenIdle), true);
            AvoidCombat = settings.DefineSetting(nameof(AvoidCombat), true);
            ShowCornerIcon = settings.DefineSetting(nameof(ShowCornerIcon), true);

            SpawnChoyaKey = settings.DefineSetting(nameof(SpawnChoyaKey), new KeyBinding(Keys.None));
            ToggleChoyaHunt = settings.DefineSetting(nameof(ToggleChoyaHunt), new KeyBinding(Keys.None));

            StaticChoya = settings.AddSubCollection(nameof(StaticChoya));
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
