using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Kenedia.Modules.Core.Services;
using Kenedia.Modules.Core.Structs;
using Microsoft.Xna.Framework;
using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using Kenedia.Modules.Core.Views;
using Blish_HUD.Controls;
using Blish_HUD.Content;
using static Kenedia.Modules.Core.Views.SettingsWindow;

namespace Kenedia.Modules.Core.Models
{
    public abstract class BaseModule : Module
    {
        protected BaseSettingsTab SettingsTab;

        protected AsyncTexture2D Emblem = AsyncTexture2D.FromAssetId(156026);

        protected int Index = 1;

        protected BaseModule([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters)
        {
        }

        public SettingsManager SettingsManager => this.ModuleParameters.SettingsManager;

        public ContentsManager ContentsManager => this.ModuleParameters.ContentsManager;

        public DirectoriesManager DirectoriesManager => this.ModuleParameters.DirectoriesManager;

        public Gw2ApiManager Gw2ApiManager => this.ModuleParameters.Gw2ApiManager;

        public SettingsWindow SettingsWindow => SettingsWindow.Instance;

        public static SettingEntry<RectangleDimensions> WindowOffset { get; set; }

        protected string MainFolderPath { get; set; }

        protected string ModuleFolderPath { get; set; }

        protected override void Initialize()
        {
            base.Initialize();

            Debug.WriteLine($"[{Name}]: {nameof(Initialize)}");

            MainFolderPath = DirectoriesManager.GetFullDirectoryPath("kenedia_modules");
            ModuleFolderPath = $@"{MainFolderPath}\{Name.Replace(' ', '_').ToLower()}";

            if (!Directory.Exists(ModuleFolderPath))
            {
                _ = Directory.CreateDirectory(ModuleFolderPath);
            }
        }

        protected override async Task LoadAsync()
        {
            await base.LoadAsync();

            // Load Global Settings
            await SharedSettings.Load(MainFolderPath);
        }

        protected override void OnModuleLoaded(EventArgs e)
        {
            base.OnModuleLoaded(e);

        }

        protected override void DefineSettings(SettingCollection settings)
        {
            Debug.WriteLine($"[{Name}]: {nameof(DefineSettings)}");
            base.DefineSettings(settings);

            WindowOffset = settings.DefineSetting<RectangleDimensions>(nameof(WindowOffset), new(31, 8, -8, -8));
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            GameState.Run(gameTime);
            ClientWindowService.Run(gameTime);
        }
    }
}
