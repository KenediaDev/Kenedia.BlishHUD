using Blish_HUD.Settings;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kenedia.Modules.QoL.SubModules.SkipCutscenes
{
    public class SkipCutscenes : SubModule
    {
        public SkipCutscenes(SettingCollection settings) : base(settings)
        {

        }

        public override SubModuleType SubModuleType => SubModuleType.SkipCutscenes;

        public override void Update(GameTime gameTime)
        {

        }
    }
}
