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
            SubModuleType = SubModuleType.EnhancedCrosshair;

            Icon = new()
            {
                Texture = QoL.ModuleInstance.ContentsManager.GetTexture($@"textures\{SubModuleType}.png"),
                HoveredTexture = QoL.ModuleInstance.ContentsManager.GetTexture($@"textures\{SubModuleType}_Hovered.png"),
            };

            Load();
        }

        public override void Update(GameTime gameTime)
        {

        }
    }
}
