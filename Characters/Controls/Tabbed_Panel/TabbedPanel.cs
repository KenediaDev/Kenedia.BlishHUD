using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Kenedia.Modules.Characters.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.Characters.Controls
{
    public class TabbedPanel : Panel
    {
        protected readonly FlowPanel _tabsButtonPanel = new()
        {
            FlowDirection = ControlFlowDirection.SingleLeftToRight,
            WidthSizingMode = SizingMode.Fill,
            ControlPadding = new Vector2(1, 0),
            Height = 25,
        };

        private PanelTab _activeTab;

        public TabbedPanel()
        {
            _tabsButtonPanel.Parent = this;
            _tabsButtonPanel.Resized += OnTabButtonPanelResized;

            HeightSizingMode = SizingMode.AutoSize;

            // Background = AsyncTexture2D.FromAssetId(156003);
            Parent = GameService.Graphics.SpriteScreen;
            ZIndex = 999;
            Visible = true;
            BackgroundTint = Color.Honeydew * 0.95f;
        }

        private event EventHandler TabAdded;

        private event EventHandler TabRemoved;

        public List<PanelTab> Tabs { get; } = new();

        public PanelTab ActiveTab
        {
            get => _activeTab;
            set => SwitchTab(value);
        }

        public AsyncTexture2D Background { get; set; }

        public Color BackgroundTint { get; set; }

        public Color ColorBackground { get; set; }

        public Rectangle TextureRectangle { get; set; } = Rectangle.Empty;

        public Point TextureOffset { get; set; } = Point.Zero;

        public void AddTab(PanelTab tab)
        {
            tab.Parent = this;
            tab.Disposed += OnTabDisposed;
            tab.TabButton.Parent = _tabsButtonPanel;
            tab.TabButton.Click += (s, m) => TabButton_Click(tab);
            tab.Location = new(0, _tabsButtonPanel.Bottom);
            Tabs.Add(tab);
            TabAdded?.Invoke(this, EventArgs.Empty);
            ActiveTab ??= tab;
            RecalculateLayout();
        }

        public void RemoveTab(PanelTab tab)
        {
            tab.Disposed -= OnTabDisposed;
            tab.TabButton.Click -= (s, m) => TabButton_Click(tab);
            tab.Parent = null;
            tab.TabButton.Parent = null;

            _ = Tabs.Remove(tab);
            TabRemoved?.Invoke(this, EventArgs.Empty);
            RecalculateLayout();
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int button_amount = Math.Max(1, _tabsButtonPanel.Children.Count);
            int width = (_tabsButtonPanel.Width - ((button_amount - 1) * (int)_tabsButtonPanel.ControlPadding.X)) / button_amount;
            foreach (Control c in _tabsButtonPanel.Children)
            {
                c.Width = width;
            }
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            // if(ColorBackground != null) spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, _tabsButtonPanel.Bottom, bounds.Width, bounds.Height - _tabsButtonPanel.Bottom), Rectangle.Empty, ColorBackground);
            if (ColorBackground != null)
            {
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, bounds.Width, bounds.Height), Rectangle.Empty, ColorBackground);
            }

            if (Background != null)
            {
                Rectangle textRect = TextureRectangle != Rectangle.Empty ? TextureRectangle : new Rectangle(Point.Zero, _size);

                spriteBatch.DrawOnCtrl(
                    this,
                    Background,
                    bounds,
                    new Rectangle(TextureOffset.X + TextureRectangle.X, TextureOffset.Y + TextureRectangle.Y, textRect.Width, textRect.Height),
                    BackgroundTint,
                    0f,
                    default);
            }

            Color color = Color.Black;

            // Top
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, bounds.Width, 2), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, bounds.Width, 1), Rectangle.Empty, color * 0.6f);

            // Bottom
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Bottom - 2, bounds.Width, 2), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Bottom - 1, bounds.Width, 1), Rectangle.Empty, color * 0.6f);

            // Left
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, 2, bounds.Height), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, 1, bounds.Height), Rectangle.Empty, color * 0.6f);

            // Right
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Right - 2, bounds.Top, 2, bounds.Height), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Right - 1, bounds.Top, 1, bounds.Height), Rectangle.Empty, color * 0.6f);
        }

        protected override void OnResized(ResizedEventArgs e)
        {
            base.OnResized(e);
        }

        protected virtual bool SwitchTab(PanelTab tab = null)
        {
            foreach (PanelTab t in Tabs)
            {
                if (t != tab)
                {
                    t.Active = false;
                }
            }

            if (tab != null)
            {
                tab.Active = true;
            }

            _activeTab = tab;

            return false;
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            Tabs.DisposeAll();
        }

        private void OnTabButtonPanelResized(object sender, ResizedEventArgs e)
        {
            RecalculateLayout();
        }

        private void TabButton_Click(PanelTab t)
        {
            SwitchTab(t);
        }

        private void OnTabDisposed(object sender, EventArgs e)
        {
            RemoveTab((PanelTab)sender);
        }
    }
}
