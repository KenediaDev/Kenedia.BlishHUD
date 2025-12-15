using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Blish_HUD.Input;
using Microsoft.Xna.Framework.Input;
using Kenedia.Modules.Core.Extensions;
using Blish_HUD.Content;
using System.Linq;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Models;
using Control = Blish_HUD.Controls.Control;
using ContentService = Blish_HUD.ContentService;
using ICheckable = Blish_HUD.Controls.ICheckable;
using Microsoft.Xna.Framework.Graphics;
using Blish_HUD;
using Kenedia.Modules.Core.Res;

namespace Kenedia.Modules.Core.Controls
{
    public enum ExpandType
    {
        LeftToRight,
        RightToLeft,
        TopToBottom,
        BottomToTop
    }

    public enum SortType
    {
        ActivesFirst,
        ByModuleName,
    }

    public class Hotbar : Panel
    {
        private readonly DetailedTexture _expander = new(155909, 155910);
        private readonly Dummy _expandDummy;
        private readonly int _itemPadding = 4;

        private bool _resizeBarPending;
        private Point _dragStart;
        private bool _dragging;

        private Point _start;
        private Point _start_ItemWidth;
        private Point _delta;

        private Rectangle _expanderBackgroundBounds;
        protected readonly FlowPanel ItemsPanel;

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

            ItemsPanel = new()
            {
                Parent = this,
                FlowDirection = Blish_HUD.Controls.ControlFlowDirection.SingleLeftToRight,
                HeightSizingMode = Blish_HUD.Controls.SizingMode.AutoSize,
            };

            OnExpandTypeChanged(this, new(ExpandType.LeftToRight, ExpandType.LeftToRight));
            ExpandType = ExpandType.BottomToTop;

            BasicTooltipText = $"Press {MoveModifier} and drag the hotbar to the desired position";
            _expandDummy.BasicTooltipText = $"Press {MoveModifier} and drag the hotbar to the desired position";

            Menu = new();
            _ = Menu.AddMenuItem(new ContextMenuItem(() => strings_common.OpenSettings, () => OpenSettingsAction?.Invoke()));
        }

        public ExpandType ExpandType { get; set => Common.SetProperty(field, value, v => field = v, OnExpandTypeChanged); } = ExpandType.LeftToRight;

        public SortType SortType { get; set => Common.SetProperty(field, value, v => field = v, OnSortTypeCanged); } = SortType.ActivesFirst;

        public bool ExpandBar { get; set => Common.SetProperty(field, value, v => field = v, OnExpandChanged); }

        public ModifierKeys MoveModifier { get; set; } = ModifierKeys.Alt;

        public int MinButtonSize { get; set; } = 24;

        public Action<Point> OnMoveAction { get; set; }

        public Action OpenSettingsAction { get; set; }

        private void OnSortTypeCanged(object sender, Models.ValueChangedEventArgs<SortType> e)
        {
            if (ItemsPanel is null) return;

            SetButtonsExpanded();
            ForceOnScreen();
            RecalculateLayout();
        }

        private void OnExpandTypeChanged(object sender, Models.ValueChangedEventArgs<ExpandType> e)
        {
            if (ItemsPanel is null) return;
            _resizeBarPending = true;

            ItemsPanel.Size = Point.Zero;
            Size = Point.Zero;

            switch (e.NewValue)
            {
                case ExpandType.LeftToRight:
                    _expander.Texture = AsyncTexture2D.FromAssetId(155909);
                    _expander.HoveredTexture = AsyncTexture2D.FromAssetId(155910);
                    _expander.TextureRegion = new(new(0, 0), new(16, 32));
                    _expandDummy.Size = new(16, 32);
                    ItemsPanel.FlowDirection = Blish_HUD.Controls.ControlFlowDirection.SingleLeftToRight;
                    ItemsPanel.WidthSizingMode = Blish_HUD.Controls.SizingMode.Standard;
                    ItemsPanel.HeightSizingMode = Blish_HUD.Controls.SizingMode.AutoSize;
                    ItemsPanel.ContentPadding = new(5, 4, 0, 4);
                    ItemsPanel.ControlPadding = new(5);
                    ContentPadding = new(0);
                    break;

                case ExpandType.RightToLeft:
                    _expander.Texture = AsyncTexture2D.FromAssetId(155906);
                    _expander.HoveredTexture = AsyncTexture2D.FromAssetId(155907);
                    _expander.TextureRegion = new(new(16, 0), new(16, 32));
                    _expandDummy.Size = new(16, 32);
                    ItemsPanel.FlowDirection = Blish_HUD.Controls.ControlFlowDirection.SingleRightToLeft;
                    ItemsPanel.WidthSizingMode = Blish_HUD.Controls.SizingMode.Standard;
                    ItemsPanel.HeightSizingMode = Blish_HUD.Controls.SizingMode.AutoSize;
                    ItemsPanel.ContentPadding = new(-5, 4, 5, 4);
                    ItemsPanel.ControlPadding = new(5);
                    ContentPadding = new(0);
                    break;

                case ExpandType.TopToBottom:
                    _expander.Texture = AsyncTexture2D.FromAssetId(155929);
                    _expander.HoveredTexture = AsyncTexture2D.FromAssetId(155929);
                    _expander.TextureRegion = new(new(0, 8), new(32, 16));
                    _expandDummy.Size = new(32, 16);
                    ItemsPanel.WidthSizingMode = Blish_HUD.Controls.SizingMode.AutoSize;
                    ItemsPanel.HeightSizingMode = Blish_HUD.Controls.SizingMode.Standard;
                    ItemsPanel.FlowDirection = Blish_HUD.Controls.ControlFlowDirection.SingleTopToBottom;
                    ItemsPanel.ContentPadding = new(5, 4, 5, 4);
                    ItemsPanel.ControlPadding = new(5);
                    ContentPadding = new(0, 2);
                    break;

                case ExpandType.BottomToTop:
                    _expander.Texture = AsyncTexture2D.FromAssetId(155929);
                    _expander.HoveredTexture = AsyncTexture2D.FromAssetId(155929);
                    _expander.TextureRegion = new(new(0, 8), new(32, 16));
                    _expandDummy.Size = new(32, 16);
                    ItemsPanel.WidthSizingMode = Blish_HUD.Controls.SizingMode.AutoSize;
                    ItemsPanel.HeightSizingMode = Blish_HUD.Controls.SizingMode.Standard;
                    ItemsPanel.FlowDirection = Blish_HUD.Controls.ControlFlowDirection.SingleBottomToTop;
                    ItemsPanel.ContentPadding = new(5, 0, 5, 2);
                    ItemsPanel.ControlPadding = new(5);
                    ContentPadding = new(0, 2);
                    break;
            }

            ForceOnScreen();
            RecalculateLayout();
        }

        private void OnExpandChanged(object sender, Models.ValueChangedEventArgs<bool> e)
        {
            bool childDisplayed = e.NewValue || ItemsPanel.Children.FirstOrDefault(e => e.Visible) is not null;
            _resizeBarPending = true;

            SetButtonsExpanded();
            RecalculateLayout();
        }

        public virtual void SetButtonsExpanded()
        {
            foreach (var c in ItemsPanel.Children.OfType<ICheckable>())
            {
                (c as Control).Visible = ExpandBar || c.Checked;
            }
        }

        public void AddItem(ICheckable item)
        {
            if (item is Control control)
            {
                control.Parent = ItemsPanel;
                item.CheckedChanged += Item_CheckedChanged;
            }

            RecalculateLayout();
            SetButtonsExpanded();
        }

        private void Item_CheckedChanged(object sender, Blish_HUD.Controls.CheckChangedEvent e)
        {
            if (sender is Control control)
            {
                control.Visible = ExpandBar || e.Checked;
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

        protected virtual void SortButtons()
        {
            switch (SortType)
            {
                case SortType.ActivesFirst:
                    ItemsPanel.SortChildren<HotbarButton>((a, b) => b.Checked.CompareTo(a.Checked));
                    break;

                case SortType.ByModuleName:
                    ItemsPanel.SortChildren<HotbarButton>((a, b) => a.BasicTooltipText.CompareTo(b.BasicTooltipText));
                    break;
            }
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();
            if (ItemsPanel is null)
                return;

            if (BackgroundImage is not null)
                TextureRectangle = new(50, 50, Math.Min(BackgroundImage.Bounds.Size.X, Width), Math.Min(Height, BackgroundImage.Bounds.Size.Y));

            SortButtons();

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
                ItemsPanel.Children.OfType<ICheckable>().Where(e => any || e.Checked).Cast<Control>() :
                ItemsPanel.Children.Where(e => any || e.Visible);

            return (int)visibleItems.Sum(e => (vertical ? e.Height : e.Width) + (vertical ? ItemsPanel.ControlPadding.Y : ItemsPanel.ControlPadding.X));
        }

        private void CalculateLeftToRight()
        {
            if (BackgroundImage is not null)
                TextureRectangle = new(50, 50, Math.Min(BackgroundImage.Bounds.Size.X, Width), Math.Min(Height, BackgroundImage.Bounds.Size.Y));

            if (ItemsPanel is not null)
            {
                var visibleItems = ItemsPanel.Children.Where(e => e.Visible);
                ItemsPanel.Size = new((int)visibleItems.Sum(e => e.Width + ItemsPanel.ControlPadding.X) + (visibleItems?.Count() > 0 ? ItemsPanel.ContentPadding.Horizontal : 0), Height - AutoSizePadding.Y);
                ItemsPanel.Location = new(0, 0);
            }

            if (_expandDummy is not null)
            {
                _expandDummy.Location = new(Math.Max(ItemsPanel?.Right ?? 0, 5), ((ItemsPanel?.Height ?? Height) - _expandDummy.Height) / 2);
                _expanderBackgroundBounds = new(_expandDummy.Left - 2, BorderWidth.Top, _expandDummy.Width + 2, Height - BorderWidth.Vertical);
            }
        }

        private void CalculateRightToLeft()
        {
            bool isAnyVisible = ItemsPanel.Children.Any(e => e.Visible);
            int expandedItemsWidth = GetItemPanelSize(true, false);
            int checkedItemsWidth = GetItemPanelSize(false, true);
            int padding = isAnyVisible ? ItemsPanel.ContentPadding.Horizontal : 0;

            if (_resizeBarPending)
            {
                // Move bar to the left and expand
                if (ExpandBar)
                {
                    _start = Location;
                    _start_ItemWidth = new(checkedItemsWidth, 0);

                    Location = _start.Add(new(-(expandedItemsWidth - checkedItemsWidth), 0));
                    ItemsPanel.Width = expandedItemsWidth + padding;
                }
                // Move bar to the right                
                else
                {
                    _delta = new(_start_ItemWidth.X - checkedItemsWidth, 0);
                    Location = _start.Add(_delta);

                    ItemsPanel.Width = isAnyVisible ? checkedItemsWidth + padding : 0;
                }

                _resizeBarPending = false;
            }

            _expandDummy.Location = new(0, ((ItemsPanel?.Height ?? Height) - _expandDummy.Height) / 2);
            _expanderBackgroundBounds = new(BorderWidth.Left, BorderWidth.Top, _expandDummy.Width + 2, Height - BorderWidth.Vertical);
            ItemsPanel.Location = new(_expandDummy.Right + BorderWidth.Horizontal, 0);
        }

        private void CalculateTopToBottom()
        {
            int buttonSize = Math.Max(MinButtonSize, Math.Min(Width, Height) - _itemPadding - 10);

            if (BackgroundImage is not null)
                TextureRectangle = new(50, 50, Math.Min(BackgroundImage.Bounds.Size.X, Width), Math.Min(Height, BackgroundImage.Bounds.Size.Y));

            if (ItemsPanel is not null)
            {
                var visibleItems = ItemsPanel.Children.Where(e => e.Visible);
                ItemsPanel.Size = new(Width - AutoSizePadding.X, (int)visibleItems.Sum(e => e.Width + ItemsPanel.ControlPadding.X) + (visibleItems?.Count() > 0 ? ItemsPanel.ContentPadding.Horizontal : 0));
                ItemsPanel.Location = new(0, 0);
            }

            if (_expandDummy is not null)
            {
                _expandDummy.Location = new(((ItemsPanel?.Width ?? Width) - _expandDummy.Width) / 2, Math.Max(ItemsPanel?.Bottom ?? 0, 5) - 5);
                _expanderBackgroundBounds = new(BorderWidth.Left, Height - _expandDummy.Height - BorderWidth.Bottom, Width - BorderWidth.Horizontal, _expandDummy.Height);
            }
        }

        private void CalculateBottomToTop()
        {
            bool isAnyVisible = ItemsPanel.Children.Any(e => e.Visible);
            int expandedItemsWidth = GetItemPanelSize(true, false, true);
            int checkedItemsWidth = GetItemPanelSize(false, true, true);
            int padding = isAnyVisible ? ItemsPanel.ContentPadding.Vertical : 0;

            if (_resizeBarPending)
            {
                // Move bar to the left and expand
                if (ExpandBar)
                {
                    _start = Location;
                    _start_ItemWidth = new(checkedItemsWidth, 0);

                    Location = _start.Add(new(0, -(expandedItemsWidth - checkedItemsWidth)));
                    ItemsPanel.Height = expandedItemsWidth + padding;
                }
                // Move bar to the right                
                else
                {
                    _delta = new(0, _start_ItemWidth.X - checkedItemsWidth);
                    Location = _start.Add(_delta);

                    ItemsPanel.Height = isAnyVisible ? checkedItemsWidth + padding : 0;
                }

                _resizeBarPending = false;
            }

            _expandDummy.Location = new((Width - AutoSizePadding.X - _expandDummy.Width) / 2, 0);
            _expanderBackgroundBounds = new(BorderWidth.Left, BorderWidth.Top, Width - BorderWidth.Horizontal, _expandDummy.Height);
            ItemsPanel.Location = new(0, _expandDummy.Bottom);
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

            if (Location.X + Width > screen.Right)
                Location = new(screen.Right - Width, Location.Y);

            if (Location.Y < screen.Top)
                Location = new(Location.X, screen.Top);

            if (Location.Y + Height > screen.Bottom)
                Location = new(Location.X, screen.Bottom - Height);
        }
    }
}
