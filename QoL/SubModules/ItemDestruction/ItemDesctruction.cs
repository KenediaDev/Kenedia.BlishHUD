using Blish_HUD.Settings;
using Kenedia.Modules.Core.Extensions;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kenedia.Modules.QoL.SubModules.ItemDestruction
{
    internal class ItemDesctruction : SubModule
    {
        private double _tick;

        public ItemDesctruction(SettingCollection settings) : base(settings)
        {
            SubModuleType = SubModuleType.ItemDesctruction;

            Icon = new()
            {
                Texture = QoL.ModuleInstance.ContentsManager.GetTexture($@"textures\{SubModuleType}.png"),
                HoveredTexture = QoL.ModuleInstance.ContentsManager.GetTexture($@"textures\{SubModuleType}_Hovered.png"),
            };

            Load();
        }

        public override void Update(GameTime gameTime)
        {

            if (gameTime.TotalGameTime.TotalMilliseconds - _tick > 500)
            {
                _tick = gameTime.TotalGameTime.TotalMilliseconds;

            }
        }

        protected override void DefineSettings(SettingCollection settings)
        {
            base.DefineSettings(settings);
        }

        protected override void Disable()
        {
            base.Disable();
        }

        override protected void Enable()
        {
            base.Enable();
        }

        public override void Unload()
        {
            base.Unload();
        }

        override public void Load()
        {
            base.Load();
        }

        protected override void SwitchLanguage()
        {
            base.SwitchLanguage();
        }
    }
}
