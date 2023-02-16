using Blish_HUD.Controls;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kenedia.Modules.BuildsManager.Controls.BuildPage.ProfessionSpecific
{
    public class ThiefSpecifics
    {
        private readonly DetailedTexture _noInitiative = new(156439);
        private readonly DetailedTexture _initiative = new(156440);
        private readonly DetailedTexture _specterBar = new(2468316);

        private Template _template;

        public ThiefSpecifics()
        {

        }

        public Template Template { get => _template; set => Common.SetProperty(ref _template, value, ApplyTemplate, value != null); }

        public void Paint(Control ctrl, SpriteBatch spriteBatch, Rectangle bounds)
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
