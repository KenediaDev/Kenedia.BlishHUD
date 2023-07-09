using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Kenedia.Modules.Core.Services;
using Microsoft.Xna.Framework;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Blish_HUD;
using Kenedia.Modules.Core.Views;
using Blish_HUD.Controls.Extern;
using Microsoft.Xna.Framework.Input;
using Blish_HUD.Controls;
using Blish_HUD.Gw2Mumble;
using Newtonsoft.Json;

namespace Kenedia.Modules.Core.Models
{
    public abstract class BaseModule<ModuleType, ModuleWindow, ModuleSettings, ModulePaths> : Module
        where ModuleType : Module
        where ModuleWindow : Container
        where ModuleSettings : BaseSettingsModel
        where ModulePaths : PathCollection, new()
    {
        public static readonly Logger Logger = Logger.GetLogger<ModuleType>();
        protected bool HasGUI = false;
        protected bool AutoLoadGUI = true;
        protected bool IsGUICreated = false;

        protected BaseModule([Import("ModuleParameters")] ModuleParameters moduleParameters) 
            : base(moduleParameters)
        {
            var clientWindowService = new ClientWindowService();
            var sharedSettings = new SharedSettings();
            var texturesService = new TexturesService(ContentsManager);
            var inputDetectionService = new InputDetectionService();
            var gameState = new GameState(clientWindowService, sharedSettings);

            Services = new(gameState, clientWindowService, sharedSettings, texturesService, inputDetectionService);
            SharedSettingsView = new SharedSettingsView(sharedSettings, clientWindowService);
                        
            GameService.Overlay.UserLocale.SettingChanged += OnLocaleChanged;
        }

        public static string ModuleName => ModuleInstance.Name;

        public static ModuleType ModuleInstance { get; protected set; }

        public static VirtualKeyShort[] ModKeyMapping { get; private set; }

        public SemVer.Version ModuleVersion { get; private set; }

        public ModulePaths Paths { get; protected set; }

        public ServiceCollection Services { get; private set; }

        public SharedSettingsView SharedSettingsView { get; private set; }

        public SettingsManager SettingsManager => ModuleParameters.SettingsManager;

        public ContentsManager ContentsManager => ModuleParameters.ContentsManager;

        public DirectoriesManager DirectoriesManager => ModuleParameters.DirectoriesManager;

        public Gw2ApiManager Gw2ApiManager => ModuleParameters.Gw2ApiManager;

        public ModuleWindow MainWindow { get; protected set; }

        public BaseSettingsWindow SettingsWindow { get; protected set; }

        public ModuleSettings Settings { get; protected set; }

        protected SettingEntry<Blish_HUD.Input.KeyBinding> ReloadKey { get; set; }

        protected override void Initialize()
        {
            base.Initialize();
            ModuleVersion = Version;

            Logger.Info($"Initializing {Name} {ModuleVersion}");

            ModKeyMapping = new VirtualKeyShort[5];
            ModKeyMapping[(int)ModifierKeys.Ctrl] = VirtualKeyShort.CONTROL;
            ModKeyMapping[(int)ModifierKeys.Alt] = VirtualKeyShort.MENU;
            ModKeyMapping[(int)ModifierKeys.Shift] = VirtualKeyShort.LSHIFT;

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
            };
        }

        protected override async Task LoadAsync()
        {
            await base.LoadAsync();

            // Load Global Settings
            await Services.SharedSettings.Load(Paths.SharedSettingsPath);
        }

        protected virtual void OnLocaleChanged(object sender, Blish_HUD.ValueChangedEventArgs<Gw2Sharp.WebApi.Locale> eventArgs)
        {
            LocalizingService.OnLocaleChanged(sender, eventArgs);
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
            Logger.Info($"{Name} is started in Debug mode. Enabling Reload Key.");
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
            Services.InputDetectionService.Run(gameTime);

            if (HasGUI && !IsGUICreated && AutoLoadGUI)
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
            UnloadGUI();

            Services?.Dispose();
            GameService.Overlay.UserLocale.SettingChanged -= OnLocaleChanged;

#if DEBUG
            ReloadKey.Value.Activated -= ReloadKey_Activated;
#endif
            ModuleInstance = null;
        }

        protected virtual void ReloadKey_Activated(object sender, EventArgs e)
        {
            LoadGUI();
        }
    }
}
