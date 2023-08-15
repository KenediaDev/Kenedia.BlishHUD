using Blish_HUD.Settings;
using Kenedia.Modules.Core.Models;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kenedia.Modules.QoL.SubModules.WikiSearch
{
    public class WikiSearch : SubModule
    {
        public WikiSearch(SettingCollection settings) : base(settings)
        {
            SubModuleType = SubModuleType.WikiSearch;

            Icon = new()
            {
                Texture = QoL.ModuleInstance.ContentsManager.GetTexture($@"textures\{SubModuleType}.png"),
                HoveredTexture = QoL.ModuleInstance.ContentsManager.GetTexture($@"textures\{SubModuleType}_Hovered.png"),
            };
        }

        public override void Update(GameTime gameTime)
        {

        }
    }
}
