using Blish_HUD;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Kenedia.Modules.Core.Structs;
using Kenedia.Modules.Core.Extensions;

namespace Kenedia.Modules.Core.Controls
{
    public class BorderedImage : Image
    {
        public BorderedImage()
        {

        }

        public RectangleDimensions BorderWidth { get; set; } = new(2);

        public Color BorderColor { get; set; } = ContentService.Colors.ColonialWhite;

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.Paint(spriteBatch, bounds);
            
            spriteBatch.DrawFrame(this, bounds, BorderColor, 2);
        }
    }
}