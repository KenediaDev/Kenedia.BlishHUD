using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kenedia.Modules.BuildsManager.Controls.BuildPage.ProfessionSpecific
{
    public class NecromancerSpecifics : ProfessionSpecifics
    {
        private readonly DetailedTexture _lifeForceBarBackground = new(1636710); // black background?
        private readonly DetailedTexture _lifeForceBar = new(2479935); // needs black background
        private readonly DetailedTexture _lifeForceScourge = new(1636711);
        private readonly DetailedTexture _lifeForce = new(156436);
        private readonly DetailedTexture _noTripleShade = new(1636739);
        private readonly DetailedTexture _oneTripleShade = new(1636741);
        private readonly DetailedTexture _twoTripleShade = new(1636743);
        private readonly DetailedTexture _threeTripleShade = new(1636744);
        private readonly DetailedTexture _oneSingleShade = new(1636742);
        private readonly DetailedTexture _noSingleShade = new(1636740);

        public NecromancerSpecifics()
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
