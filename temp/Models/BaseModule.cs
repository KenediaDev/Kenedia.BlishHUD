﻿using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Kenedia.Modules.Core.Services;
using Microsoft.Xna.Framework;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Blish_HUD;
using Kenedia.Modules.Core.Res;
using Kenedia.Modules.Core.Views;
using System.Data.SqlTypes;
using Blish_HUD.Controls.Extern;
using Microsoft.Xna.Framework.Input;
using Blish_HUD.Controls;
using Blish_HUD.Gw2Mumble;

namespace Kenedia.Modules.Core.Models
{
    public abstract class BaseModule<ModuleType, ModuleWindow> : Module 
        where ModuleType : Module
        where ModuleWindow : Container
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
        protected static readonly Logger Logger = Logger.GetLogger<ModuleType>();
        protected bool HasGUI = false;
        protected bool IsGUICreated = false;

        protected BaseModule([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters)
        {
            var clientWindowService = new ClientWindowService();
            var sharedSettings = new SharedSettings();
            var gameState = new GameState(clientWindowService, sharedSettings);

            Services = new(gameState, clientWindowService, sharedSettings);
            SharedSettingsView = new SharedSettingsView(sharedSettings, clientWindowService);          
        }

        public static ModuleType ModuleInstance { get; private set; }

        public static VirtualKeyShort[] ModKeyMapping { get; private set; }

        public SemVer.Version ModuleVersion { get; private set; }

        public PathCollection Paths { get; private set; }

        public ServiceCollection Services { get; private set; }

        public SharedSettingsView SharedSettingsView { get; private set; }

        public SettingsManager SettingsManager => ModuleParameters.SettingsManager;

        public ContentsManager ContentsManager => ModuleParameters.ContentsManager;

        public DirectoriesManager DirectoriesManager => ModuleParameters.DirectoriesManager;

        public Gw2ApiManager Gw2ApiManager => ModuleParameters.Gw2ApiManager;

        public ModuleWindow MainWindow { get; protected set; }
        
        protected SettingEntry<Blish_HUD.Input.KeyBinding> ReloadKey { get; set; }

        protected override void Initialize()
        {
            base.Initialize();
            ModuleVersion = Version.BaseVersion();

            Logger.Debug($"Initializing {Name} {ModuleVersion}");
            Paths = new(DirectoriesManager, Name);

            ModKeyMapping = new VirtualKeyShort[5];
            ModKeyMapping[(int)ModifierKeys.Ctrl] = VirtualKeyShort.CONTROL;
            ModKeyMapping[(int)ModifierKeys.Alt] = VirtualKeyShort.MENU;
            ModKeyMapping[(int)ModifierKeys.Shift] = VirtualKeyShort.LSHIFT;
        }

        protected override async Task LoadAsync()
        {
            await base.LoadAsync();

            // Load Global Settings
            await Services.SharedSettings.Load(Paths.SharedSettingsPath);
        }

        protected override void OnModuleLoaded(EventArgs e)
        {
            base.OnModuleLoaded(e);

        }

        protected virtual void LoadGUI()
        {
            UnloadGUI();
            IsGUICreated = true;
        }

        protected virtual void UnloadGUI()
        {
        }

        protected override void DefineSettings(SettingCollection settings)
        {
            base.DefineSettings(settings);

#if DEBUG
            ReloadKey = settings.DefineSetting(nameof(ReloadKey), new Blish_HUD.Input.KeyBinding(ModifierKeys.Alt, Keys.R));
            ReloadKey.Value.Enabled = true;
            ReloadKey.Value.Activated += ReloadKey_Activated;
#endif
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);         

            Services.GameState.Run(gameTime);
            Services.ClientWindowService.Run(gameTime);

            if (HasGUI && !IsGUICreated)
            {
                PlayerCharacter player = GameService.Gw2Mumble.PlayerCharacter;

                if (player != null && !string.IsNullOrEmpty(player.Name))
                {
                    LoadGUI();
                }
            }
        }

        protected override void Unload()
        {
            base.Unload();

#if DEBUG
            ReloadKey.Value.Activated += ReloadKey_Activated;
#endif
            ModuleInstance = null;
        }

        protected virtual void ReloadKey_Activated(object sender, EventArgs e)
        {
            LoadGUI();
        }
    }
}
