using Blish_HUD;
using Blish_HUD.Content;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Res;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpDX.MediaFoundation;
using System;
using System.Collections.Generic;
using System.Text;
using Kenedia.Modules.Core.Structs;
using System.Linq;
using System.Diagnostics;

namespace Kenedia.Modules.Core.Controls
{
    public class ButtonImage : ImageButton
    {
        private readonly AsyncTexture2D _buttonImage;
        private readonly AsyncTexture2D _hoveredButton;
        private readonly List<(Rectangle bounds, float alpha)> _frameBounds = new();
        private Rectangle _textureBounds;
        private Point? _textureSize;

        public ButtonImage()
        {
            _buttonImage = textures_common.ImageButtonBackground.CreateTexture2D();
            _hoveredButton = textures_common.ImageButtonBackground_Hovered.CreateTexture2D();
        }

        public Point? TextureSize { get => _textureSize; set => Common.SetProperty(ref _textureSize, value, RecalculateLayout); }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (_buttonImage is not null)
                spriteBatch.DrawOnCtrl(this, _buttonImage, bounds, _buttonImage.Bounds, Color.White);

            if (MouseOver)
            {

                for (int i = 0; i < _frameBounds.Count; i++)
                {
                    Rectangle b = _frameBounds[i].bounds;
                    float alpha = _frameBounds[i].alpha;

                    spriteBatch.DrawFrame(this, b, ContentService.Colors.ColonialWhite * alpha, 1);
                }
            }

            base.Paint(spriteBatch, _textureBounds);

        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            Point size = TextureSize ?? Size;
            Point padding = new((Width - size.X) / 2, (Height - size.Y) / 2);
            int xOffset = (int)(Width * 0.15);
            int yOffset = (int)(Height * 0.15);

            _textureBounds = new((xOffset / 2) + padding.X, (yOffset / 2) + padding.Y, size.X - (xOffset * 1), size.Y - (yOffset * 1));
            _frameBounds.Clear();

            int frameWidth = Math.Max(2 , (int)(Width * 0.05));
            float stepSize = 0.75F / (frameWidth - 1);
            for (int i = 0; i < frameWidth; i++)
            {
                _frameBounds.Add(new(new Rectangle(
                    i,
                    i,
                    Width - (i * 2),
                    Height - (i * 2)), i * stepSize));
            }
        }
    }
}
