using Blish_HUD;
using Blish_HUD.Modules;
using Blish_HUD.Settings;
using Kenedia.Modules.Core.Models;
using Microsoft.Xna.Framework;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace Kenedia.Modules.AnotherModule
{
    [Export(typeof(Module))]
    public class AnotherModule : BaseModule<AnotherModule>
    {
        private double _tick;
        internal static AnotherModule ModuleInstance;
        public static readonly Logger Logger = Logger.GetLogger<AnotherModule>();

        [ImportingConstructor]
        public AnotherModule([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters)
        {
            ModuleInstance = this;
        }

        //public static GameState GameState = new();

        public SettingEntry<Blish_HUD.Input.KeyBinding> GameStateAdjustKey { get; set; }

        protected override void DefineSettings(SettingCollection settings)
        {
            base.DefineSettings(settings);

        }

        protected override void Initialize()
        {
            base.Initialize();

        }

        protected override async Task LoadAsync()
        {
            await base.LoadAsync();

            await Task.Delay(1);
        }

        protected override void OnModuleLoaded(EventArgs e)
        {
            // Base handler must be called
            base.OnModuleLoaded(e);
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (gameTime.TotalGameTime.TotalMilliseconds - _tick > 500)
            {
                _tick = gameTime.TotalGameTime.TotalMilliseconds;

                //Logger.Debug($"[{Name}] GameStatus: {GameState.GameStatus} [{GameState.Count}]");
            }
        }

        protected override void Unload()
        {
            base.Unload();

            ModuleInstance = null;
        }
    }
}