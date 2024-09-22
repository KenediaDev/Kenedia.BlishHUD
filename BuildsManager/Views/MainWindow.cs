﻿using Blish_HUD.Content;
using Kenedia.Modules.Core.Views;
using Microsoft.Xna.Framework;
using Kenedia.Modules.Core.Services;
using Kenedia.Modules.BuildsManager.Controls_Old;
using Kenedia.Modules.BuildsManager.Controls_Old.Selection;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.Res;
using Blish_HUD.Modules;
using Kenedia.Modules.BuildsManager.Controls.Tabs;

namespace Kenedia.Modules.BuildsManager.Views
{
    public class MainWindow : StandardWindow
    {
        private readonly TabbedRegion _tabbedRegion;

        public MainWindow(Module module, TemplatePresenter templatePresenter, SelectionPanel selectionPanel, AboutTab aboutTab, BuildTab buildTab, GearTab gearTab) : base(
            TexturesService.GetTextureFromRef(@"textures\mainwindow_background.png", "mainwindow_background"),
                new Rectangle(30, 30, 915, 670 + 30),
                new Rectangle(30, 20, 915 - 3, 670 + 15))
        {
            TemplatePresenter = templatePresenter;
            SelectionPanel = selectionPanel;
            Parent = Graphics.SpriteScreen;

            AboutTab = aboutTab;
            BuildTab = buildTab;
            GearTab = gearTab;

            selectionPanel.Parent = this;

            Title = "❤";
            Subtitle = "❤";
            SavesPosition = true;
            Id = $"{module.Name} MainWindow";
            MainWindowEmblem = AsyncTexture2D.FromAssetId(156020);
            Name = module.Name;
            Version = module.Version;
        
            _tabbedRegion = new()
            {
                Parent = this,
                Location = new(selectionPanel.Right + 15, 0),
                Width = ContentRegion.Width - 144,
                HeightSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                OnTabSwitched = selectionPanel.ResetAnchor,
            };

            TabbedRegionTab tab;

            GearTab.Width = BuildTab.Width = AboutTab.Width = ContentRegion.Width - 144;

            _tabbedRegion.AddTab(new TabbedRegionTab(AboutTab)
            {
                Header = () => strings.About,
                Icon = AsyncTexture2D.FromAssetId(440023),                
            });

            _tabbedRegion.AddTab(tab = new TabbedRegionTab(BuildTab)
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

            TemplatePresenter.NameChanged += TemplatePresenter_NameChanged;

            Width = 1200;
            Height = 900;
        }

        private TemplatePresenter TemplatePresenter { get; } = new();

        public SelectionPanel SelectionPanel { get; }

        public AboutTab AboutTab { get; }

        public BuildTab BuildTab { get; }

        public GearTab GearTab { get; }

        private void TemplatePresenter_NameChanged(object sender, Core.Models.ValueChangedEventArgs<string> e)
        {
            SubName = e.NewValue;
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();
            _tabbedRegion?.Dispose();
            BuildTab?.Dispose();
            GearTab?.Dispose();
            AboutTab?.Dispose();
            SelectionPanel?.Dispose();

            TemplatePresenter.Template = null;
        }
    }
}
