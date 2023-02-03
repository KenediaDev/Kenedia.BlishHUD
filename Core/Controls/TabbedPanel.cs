using Blish_HUD;
using Blish_HUD.Controls;
using Kenedia.Modules.Core.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.Core.Controls
{
    public class TabbedPanel : AnchoredContainer
    {
        protected readonly FlowPanel TabsButtonPanel = new()
        {
            FlowDirection = ControlFlowDirection.SingleLeftToRight,
            WidthSizingMode = SizingMode.Fill,
            ControlPadding = new Vector2(1, 0),
            Height = 25,
        };

        private PanelTab _activeTab;

        public TabbedPanel()
        {
            TabsButtonPanel.Parent = this;
            TabsButtonPanel.Resized += OnTabButtonPanelResized;

            HeightSizingMode = SizingMode.AutoSize;

            BackgroundImageColor = Color.Honeydew * 0.95f;
        }

        private event EventHandler TabAdded;

        private event EventHandler TabRemoved;

        public List<PanelTab> Tabs { get; } = new();

        public PanelTab ActiveTab
        {
            get => _activeTab;
            set => SwitchTab(value);
        }

        public void AddTab(PanelTab tab)
        {
            tab.Parent = this;
            tab.Disposed += OnTabDisposed;
            tab.TabButton.Parent = TabsButtonPanel;
            tab.TabButton.Click += (s, m) => TabButton_Click(tab);
            tab.Location = new(0, TabsButtonPanel.Bottom);
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

            int button_amount = Math.Max(1, TabsButtonPanel.Children.Count);
            int width = (TabsButtonPanel.Width - ((button_amount - 1) * (int)TabsButtonPanel.ControlPadding.X)) / button_amount;
            foreach (Control c in TabsButtonPanel.Children)
            {
                c.Width = width;
            }
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);
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
