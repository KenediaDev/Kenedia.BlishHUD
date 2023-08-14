using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kenedia.Modules.QoL.SubModules.Resets
{
    public class Resets : SubModule
    {
        public Resets(SettingCollection settings) : base(settings)
        {
            SubModuleType = SubModuleType.Resets;

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
