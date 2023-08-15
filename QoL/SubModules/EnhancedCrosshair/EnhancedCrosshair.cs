using Blish_HUD.Settings;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kenedia.Modules.QoL.SubModules.EnhancedCrosshair
{
    public class EnhancedCrosshair : SubModule
    {
        //1058519
        //1677342
        public EnhancedCrosshair(SettingCollection settings) : base(settings)
        {

        }

        public override SubModuleType SubModuleType => SubModuleType.EnhancedCrosshair;

        public override void Update(GameTime gameTime)
        {

        }
    }
}
