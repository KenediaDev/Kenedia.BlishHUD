using Blish_HUD;
using Blish_HUD.Input;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using static Blish_HUD.ContentService;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Utility;
using Blish_HUD.Content;
using MathUtil = SharpDX.MathUtil;
using System.ComponentModel;

namespace Kenedia.Modules.BuildsManager.Controls.Selection
{
    public class SelectionPanel : Blish_HUD.Controls.Container
    {
        //Pointer Arrow 784266
        //Back Button Arrow 784268

        private readonly ItemIdSelection _itemIdSelection;
        private readonly GearSelection _gearSelection;
        private readonly BuildSelection _buildSelection;
        private readonly AsyncTexture2D _separator = AsyncTexture2D.FromAssetId(156055);

        private DetailedTexture _backButton = new(784268);
        private DetailedTexture _pointerArrow = new(784266) { TextureRegion = new(16, 16, 32, 32) };
        private Blish_HUD.Controls.Control _anchor;
        private Rectangle _anchorDrawBounds;
        private Rectangle _anchorAbsBounds;
        private Rectangle _anchorBounds;
        private Rectangle _backBounds;
        private Rectangle _backTextBounds;
        private Template _template;

        float _animationStart = 0f;

        public SelectionPanel()
        {
            ClipsBounds = false;

            _itemIdSelection = new()
            {
                Parent = this,
                Visible = false,
            };

            _gearSelection = new()
            {
                Parent = this,
                Visible = false,
            };

            _buildSelection = new()
            {
                Parent = this,
                Visible = true,
            };
        }

        public string Title { get; set; }

        public Rectangle AnchorBounds { get => _anchorBounds; set => _anchorBounds = value; }

        public Template Template
        {
            get => _template; set
            {
                var temp = _template;
                if (Common.SetProperty(ref _template, value, ApplyTemplate, value != null))
                {
                    if (temp != null) temp.Changed -= TemplateChanged;
                    if (temp != null) temp.Changed -= TemplateChanged;

                    if (_template != null) _template.Changed += TemplateChanged;
                    if (_template != null) _template.Changed += TemplateChanged;
                }
            }
        }

        private void TemplateChanged(object sender, PropertyChangedEventArgs e)
        {
            ApplyTemplate();
        }

        private void ApplyTemplate()
        {
            _gearSelection.Template = Template;
        }

        public void SetGearAnchor(Blish_HUD.Controls.Control anchor, Rectangle anchorBounds, GearTemplateSlot slot, string title = "Selection")
        {
            SetAnchor(anchor, anchorBounds, title);
            _gearSelection.ActiveSlot = slot;
            _gearSelection.Show();
        }

        public void SetItemIdAnchor(Blish_HUD.Controls.Control anchor, Rectangle anchorBounds, string title = "Selection")
        {
            SetAnchor(anchor, anchorBounds, title);
            _itemIdSelection.Show();
        }

        public void SetAnchor(Blish_HUD.Controls.Control anchor, Rectangle anchorBounds, string title = "Selection")
        {
            _anchor = anchor;
            Title = title;

            _itemIdSelection.Hide();
            _gearSelection.Hide();
            _buildSelection.Hide();

            if (_anchor != null)
            {
                int size = anchorBounds.Height;
                int y = anchorBounds.Center.Y - (size / 2);
                //_anchorBounds = new(anchorBounds.Left - AbsoluteBounds.Left - (size / 3), y - AbsoluteBounds.Top, size, size);
                _anchorAbsBounds = anchorBounds.Subtract(new(AbsoluteBounds.Location, Point.Zero));
                _anchorBounds = new(anchorBounds.Left - AbsoluteBounds.Left - (size / 2), anchorBounds.Top - AbsoluteBounds.Top + (anchorBounds.Height / 2) - (size / 2), size, size);

                size = Math.Min(size, 32);
                _anchorDrawBounds = new(anchorBounds.Left - AbsoluteBounds.Left - (size / 2), anchorBounds.Top - AbsoluteBounds.Top + (anchorBounds.Height / 2) - (size / 2), size, size);
            }
            else
            {
                _buildSelection.Show();
            }
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            _backBounds = new(10, 5, Width - 32, 55);
            _backButton.Bounds = new(_backBounds.Left + 10, _backBounds.Top + 10, _backBounds.Height - 20, _backBounds.Height - 20);
            _backTextBounds = new(_backButton.Bounds.Right + 10, _backBounds.Top + 10, _backBounds.Width - (_backButton.Bounds.Right + 10), _backBounds.Height - 20);

            if (_itemIdSelection != null) _itemIdSelection.Location = new(10, _backBounds.Bottom + 10);
            if (_gearSelection != null) _gearSelection.Location = new(10, _backBounds.Bottom + 10);
            if (_buildSelection != null) _buildSelection.Location = new(10, 10);
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintAfterChildren(spriteBatch, bounds);
            //RecalculateLayout();

            if (_anchor != null)
            {
                DrawGearSelection(spriteBatch, bounds);
            }
            else
            {
                DrawBuildSelection(spriteBatch, bounds);
            }

            var section = _buildSelection.SelectionBounds;
            //spriteBatch.DrawCenteredRotationOnCtrl(this, _separator, new Rectangle(-28, section.Center.Y + _buildSelection.Top, section.Height, 16), _separator.Bounds, Color.Black, MathUtil.DegreesToRadians(90), false, false);
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

            if (_anchor != null)
            {
                _animationStart += (float)gameTime.ElapsedGameTime.TotalSeconds;

                int easeDistance = _anchorDrawBounds.Width / 3;
                int animationOffset;
                float duration = 0.75F;

                animationOffset = (int)Tweening.Quartic.EaseOut(_animationStart, -easeDistance, easeDistance, duration);
                _pointerArrow.Bounds = _anchorDrawBounds.Add(animationOffset, 0, 0, 0);

                if (animationOffset < -easeDistance)
                {
                    _animationStart -= duration * 2;
                }
            }
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            if (_anchor != null)
            {
                if (_backBounds.Contains(RelativeMousePosition))
                {
                    _anchor = null;

                    _itemIdSelection.Hide();
                    _gearSelection.Hide();
                    _buildSelection.Show();
                }
            }
        }

        private void DrawGearSelection(SpriteBatch spriteBatch, Rectangle bounds)
        {
            //spriteBatch.DrawFrame(this, _anchorAbsBounds, Colors.ColonialWhite);

            _pointerArrow.Draw(this, spriteBatch, null, Color.White);

            if (_backBounds.Contains(RelativeMousePosition))
            {
                spriteBatch.DrawOnCtrl(this, Textures.Pixel, _backBounds, Colors.ColonialWhite * 0.3F);
            }

            _backButton.Draw(this, spriteBatch, RelativeMousePosition, Color.White);
            spriteBatch.DrawStringOnCtrl(this, Title, Content.DefaultFont18, _backTextBounds, Color.White, false, Blish_HUD.Controls.HorizontalAlignment.Left, Blish_HUD.Controls.VerticalAlignment.Middle);
        }

        private void DrawBuildSelection(SpriteBatch spriteBatch, Rectangle bounds)
        {

        }
    }
}
