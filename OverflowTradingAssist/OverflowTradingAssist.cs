﻿using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Modules;
using Blish_HUD.Settings;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.OverflowTradingAssist.Services;
using Kenedia.Modules.OverflowTradingAssist.Models;
using Kenedia.Modules.OverflowTradingAssist.Views;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Blish_HUD.Controls;
using Kenedia.Modules.Core.Res;
using NotificationBadge = Kenedia.Modules.Core.Controls.NotificationBadge;
using CornerIcon = Kenedia.Modules.Core.Controls.CornerIcon;
using LoadingSpinner = Kenedia.Modules.Core.Controls.LoadingSpinner;
using AnchoredContainer = Kenedia.Modules.Core.Controls.AnchoredContainer;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Version = SemVer.Version;
using TextBox = Kenedia.Modules.Core.Controls.TextBox;
using Kenedia.Modules.OverflowTradingAssist.DataEntries;

namespace Kenedia.Modules.OverflowTradingAssist
{
    [Export(typeof(Module))]
    public class OverflowTradingAssist : BaseModule<OverflowTradingAssist, MainWindow, Settings, Paths>
    {
        private double _tick;

        private NotificationBadge _notificationBadge;
        private LoadingSpinner _apiSpinner;
        private CornerIcon _cornerIcon;
        private AnchoredContainer _cornerContainer;

        public static Data Data { get; set; }

        [ImportingConstructor]
        public OverflowTradingAssist([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters)
        {
            ModuleInstance = this;
            HasGUI = true;

            CreateCornerIcons();
        }

        protected override void DefineSettings(SettingCollection settings)
        {
            base.DefineSettings(settings);

            Settings = new Settings(settings);
            Settings.ShowCornerIcon.SettingChanged += ShowCornerIcon_SettingChanged;
        }

        protected override void Initialize()
        {
            base.Initialize();
            Paths = new(DirectoriesManager, Name);
            Logger.Info($"Starting {Name} v." + Version.BaseVersion());

            Data = new(Paths, Gw2ApiManager, () => _notificationBadge, () => _apiSpinner);
        }

        protected override async void ReloadKey_Activated(object sender, EventArgs e)
        {
            Logger.Debug($"ReloadKey_Activated: {Name}");
            base.ReloadKey_Activated(sender, e);

            //var itemids = Gw2ApiManager.Gw2ApiClient.V2.Commerce.Listings.
            //_ = await Data.Load(true);
        }

        protected override async Task LoadAsync()
        {
            await base.LoadAsync();
            _ = Task.Run(Data.Load);
        }

        protected override void OnModuleLoaded(EventArgs e)
        {
            base.OnModuleLoaded(e);

            if (!Settings.ShowCornerIcon.Value)
            {
                DeleteCornerIcons();
            }

            Settings.ToggleWindowKey.Value.Enabled = true;
            Settings.ToggleWindowKey.Value.Activated += OnToggleWindowKey;
        }

        private void OnToggleWindowKey(object sender, EventArgs e)
        {
            if (Control.ActiveControl is not TextBox)
            {
                MainWindow?.ToggleWindow();
            }
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (gameTime.TotalGameTime.TotalMilliseconds - _tick > 500)
            {
                _tick = gameTime.TotalGameTime.TotalMilliseconds;

            }
        }

        protected override void Unload()
        {
            base.Unload();
        }

        protected override void LoadGUI()
        {
            if (!Data.IsLoaded) return;

            base.LoadGUI();

            var settingsBg = AsyncTexture2D.FromAssetId(155983);
            Texture2D cutSettingsBg = settingsBg.Texture.GetRegion(0, 0, settingsBg.Width - 100, settingsBg.Height - 390);

            MainWindow = new(
                settingsBg,
                new Rectangle(30, 30, cutSettingsBg.Width + 10, cutSettingsBg.Height),
                new Rectangle(30, 35, cutSettingsBg.Width - 5, cutSettingsBg.Height - 15))
            {
                Parent = GameService.Graphics.SpriteScreen,
                Title = "❤",
                Subtitle = "❤",
                SavesPosition = true,
                Id = $"{Name} MainWindow",
                Version = ModuleVersion,
                MainWindowEmblem = AsyncTexture2D.FromAssetId(156014),
                SubWindowEmblem = AsyncTexture2D.FromAssetId(156019),
                Name = Name,
                Width = 1200,
                Height = 900,
            };

            MainWindow?.Show();
        }

        protected override void UnloadGUI()
        {
            base.UnloadGUI();

            MainWindow?.Dispose();
        }

        private void DeleteCornerIcons()
        {
            _cornerIcon?.Dispose();
            _cornerIcon = null;

            _cornerContainer?.Dispose();
            _cornerContainer = null;
        }

        private void ShowCornerIcon_SettingChanged(object sender, Blish_HUD.ValueChangedEventArgs<bool> e)
        {
            if (e.NewValue)
            {
                CreateCornerIcons();
            }
            else
            {
                DeleteCornerIcons();
            }
        }

        private void CreateCornerIcons()
        {
            DeleteCornerIcons();

            _cornerIcon = new CornerIcon()
            {
                Icon = AsyncTexture2D.FromAssetId(157088),
                HoverIcon = AsyncTexture2D.FromAssetId(157089),
                SetLocalizedTooltip = () => string.Format(strings_common.ToggleItem, $"{Name}"),
                Parent = GameService.Graphics.SpriteScreen,
                Visible = Settings?.ShowCornerIcon?.Value ?? false,
                ClickAction = () =>
                {

                    Debug.WriteLine($"Loaded? {Data.IsLoaded}");

                    if (!Data.IsLoaded)
                    {
                        _ = Task.Run(() => Data.Load(true));
                        return;
                    }

                    MainWindow?.ToggleWindow();
                }
            };

            _cornerContainer = new()
            {
                Parent = GameService.Graphics.SpriteScreen,
                WidthSizingMode = SizingMode.AutoSize,
                HeightSizingMode = SizingMode.AutoSize,
                Anchor = _cornerIcon,
                AnchorPosition = AnchoredContainer.AnchorPos.Bottom,
                RelativePosition = new(0, -_cornerIcon.Height / 2),
                CaptureInput = CaptureType.Filter,
            };

            _notificationBadge = new NotificationBadge()
            {
                Location = new(_cornerIcon.Width - 15, 0),
                Parent = _cornerContainer,
                Size = new(20),
                Opacity = 0.6f,
                HoveredOpacity = 1f,
                CaptureInput = CaptureType.Filter,
                Anchor = _cornerIcon,
                Visible = false,
                ClickAction = () =>
                {
                    if (!Data.IsLoaded)
                    {
                        _ = Task.Run(() => Data.Load(true));
                        return;
                    }

                    MainWindow?.ToggleWindow();
                }
            };

            _apiSpinner = new LoadingSpinner()
            {
                Location = new Point(0, _notificationBadge.Bottom),
                Parent = _cornerContainer,
                Size = _cornerIcon.Size,
                BasicTooltipText = strings_common.FetchingApiData,
                Visible = false,
                CaptureInput = null,
            };
        }
    }
}