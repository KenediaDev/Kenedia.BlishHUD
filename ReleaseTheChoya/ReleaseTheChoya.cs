using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using Blish_HUD.Modules;
using Blish_HUD.Settings;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Res;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.ReleaseTheChoya.Models;
using Kenedia.Modules.ReleaseTheChoya.Services;
using Kenedia.Modules.ReleaseTheChoya.Views;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using CornerIcon = Kenedia.Modules.Core.Controls.CornerIcon;

namespace Kenedia.Modules.ReleaseTheChoya
{
    [Export(typeof(Module))]
    public class ReleaseTheChoya : BaseModule<ReleaseTheChoya, StandardWindow, Settings>
    {
        private double _lastMoveTick;
        private double _randomTick;
        private double _idletick;
        private double _choyaHuntTick;
        private Vector3 _lastPlayerPos;
        private CornerIcon _cornerIcon;
        private readonly List<Control> _idleChoya = new();
        private readonly List<Control> _randomChoya = new();
        private readonly List<Control> _persistentChoya = new();
        private readonly List<Control> _staticChoya = new();
        private readonly List<Control> _huntingChoya = new();
        private bool _choyaHuntActive = false;

        [ImportingConstructor]
        public ReleaseTheChoya([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters)
        {
            ModuleInstance = this;
            HasGUI = true;
        }

        protected override void DefineSettings(SettingCollection settings)
        {
            base.DefineSettings(settings);

            Settings = new Settings(settings);
            Settings.ChoyaDelay.SettingChanged += ChoyaDelay_SettingChanged;
            Settings.ChoyaIdleDelay.SettingChanged += ChoyaDelay_SettingChanged;
            Settings.ShowCornerIcon.SettingChanged += ShowCornerIcon_SettingChanged;

            Settings.SpawnChoyaKey.Value.Enabled = true;
            Settings.SpawnChoyaKey.Value.Activated += SpawnManualChoya;

            Settings.ToggleChoyaHunt.Value.Enabled = true;
            Settings.ToggleChoyaHunt.Value.Activated += ToggleChoyaHunt; ;
        }

        private void ToggleChoyaHunt(object sender, EventArgs e)
        {
            _choyaHuntActive = !_choyaHuntActive;
            if (_choyaHuntActive)
            {
                _randomChoya.DisposeAll();
                _randomChoya.Clear();

                _idleChoya.DisposeAll();
                _idleChoya.Clear();
            }
            else
            {
                _huntingChoya.DisposeAll();
                _huntingChoya.Clear();
            }
        }

        private void SpawnManualChoya(object sender, EventArgs e)
        {
            ReleaseAChoya(_persistentChoya);
        }

        private void ShowCornerIcon_SettingChanged(object sender, ValueChangedEventArgs<bool> e)
        {
            _cornerIcon?.Dispose();

            if (e.NewValue)
            {
                _cornerIcon = new CornerIcon()
                {
                    Icon = Services.TexturesService.GetTexture(@"textures\choya_corner_bg.png", "choya_corner_bg"),
                    HoverIcon = Services.TexturesService.GetTexture(@"textures\choya_corner_bg_big.png", "choya_corner_bg_big"),
                    SetLocalizedTooltip = () => string.Format(strings_common.ToggleItem, $"{Name}"),
                    Parent = GameService.Graphics.SpriteScreen,
                    Visible = Settings.ShowCornerIcon.Value,
                    ClickAction = () => SettingsWindow?.ToggleWindow(),
                };
            }
        }

        private void ChoyaDelay_SettingChanged(object sender, ValueChangedEventArgs<Core.Structs.Range> e)
        {
            _randomTick = 0;
            _idletick = 0;
        }

        protected override void Initialize()
        {
            base.Initialize();

            Logger.Info($"Starting {Name} v." + Version.BaseVersion());
        }

        protected override async Task LoadAsync()
        {
            await base.LoadAsync();
        }

        protected override void OnModuleLoaded(EventArgs e)
        {
            base.OnModuleLoaded(e);

            Services.InputDetectionService.Interacted += InputDetectionService_Interacted;
        }

        private void InputDetectionService_Interacted(object sender, double e)
        {
            double now = Common.Now();

            bool idle = Settings.ShowWhenIdle.Value && now - Services.InputDetectionService.LastInteraction >= (Settings.IdleDelay.Value * 1000);
            bool noMove = Settings.ShowWhenStandingStill.Value && now - _lastMoveTick >= (Settings.NoMoveDelay.Value * 1000);

            if (!idle && !noMove)
            {
                _idleChoya?.DisposeAll();
                _idleChoya?.Clear();
            }
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            double now = gameTime.TotalGameTime.TotalMilliseconds;

            if (!_choyaHuntActive)
            {
                if (_lastPlayerPos != GameService.Gw2Mumble.PlayerCharacter.Position)
                {
                    _lastPlayerPos = GameService.Gw2Mumble.PlayerCharacter.Position;
                    _lastMoveTick = now;
                }

                bool idle = Settings.ShowWhenIdle.Value && now - Services.InputDetectionService.LastInteraction >= (Settings.IdleDelay.Value * 1000);
                bool noMove = Settings.ShowWhenStandingStill.Value && now - _lastMoveTick >= (Settings.NoMoveDelay.Value * 1000);
                bool canReleaseChoyas = (Settings.ShowRandomly.Value || idle || noMove) && (!Settings.AvoidCombat.Value || !GameService.Gw2Mumble.PlayerCharacter.IsInCombat);

                if (canReleaseChoyas)
                {
                    if ((idle || noMove) && now - _idletick > 0)
                    {
                        _idletick = now + RandomUtil.GetRandom(
                            Settings.ChoyaIdleDelay.Value.Start,
                            Settings.ChoyaIdleDelay.Value.End);

                        ReleaseAChoya(_idleChoya);
                    }
                    else if (Settings.ShowRandomly.Value && now - _randomTick > 0)
                    {
                        _randomTick = now + RandomUtil.GetRandom(
                            Settings.ChoyaDelay.Value.Start * 1000,
                            Settings.ChoyaDelay.Value.End * 1000);

                        ReleaseAChoya(_randomChoya);
                    }
                }
            }

            if (_choyaHuntActive)
            {
                if(now - _choyaHuntTick > 0)
                {
                    _choyaHuntTick = now + RandomUtil.GetRandom(
                        250,
                        1000);

                    ReleaseAChoya(_huntingChoya, true);
                }
            }
        }

        private void ReleaseAChoya(List<Control> choyas, bool choyaHunt = false)
        {
            Vector2 travelDistance()
            {
                var td = !choyaHunt ? new Vector2(RandomUtil.GetRandom(-Settings.ChoyaTravelDistance.Value.Start, Settings.ChoyaTravelDistance.Value.End), RandomUtil.GetRandom(-Settings.ChoyaTravelDistance.Value.Start, Settings.ChoyaTravelDistance.Value.End)) : new(RandomUtil.GetRandom(-5, 5));
                return td;
            }

            var t = travelDistance();

            while (t == Vector2.Zero)
            {
                t = travelDistance();
            }

            var rollingChoya = new RollingChoya(Services.InputDetectionService)
            {
                ChoyaTexture = Services.TexturesService.GetTexture(textures_common.RollingChoya, nameof(textures_common.RollingChoya)),
                Parent = GameService.Graphics.SpriteScreen,
                Size = new(RandomUtil.GetRandom(Settings.ChoyaSize.Value.Start, Settings.ChoyaSize.Value.End)),
                Steps = RandomUtil.GetRandom(Settings.ChoyaSpeed.Value.Start, Settings.ChoyaSpeed.Value.End),
                TravelDistance = t,
                ZIndex = int.MaxValue - 10,
                ChoyaHunt = choyaHunt,
                CaptureInput = false,
                Enabled = false,
            };

            int y = RandomUtil.GetRandom(1 - rollingChoya.Size.Y, GameService.Graphics.SpriteScreen.Height - (rollingChoya.Size.Y / 4));

            int x = y < 0 || y > GameService.Graphics.SpriteScreen.Height - rollingChoya.Size.Y ?
                RandomUtil.GetRandom(1 - rollingChoya.Size.X, GameService.Graphics.SpriteScreen.Width - (rollingChoya.Size.X / 4)) :
                RandomUtil.GetRandom(0, 1) == 1 ? GameService.Graphics.SpriteScreen.Width - (rollingChoya.Size.X / 4) : 1 - rollingChoya.Size.X;

            rollingChoya.StartPoint = new(x, y);

            rollingChoya.ChoyaLeftBounds += RollingChoya_ChoyaLeftBounds;
            choyas.Add(rollingChoya);

            if (choyaHunt)
            {
                rollingChoya.Click += RollingChoya_Click;
            }
        }

        private void RollingChoya_Click(object sender, MouseEventArgs e)
        {
            ScreenNotification.ShowNotification("Right in the face!", ScreenNotification.NotificationType.Error);
            var choya = sender as RollingChoya;
            choya.Dispose();

            _ = _huntingChoya?.Remove(choya);
        }

        private void RollingChoya_ChoyaLeftBounds(object sender, EventArgs e)
        {
            var choya = sender as RollingChoya;

            choya.ChoyaLeftBounds -= RollingChoya_ChoyaLeftBounds;
            choya.Dispose();

            _ = _randomChoya?.Remove(choya);
            _ = _idleChoya?.Remove(choya);
        }

        public override IView GetSettingsView()
        {
            return new SettingsView(() => SettingsWindow?.ToggleWindow(), Services.TexturesService);
        }

        protected override void LoadGUI()
        {
            base.LoadGUI();

            var settingsBg = AsyncTexture2D.FromAssetId(155997);
            Texture2D cutSettingsBg = settingsBg.Texture.GetRegion(0, 0, settingsBg.Width - 482, settingsBg.Height - 390);

            SettingsWindow = new SettingsWindow(
                settingsBg,
                new Rectangle(30, 30, cutSettingsBg.Width + 10, cutSettingsBg.Height),
                new Rectangle(30, 35, cutSettingsBg.Width - 5, cutSettingsBg.Height - 15),
                Settings,
                Services.TexturesService)
            {
                MainWindowEmblem = Services.TexturesService.GetTexture(@"textures\choya_emblem.png", "choya_emblem"),
                Parent = GameService.Graphics.SpriteScreen,
                Title = "❤",
                Subtitle = "❤",
                SavesPosition = true,
                Id = $"{Name} SettingsWindow",
                Version = ModuleVersion,
                Name = Name,
            };

            if (Settings.ShowCornerIcon.Value)
            {
                _cornerIcon = new CornerIcon()
                {
                    Icon = Services.TexturesService.GetTexture(@"textures\choya_corner_bg.png", "choya_corner_bg"),
                    HoverIcon = Services.TexturesService.GetTexture(@"textures\choya_corner_bg_big.png", "choya_corner_bg_big"),
                    SetLocalizedTooltip = () => string.Format(strings_common.ToggleItem, $"{Name}"),
                    Parent = GameService.Graphics.SpriteScreen,
                    Visible = Settings.ShowCornerIcon.Value,
                    ClickAction = () => SettingsWindow?.ToggleWindow(),
                };
            }
        }

        protected override void UnloadGUI()
        {
            base.UnloadGUI();

            SettingsWindow?.Dispose();
            _cornerIcon?.Dispose();

            _staticChoya?.DisposeAll();
            _staticChoya?.Clear();

            _persistentChoya?.DisposeAll();
            _persistentChoya?.Clear();

            _randomChoya?.DisposeAll();
            _randomChoya?.Clear();

            _idleChoya?.DisposeAll();
            _idleChoya?.Clear();
        }

        protected override void ReloadKey_Activated(object sender, EventArgs e)
        {
            base.ReloadKey_Activated(sender, e);
            SettingsWindow?.ToggleWindow();
        }

        protected override void Unload()
        {
            base.Unload();

            foreach (SettingEntry<Choya> choya in Settings.StaticChoya)
            {
                choya.Value.Dispose();
            }

            Settings.ChoyaDelay.SettingChanged -= ChoyaDelay_SettingChanged;
            Settings.ChoyaIdleDelay.SettingChanged -= ChoyaDelay_SettingChanged;

            Services.InputDetectionService.Interacted -= InputDetectionService_Interacted;
        }
    }
}