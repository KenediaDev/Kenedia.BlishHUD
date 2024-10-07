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
using Blish_HUD.GameIntegration;
using Microsoft.Extensions.DependencyInjection;

namespace Kenedia.Modules.Core.Models
{
    public abstract class BaseModule<ModuleType, ModuleWindow, ModuleSettings, ModulePaths> : Module
        where ModuleType : Module
        where ModuleWindow : Container
        where ModuleSettings : BaseSettingsModel
        where ModulePaths : PathCollection, new()
    {
        public static Logger Logger = Logger.GetLogger<ModuleType>();

        public StaticHosting StaticHosting { get; private set; }

        protected bool HasGUI = false;
        protected bool AutoLoadGUI = true;
        protected bool IsGUICreated = false;

        protected BaseModule([Import("ModuleParameters")] ModuleParameters moduleParameters)
            : base(moduleParameters)
        {

            GameService.Overlay.UserLocale.SettingChanged += OnLocaleChanged;

            SetupServices();
        }

        private void SetupServices()
        {
            var services = new ServiceCollection();
            DefineServices(services);
            ServiceProvider = services.BuildServiceProvider();

            AssignServiceInstaces(ServiceProvider);
        }

        /// <summary>
        /// Register all services for the module
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        protected virtual ServiceCollection DefineServices(ServiceCollection services)
        {
            services.AddSingleton<Module>(this);
            services.AddSingleton<ModuleWindow>();
            services.AddSingleton<ModuleSettings>();
            services.AddSingleton<ModulePaths>();

            services.AddSingleton<ContentsManager>(ModuleParameters.ContentsManager);
            services.AddSingleton<SettingsManager>(ModuleParameters.SettingsManager);
            services.AddSingleton<Gw2ApiManager>(ModuleParameters.Gw2ApiManager);
            services.AddSingleton<DirectoriesManager>(ModuleParameters.DirectoriesManager);
            services.AddSingleton<Logger>(Logger);

            services.AddSingleton<StaticHosting>();
            services.AddSingleton<ClientWindowService>();
            services.AddSingleton<SharedSettings>();
            services.AddSingleton<InputDetectionService>();
            services.AddSingleton<GameStateDetectionService>();

            services.AddSingleton<SharedSettingsView>();
            services.AddSingleton<CoreServiceCollection>();

            //services.AddSingleton<TexturesService>();

            return services;
        }

        /// <summary>
        /// Assign all necessary service instances to the module
        /// </summary>
        /// <param name="serviceProvider"></param>
        protected virtual void AssignServiceInstaces(IServiceProvider serviceProvider)
        {
            CoreServices = serviceProvider.GetRequiredService<CoreServiceCollection>();
            SharedSettingsView = serviceProvider.GetRequiredService<SharedSettingsView>();  
            ContentsManager = serviceProvider.GetRequiredService<ContentsManager>();
            DirectoriesManager = serviceProvider.GetRequiredService<DirectoriesManager>();
            Gw2ApiManager = serviceProvider.GetRequiredService<Gw2ApiManager>();
            SettingsManager = serviceProvider.GetRequiredService<SettingsManager>();
            Logger = serviceProvider.GetRequiredService<Logger>();
            StaticHosting = serviceProvider.GetRequiredService<StaticHosting>();

            Paths =  serviceProvider.GetRequiredService<ModulePaths>();
            Settings =  serviceProvider.GetRequiredService<ModuleSettings>();

            TexturesService.Initilize(ContentsManager);
        }

        public static string ModuleName => ModuleInstance.Name;

        public static ModuleType ModuleInstance { get; protected set; }

        public static VirtualKeyShort[] ModKeyMapping { get; private set; }

        public SemVer.Version ModuleVersion { get; private set; }

        public ModulePaths Paths { get; protected set; }

        public SettingCollection SettingCollection { get; private set; }

        public CoreServiceCollection CoreServices { get; private set; }

        public SharedSettingsView SharedSettingsView { get; private set; }

        public SettingsManager SettingsManager { get; private set; }

        public ContentsManager ContentsManager { get; private set; }

        public DirectoriesManager DirectoriesManager { get; private set; }

        public Gw2ApiManager Gw2ApiManager { get; private set; }

        public ModuleWindow MainWindow { get; protected set; }

        public BaseSettingsWindow SettingsWindow { get; protected set; }

        public ModuleSettings Settings { get; protected set; }

        protected SettingEntry<Blish_HUD.Input.KeyBinding> ReloadKey { get; set; }

        public IServiceProvider ServiceProvider { get; private set; }

        protected override void Initialize()
        {
            base.Initialize();
            ModuleVersion = Version;

            Logger.Info($"Initializing {Name} {ModuleVersion}");

            ModKeyMapping = new VirtualKeyShort[5];
            ModKeyMapping[(int)ModifierKeys.Ctrl] = VirtualKeyShort.CONTROL;
            ModKeyMapping[(int)ModifierKeys.Alt] = VirtualKeyShort.MENU;
            ModKeyMapping[(int)ModifierKeys.Shift] = VirtualKeyShort.LSHIFT;

            // SOTO Fix
            if (Program.OverlayVersion < new SemVer.Version(1, 1, 0))
            {
                try
                {
                    var tacoActive = typeof(TacOIntegration).GetProperty(nameof(TacOIntegration.TacOIsRunning)).GetSetMethod(true);
                    _ = (tacoActive?.Invoke(GameService.GameIntegration.TacO, new object[] { true }));
                }
                catch { /* NOOP */ }
            }
        }

        protected override async Task LoadAsync()
        {
            await base.LoadAsync();

            // Load Global Settings
            await CoreServices.SharedSettings.Load(Paths.SharedSettingsPath);
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
            SettingCollection = settings;

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

            CoreServices.GameStateDetectionService.Run(gameTime);
            CoreServices.ClientWindowService.Run(gameTime);
            CoreServices.InputDetectionService.Run(gameTime);

            if (HasGUI && !IsGUICreated && AutoLoadGUI)
            {
                PlayerCharacter player = GameService.Gw2Mumble.PlayerCharacter;

                if (player is not null && !string.IsNullOrEmpty(player.Name))
                {
                    LoadGUI();
                }
            }
        }

        protected override void Unload()
        {
            UnloadGUI();

            CoreServices?.Dispose();
            GameService.Overlay.UserLocale.SettingChanged -= OnLocaleChanged;

#if DEBUG
            ReloadKey.Value.Activated -= ReloadKey_Activated;
#endif
            ModuleInstance = null;
            base.Unload();
        }

        protected virtual void ReloadKey_Activated(object sender, EventArgs e)
        {
#if DEBUG
            LoadGUI();
#endif
        }
    }
}
