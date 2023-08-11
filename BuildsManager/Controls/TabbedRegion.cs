﻿using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Structs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace Kenedia.Modules.BuildsManager.Controls
{
    public class TabbedRegionTab
    {
        private readonly DetailedTexture _inactiveHeader = new(2200567);
        private readonly DetailedTexture _activeHeader = new(2200566);
        private AsyncTexture2D _icon;
        private RectangleDimensions _padding = new(5, 2);
        private Rectangle _bounds;
        private Rectangle _iconBounds;
        private Rectangle _textBounds;
        private string _header;
        private BitmapFont _font = GameService.Content.DefaultFont18;

        public TabbedRegionTab(Container container)
        {
            Container = container;
        }

        public Container Container { get; set; }

        public bool IsActive { get; set; }

        public string Header
        {
            get { return _header; }
            set { _header = value; }
        }

        public Rectangle Bounds
        {
            get => _bounds;
            set
            {
                _bounds = value;
                _inactiveHeader.Bounds = value;
                _activeHeader.Bounds = value;
                RecalculateLayout();
            }
        }

        public bool IsHovered(Point p)
        {
            return Bounds.Contains(p);
        }

        public AsyncTexture2D Icon { get => _icon; set => _icon = value; }
        public BitmapFont Font { get => _font; set => _font = value; }

        public void DrawHeader(Control ctrl, SpriteBatch spriteBatch, Point mousePos)
        {
            Color color = IsActive ? Color.White : Color.White * (IsHovered(mousePos) ? 0.9F : 0.6F);
            (!IsActive ? _activeHeader : _inactiveHeader).Draw(ctrl, spriteBatch, mousePos, color);

            if (!string.IsNullOrEmpty(_header))
            {
                spriteBatch.DrawStringOnCtrl(ctrl, _header, _font, _textBounds, Color.White);
            }

            if (Icon != null)
            {
                spriteBatch.DrawOnCtrl(ctrl, Icon, _iconBounds, Color.White);
            }
        }

        private void RecalculateLayout()
        {
            _iconBounds = new(Bounds.Left + _padding.Left, Bounds.Top + _padding.Top, Bounds.Height - _padding.Vertical, Bounds.Height - _padding.Vertical);
            _textBounds = new(_iconBounds.Right + 10, _padding.Top, Bounds.Width - _iconBounds.Width - 10 - _padding.Right, Bounds.Height - _padding.Vertical);
        }
    }

    public class TabbedRegion : Container
    {
        private readonly AsyncTexture2D _inactiveHeader = AsyncTexture2D.FromAssetId(2200566);
        private readonly AsyncTexture2D _activeHeader = AsyncTexture2D.FromAssetId(2200567);
        private readonly AsyncTexture2D _separator = AsyncTexture2D.FromAssetId(156055);
        private readonly Panel _contentPanel;
        private TabbedRegionTab _activeTab;
        private ObservableCollection<TabbedRegionTab> _tabs = new();

        private Rectangle _contentRegion;
        private Rectangle _headerRegion;
        private RectangleDimensions _headerPading = new(0, 8);
        private RectangleDimensions _contentPadding = new(0, 4);
        private BitmapFont _headerFont = Content.DefaultFont18;

        public TabbedRegion()
        {
            _tabs.CollectionChanged += Tab_CollectionChanged;
            _contentPanel = new()
            {
                Parent = this,
            };
        }

        private void Tab_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (_activeTab == null)
            {
                SwitchTab(_tabs.FirstOrDefault());
            }

            RecalculateLayout();
        }

        public TabbedRegionTab ActiveTab
        {
            get => _activeTab;
            set => SwitchTab(value);
        }

        public List<Container> Tabs => _tabs.ToList().Select(e => e.Container).ToList();

        public BitmapFont HeaderFont
        {
            get => _headerFont; set
            {
                _headerFont = value ?? Content.DefaultFont18;
                foreach (var tab in _tabs)
                {
                    tab.Font = _headerFont;
                }
            }
        }

        public RectangleDimensions ContentPadding { get => _contentPadding; set => _contentPadding = value; }

        public Action OnTabSwitched { get; set; }

        public void AddTab(TabbedRegionTab tab)
        {
            tab.Container.Visible = false;
            tab.Container.Parent = _contentPanel;

            _tabs.Add(tab);
        }

        public void AddTab(Container tab)
        {

            _tabs.Add(new(tab));
        }

        public void RemoveTab(Container tab)
        {
            _ = _tabs.ToList().RemoveAll(e => e.Container == tab);
        }

        public void SwitchTab(TabbedRegionTab tab)
        {

            Debug.WriteLine($"SwitchTab!");
            OnTabSwitched?.Invoke();

            if (tab != null)
            {
                tab.Container.Visible = true;
                _activeTab = tab;
                _activeTab.Container?.Invalidate();
            }

            foreach (var item in _tabs)
            {
                item.IsActive = item == _activeTab;
                item.Container.Visible = item.IsActive;
            }

            RecalculateLayout();
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            _headerRegion = new Rectangle(0, 0, Width, HeaderFont.LineHeight + _headerPading.Vertical);
            _contentRegion = new Rectangle(0 + _contentPadding.Left, _headerRegion.Bottom + _contentPadding.Top, Width - +_contentPadding.Horizontal, Height - _headerRegion.Bottom - _contentPadding.Vertical);

            if (_contentPanel != null)
            {
                _contentPanel.Location = _contentRegion.Location;
                _contentPanel.Size = _contentRegion.Size;
            }

            int tabHeaderWidth = Width / Math.Max(1, _tabs.Count);

            for (int i = 0; i < _tabs.Count; i++)
            {
                _tabs[i].Bounds = new(_headerRegion.Left + (i * tabHeaderWidth), _headerRegion.Top, tabHeaderWidth, _headerRegion.Height);
            }
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);
            for (int i = 0; i < _tabs.Count; i++)
            {
                _tabs[i].DrawHeader(this, spriteBatch, RelativeMousePosition);
            }
            spriteBatch.DrawOnCtrl(this, _separator, new Rectangle(_headerRegion.Left, _headerRegion.Bottom - 9, _headerRegion.Width, 16), Color.Black);
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            for (int i = 0; i < _tabs.Count; i++)
            {
                if (_tabs[i].IsHovered(RelativeMousePosition))
                {
                    SwitchTab(_tabs[i]);
                    break;
                }
            }
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            _tabs.CollectionChanged -= Tab_CollectionChanged;
        }
    }
}
