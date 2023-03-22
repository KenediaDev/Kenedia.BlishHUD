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
using System.ComponentModel;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.Controls.GearPage;

namespace Kenedia.Modules.BuildsManager.Controls.Selection
{
    public class SelectionPanel : Blish_HUD.Controls.Container
    {
        //Pointer Arrow 784266
        //Back Button Arrow 784268

        private readonly GearSelection _gearSelection;
        private readonly BuildSelection _buildSelection;
        private readonly StatSelection _statSelection;
        private readonly AsyncTexture2D _separator = AsyncTexture2D.FromAssetId(156055);

        private DetailedTexture _backButton = new(784268);
        private DetailedTexture _pointerArrow = new(784266) { TextureRegion = new(16, 16, 32, 32) };
        private Blish_HUD.Controls.Control _gearAnchor;
        private Blish_HUD.Controls.Control _templateAnchor;

        private Rectangle _anchorDrawBounds => _gearAnchor != null ? _gearAnchorDrawBounds : _templateAnchorDrawBounds;
        private Rectangle _templateAnchorDrawBounds;
        private Rectangle _gearAnchorDrawBounds;

        private Rectangle AnchorBounds => _gearAnchor != null ? _gearAnchorBounds : _templateAnchorBounds;
        private Rectangle _templateAnchorBounds;
        private Rectangle _gearAnchorBounds;

        private Rectangle _backBounds;
        private Rectangle _backTextBounds;
        private Template _template;
        private SelectionTypes _selectionType = SelectionTypes.Templates;

        float _animationStart = 0f;

        public SelectionPanel()
        {
            ClipsBounds = false;

            _gearSelection = new()
            {
                Parent = this,
                Visible = false,
            };

            _buildSelection = new()
            {
                Parent = this,
                Visible = true,
                SelectionPanel = this
            };

            _statSelection = new()
            {
                Parent = this,
                Visible = false,
            };
        }

        public enum SelectionTypes
        {
            None = 0,
            Templates,
            Items,
            Stats,
        }

        public string Title { get; set; }

        public Template Template
        {
            get => _template; set
            {
                var temp = _template;
                if (Common.SetProperty(ref _template, value, ApplyTemplate))
                {
                    if (temp != null) temp.Changed -= TemplateChanged;
                    if (_template != null) _template.Changed += TemplateChanged;
                }
            }
        }

        public SelectionTypes SelectionType
        {
            get => _selectionType; set
            {
                if (Common.SetProperty(ref _selectionType, value))
                {
                    _gearSelection.Visible = _selectionType == SelectionTypes.Items;
                    _buildSelection.Visible = _selectionType == SelectionTypes.Templates;
                    _statSelection.Visible = _selectionType == SelectionTypes.Stats;
                }
            }
        }

        private Blish_HUD.Controls.Control Anchor => _gearAnchor ?? _templateAnchor;

        private void TemplateChanged(object sender, PropertyChangedEventArgs e)
        {
            ApplyTemplate();
        }

        private void ApplyTemplate()
        {
            _gearSelection.Template = Template;
            _statSelection.Template = Template;
        }

        public void SetTemplateAnchor(Blish_HUD.Controls.Control anchor)
        {
            SelectionType = SelectionTypes.Templates;
            _templateAnchor = anchor;

            if (_templateAnchor != null)
            {
                int size = anchor.AbsoluteBounds.Height;
                int y = anchor.AbsoluteBounds.Center.Y - (size / 2);
                _templateAnchorBounds = new(Anchor.AbsoluteBounds.Left - AbsoluteBounds.Left - (size / 2), Anchor.AbsoluteBounds.Top - AbsoluteBounds.Top + (Anchor.AbsoluteBounds.Height / 2) - (size / 2), size, size);

                size = Math.Min(size, 32);
                _templateAnchorDrawBounds = new(Anchor.AbsoluteBounds.Left - AbsoluteBounds.Left - (size / 2), Anchor.AbsoluteBounds.Top - AbsoluteBounds.Top + (Anchor.AbsoluteBounds.Height / 2) - (size / 2), size, size);
            }
        }

        public void SetGearAnchor(Blish_HUD.Controls.Control anchor, Rectangle anchorBounds, GearTemplateSlot slot, GearSubSlotType subslot = GearSubSlotType.Item, string title = "Selection", Action<BaseItem> onItemSelected = null)
        {
            SelectionType = anchor != null ? SelectionTypes.Items : SelectionTypes.Templates;
            _gearAnchor = anchor;
            if (_gearAnchor == null) return;

            Title = title;

            int size = anchorBounds.Height;
            int y = anchorBounds.Center.Y - (size / 2);
            //_anchorBounds = new(anchorBounds.Left - AbsoluteBounds.Left - (size / 3), y - AbsoluteBounds.Top, size, size);
            _gearAnchorBounds = new(anchorBounds.Left - AbsoluteBounds.Left - (size / 2), anchorBounds.Top - AbsoluteBounds.Top + (anchorBounds.Height / 2) - (size / 2), size, size);

            size = Math.Min(size, 32);
            _gearAnchorDrawBounds = new(anchorBounds.Left - AbsoluteBounds.Left - (size / 2), anchorBounds.Top - AbsoluteBounds.Top + (anchorBounds.Height / 2) - (size / 2), size, size);

            _gearSelection.ActiveSlot = slot;
            _gearSelection.SubSlotType = subslot;
            _gearSelection.TemplateSlot = (anchor as GearSlotControl)?.TemplateSlot;
            _gearSelection.OnItemSelected = onItemSelected;
        }

        public void SetStatAnchor(Blish_HUD.Controls.Control anchor, Rectangle anchorBounds, GearTemplateSlot slot, GearSubSlotType subslot = GearSubSlotType.Item, string title = "Selection", Action<BaseItem> onItemSelected = null)
        {
            SelectionType = anchor != null ? SelectionTypes.Stats : SelectionTypes.Templates;
            _gearAnchor = anchor;
            if (_gearAnchor == null) return;

            Title = title;

            int size = anchorBounds.Height;
            int y = anchorBounds.Center.Y - (size / 2);
            //_anchorBounds = new(anchorBounds.Left - AbsoluteBounds.Left - (size / 3), y - AbsoluteBounds.Top, size, size);
            _gearAnchorBounds = new(anchorBounds.Left - AbsoluteBounds.Left - (size / 2), anchorBounds.Top - AbsoluteBounds.Top + (anchorBounds.Height / 2) - (size / 2), size, size);

            size = Math.Min(size, 32);
            _gearAnchorDrawBounds = new(anchorBounds.Left - AbsoluteBounds.Left - (size / 2), anchorBounds.Top - AbsoluteBounds.Top + (anchorBounds.Height / 2) - (size / 2), size, size);

            _statSelection.TemplateSlot = (anchor as GearSlotControl)?.TemplateSlot;
            _gearSelection.OnItemSelected = onItemSelected;

        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            _backBounds = new(10, 5, Width - 32, 55);
            _backButton.Bounds = new(_backBounds.Left + 10, _backBounds.Top + 10, _backBounds.Height - 20, _backBounds.Height - 20);
            _backTextBounds = new(_backButton.Bounds.Right + 10, _backBounds.Top + 10, _backBounds.Width - (_backButton.Bounds.Right + 10), _backBounds.Height - 20);

            if (_gearSelection != null) _gearSelection.Location = new(10, _backBounds.Bottom + 10);
            if (_statSelection != null) _statSelection.Location = new(10, _backBounds.Bottom + 10);
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

            if (Anchor != null && Anchor.Visible && Anchor.Parent != null && Anchor.Parent.AbsoluteBounds.Contains(Anchor.AbsoluteBounds.Center))
            {
                if (SelectionType == SelectionTypes.Items)
                {
                    DrawGearSelection(spriteBatch, bounds);
                }
                else if (SelectionType == SelectionTypes.Templates)
                {
                    DrawBuildSelection(spriteBatch, bounds);
                }
                else if (SelectionType == SelectionTypes.Stats)
                {
                    DrawStatSelection(spriteBatch, bounds);
                }
            }
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

            if (Anchor != null && Anchor.Parent != null && Anchor.Parent.AbsoluteBounds.Contains(Anchor.AbsoluteBounds.Center))
            {
                _animationStart += (float)gameTime.ElapsedGameTime.TotalSeconds;

                int size = Anchor.AbsoluteBounds.Height;
                int y = Anchor.AbsoluteBounds.Center.Y - (size / 2);
                _templateAnchorBounds = new(Anchor.AbsoluteBounds.Left - AbsoluteBounds.Left - (size / 2), Anchor.AbsoluteBounds.Top - AbsoluteBounds.Top + (Anchor.AbsoluteBounds.Height / 2) - (size / 2), size, size);

                size = Math.Min(size, 32);
                _templateAnchorDrawBounds = new(Anchor.AbsoluteBounds.Left - AbsoluteBounds.Left - (size / 2), Anchor.AbsoluteBounds.Top - AbsoluteBounds.Top + (Anchor.AbsoluteBounds.Height / 2) - (size / 2), size, size);

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

            if (_gearAnchor != null)
            {
                if (_backBounds.Contains(RelativeMousePosition))
                {
                    _gearAnchor = null;
                    SelectionType = SelectionTypes.Templates;
                }
            }
        }

        private void DrawGearSelection(SpriteBatch spriteBatch, Rectangle bounds)
        {
            _pointerArrow.Draw(this, spriteBatch, null, Color.White);

            if (_backBounds.Contains(RelativeMousePosition))
            {
                spriteBatch.DrawOnCtrl(this, Textures.Pixel, _backBounds, Colors.ColonialWhite * 0.3F);
            }

            _backButton.Draw(this, spriteBatch, RelativeMousePosition, Color.White);
            spriteBatch.DrawStringOnCtrl(this, Title, Content.DefaultFont18, _backTextBounds, Color.White, false, Blish_HUD.Controls.HorizontalAlignment.Left, Blish_HUD.Controls.VerticalAlignment.Middle);
        }

        private void DrawStatSelection(SpriteBatch spriteBatch, Rectangle bounds)
        {
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
            _pointerArrow.Draw(this, spriteBatch, null, Color.White);

        }
    }
}
