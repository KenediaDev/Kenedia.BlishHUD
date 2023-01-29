using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kenedia.Modules.Core.Views
{
    public class BaseTab
    {
        public AsyncTexture2D Icon { get; set; }

        public string Name { get; set; }

        public int Priority { get; set; }

        public Container ContentContainer { get; set; } = null;
    }

    public class BaseTabbedWindow : WindowBase
    {
        private readonly Rectangle StandardTabBounds;

        public BaseTabbedWindow()
        {
            StandardTabBounds = new Rectangle(_tabSectionWidth, 24, _tabWidth, _tabHeight);
            _background = _rawbackground.Texture.GetRegion(0, 0, _rawbackground.Width, _rawbackground.Height);

            var tabWindowTexture = _background;
            tabWindowTexture = tabWindowTexture.Duplicate().SetRegion(0, 0, 64, _background.Height, Color.Transparent);
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

            var tabWindowTexture = _background;
            tabWindowTexture = tabWindowTexture.Duplicate().SetRegion(0, 0, 64, _background.Height, Color.Transparent);
            _tabBarBackground = _background.Duplicate().SetRegion(0, 0, 64, _background.Height, Color.Transparent);
            //ConstructWindow(tabWindowTexture, new Vector2(0), _background.Bounds, new Thickness(60, 75, 45, 25), 40);

            _backgroundBounds = new Rectangle(
                -20,
                15,
                _tabBarBackground.Width,
                _tabBarBackground.Height
                );
        }

        private int _tabHeight = 52;
        private int _tabWidth = 104;
        private int _tabIconSize = 32;

        private int _tabSectionWidth = 46;

        private int _windowContentWidth = 500;
        private int _windowContentHeight = 640;

        private Rectangle _backgroundBounds;

        private Rectangle _layoutTopTabBarBounds;
        private Rectangle _layoutBottomTabBarBounds;

        private Rectangle _layoutTopSplitLineBounds;
        private Rectangle _layoutBottomSplitLineBounds;

        private Rectangle _layoutTopSplitLineSourceBounds;
        private Rectangle _layoutBottomSplitLineSourceBounds;

        private List<BaseTab> _tabs = new List<BaseTab>();
        private readonly BaseTab _activeBaseTab;

        private readonly AsyncTexture2D _rawbackground = AsyncTexture2D.FromAssetId(155997);
        private Texture2D _background;
        private Texture2D _tabBarBackground;
        private readonly AsyncTexture2D _textureSplitLine = AsyncTexture2D.FromAssetId(605024);
        private static readonly Texture2D _textureBlackFade = Content.GetTexture("fade-down-46");
        private static readonly Texture2D _textureTabActive = Content.GetTexture("window-tab-active");

        private readonly Dictionary<BaseTab, Rectangle> _tabRegions = new Dictionary<BaseTab, Rectangle>();

        public BaseTab SelectedTab => _tabs.Count > _selectedTabIndex ? _tabs[_selectedTabIndex] : null;

        protected int _selectedTabIndex = -1;
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set
            {
                if (SetProperty(ref _selectedTabIndex, value, true))
                {
                    OnTabChanged(EventArgs.Empty);
                }
            }
        }

        private int _hoveredTabIndex = 0;

        private int HoveredTabIndex
        {
            get => _hoveredTabIndex;
            set => SetProperty(ref _hoveredTabIndex, value);
        }

        private void OnTabChanged(EventArgs empty)
        {

        }

        private Rectangle TabBoundsFromIndex(int index)
        {
            return StandardTabBounds.OffsetBy(-_tabWidth, ContentRegion.Y + index * _tabHeight);
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            if (_tabs.Count == 0) return;

            var firstTabBounds = TabBoundsFromIndex(0);
            var selectedTabBounds = _tabRegions[this.SelectedTab];
            var lastTabBounds = TabBoundsFromIndex(_tabRegions.Count - 1);

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

        protected override void PaintWindowBackground(SpriteBatch spriteBatch, Rectangle bounds)
        {

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
            foreach (var tab in _tabs)
            {
                bool active = (i == this.SelectedTabIndex);
                bool hovered = (i == this.HoveredTabIndex);

                var tabBounds = _tabRegions[tab];
                var subBounds = new Rectangle(tabBounds.X + tabBounds.Width / 2, tabBounds.Y, _tabWidth / 2, tabBounds.Height);

                if (active)
                {
                    spriteBatch.DrawOnCtrl(this, _background,
                                           tabBounds,
                                           tabBounds.OffsetBy(_windowBackgroundOrigin.ToPoint()).OffsetBy(+1, -5).Add(0, 0, 0, 0).Add(tabBounds.Width / 3 + 20, 0, -tabBounds.Width / 3, 0),
                                     Color.White);

                    spriteBatch.DrawOnCtrl(this, _textureTabActive, tabBounds);
                }
                else
                {
                    spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(0, tabBounds.Y, _tabSectionWidth, tabBounds.Height), Color.Black);
                }

                spriteBatch.DrawOnCtrl(this, tab.Icon,
                                 new Rectangle(_tabWidth / 4 - _tabIconSize / 2 + 2,
                                               _tabHeight / 2 - _tabIconSize / 2,
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

            var prevTab = _tabs.Count > 0 ? _tabs[this.SelectedTabIndex] : tab;
            tab.ContentContainer.Parent = this;
            tab.ContentContainer.Visible = false;

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

        public void RemoveTab(BaseTab tab)
        {
            _ = _tabs.Remove(tab);
        }

        public void SwitchTab(BaseTab tab)
        {
            _selectedTabIndex = _tabs.IndexOf(tab);
            _subtitle = tab.Name;
            _tabs.ForEach(t => t.ContentContainer.Visible = t == tab);
            RecalculateLayout();
        }

        protected virtual void OnTabChanged(ValueChangedEventArgs<BaseTab> tab)
        {

        }
        protected override void OnLeftMouseButtonPressed(MouseEventArgs e)
        {
            if (RelativeMousePosition.X < StandardTabBounds.Right && RelativeMousePosition.Y > StandardTabBounds.Y)
            {
                var tabList = _tabs.ToList();
                for (int tabIndex = 0; tabIndex < _tabs.Count; tabIndex++)
                {
                    var tab = tabList[tabIndex];
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
    }
}
