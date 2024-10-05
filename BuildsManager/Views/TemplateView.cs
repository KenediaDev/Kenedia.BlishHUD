﻿using Blish_HUD.Content;
using Kenedia.Modules.BuildsManager.Res;
using Kenedia.Modules.BuildsManager.Controls.Tabs;
using Kenedia.Modules.BuildsManager.Controls;
using Kenedia.Modules.BuildsManager.Controls.Selection;
using Kenedia.Modules.Core.Controls;

namespace Kenedia.Modules.BuildsManager.Views
{
    public class TemplateView : Blish_HUD.Graphics.UI.View
    {
        private TabbedRegion _tabbedRegion;

        public TemplateView(MainWindow mainWindow, SelectionPanel selectionPanel, AboutTab aboutTab, BuildTab buildTab, GearTab gearTab, QuickFiltersPanel quickFiltersPanel)
        {
            AboutTab = aboutTab;
            BuildTab = buildTab;
            GearTab = gearTab;
            QuickFiltersPanel = quickFiltersPanel;
            MainWindow = mainWindow;
            SelectionPanel = selectionPanel;

            QuickFiltersPanel.Anchor = mainWindow;
            QuickFiltersPanel.AnchorPosition = AnchoredContainer.AnchorPos.Left;
            QuickFiltersPanel.RelativePosition = new(0, 50, 0, 0);
        }

        public MainWindow MainWindow { get; }

        public SelectionPanel SelectionPanel { get; }

        public AboutTab AboutTab { get; }

        public BuildTab BuildTab { get; }

        public GearTab GearTab { get; }
        public QuickFiltersPanel QuickFiltersPanel { get; }

        protected override void Build(Blish_HUD.Controls.Container buildPanel)
        {
            base.Build(buildPanel);
            SelectionPanel.Parent = buildPanel;
            SelectionPanel.Location = new(35, 0);

            _tabbedRegion = new()
            {
                Parent = buildPanel,
                Location = new(SelectionPanel.Right + 15, 0),
                Width = buildPanel.ContentRegion.Width - 144,
                HeightSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                OnTabSwitched = () => SelectionPanel?.ResetAnchor(),
            };

            TabbedRegionTab tab;
            _tabbedRegion.Width = buildPanel.ContentRegion.Width - (SelectionPanel.Right + 15);
            _tabbedRegion.AddTab(tab = new TabbedRegionTab(AboutTab)
            {
                Header = () => strings.About,
                Icon = AsyncTexture2D.FromAssetId(440023),
            });

            _tabbedRegion.AddTab(new TabbedRegionTab(BuildTab)
            {
                Header = () => strings.Build,
                Icon = AsyncTexture2D.FromAssetId(156720),
            });

            _tabbedRegion.AddTab(new TabbedRegionTab(GearTab)
            {
                Header = () => strings.Equipment,
                Icon = AsyncTexture2D.FromAssetId(156714),
            });

            _tabbedRegion.SwitchTab(tab);

            BuildTab.Width = GearTab.Width = AboutTab.Width = buildPanel.ContentRegion.Width - 144;
        }
    }
}