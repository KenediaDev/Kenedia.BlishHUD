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

        public bool CaptureInput { get; set; } = false;

        protected override CaptureType CapturesInput()
        {
            return CaptureInput ? base.CapturesInput() : CaptureType.None;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            var b = new Rectangle(Location, Size);

            spriteBatch.Draw(ContentService.Textures.TransparentPixel, b, Color.Transparent);
            spriteBatch.End();

            spriteBatch.Begin(_batchParameters);
            //spriteBatch.Draw(ContentService.Textures.Pixel, bounds, Color.Blue);
        }
    }
}
