using Blish_HUD.Modules;
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

namespace Kenedia.Modules.Core.Models
{
    public abstract class BaseModule<T> : Module 
        where T : class
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
        private static readonly Logger Logger = Logger.GetLogger<T>();

        protected BaseModule([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters)
        {
            var clientWindowService = new ClientWindowService();
            var sharedSettings = new SharedSettings();
            var gameState = new GameState(clientWindowService, sharedSettings);

            Services = new(gameState, clientWindowService, sharedSettings);
            SharedSettingsView = new SharedSettingsView(sharedSettings, clientWindowService);          
        }

        public static T ModuleInstance { get; protected set; }

        public static VirtualKeyShort[] ModKeyMapping { get; private set; }

        public SemVer.Version ModuleVersion { get; private set; }

        public PathCollection Paths { get; set; }

        public ServiceCollection Services { get; set; }

        public SharedSettingsView SharedSettingsView;

        public SettingsManager SettingsManager => ModuleParameters.SettingsManager;

        public ContentsManager ContentsManager => ModuleParameters.ContentsManager;

        public DirectoriesManager DirectoriesManager => ModuleParameters.DirectoriesManager;

        public Gw2ApiManager Gw2ApiManager => ModuleParameters.Gw2ApiManager;

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

        protected override void DefineSettings(SettingCollection settings)
        {
            base.DefineSettings(settings);
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);         

            Services.GameState.Run(gameTime);
            Services.ClientWindowService.Run(gameTime);

        }

        protected override void Unload()
        {
            base.Unload();

            ModuleInstance = null;
        }
    }
}
