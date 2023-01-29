using Blish_HUD;
using Blish_HUD.Modules;
using Blish_HUD.Settings;
using Kenedia.Modules.Core.Models;
using Microsoft.Xna.Framework;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace Kenedia.Modules.AModule
{
    [Export(typeof(Module))]
    public class AModule : BaseModule
    {
        internal static AModule ModuleInstance;
        private double _tick;
        public static readonly Logger Logger = Logger.GetLogger<AModule>();

        [ImportingConstructor]
        public AModule([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters)
        {
            ModuleInstance = this;
        }

        protected override void DefineSettings(SettingCollection settings)
        {
            base.DefineSettings(settings);

        }

        protected override void Initialize()
        {
            base.Initialize();

            Logger.Info($"Starting  {Name} v. {0}");
            //Logger.Info($"Starting  {Name} v." + Version.BaseVersion());            
        }

        protected override async Task LoadAsync()
        {
            await base.LoadAsync();
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