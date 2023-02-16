using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kenedia.Modules.BuildsManager.Controls.BuildPage.ProfessionSpecific
{
    public class WarriorSpecifics
    {
        //F:\Guild Wars 2 Assets\Textures\UI Textures\256x64
        private readonly DetailedTexture _emptyAdrenalin = new(156441);
        private readonly DetailedTexture _adrenalin1 = new(156442);
        private readonly DetailedTexture _adrenalin2 = new(156443);
        private readonly DetailedTexture _adrenalin3 = new(156444);

        private readonly DetailedTexture _bladeswornCharges= new(2492047, 2492048);

        private Template _template;

        public WarriorSpecifics()
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
