using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kenedia.Modules.Core.Controls
{
    public class MaskedRegion : Control
    {
        private readonly SpriteBatchParameters _batchParameters;

        public MaskedRegion()
        {
            ZIndex = int.MaxValue;

            _batchParameters = SpriteBatchParameters;
            SpriteBatchParameters = new SpriteBatchParameters(SpriteSortMode.Deferred, BlendState.Opaque);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            var b = new Rectangle(bounds.Location, bounds.Size);

            spriteBatch.Draw(ContentService.Textures.TransparentPixel, b, Color.Transparent);
            spriteBatch.End();

            spriteBatch.Begin(_batchParameters);
        }
    }
}
