using Blish_HUD;
using Blish_HUD.Controls;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Kenedia.Modules.Core.Controls
{
    public class Separator : Control
    {
        //private readonly DetailedTexture _headerSeparator = new(1002163);
        private readonly DetailedTexture _headerSeparator = new(155900);

        public Color Color { get; set; } = Color.White;

        public Separator()
        {
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            for (int i = 0; i < (int)Math.Ceiling(Width / (double)_headerSeparator.Size.X); i++)
            {
                Rectangle r = new(i * _headerSeparator.Size.X, -_headerSeparator.Size.Y / 2, _headerSeparator.Size.X, _headerSeparator.Size.Y);
                spriteBatch.DrawOnCtrl(this, _headerSeparator.Texture, r, _headerSeparator.TextureRegion, Color);
            }
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            //_headerSeparator.Bounds = new Rectangle(0, 0, Width, Math.Min(16, Height));
        }
    }
}
