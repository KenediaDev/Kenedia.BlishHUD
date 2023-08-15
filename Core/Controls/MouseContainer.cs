using Container = Blish_HUD.Controls.Container;
using Control = Blish_HUD.Controls.Control;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Blish_HUD;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Structs;
using Microsoft.Xna.Framework.Graphics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;

namespace Kenedia.Modules.Core.Controls
{
    public class MouseContainer : Panel
    {
        public MouseContainer()
        {

        }

        public DetailedTexture Background { get; set; }

        public RectangleDimensions TexturePadding { get; set; } = new(50);

        public Point MouseOffset { get; set; } = new(15);

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            if (Background != null)
            {
                Background.Bounds = new Rectangle(0, 0, Width, Height);
                Background.TextureRegion = new Rectangle(
                    TexturePadding.Left,
                    TexturePadding.Top,
                    Math.Min(Background.Texture.Width, Width) - TexturePadding.Horizontal,
                    Math.Min(Background.Texture.Height, Height) - TexturePadding.Vertical);
            }
        }
        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

            Location = Input.Mouse.Position.Add(MouseOffset);
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            Background?.Draw(this, spriteBatch);
            base.PaintBeforeChildren(spriteBatch, bounds);
        }
    }
}
