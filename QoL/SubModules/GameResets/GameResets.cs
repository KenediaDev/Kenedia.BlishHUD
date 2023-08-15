using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kenedia.Modules.QoL.SubModules.GameResets
{
    public class GameResets : SubModule
    {
        public GameResets(SettingCollection settings) : base(settings)
        {

        }

        public override SubModuleType SubModuleType => SubModuleType.GameResets;

        public override void Update(GameTime gameTime)
        {

        }
    }
}
