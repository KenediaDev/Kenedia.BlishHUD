using Container = Blish_HUD.Controls.Container;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Blish_HUD.Input;
using Microsoft.Xna.Framework.Input;
using Kenedia.Modules.Core.Extensions;
using System.Diagnostics;
using Blish_HUD.Content;
using System.Linq;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Models;
using Control = Blish_HUD.Controls.Control;
using ContentService = Blish_HUD.ContentService;
using ICheckable = Blish_HUD.Controls.ICheckable;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics.Eventing.Reader;
using Blish_HUD;

namespace Kenedia.Modules.Core.Controls
{
    public enum ExpandType
    {
        LeftToRight,
        RightToLeft,
        TopToBottom,
        BottomToTop
    }

    public class Hotbar : Panel
    {
        private readonly DetailedTexture _expander = new(155909, 155910);
        private readonly Dummy _expandDummy;
        private readonly FlowPanel _itemsPanel;
        private readonly int _itemPadding = 4;

        private bool _resizeBarPending;
        private bool _expandBar;
        private Point _dragStart;
        private bool _dragging;

        private Point _start;
        private Point _start_ItemWidth;
        private Point _delta;

        private Rectangle _expanderBackgroundBounds;

        private ExpandType _expandType = ExpandType.LeftToRight;

        //517181 | 517182 Arrow up
        public Hotbar()
        {
            WidthSizingMode = Blish_HUD.Controls.SizingMode.AutoSize;
            HeightSizingMode = Blish_HUD.Controls.SizingMode.AutoSize;

            BorderColor = Color.Black;
            BorderWidth = new(2);
            BackgroundImage = AsyncTexture2D.FromAssetId(155960);
            BackgroundImageColor = Color.DarkGray * 0.8f;

            _expandDummy = new()
            {
                Parent = this,
                Size = new(16, 32),
            };

            _itemsPanel = new()
            {
                Parent = this,
                FlowDirection = Blish_HUD.Controls.ControlFlowDirection.SingleLeftToRight,
                HeightSizingMode = Blish_HUD.Controls.SizingMode.AutoSize,
            };

            OnExpandTypeChanged(this, new(ExpandType.LeftToRight, ExpandType.LeftToRight));
            ExpandType = ExpandType.BottomToTop;
        }

        public ExpandType ExpandType { get => _expandType; set => Common.SetProperty(ref _expandType, value, OnExpandTypeChanged); }

        public bool ExpandBar { get => _expandBar; set => Common.SetProperty(ref _expandBar, value, OnExpandChanged); }

        public ModifierKeys MoveModifier { get; set; } = ModifierKeys.Alt;

        public int MinButtonSize { get; set; } = 24;

        public Action<Point> OnMoveAction { get; set; }

        private void OnExpandTypeChanged(object sender, Models.ValueChangedEventArgs<ExpandType> e)
        {
            if (_itemsPanel is null) return;
            _resizeBarPending = true;

            _itemsPanel.Size = Point.Zero;
            Size = Point.Zero;

            switch (e.NewValue)
            {
                case ExpandType.LeftToRight:
                    _expander.Texture = AsyncTexture2D.FromAssetId(155909);
                    _expander.HoveredTexture = AsyncTexture2D.FromAssetId(155910);
                    _expander.TextureRegion = new(new(0, 0), new(16, 32));
                    _expandDummy.Size = new(16, 32);
                    _itemsPanel.FlowDirection = Blish_HUD.Controls.ControlFlowDirection.SingleLeftToRight;
                    _itemsPanel.WidthSizingMode = Blish_HUD.Controls.SizingMode.Standard;
                    _itemsPanel.HeightSizingMode = Blish_HUD.Controls.SizingMode.AutoSize;
                    _itemsPanel.ContentPadding = new(5, 4, 0, 4);
                    _itemsPanel.ControlPadding = new(5);
                    ContentPadding = new(0);
                    break;

                case ExpandType.RightToLeft:
                    _expander.Texture = AsyncTexture2D.FromAssetId(155906);
                    _expander.HoveredTexture = AsyncTexture2D.FromAssetId(155907);
                    _expander.TextureRegion = new(new(16, 0), new(16, 32));
                    _expandDummy.Size = new(16, 32);
                    _itemsPanel.FlowDirection = Blish_HUD.Controls.ControlFlowDirection.SingleRightToLeft;
                    _itemsPanel.WidthSizingMode = Blish_HUD.Controls.SizingMode.Standard;
                    _itemsPanel.HeightSizingMode = Blish_HUD.Controls.SizingMode.AutoSize;
                    _itemsPanel.ContentPadding = new(-5, 4, 5, 4);
                    _itemsPanel.ControlPadding = new(5);
                    ContentPadding = new(0);
                    break;

                case ExpandType.TopToBottom:
                    _expander.Texture = AsyncTexture2D.FromAssetId(155929);
                    _expander.HoveredTexture = AsyncTexture2D.FromAssetId(155929);
                    _expander.TextureRegion = new(new(0, 8), new(32, 16));
                    _expandDummy.Size = new(32, 16);
                    _itemsPanel.WidthSizingMode = Blish_HUD.Controls.SizingMode.AutoSize;
                    _itemsPanel.HeightSizingMode = Blish_HUD.Controls.SizingMode.Standard;
                    _itemsPanel.FlowDirection = Blish_HUD.Controls.ControlFlowDirection.SingleTopToBottom;
                    _itemsPanel.ContentPadding = new(5, 4, 5, 4);
                    _itemsPanel.ControlPadding = new(5);
                    ContentPadding = new(0, 2);
                    break;

                case ExpandType.BottomToTop:
                    _expander.Texture = AsyncTexture2D.FromAssetId(155929);
                    _expander.HoveredTexture = AsyncTexture2D.FromAssetId(155929);
                    _expander.TextureRegion = new(new(0, 8), new(32, 16));
                    _expandDummy.Size = new(32, 16);
                    _itemsPanel.WidthSizingMode = Blish_HUD.Controls.SizingMode.AutoSize;
                    _itemsPanel.HeightSizingMode = Blish_HUD.Controls.SizingMode.Standard;
                    _itemsPanel.FlowDirection = Blish_HUD.Controls.ControlFlowDirection.SingleBottomToTop;
                    _itemsPanel.ContentPadding = new(5, 0, 5, 2);
                    _itemsPanel.ControlPadding = new(5);
                    ContentPadding = new(0, 2);
                    break;
            }

            RecalculateLayout();
        }

        private void OnExpandChanged(object sender, Models.ValueChangedEventArgs<bool> e)
        {
            bool childDisplayed = e.NewValue || _itemsPanel.Children.FirstOrDefault(e => e.Visible) is not null;
            _resizeBarPending = true;

            foreach (var c in _itemsPanel.Children.OfType<ICheckable>())
            {
                (c as Control).Visible = e.NewValue || c.Checked;
            }

            _itemsPanel.SortChildren<ImageToggle>((a, b) => b.Visible.CompareTo(a.Visible));

            RecalculateLayout();
        }

        public void AddItem(ICheckable item)
        {
            if (item is Control control)
            {
                control.Parent = _itemsPanel;
                item.CheckedChanged += Item_CheckedChanged;
            }

            RecalculateLayout();
        }

        private void Item_CheckedChanged(object sender, Blish_HUD.Controls.CheckChangedEvent e)
        {
            if (sender is Control control)
            {
                control.Visible = e.Checked;
            }

            RecalculateLayout();
        }

        public void RemoveItem(ICheckable item)
        {
            if (item is Control control)
            {
                item.CheckedChanged -= Item_CheckedChanged;
                control.Dispose();
            }
        }

        protected override void OnLeftMouseButtonPressed(MouseEventArgs e)
        {
            base.OnLeftMouseButtonPressed(e);

            _dragging = Input.Keyboard.ActiveModifiers == MoveModifier;
            _dragStart = _dragging ? RelativeMousePosition : Point.Zero;
        }

        protected override void OnLeftMouseButtonReleased(MouseEventArgs e)
        {
            base.OnLeftMouseButtonReleased(e);

            _dragging = false;
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

            ExpandBar = MouseOver || _expandDummy.MouseOver;

            _dragging = _dragging && MouseOver && Input.Keyboard.ActiveModifiers == MoveModifier;

            if (_dragging)
            {
                MoveBar();
            }
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            if (_itemsPanel is null)
                return;

            if (BackgroundImage is not null)
                TextureRectangle = new(50, 50, Math.Min(BackgroundImage.Bounds.Size.X, Width), Math.Min(Height, BackgroundImage.Bounds.Size.Y));

            switch (ExpandType)
            {
                case ExpandType.LeftToRight:
                    _expander.Bounds = _expandDummy.LocalBounds;
                    CalculateLeftToRight();
                    break;
                case ExpandType.RightToLeft:
                    _expander.Bounds = _expandDummy.LocalBounds;
                    CalculateRightToLeft();
                    break;
                case ExpandType.TopToBottom:
                    _expander.Bounds = _expandDummy.LocalBounds;
                    CalculateTopToBottom();
                    break;
                case ExpandType.BottomToTop:
                    _expander.Bounds = new(_expandDummy.Location.Add(new(0, _expandDummy.Height - 5)), _expandDummy.Size);
                    CalculateBottomToTop();
                    break;
            }
        }

        public int GetItemPanelSize(bool any = false, bool isChecked = false, bool vertical = false)
        {
            IEnumerable<Control> visibleItems = isChecked ?
                _itemsPanel.Children.OfType<ICheckable>().Where(e => any || e.Checked).Cast<Control>() :
                _itemsPanel.Children.Where(e => any || e.Visible);

            return (int)visibleItems.Sum(e => (vertical ? e.Height : e.Width) + (vertical ? _itemsPanel.ControlPadding.Y : _itemsPanel.ControlPadding.X));
        }

        private void CalculateLeftToRight()
        {
            if (BackgroundImage is not null)
                TextureRectangle = new(50, 50, Math.Min(BackgroundImage.Bounds.Size.X, Width), Math.Min(Height, BackgroundImage.Bounds.Size.Y));

            if (_itemsPanel is not null)
            {
                var visibleItems = _itemsPanel.Children.Where(e => e.Visible);
                _itemsPanel.Size = new((int)visibleItems.Sum(e => e.Width + _itemsPanel.ControlPadding.X) + (visibleItems?.Count() > 0 ? _itemsPanel.ContentPadding.Horizontal : 0), Height - AutoSizePadding.Y);
                _itemsPanel.Location = new(0, 0);
            }

            if (_expandDummy is not null)
            {
                _expandDummy.Location = new(Math.Max(_itemsPanel?.Right ?? 0, 5), ((_itemsPanel?.Height ?? Height) - _expandDummy.Height) / 2);
                _expanderBackgroundBounds = new(_expandDummy.Left - 2, BorderWidth.Top, _expandDummy.Width + 2, Height - BorderWidth.Vertical);
            }
        }

        private void CalculateRightToLeft()
        {
            bool isAnyVisible = _itemsPanel.Children.Any(e => e.Visible);
            int expandedItemsWidth = GetItemPanelSize(true, false);
            int checkedItemsWidth = GetItemPanelSize(false, true);
            int padding = isAnyVisible ? _itemsPanel.ContentPadding.Horizontal : 0;

            if (_resizeBarPending)
            {
                // Move bar to the left and expand
                if (ExpandBar)
                {
                    _start = Location;
                    _start_ItemWidth = new(checkedItemsWidth, 0);

                    Location = _start.Add(new(-(expandedItemsWidth - checkedItemsWidth), 0));
                    _itemsPanel.Width = expandedItemsWidth + padding;
                }
                // Move bar to the right                
                else
                {
                    _delta = new(_start_ItemWidth.X - checkedItemsWidth, 0);
                    Location = _start.Add(_delta);

                    _itemsPanel.Width = isAnyVisible ? checkedItemsWidth + padding : 0;
                }

                _resizeBarPending = false;
            }

            _expandDummy.Location = new(0, ((_itemsPanel?.Height ?? Height) - _expandDummy.Height) / 2);
            _expanderBackgroundBounds = new(BorderWidth.Left, BorderWidth.Top, _expandDummy.Width + 2, Height - BorderWidth.Vertical);
            _itemsPanel.Location = new(_expandDummy.Right + BorderWidth.Horizontal, 0);
        }

        private void CalculateTopToBottom()
        {
            int buttonSize = Math.Max(MinButtonSize, Math.Min(Width, Height) - _itemPadding - 10);

            if (BackgroundImage is not null)
                TextureRectangle = new(50, 50, Math.Min(BackgroundImage.Bounds.Size.X, Width), Math.Min(Height, BackgroundImage.Bounds.Size.Y));

            if (_itemsPanel is not null)
            {
                var visibleItems = _itemsPanel.Children.Where(e => e.Visible);
                _itemsPanel.Size = new(Width - AutoSizePadding.X, (int)visibleItems.Sum(e => e.Width + _itemsPanel.ControlPadding.X) + (visibleItems?.Count() > 0 ? _itemsPanel.ContentPadding.Horizontal : 0));
                _itemsPanel.Location = new(0, 0);
            }

            if (_expandDummy is not null)
            {
                _expandDummy.Location = new(((_itemsPanel?.Width ?? Width) - _expandDummy.Width) / 2, Math.Max(_itemsPanel?.Bottom ?? 0, 5) - 5);
                _expanderBackgroundBounds = new(BorderWidth.Left, Height - _expandDummy.Height - BorderWidth.Bottom, Width - BorderWidth.Horizontal, _expandDummy.Height);
            }
        }

        private void CalculateBottomToTop()
        {
            bool isAnyVisible = _itemsPanel.Children.Any(e => e.Visible);
            int expandedItemsWidth = GetItemPanelSize(true, false, true);
            int checkedItemsWidth = GetItemPanelSize(false, true, true);
            int padding = isAnyVisible ? _itemsPanel.ContentPadding.Vertical : 0;

            if (_resizeBarPending)
            {
                // Move bar to the left and expand
                if (ExpandBar)
                {
                    _start = Location;
                    _start_ItemWidth = new(checkedItemsWidth, 0);

                    Location = _start.Add(new(0, -(expandedItemsWidth - checkedItemsWidth)));
                    _itemsPanel.Height = expandedItemsWidth + padding;
                }
                // Move bar to the right                
                else
                {
                    _delta = new(0, _start_ItemWidth.X - checkedItemsWidth);
                    Location = _start.Add(_delta);

                    _itemsPanel.Height = isAnyVisible ? checkedItemsWidth + padding : 0;
                }

                _resizeBarPending = false;
            }

            _expandDummy.Location = new((Width - AutoSizePadding.X - _expandDummy.Width) / 2, 0);
            _expanderBackgroundBounds = new(BorderWidth.Left, BorderWidth.Top, Width - BorderWidth.Horizontal, _expandDummy.Height);
            _itemsPanel.Location = new(0, _expandDummy.Bottom);
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintAfterChildren(spriteBatch, bounds);

            ClipsBounds = false;

            if (ExpandType == ExpandType.BottomToTop)
            {
                spriteBatch.Draw(ContentService.Textures.Pixel, _expanderBackgroundBounds.Add(new(Location, Point.Zero)), Color.Black * 0.5F);
                spriteBatch.DrawCenteredRotationOnCtrl(this, _expander.Texture, _expander.Bounds, _expander.TextureRegion, Color.White, 0F, true, false);
            }
            else
            {
                spriteBatch.Draw(ContentService.Textures.Pixel, _expanderBackgroundBounds.Add(new(Location, Point.Zero)), Color.Black * 0.5F);
                _expander?.Draw(this, spriteBatch, RelativeMousePosition);
            }

        }

        private void MoveBar()
        {
            RecalculateLayout();

            switch (ExpandType)
            {
                case ExpandType.LeftToRight:
                    Location = Input.Mouse.Position.Add(new Point(-_dragStart.X, -_dragStart.Y));
                    break;
                case ExpandType.RightToLeft:
                    int expandedItemsWidth = GetItemPanelSize(true, false);
                    int checkedItemsWidth = GetItemPanelSize(false, true);
                    Location = Input.Mouse.Position.Add(new Point(-_dragStart.X, -_dragStart.Y));
                    _start = Location.Add(new(expandedItemsWidth - checkedItemsWidth, 0));
                    break;
                case ExpandType.TopToBottom:
                    Location = Input.Mouse.Position.Add(new Point(-_dragStart.X, -_dragStart.Y));
                    break;
                case ExpandType.BottomToTop:
                    int expandedItemsWidth2 = GetItemPanelSize(true, false, true);
                    int checkedItemsWidth2 = GetItemPanelSize(false, true, true);
                    Location = Input.Mouse.Position.Add(new Point(-_dragStart.X, -_dragStart.Y));
                    _start = Location.Add(new(0, expandedItemsWidth2 - checkedItemsWidth2));
                    break;
            }

            ForceOnScreen();
            OnMoveAction?.Invoke(Location);
        }

        private void ForceOnScreen()
        {
            var screen = Graphics.SpriteScreen.LocalBounds;

            if (Location.X < screen.Left)
                Location = new(screen.Left, Location.Y);
            else if (Location.X + Width > screen.Right)
                Location = new(screen.Right - Width, Location.Y);
            else if (Location.Y < screen.Top)
                Location = new(Location.X, screen.Top);
            else if (Location.Y + Height > screen.Bottom)
                Location = new(Location.X, screen.Bottom - Height);
        }
    }
}
