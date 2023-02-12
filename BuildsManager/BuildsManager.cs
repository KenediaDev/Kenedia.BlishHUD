using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Modules;
using Blish_HUD.Settings;
using Gw2Sharp.ChatLinks;
using Gw2Sharp.WebApi;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.BuildsManager.Services;
using Kenedia.Modules.BuildsManager.Views;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Services;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
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
            HasGUI = true;
        }

        private GW2API GW2API { get; set; }

        private Data Data { get; set; }

        protected override void DefineSettings(SettingCollection settings)
        {
            base.DefineSettings(settings);

        }

        protected override void Initialize()
        {
            base.Initialize();

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
            };

            Logger.Info($"Starting {Name} v." + Version.BaseVersion());
            Data = new(ContentsManager, Paths);
        }

        protected override async void OnLocaleChanged(object sender, ValueChangedEventArgs<Locale> e)
        {
            if (e.NewValue is not Locale.Korean and not Locale.Chinese)
            {
                GW2API.Cancel();

                if (!Data.Professions.TryGetValue(Gw2Sharp.Models.ProfessionType.Guardian, out Profession profession) || !profession.Names.TryGetValue(e.NewValue, out string name) || string.IsNullOrEmpty(name))
                {
                    Logger.Info($"No data for {e.NewValue} loaded yet. Fetching new data from the API.");
                    await GW2API.UpdateData();

                    if (!GW2API.IsCanceled())
                    {
                        Logger.Info($"Apply fresh {e.NewValue} data to the UI.");
                    }
                }
                else
                {
                    Logger.Info($"Apply {e.NewValue} data to the UI.");
                }

                base.OnLocaleChanged(sender, e);
            }
        }

        protected override async Task LoadAsync()
        {
            await base.LoadAsync();

            await Data.Load();
            GW2API = new(Gw2ApiManager, Data)
            {
                Paths = Paths,
            };

            if (GameService.Overlay.UserLocale.Value is not Locale.Korean and not Locale.Chinese)
            {
                if (!Data.Professions.TryGetValue(Gw2Sharp.Models.ProfessionType.Guardian, out Profession profession) || !profession.Names.TryGetValue(GameService.Overlay.UserLocale.Value, out string name) || string.IsNullOrEmpty(name))
                {
                    Logger.Info($"No data for {GameService.Overlay.UserLocale.Value} loaded yet. Fetching new data from the API.");
                    OnLocaleChanged(this, new(Locale.Chinese, GameService.Overlay.UserLocale.Value));
                }
            }
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

        protected override async void ReloadKey_Activated(object sender, EventArgs e)
        {
            base.ReloadKey_Activated(sender, e);
            //GW2API.GetAllDataAllLocales();
            await GW2API.UpdateData();
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
                new Rectangle(30, 20, Width - 3, Height + 25),
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