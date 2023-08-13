using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kenedia.Modules.BuildsManager.Controls.BuildPage
{
    public class TraitIcon : Blish_HUD.Controls.Control
    {
        private readonly DetailedTexture _texture = new();
        private Trait _trait;
        
        public TraitIcon()
        {
            Tooltip = new TraitTooltip();

        }

        public Trait Trait { get => _trait; set => Common.SetProperty(ref _trait, value, ApplyTrait); }

        public bool Selected { get; set; }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            _texture.Bounds = new(Point.Zero, Size);
            _texture.Draw(this, spriteBatch, RelativeMousePosition, Selected ? Color.White : MouseOver ? Color.LightGray : Color.Gray);
        }

        private void ApplyTrait()
        {
            _texture.Texture = Trait?.Icon;

            if(Tooltip is TraitTooltip tooltip)
            {
                tooltip.Trait = Trait;
            }

            if (Trait != null && Trait.Icon != null)
            {
                int padding = Trait.Icon.Width / 16;
                _texture.TextureRegion = new(padding, padding, Trait.Icon.Width - (padding * 2), Trait.Icon.Height - (padding * 2));
            }
        }
    }
}
