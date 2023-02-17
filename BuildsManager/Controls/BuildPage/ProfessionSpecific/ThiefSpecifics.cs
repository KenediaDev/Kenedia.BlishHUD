using Blish_HUD.Controls;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kenedia.Modules.BuildsManager.Controls.BuildPage.ProfessionSpecific
{
    public class ThiefSpecifics : ProfessionSpecifics
    {
        private readonly DetailedTexture _noInitiative = new(156439);
        private readonly DetailedTexture _initiative = new(156440);
        private readonly DetailedTexture _specterBar = new(2468316);

        public ThiefSpecifics()
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
