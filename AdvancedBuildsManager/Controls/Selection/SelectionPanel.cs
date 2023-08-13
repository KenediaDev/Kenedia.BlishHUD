using Blish_HUD;
using Blish_HUD.Input;
using Kenedia.Modules.AdvancedBuildsManager.Models.Templates;
using Kenedia.Modules.Core.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using static Blish_HUD.ContentService;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Utility;
using Blish_HUD.Content;
using System.ComponentModel;
using Kenedia.Modules.AdvancedBuildsManager.DataModels.Items;
using Kenedia.Modules.AdvancedBuildsManager.Controls.GearPage;
using Kenedia.Modules.AdvancedBuildsManager.Controls.NotesPage;
using Blish_HUD.Controls;

namespace Kenedia.Modules.AdvancedBuildsManager.Controls.Selection
{
    public class SelectionPanel : Blish_HUD.Controls.Container
    {
        //Pointer Arrow 784266
        //Back Button Arrow 784268

        private readonly GearSelection _gearSelection;
        private readonly BuildSelection _buildSelection;
        private readonly StatSelection _statSelection;
        private readonly SkillSelection _skillSelection;
        private readonly AsyncTexture2D _separator = AsyncTexture2D.FromAssetId(156055);

        private readonly DetailedTexture _backButton = new(784268);
        private readonly DetailedTexture _pointerArrow = new(784266) { TextureRegion = new(16, 16, 32, 32) };
        private Control _subAnchor;
        private Control _mainAnchor;

        private Rectangle AnchorDrawBounds
        {
            get => _selectionType switch
            {
                SelectionTypes.Templates => _mainAnchorDrawBounds,
                _ => _subAnchorDrawBounds
            };

            set
            {
                if (_selectionType == SelectionTypes.Templates)
                {
                    _mainAnchorDrawBounds = value;
                }
                else
                {
                    _subAnchorDrawBounds = value;
                }

                _anchorDrawBounds =
                    _selectionType == SelectionTypes.Templates ? _mainAnchorDrawBounds :
                    _subAnchorDrawBounds;
            }
        }

        private Rectangle _mainAnchorDrawBounds;
        private Rectangle _subAnchorDrawBounds;

        private Rectangle _backBounds;
        private Rectangle _backTextBounds;
        private Template _template;
        private SelectionTypes _selectionType = SelectionTypes.Templates;

        float _animationStart = 0f;
        private Control _anchor;
        private Rectangle _anchorDrawBounds;

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

            _skillSelection = new()
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
            Skills,
        }

        public string Title { get; set; }

        public Template Template
        {
            get => _template; set
            {
                var temp = _template;
                if (Common.SetProperty(ref _template, value, ApplyTemplate))
                {
                    if (temp is not null) temp.PropertyChanged -= TemplateChanged;
                    if (_template is not null) _template.PropertyChanged += TemplateChanged;
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
                    _skillSelection.Visible = _selectionType == SelectionTypes.Skills;
                }
            }
        }

        private Control Anchor
        {
            get => _selectionType switch
            {
                SelectionTypes.Templates => _mainAnchor,
                _ => _subAnchor
            };

            set
            {
                if (_selectionType == SelectionTypes.Templates)
                {
                    _mainAnchor = value;
                }
                else
                {
                    _subAnchor = value;
                }

                _anchor =
                        _selectionType == SelectionTypes.Templates ? _mainAnchor :
                        _subAnchor;
            }
        }

        private void TemplateChanged(object sender, PropertyChangedEventArgs e)
        {
            ApplyTemplate();
        }

        private void ApplyTemplate()
        {
            _gearSelection.Template = Template;
            _statSelection.Template = Template;
            _skillSelection.Template = Template;
        }

        private void SetAnchor(Control anchor, Rectangle? anchorBounds = null)
        {
            Anchor = anchor;

            if (Anchor is not null)
            {
                int size = anchor.AbsoluteBounds.Height;
                size = Math.Min(size, 32);

                AnchorDrawBounds = anchorBounds ?? new(Anchor.AbsoluteBounds.Left - AbsoluteBounds.Left - (size / 2), Anchor.AbsoluteBounds.Top - AbsoluteBounds.Top + (Anchor.AbsoluteBounds.Height / 2) - (size / 2), size, size);
            }
        }

        public void SetSkillAnchor(RotationElementControl anchor)
        {
            SelectionType = SelectionTypes.Skills;
            _skillSelection.Anchor = anchor.RotationElement;
            SetAnchor(anchor);
        }

        public void SetTemplateAnchor(Control anchor)
        {
            SelectionType = SelectionTypes.Templates;
            SetAnchor(anchor);
        }

        public void SetGearAnchor(Control anchor, Rectangle anchorBounds, GearTemplateSlot slot, GearSubSlotType subslot = GearSubSlotType.Item, string title = "Selection", Action<BaseItem> onItemSelected = null)
        {
            SelectionType = SelectionTypes.Items;
            Anchor = anchor;

            if (Anchor == null) return;

            Title = title;

            int size = Math.Min(anchorBounds.Height, 32);
            SetAnchor(anchor, new(anchorBounds.Left - AbsoluteBounds.Left - (size / 2), anchorBounds.Top - AbsoluteBounds.Top + (anchorBounds.Height / 2) - (size / 2), size, size));

            _gearSelection.ActiveSlot = slot;
            _gearSelection.SubSlotType = subslot;
            _gearSelection.TemplateSlot = (anchor as BaseSlotControl)?.TemplateSlot;
            _gearSelection.OnItemSelected = onItemSelected;
        }

        public void SetStatAnchor(Control anchor, Rectangle anchorBounds, string title = "Selection", Action<BaseItem> onItemSelected = null)
        {
            SelectionType = SelectionTypes.Stats;
            Anchor = anchor;

            if (Anchor == null) return;

            Title = title;

            int size = Math.Min(anchorBounds.Height, 32);
            SetAnchor(anchor, new(anchorBounds.Left - AbsoluteBounds.Left - (size / 2), anchorBounds.Top - AbsoluteBounds.Top + (anchorBounds.Height / 2) - (size / 2), size, size));

            _statSelection.TemplateSlot = (anchor as BaseSlotControl)?.TemplateSlot;
            _gearSelection.OnItemSelected = onItemSelected;

        }

        public void ResetAnchor()
        {
            SelectionType = SelectionTypes.Templates;
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            _backBounds = new(10, 5, Width - 32, 55);
            _backButton.Bounds = new(_backBounds.Left + 10, _backBounds.Top + 10, _backBounds.Height - 20, _backBounds.Height - 20);
            _backTextBounds = new(_backButton.Bounds.Right + 10, _backBounds.Top + 10, _backBounds.Width - (_backButton.Bounds.Right + 10), _backBounds.Height - 20);

            if (_gearSelection is not null) _gearSelection.Location = new(10, _backBounds.Bottom + 10);
            if (_statSelection is not null) _statSelection.Location = new(10, _backBounds.Bottom + 10);
            if (_skillSelection is not null) _skillSelection.Location = new(10, 10);
            if (_buildSelection is not null) _buildSelection.Location = new(10, 10);
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintAfterChildren(spriteBatch, bounds);
            //RecalculateLayout();

            if (Anchor is not null && Anchor.Visible && Anchor.Parent is not null && Anchor.Parent.AbsoluteBounds.Contains(Anchor.AbsoluteBounds.Center))
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
                else if (SelectionType == SelectionTypes.Skills)
                {
                    DrawSkillSelection(spriteBatch, bounds);
                }
            }
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

            if (Anchor is not null && Anchor.Parent is not null && Anchor.Parent.AbsoluteBounds.Contains(Anchor.AbsoluteBounds.Center))
            {
                _animationStart += (float)gameTime.ElapsedGameTime.TotalSeconds;

                int size = Anchor.AbsoluteBounds.Height;
                int y = Anchor.AbsoluteBounds.Center.Y - (size / 2);

                size = Math.Min(size, 32);
                _mainAnchorDrawBounds = new(Anchor.AbsoluteBounds.Left - AbsoluteBounds.Left - (size / 2), Anchor.AbsoluteBounds.Top - AbsoluteBounds.Top + (Anchor.AbsoluteBounds.Height / 2) - (size / 2), size, size);

                int easeDistance = AnchorDrawBounds.Width / 3;
                int animationOffset;
                float duration = 0.75F;

                animationOffset = (int)Tweening.Quartic.EaseOut(_animationStart, -easeDistance, easeDistance, duration);
                _pointerArrow.Bounds = AnchorDrawBounds.Add(animationOffset, 0, 0, 0);

                if (animationOffset < -easeDistance)
                {
                    _animationStart -= duration * 2;
                }
            }
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            if (Anchor is not null)
            {
                if (_selectionType is SelectionTypes.Stats or SelectionTypes.Items && _backBounds.Contains(RelativeMousePosition))
                {
                    ResetAnchor();
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
            spriteBatch.DrawStringOnCtrl(this, Title, Content.DefaultFont18, _backTextBounds, Color.White, false, HorizontalAlignment.Left, VerticalAlignment.Middle);
        }

        private void DrawStatSelection(SpriteBatch spriteBatch, Rectangle bounds)
        {
            _pointerArrow.Draw(this, spriteBatch, null, Color.White);

            if (_backBounds.Contains(RelativeMousePosition))
            {
                spriteBatch.DrawOnCtrl(this, Textures.Pixel, _backBounds, Colors.ColonialWhite * 0.3F);
            }

            _backButton.Draw(this, spriteBatch, RelativeMousePosition, Color.White);
            spriteBatch.DrawStringOnCtrl(this, Title, Content.DefaultFont18, _backTextBounds, Color.White, false, HorizontalAlignment.Left, VerticalAlignment.Middle);
        }

        private void DrawSkillSelection(SpriteBatch spriteBatch, Rectangle bounds)
        {
            _pointerArrow.Draw(this, spriteBatch, null, Color.White);

        }

        private void DrawBuildSelection(SpriteBatch spriteBatch, Rectangle bounds)
        {
            _pointerArrow.Draw(this, spriteBatch, null, Color.White);

        }
    }
}
