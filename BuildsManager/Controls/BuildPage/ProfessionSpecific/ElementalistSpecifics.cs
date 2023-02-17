using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kenedia.Modules.BuildsManager.Controls.BuildPage.ProfessionSpecific
{
    public class ElementalistSpecifics : ProfessionSpecifics
    {
        private readonly DetailedTexture _catalistSeparator = new(2492046);
        private readonly SkillIcon[] skills =
        {
            new(),
            new(),
            new(),
            new(),
            new(),
        };

        public ElementalistSpecifics()
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
