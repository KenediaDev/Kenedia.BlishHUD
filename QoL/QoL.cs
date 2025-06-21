using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Modules;
using Blish_HUD.Settings;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Services;
using Kenedia.Modules.QoL.Controls;
using Kenedia.Modules.QoL.Services;
using Kenedia.Modules.QoL.SubModules;
using Kenedia.Modules.QoL.SubModules.AutoSniff;
using Kenedia.Modules.QoL.SubModules.CopyItemName;
using Kenedia.Modules.QoL.SubModules.GameResets;
using Kenedia.Modules.QoL.SubModules.ItemDestruction;
using Kenedia.Modules.QoL.SubModules.SchemanticProcessing;
using Kenedia.Modules.QoL.SubModules.SkipCutscenes;
using Kenedia.Modules.QoL.SubModules.WaypointPaste;
using Kenedia.Modules.QoL.SubModules.WikiSearch;
using Kenedia.Modules.QoL.SubModules.ZoomOut;
using Kenedia.Modules.QoL.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace Kenedia.Modules.QoL
{
    [Export(typeof(Module))]
    public class QoL : BaseModule<QoL, StandardWindow, Settings, PathCollection>
    {
        private double _tick;

        [ImportingConstructor]
        public QoL([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters)
        {
            HasGUI = true;
            AutoLoadGUI = true;
        }

        public ModuleHotbar Hotbar { get; set; }

        public ClientWindowService ClientWindowService { get; } = new();

        public Dictionary<SubModuleType, SubModule> SubModules { get; } = new();

        protected override void DefineSettings(SettingCollection settings)
        {
            base.DefineSettings(settings);

            Settings.HotbarExpandDirection.SettingChanged += HotbarExpandDirection_SettingChanged;
            Settings.HotbarButtonSorting.SettingChanged += HotbarButtonSorting_SettingChanged;
        }

        private void HotbarButtonSorting_SettingChanged(object sender, Blish_HUD.ValueChangedEventArgs<SortType> e)
        {
            if (Hotbar is not null)
                Hotbar.SortType = e.NewValue;
        }

        private void HotbarExpandDirection_SettingChanged(object sender, Blish_HUD.ValueChangedEventArgs<ExpandType> e)
        {
            if (Hotbar is not null)
                Hotbar.ExpandType = e.NewValue;
        }

        public override IView GetSettingsView()
        {
            return new SettingsView(() => SettingsWindow?.ToggleWindow());
        }

        protected override void Initialize()
        {
            base.Initialize();

            Logger.Info($"Starting {Name} v." + Version.BaseVersion());

            LoadSubModules();
        }

        protected override ServiceCollection DefineServices(ServiceCollection services)
        {
            var serviceCollection =  base.DefineServices(services);

            services.AddSingleton<GameResets>();
            services.AddSingleton<ZoomOut>();
            services.AddSingleton<SkipCutscenes>();
            services.AddSingleton<ItemDestruction>();
            services.AddSingleton<WikiSearch>();
            services.AddSingleton<WaypointPaste>();
            services.AddSingleton<CopyItemName>();
            //services.AddSingleton<SchemanticProcessing>();
            //services.AddSingleton<AutoSniff>();

            return serviceCollection;
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

            if (Hotbar is not null)
                Hotbar.Visible = GameService.GameIntegration.Gw2Instance.IsInGame && !GameService.Gw2Mumble.UI.IsMapOpen;

            foreach (var subModule in SubModules)
            {
                subModule.Value.Update(gameTime);
            }
        }

        protected override void LoadGUI()
        {
            base.LoadGUI();

            Hotbar?.Dispose();

            Hotbar = new()
            {
                Parent = GameService.Graphics.SpriteScreen,
                TextureRectangle = new(new(50, 50), new(200, 50)),
                Location = Settings.HotbarPosition.Value,
                ExpandType = Settings.HotbarExpandDirection.Value,
                SortType = Settings.HotbarButtonSorting.Value,
                OnMoveAction = (p) => Settings.HotbarPosition.Value = p,
                OpenSettingsAction = () => SettingsWindow?.ToggleWindow(),
            };

            foreach (var subModule in SubModules.Values)
            {
                Hotbar.AddItem(subModule.ToggleControl);
            }

            var settingsBg = AsyncTexture2D.FromAssetId(155997);
            Texture2D cutSettingsBg = settingsBg.Texture.GetRegion(0, 0, settingsBg.Width - 482, settingsBg.Height - 390);

            SettingsWindow = new SettingsWindow(
                settingsBg,
                new Rectangle(30, 30, cutSettingsBg.Width + 10, cutSettingsBg.Height),
                new Rectangle(30, 35, cutSettingsBg.Width - 5, cutSettingsBg.Height - 15),
                Settings,
                SharedSettingsView,
                SubModules)
            {
                Parent = GameService.Graphics.SpriteScreen,
                Title = "❤",
                Subtitle = "❤",
                SavesPosition = true,
                Id = $"{Name} SettingsWindow",
                Version = ModuleVersion,
            };
        }

        protected override void UnloadGUI()
        {
            base.UnloadGUI();

            SettingsWindow?.Dispose();
            Hotbar?.Dispose();
        }

        protected override void Unload()
        {
            base.Unload();

            foreach (var subModule in SubModules.Values)
            {
                subModule?.Unload();
            }

            SubModules.Clear();

            Settings.HotbarExpandDirection.SettingChanged -= HotbarExpandDirection_SettingChanged;
            Settings.HotbarButtonSorting.SettingChanged -= HotbarButtonSorting_SettingChanged;
        }

        protected override void ReloadKey_Activated(object sender, EventArgs e)
        {
            Logger.Debug($"ReloadKey_Activated: {Name}");
            base.ReloadKey_Activated(sender, e);

            foreach (var subModule in SubModules.Values)
            {
                subModule?.Unload();
            }
            SubModules.Clear();

            LoadSubModules();
        }

        private void LoadSubModules()
        {

            SubModules.Add(SubModuleType.GameResets, ServiceProvider.GetRequiredService<GameResets>());
            SubModules.Add(SubModuleType.ZoomOut, ServiceProvider.GetRequiredService<ZoomOut>());
            SubModules.Add(SubModuleType.SkipCutscenes, ServiceProvider.GetRequiredService<SkipCutscenes>());
            SubModules.Add(SubModuleType.ItemDestruction, ServiceProvider.GetRequiredService<ItemDestruction>());
            SubModules.Add(SubModuleType.WikiSearch, ServiceProvider.GetRequiredService<WikiSearch>());
            SubModules.Add(SubModuleType.WaypointPaste, ServiceProvider.GetRequiredService<WaypointPaste>());
            SubModules.Add(SubModuleType.CopyItemName, ServiceProvider.GetRequiredService<CopyItemName>());
            //SubModules.Add(SubModuleType.SchemanticProcessing, ServiceProvider.GetRequiredService<SchemanticProcessing>());
            //SubModules.Add(SubModuleType.AutoSniff, ServiceProvider.GetRequiredService<AutoSniff>());

            foreach (var module in SubModules.Values)
            {
                module?.Load();
            }
        }
    }
}