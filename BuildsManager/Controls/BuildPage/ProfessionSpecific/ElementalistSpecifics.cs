using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kenedia.Modules.BuildsManager.Controls.BuildPage.ProfessionSpecific
{
    public class ElementalistSpecifics : ProfessionSpecifics
    {

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
