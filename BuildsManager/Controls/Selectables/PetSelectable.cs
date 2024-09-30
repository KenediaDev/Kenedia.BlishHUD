using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Blish_HUD;
using Kenedia.Modules.Core.Models;

namespace Kenedia.Modules.BuildsManager.Controls.Selectables
{
    public class PetSelectable : Selectable<Pet>
    {
        private readonly DetailedTexture _highlight = new(156844) { TextureRegion = new(16, 16, 200, 200) };
        private Rectangle _textureBounds;

        public PetSelectable()
        {
            Tooltip = new PetTooltip();
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int pad = 16;
            _textureBounds = new Rectangle(-pad, -pad, Width + (pad * 2), Height + (pad * 2));
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            spriteBatch.DrawOnCtrl(this, IsSelected ? Data.SelectedIcon : Data.Icon, _textureBounds, TextureRegion, Color.White);

            if (MouseOver)
                spriteBatch.DrawOnCtrl(this, _highlight.Texture, _textureBounds, TextureRegion, Color.White);
        }

        protected override void ApplyData(object sender, Core.Models.ValueChangedEventArgs<Pet?> e)
        {
            base.ApplyData(sender, e);

            if (Tooltip is PetTooltip petTooltip)
            {
                petTooltip.Pet = e.NewValue;
            }
        }
    }
}
