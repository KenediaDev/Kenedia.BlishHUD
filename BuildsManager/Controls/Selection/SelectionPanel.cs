using Blish_HUD;
using Blish_HUD.Input;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using static Blish_HUD.ContentService;
using Kenedia.Modules.Core.Utility;
using Blish_HUD.Controls;
using Kenedia.Modules.BuildsManager.Views;
using Kenedia.Modules.BuildsManager.Models;
using System.Collections.Generic;
using System.Linq;
using Kenedia.Modules.BuildsManager.Extensions;
using Kenedia.Modules.BuildsManager.Services;
using Kenedia.Modules.Core.Controls;

namespace Kenedia.Modules.BuildsManager.Controls.Selection
{
    public class SelectionPanel : Container
    {
        //Pointer Arrow 784266
        //Back Button Arrow 784268

        private readonly GearSelection _gearSelection;
        public BuildSelection BuildSelection { get; }

        private readonly StatSelection _statSelection;

        private readonly DetailedTexture _backButton = new(784268);
        private Control? _subAnchor;
        private Control? _mainAnchor;
        private Rectangle _backBounds;
        private Rectangle _backTextBounds;
        private SelectionTypes _selectionType = SelectionTypes.Templates;

        private Control? _anchor;
        private MainWindow _mainWindow;

        public SelectionPanel(TemplatePresenter templatePresenter, TemplateCollection templates, TemplateTags templateTags, Data data, TemplateFactory templateFactory, Settings settings)
        {
            TemplatePresenter = templatePresenter;
            Data = data;
            Settings = settings;
            Pointer = new();

            ClipsBounds = false;
            //Location = new(0, 0);
            HeightSizingMode = SizingMode.Fill;
            Width = 375;

            _gearSelection = new(TemplatePresenter, data)
            {
                Parent = this,
                Visible = false,
                ZIndex = ZIndex,
            };

            BuildSelection = new(templates, templateTags, data, templatePresenter, templateFactory, settings)
            {
                Parent = this,
                Visible = true,
                SelectionPanel = this,
                ZIndex = ZIndex,
            };

            _statSelection = new(TemplatePresenter, data)
            {
                Parent = this,
                Visible = false,
                ZIndex = ZIndex,
            };

            GameService.Gw2Mumble.PlayerCharacter.NameChanged += ApplyAutoFilter;
            ApplyAutoFilter();
            TemplatePresenter.TemplateChanged += TemplatePresenter_TemplateChanged;
        }

        public void ApplyAutoFilter(object sender = null, EventArgs e = null)
        {
            SelectFirstTemplate();
        }

        public TemplatePresenter TemplatePresenter { get; private set; }

        public Data Data { get; }

        public Settings Settings { get; }

        public MainWindow MainWindow
        {
            get => _mainWindow;
            set
            {
                _mainWindow = value;                
            }
        }

        public string Title { get; set; }

        public SelectionTypes SelectionType
        {
            get => _selectionType; set
            {
                if (Common.SetProperty(ref _selectionType, value))
                {
                    _gearSelection.Visible = _selectionType == SelectionTypes.Items;
                    BuildSelection.Visible = _selectionType == SelectionTypes.Templates;
                    _statSelection.Visible = _selectionType == SelectionTypes.Stats;
                }
            }
        }

        public Control Anchor
        {
            get => _anchor;

            private set
            {
                if (_selectionType == SelectionTypes.Templates)
                {
                    _mainAnchor = value;
                }
                else
                {
                    _subAnchor = value;
                }

                Pointer.Anchor = _anchor =
                        _selectionType == SelectionTypes.Templates ? _mainAnchor :
                        _subAnchor;
            }
        }

        public GearSubSlotType SubSlotType { get; private set; }

        public Pointer Pointer { get; set; }

        private void TemplatePresenter_TemplateChanged(object sender, Core.Models.ValueChangedEventArgs<Template> e)
        {
            SetTemplateAnchor(BuildSelection.TemplateSelectables.FirstOrDefault(x => x.Template == e.NewValue));
        }

        private void SetAnchor(Control anchor, Rectangle? anchorBounds = null)
        {
            Anchor = anchor;
        }

        public void SetAnchor<T>(Control anchor, Rectangle anchorBounds, SelectionTypes selectionType, Enum slot, Enum subslot, Action<T> onClickAction, IReadOnlyList<int> statChoices = null, double? attributeAdjustment = null, string? title = null) where T : class
        {
            Anchor = anchor;
            SelectionType = selectionType;
            Title = title ?? SelectionType.ToString();

            if (Anchor is not null)
            {
                switch (SelectionType)
                {
                    case SelectionTypes.Items:
                        if (!Data.IsLoaded) return;

                        if (((TemplateSlotType)slot).GetGroupType() != _gearSelection.ActiveSlot.GetGroupType())
                        {
                            _gearSelection.Search.Text = string.Empty;
                        }
                        else if (subslot is not null && (GearSubSlotType)subslot != _gearSelection.SubSlotType)
                        {
                            _gearSelection.Search.Text = string.Empty;
                        }

                        _gearSelection.ActiveSlot = (TemplateSlotType)slot;
                        _gearSelection.SubSlotType = SubSlotType = (GearSubSlotType)subslot;

                        _gearSelection.OnClickAction = (obj) =>
                        {
                            if (obj is T item)
                            {
                                onClickAction(item);
                            }
                        };
                        break;

                    case SelectionTypes.Stats:
                        if (!Data.IsLoaded) return;

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
                        BuildSelection.OnClickAction = (obj) =>
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

        public void SelectFirstTemplate()
        {
            if (BuildSelection?.GetFirstTemplateSelectable() is TemplateSelectable selectable)
            {
                SetTemplateAnchor(selectable);
                return;
            }

            ResetAnchor();
        }

        public void SetTemplateAnchor(Control anchor)
        {
            SelectionType = SelectionTypes.Templates;
            SetAnchor(anchor);
        }

        public void ResetAnchor()
        {
            SelectionType = SelectionTypes.Templates;
            SetAnchor(BuildSelection.TemplateSelectables.FirstOrDefault(e => e.Template == TemplatePresenter.Template));
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            _backBounds = new(10, 5, Width - 32, 55);
            _backButton.Bounds = new(_backBounds.Left + 10, _backBounds.Top + 10, _backBounds.Height - 20, _backBounds.Height - 20);
            _backTextBounds = new(_backButton.Bounds.Right + 10, _backBounds.Top + 10, _backBounds.Width - (_backButton.Bounds.Right + 10), _backBounds.Height - 20);

            if (_gearSelection is not null) _gearSelection.Location = new(10, _backBounds.Bottom + 10);
            if (_statSelection is not null) _statSelection.Location = new(10, _backBounds.Bottom + 10);
            if (BuildSelection is not null) BuildSelection.Location = new(10, 10);
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintAfterChildren(spriteBatch, bounds);

            if (Anchor is not null && Anchor.Visible && Anchor.Parent is not null && Anchor.Parent.AbsoluteBounds.Contains(Anchor.AbsoluteBounds.Center))
            {
                if (SelectionType == SelectionTypes.Items)
                {
                    DrawGearSelection(spriteBatch, bounds);
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
            if (_backBounds.Contains(RelativeMousePosition))
            {
                spriteBatch.DrawOnCtrl(this, Textures.Pixel, _backBounds, Colors.ColonialWhite * 0.3F);
            }

            _backButton.Draw(this, spriteBatch, RelativeMousePosition, Color.White);
            spriteBatch.DrawStringOnCtrl(this, Title, Content.DefaultFont18, _backTextBounds, Color.White, false, HorizontalAlignment.Left, VerticalAlignment.Middle);
        }

        private void DrawStatSelection(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (_backBounds.Contains(RelativeMousePosition))
            {
                spriteBatch.DrawOnCtrl(this, Textures.Pixel, _backBounds, Colors.ColonialWhite * 0.3F);
            }

            _backButton.Draw(this, spriteBatch, RelativeMousePosition, Color.White);
            spriteBatch.DrawStringOnCtrl(this, Title, Content.DefaultFont18, _backTextBounds, Color.White, false, HorizontalAlignment.Left, VerticalAlignment.Middle);
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            Pointer?.Dispose();
            _backButton?.Dispose();
        }
    }
}
