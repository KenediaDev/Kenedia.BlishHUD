using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Modules;
using Blish_HUD.Settings;
using Kenedia.Modules.Core.Models;
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

namespace Kenedia.Modules.OverflowTradingAssist
{
    [Export(typeof(Module))]
    public class OverflowTradingAssist : BaseModule<OverflowTradingAssist, MainWindow, BaseSettingsModel, Paths>
    {
        private double _tick;

        [ImportingConstructor]
        public OverflowTradingAssist([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters)
        {
            ModuleInstance = this;
            HasGUI = true;
        }

        protected override void DefineSettings(SettingCollection settings)
        {
            base.DefineSettings(settings);

        }

        protected override void Initialize()
        {
            base.Initialize();
            Paths = new(DirectoriesManager, Name);

            HasGUI = false;
            Logger.Info($"Starting {Name} v." + Version.BaseVersion());
        }

        protected override async void ReloadKey_Activated(object sender, EventArgs e)
        {
            Logger.Debug($"ReloadKey_Activated: {Name}");
            base.ReloadKey_Activated(sender, e);

        }

        protected override async Task LoadAsync()
        {
            await base.LoadAsync();

        }

        protected override void OnModuleLoaded(EventArgs e)
        {
            base.OnModuleLoaded(e);
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

            MainWindow.Show();
        }

        protected override void UnloadGUI()
        {
            base.UnloadGUI();

            MainWindow?.Dispose();
        }
    }
}