using Blish_HUD.Settings;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kenedia.Modules.QoL.SubModules.WaypointPaste
{
    // Discord Suggestion: https://discord.com/channels/531175899588984842/1064906680904724621/1072599804502364182
    public class WaypointPaste : SubModule
    {
        public WaypointPaste(SettingCollection settings) : base(settings)
        {
            SubModuleType = SubModuleType.WaypointPaste;

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
