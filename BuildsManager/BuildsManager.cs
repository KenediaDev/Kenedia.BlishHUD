using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Modules;
using Blish_HUD.Settings;
using Kenedia.Modules.BuildsManager.Services;
using Kenedia.Modules.BuildsManager.Views;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using SharpDX.Direct3D9;
using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Kenedia.Modules.BuildsManager
{
    [Export(typeof(Module))]
    public class BuildsManager : BaseModule<BuildsManager, MainWindow, Settings>
    {
        private double _tick;

        [ImportingConstructor]
        public BuildsManager([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters)
        {
            ModuleInstance = this;
            GW2API = new(Gw2ApiManager);
            HasGUI = true;
        }

        private GW2API GW2API { get; }

        private Data Data { get; set; }

        protected override void DefineSettings(SettingCollection settings)
        {
            base.DefineSettings(settings);

        }

        protected override void Initialize()
        {
            base.Initialize();

            Logger.Info($"Starting {Name} v." + Version.BaseVersion());

            GW2API.Paths = Paths;
            Data = new(ContentsManager);
        }

        protected override async Task LoadAsync()
        {
            await base.LoadAsync();

            await Data.Load();
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

        protected override void ReloadKey_Activated(object sender, EventArgs e)
        {
            base.ReloadKey_Activated(sender, e);
            GW2API.GetAllDataAllLocales();
        }

        protected override void LoadGUI()
        {
            base.LoadGUI();
            int Height = 670;
            int Width = 915;

            Logger.Info($"Building UI for {Name}");

            MainWindow = new MainWindow(
                Services.TexturesService.GetTexture(@"textures\mainwindow_background.png", "mainwindow_background"),
                new Rectangle(30, 30, Width, Height + 30),
                new Rectangle(30, 15, Width - 3, Height + 25),
                Data)
            {
                Parent = GameService.Graphics.SpriteScreen,
                Title = "❤",
                Subtitle = "❤",
                SavesPosition = true,
                Id = $"{Name} MainWindow",
                MainWindowEmblem = AsyncTexture2D.FromAssetId(156020),
                Name = Name,
                Version = ModuleVersion,
            };

            MainWindow.Show();
        }

        protected override void UnloadGUI()
        {
            base.UnloadGUI();

            MainWindow?.Dispose();
        }

        protected override void Unload()
        {
            base.Unload();
        }
    }
}