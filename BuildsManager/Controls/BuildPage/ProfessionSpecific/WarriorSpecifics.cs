using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kenedia.Modules.BuildsManager.Controls.BuildPage.ProfessionSpecific
{
    public class WarriorSpecifics : ProfessionSpecifics
    {
        //F:\Guild Wars 2 Assets\Textures\UI Textures\256x64
        private readonly DetailedTexture _emptyAdrenalin = new(156441);
        private readonly DetailedTexture _adrenalin1 = new(156442);
        private readonly DetailedTexture _adrenalin2 = new(156443);
        private readonly DetailedTexture _adrenalin3 = new(156444);

        private readonly DetailedTexture _bladeswornCharges= new(2492047, 2492048);

        public WarriorSpecifics()
        {

        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {

        }

        protected override void ApplyTemplate()
        {
            RecalculateLayout();
        }
    }
}
