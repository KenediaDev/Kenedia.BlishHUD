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
            SubModuleType = SubModuleType.SkipCutscenes;

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
