using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kenedia.Modules.BuildsManager.Controls.BuildPage.ProfessionSpecific
{
    public class ElementalistSpecifics
    {
        private Template _template;

        public ElementalistSpecifics()
        {

        }

        public Template Template { get => _template; set => Common.SetProperty(ref _template, value, ApplyTemplate, value != null); }

        public void Paint(Blish_HUD.Controls.Control control, SpriteBatch spriteBatch, Rectangle bounds)
        {

        }

        public void RecalculateLayout()
        {

        }

        private void ApplyTemplate()
        {
            RecalculateLayout();
        }
    }
}
