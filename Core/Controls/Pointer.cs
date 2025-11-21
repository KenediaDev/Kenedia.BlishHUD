using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Control = Blish_HUD.Controls.Control;
using Blish_HUD;
using System;
using Blish_HUD.Controls;

namespace Kenedia.Modules.Core.Controls
{
    public class Pointer : Control
    {
        private readonly DetailedTexture _pointerArrow = new(784266) { TextureRegion = new(16, 16, 32, 32) };

        private float _animationStart = 0f;

        private Rectangle _anchorDrawBounds;
        private Control _anchor;
        private WindowBase2 _container;

        public Pointer()
        {
            Size = new Point(32);
            Parent = Graphics.SpriteScreen;
            //ZIndex = int.MaxValue;
            ClipsBounds = false;            
        }

        public Control Anchor { get => _anchor; set => Common.SetProperty(ref _anchor, value, SetAnchor); }

        public float BounceDistance { get; set; } = 0.25F;

        static Control GetAncestorsParent(Control control)
        {
            return control is null ? null : control == Graphics.SpriteScreen || !control.Visible ? control : GetAncestorsParent(control.Parent);
        }

        static bool IsDrawn(Control c, Rectangle b)
        {
            return c.Parent is not null && c.Parent.Visible is true && c.Parent.AbsoluteBounds.Contains(b.Center) is true && (c.Parent == Graphics.SpriteScreen || IsDrawn(c.Parent, b));
        }

        protected override Blish_HUD.Controls.CaptureType CapturesInput()
        {
            return Blish_HUD.Controls.CaptureType.None;
        }

        private void SetAnchor(object sender, Models.ValueChangedEventArgs<Control> e)
        {
            //Visible = Anchor is not null && Anchor.Visible && IsDrawn(Anchor, AbsoluteBounds) == true;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (Anchor is not null && Anchor.Visible && IsDrawn(Anchor, AbsoluteBounds) == true)
            {
                _pointerArrow.Draw(this, spriteBatch);
            }
        }

        public override void DoUpdate(GameTime gameTime)
        {
            base.DoUpdate(gameTime);

            if (Anchor is not null && Anchor.Visible)
            {
                _animationStart += (float)gameTime.ElapsedGameTime.TotalSeconds;

                int size = Math.Min(Width, Height);
                Location = new(Anchor.AbsoluteBounds.Left - (size / 2), Anchor.AbsoluteBounds.Center.Y - (size / 2));

                _anchorDrawBounds = new(1, 0, size, size);
                BounceDistance = 15F;

                int animationOffset;
                float duration = 0.75F;

                animationOffset = (int)Tweening.Quartic.EaseOut(_animationStart, -BounceDistance, BounceDistance, duration);
                _pointerArrow.Bounds = _anchorDrawBounds.Add(animationOffset, 0, 0, 0);

                if (animationOffset < -BounceDistance)
                {
                    _animationStart -= duration * 2;
                }
            }
        }
    }
}