﻿using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kenedia.Modules.Core.Views
{
    [Obsolete]
    public class BaseTabbedWindow : WindowBase
    {        
        private readonly AsyncTexture2D _rawbackground = AsyncTexture2D.FromAssetId(155997);
        private readonly AsyncTexture2D _textureSplitLine = AsyncTexture2D.FromAssetId(605024);
        private readonly Texture2D _textureBlackFade = Content.GetTexture("fade-down-46");
        private readonly Texture2D _textureTabActive = Content.GetTexture("window-tab-active");

        private readonly int _tabHeight = 52;
        private readonly int _tabWidth = 104;
        private readonly int _tabIconSize = 32;

        private readonly int _tabSectionWidth = 46;

        private readonly int _windowContentWidth = 500;
        private readonly int _windowContentHeight = 640;

        private readonly Rectangle _standardTabBounds;

        private readonly Dictionary<BaseTab, Rectangle> _tabRegions = [];

        private Rectangle _layoutTopTabBarBounds;
        private Rectangle _layoutBottomTabBarBounds;

        private Rectangle _layoutTopSplitLineBounds;
        private Rectangle _layoutBottomSplitLineBounds;

        private Rectangle _layoutTopSplitLineSourceBounds;
        private Rectangle _layoutBottomSplitLineSourceBounds;

        private List<BaseTab> _tabs = [];
        private Texture2D _background;
        private Texture2D _tabBarBackground;

        private int _selectedTabIndex = -1;
        private int _hoveredTabIndex = 0;

        public BaseTabbedWindow()
        {
            _standardTabBounds = new Rectangle(_tabSectionWidth, 24, _tabWidth, _tabHeight);

            _background = _rawbackground.Texture.GetRegion(0, 0, _rawbackground.Width, _rawbackground.Height);
            _tabBarBackground = _background.Duplicate().SetRegion(0, 0, 64, _background.Height, Color.Transparent);

            ConstructWindow(null, new Vector2(0), new(0, 0, _windowContentWidth + 64, _windowContentHeight + 30), new Thickness(30, 75, 45, 25), 40);
            _contentRegion = new Rectangle(_tabWidth / 2, 48, _windowContentWidth, _windowContentHeight);

            _rawbackground.TextureSwapped += Background_TextureSwapped;
        }

        private void Background_TextureSwapped(object sender, ValueChangedEventArgs<Texture2D> e)
        {
            ApplyBackground();
        }

        private void ApplyBackground()
        {
            _background = _rawbackground.Texture.GetRegion(0, 0, _rawbackground.Width, _rawbackground.Height);
            _tabBarBackground = _background.Duplicate().SetRegion(0, 0, 64, _background.Height, Color.Transparent);
        }

        public BaseTab SelectedTab => _tabs.Count > _selectedTabIndex ? _tabs[_selectedTabIndex] : null;

        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set
            {
                if (SetProperty(ref _selectedTabIndex, value, true))
                {
                    OnTabChanged();
                }
            }
        }

        private int HoveredTabIndex
        {
            get => _hoveredTabIndex;
            set => SetProperty(ref _hoveredTabIndex, value);
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            if (_tabs.Count == 0) return;

            Rectangle firstTabBounds = TabBoundsFromIndex(0);
            Rectangle selectedTabBounds = _tabRegions[SelectedTab];
            Rectangle lastTabBounds = TabBoundsFromIndex(_tabRegions.Count - 1);

            _layoutTopTabBarBounds = new Rectangle(0, 0, _tabSectionWidth, firstTabBounds.Top);
            _layoutBottomTabBarBounds = new Rectangle(0, lastTabBounds.Bottom, _tabSectionWidth, _size.Y - lastTabBounds.Bottom);

            int topSplitHeight = selectedTabBounds.Top - ContentRegion.Top;
            int bottomSplitHeight = ContentRegion.Bottom - selectedTabBounds.Bottom;

            _layoutTopSplitLineBounds = new Rectangle(ContentRegion.X - _textureSplitLine.Width + 1,
                                                      ContentRegion.Y,
                                                      _textureSplitLine.Width,
                                                      topSplitHeight);

            _layoutTopSplitLineSourceBounds = new Rectangle(0, 0, _textureSplitLine.Width, topSplitHeight);

            _layoutBottomSplitLineBounds = new Rectangle(ContentRegion.X - _textureSplitLine.Width + 1,
                                                         selectedTabBounds.Bottom,
                                                         _textureSplitLine.Width,
                                                         bottomSplitHeight);

            _layoutBottomSplitLineSourceBounds = new Rectangle(0, _textureSplitLine.Height - bottomSplitHeight, _textureSplitLine.Width, bottomSplitHeight);
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            // Draw black block for tab bar

            // Draw top of split
            spriteBatch.DrawOnCtrl(this, _tabBarBackground,
                                   _tabBarBackground.Bounds.Add(-20, 5, 0, 0),
                                    _tabBarBackground.Bounds);

            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel,
                                   _layoutTopTabBarBounds,
                                   Color.Black);

            base.PaintBeforeChildren(spriteBatch, bounds);

            // Draw black fade for tab bar
            spriteBatch.DrawOnCtrl(this, _textureBlackFade, _layoutBottomTabBarBounds);

            // Draw tabs
            int i = 0;
            foreach (BaseTab tab in _tabs)
            {
                bool active = i == SelectedTabIndex;
                bool hovered = i == HoveredTabIndex;

                Rectangle tabBounds = _tabRegions[tab];
                var subBounds = new Rectangle(tabBounds.X + (tabBounds.Width / 2), tabBounds.Y, _tabWidth / 2, tabBounds.Height);

                if (active)
                {
                    spriteBatch.DrawOnCtrl(this, _background,
                                           tabBounds,
                                           tabBounds.OffsetBy(_windowBackgroundOrigin.ToPoint()).OffsetBy(+1, -5).Add(0, 0, 0, 0).Add((tabBounds.Width / 3) + 20, 0, -tabBounds.Width / 3, 0),
                                     Color.White);

                    spriteBatch.DrawOnCtrl(this, _textureTabActive, tabBounds);
                }
                else
                {
                    spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(0, tabBounds.Y, _tabSectionWidth, tabBounds.Height), Color.Black);
                }

                spriteBatch.DrawOnCtrl(this, tab.Icon,
                                 new Rectangle((_tabWidth / 4) - (_tabIconSize / 2) + 2,
                                               (_tabHeight / 2) - (_tabIconSize / 2),
                                               _tabIconSize,
                                               _tabIconSize).OffsetBy(subBounds.Location),
                                 active || hovered
                                     ? Color.White
                                     : ContentService.Colors.DullColor);

                i++;
            }

            // Draw top of split
            spriteBatch.DrawOnCtrl(this, _textureSplitLine,
                                   _layoutTopSplitLineBounds,
                                   _layoutTopSplitLineSourceBounds);

            // Draw bottom of split
            spriteBatch.DrawOnCtrl(this, _textureSplitLine,
                                   _layoutBottomSplitLineBounds,
                                   _layoutBottomSplitLineSourceBounds);
        }

        public void AddTab(BaseTab tab)
        {
            if (tab is not null)
            {
                BaseTab prevTab = _tabs.Count > 0 ? _tabs[SelectedTabIndex] : tab;

                tab.CreateLayout(this, _windowContentWidth - 20);
                //tab.ContentContainer.Parent = this;
                //tab.ContentContainer.Visible = false;

                _tabs.Add(tab);
                _tabRegions.Add(tab, TabBoundsFromIndex(_tabRegions.Count));
                _tabs = _tabs.OrderBy(t => t.Priority).ToList();

                for (int i = 0; i < _tabs.Count; i++)
                {
                    _tabRegions[_tabs[i]] = TabBoundsFromIndex(i);
                }

                SwitchTab(prevTab);
                Invalidate();
            }
        }

        public void RemoveTab(BaseTab tab)
        {
            _ = _tabs.Remove(tab);
        }

        public void SwitchTab(BaseTab tab)
        {
            _selectedTabIndex = _tabs.IndexOf(tab);
            _subtitle = tab.Name;
            //_tabs.ForEach(t => t.ContentContainer.Visible = false);
            RecalculateLayout();
            Show();
        }

        protected override void PaintWindowBackground(SpriteBatch spriteBatch, Rectangle bounds)
        {

        }

        protected override void OnLeftMouseButtonPressed(MouseEventArgs e)
        {
            if (RelativeMousePosition.X < _standardTabBounds.Right && RelativeMousePosition.Y > _standardTabBounds.Y)
            {
                var tabList = _tabs.ToList();
                for (int tabIndex = 0; tabIndex < _tabs.Count; tabIndex++)
                {
                    BaseTab tab = tabList[tabIndex];
                    if (_tabRegions[tab].Contains(RelativeMousePosition))
                    {
                        SwitchTab(tab);
                        break;
                    }
                }

                tabList.Clear();
            }

            base.OnLeftMouseButtonPressed(e);
        }

        protected virtual void OnTabChanged(ValueChangedEventArgs<BaseTab> tab)
        {

        }

        private void OnTabChanged()
        {

        }

        private Rectangle TabBoundsFromIndex(int index)
        {
            return _standardTabBounds.OffsetBy(-_tabWidth, ContentRegion.Y + (index * _tabHeight));
        }
    }
}
