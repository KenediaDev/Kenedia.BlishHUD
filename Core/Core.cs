using Blish_HUD;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Services;
using Kenedia.Modules.Core.Structs;
using Microsoft.Xna.Framework;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace Kenedia.Modules.Core
{
    [Export(typeof(Module))]
    public class Core : BaseModule
    {
        internal static Core ModuleInstance;
        public static readonly Logger Logger = Logger.GetLogger<Core>();

        [ImportingConstructor]
        public Core([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters)
        {
            ModuleInstance = this;
        }

        protected override void DefineSettings(SettingCollection settings)
        {
            base.DefineSettings(settings);

            WindowOffset = settings.DefineSetting(nameof(WindowOffset), new RectangleDimensions(8, 31, -8, -8));
        }

        protected override void Initialize()
        {
            Logger.Info($"Starting  {Name} v. {0}");            
            //Logger.Info($"Starting  {Name} v." + Version.BaseVersion());            
        }

        protected override async Task LoadAsync()
        {
            await Task.Delay(1);
        }

        protected override void OnModuleLoaded(EventArgs e)
        {
            // Base handler must be called
            base.OnModuleLoaded(e);
        }

        protected override void Update(GameTime gameTime)
        {
            ClientWindowService.Run(gameTime);
            //GameState.Run(gameTime);
        }

        protected override void Unload()
        {
            ModuleInstance = null;
        }
    }
}