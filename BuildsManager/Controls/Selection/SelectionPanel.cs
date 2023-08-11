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
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.Controls.GearPage;
using Blish_HUD.Controls;
using Kenedia.Modules.BuildsManager.Views;
using Kenedia.Modules.BuildsManager.Models;
using System.Diagnostics;
using System.Collections.Generic;
using Kenedia.Modules.BuildsManager.DataModels.Stats;
using System.Linq;

namespace Kenedia.Modules.BuildsManager.Controls.Selection
{
    public class SelectionPanel : Container
    {
        //Pointer Arrow 784266
        //Back Button Arrow 784268

        private readonly GearSelection _gearSelection;
        private readonly BuildSelection _buildSelection;
        private readonly StatSelection _statSelection;
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
            }
        }

        private Rectangle _mainAnchorDrawBounds;
        private Rectangle _subAnchorDrawBounds;

        private Rectangle _backBounds;
        private Rectangle _backTextBounds;
        private SelectionTypes _selectionType = SelectionTypes.Templates;

        float _animationStart = 0f;
        private Control _anchor;

        public SelectionPanel(TemplatePresenter templatePresenter)
        {
            TemplatePresenter = templatePresenter;

            ClipsBounds = false;

            _gearSelection = new()
            {
                Parent = this,
                Visible = false,
                ZIndex = ZIndex,
                TemplatePresenter = TemplatePresenter,
            };

            _buildSelection = new()
            {
                Parent = this,
                Visible = true,
                SelectionPanel = this,
                ZIndex = ZIndex,
                TemplatePresenter = TemplatePresenter,
            };

            _statSelection = new(TemplatePresenter)
            {
                Parent = this,
                Visible = false,
                ZIndex = ZIndex,
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

        public TemplatePresenter TemplatePresenter { get; private set; }

        public MainWindow MainWindow { get; set; }

        public string Title { get; set; }

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

        private Control Anchor
        {
            get => _anchor;

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

        private void SetAnchor(Control anchor, Rectangle? anchorBounds = null)
        {
            Anchor = anchor;

            if (Anchor != null)
            {
                int size = anchor.AbsoluteBounds.Height;
                size = Math.Min(size, 32);

                AnchorDrawBounds = anchorBounds ?? new(Anchor.AbsoluteBounds.Left - AbsoluteBounds.Left - (size / 2), Anchor.AbsoluteBounds.Top - AbsoluteBounds.Top + (Anchor.AbsoluteBounds.Height / 2) - (size / 2), size, size);
            }
        }

        public void SetAnchor<T>(Control anchor, Rectangle anchorBounds, SelectionTypes selectionType, Enum slot, Enum subslot, Action<T> onClickAction, IReadOnlyList<int> statChoices = null, double? attributeAdjustment = null, string? title = null) where T : class
        {
            Anchor = anchor;
            SelectionType = selectionType;
            Title = title ?? SelectionType.ToString();

            if (Anchor != null)
            {
                int size = Math.Min(anchorBounds.Height, 32);
                AnchorDrawBounds = new(anchorBounds.Left - AbsoluteBounds.Left - (size / 2), anchorBounds.Top - AbsoluteBounds.Top + (anchorBounds.Height / 2) - (size / 2), size, size);

                switch (SelectionType)
                {
                    case SelectionTypes.Items:
                        _gearSelection.ActiveSlot = (TemplateSlotType)slot;
                        _gearSelection.SubSlotType = (GearSubSlotType)subslot;

                        _gearSelection.OnClickAction = (obj) =>
                        {
                            if (obj is T item)
                            {
                                onClickAction(item);
                            }
                        };
                        break;

                    case SelectionTypes.Stats:
                        _statSelection.OnClickAction = (obj) =>
                        {
                            if (obj is T stat)
                            {
                                onClickAction(stat);
                            }
                        };
                        _statSelection.AttributeAdjustments = attributeAdjustment ?? 0;
                        _statSelection.StatChoices = statChoices;
                        break;

                    case SelectionTypes.Templates:
                        _buildSelection.OnClickAction = (obj) =>
                        {
                            if (obj is T item)
                            {
                                onClickAction(item);
                            }
                        };
                        break;
                }
            }
        }

        public void SetTemplateAnchor(Control anchor)
        {
            SelectionType = SelectionTypes.Templates;
            SetAnchor(anchor);

            if (MainWindow != null)
            {
                MainWindow.Template = (anchor as TemplateSelectable)?.Template;
            }
        }

        public void ResetAnchor()
        {
            SelectionType = SelectionTypes.Templates;
            SetAnchor(_buildSelection.Templates.FirstOrDefault(e => e.Template == MainWindow.Template));
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
                else if (SelectionType == SelectionTypes.Skills)
                {
                    DrawSkillSelection(spriteBatch, bounds);
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

            if (Anchor != null)
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
