using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Modules;
using Blish_HUD.Settings;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.QoL.Services;
using Kenedia.Modules.QoL.SubModules;
using Kenedia.Modules.QoL.SubModules.CopyItemName;
using Kenedia.Modules.QoL.SubModules.EnhancedCrosshair;
using Kenedia.Modules.QoL.SubModules.GameResets;
using Kenedia.Modules.QoL.SubModules.ItemDestruction;
using Kenedia.Modules.QoL.SubModules.SkipCutscenes;
using Kenedia.Modules.QoL.SubModules.WaypointPaste;
using Kenedia.Modules.QoL.SubModules.WikiSearch;
using Kenedia.Modules.QoL.SubModules.ZoomOut;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Timers;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Threading.Tasks;
using static Blish_HUD.ContentService;

namespace Kenedia.Modules.QoL
{
    [Export(typeof(Module))]
    public class QoL : BaseModule<QoL, StandardWindow, Settings, PathCollection>
    {
        private double _tick;

        [ImportingConstructor]
        public QoL([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters)
        {
            ModuleInstance = this;
            HasGUI = true;
            AutoLoadGUI = true;
        }

        public Hotbar Hotbar { get; set; }

        public Dictionary<SubModuleType, SubModule> SubModules { get; } = new();

        protected override void DefineSettings(SettingCollection settings)
        {
            base.DefineSettings(settings);

            Settings = new Settings(settings);

        }

        protected override void Initialize()
        {
            base.Initialize();
            Paths = new PathCollection(DirectoriesManager, Name);

            Logger.Info($"Starting {Name} v." + Version.BaseVersion());

            LoadSubModules();
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

            Hotbar = new Hotbar()
            {
                Parent = GameService.Graphics.SpriteScreen,
                TextureRectangle = new(new(50, 50), new(200, 50)),
                Location = Settings.HotbarPosition.Value,
                ExpandType = Settings.HotbarExpandDirection.Value,
                OnMoveAction = (p) => Settings.HotbarPosition.Value = p,
            };

            foreach (var subModule in SubModules.Values)
            {
                Hotbar.AddItem(subModule.ToggleControl);
            }
        }

        protected override void UnloadGUI()
        {
            base.UnloadGUI();

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
        }

        protected override void ReloadKey_Activated(object sender, EventArgs e)
        {
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
            SubModules.Add(SubModuleType.GameResets, new GameResets(SettingCollection));
            SubModules.Add(SubModuleType.ZoomOut, new ZoomOut(SettingCollection));
            SubModules.Add(SubModuleType.SkipCutscenes, new SkipCutscenes(SettingCollection, Services.GameStateDetectionService));
            SubModules.Add(SubModuleType.ItemDestruction, new ItemDestruction(SettingCollection));
            SubModules.Add(SubModuleType.WikiSearch, new WikiSearch(SettingCollection));
            SubModules.Add(SubModuleType.WaypointPaste, new WaypointPaste(SettingCollection));
            SubModules.Add(SubModuleType.CopyItemName, new CopyItemName(SettingCollection));

            foreach (var module in SubModules.Values)
            {
                module?.Load();                
            }
        }
    }
}