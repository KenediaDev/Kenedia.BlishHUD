﻿using Blish_HUD.Content;
using Kenedia.Modules.BuildsManager.Controls.BuildPage;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.BuildsManager.Services;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Views;
using Microsoft.Xna.Framework;
using Kenedia.Modules.Core.Services;
using Kenedia.Modules.BuildsManager.Controls;
using Kenedia.Modules.BuildsManager.Controls.GearPage;
using Kenedia.Modules.BuildsManager.Controls.NotesPage;
using Kenedia.Modules.BuildsManager.Controls.Selection;
using System;
using System.Diagnostics;

namespace Kenedia.Modules.BuildsManager.Views
{
    public class MainWindow : StandardWindow
    {
        private readonly Data _data;
        private readonly TexturesService _texturesService;
        private readonly BuildPage _build;
        private readonly TabbedRegion _tabbedRegion;
        private readonly GearPage _gear;
        private readonly AboutPage _notes;
        private readonly SelectionPanel _selectionPanel;
        private Template _template = new() { AutoSave = true };

        public MainWindow(AsyncTexture2D background, Rectangle windowRegion, Rectangle contentRegion, Data data, TexturesService texturesService) : base(background, windowRegion, contentRegion)
        {
            _data = data;
            _texturesService = texturesService;
            _selectionPanel = new()
            {
                Parent = this,
                Location = new(0, 0),
                HeightSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                Width = 375,
                Template = _template,
                MainWindow = this,
                ZIndex = int.MaxValue / 2,
                //BackgroundColor = Color.Yellow * 0.2F,
            };

            _tabbedRegion = new()
            {
                Parent = this,
                Location = new(_selectionPanel.Right + 15, 0),
                Width = ContentRegion.Width - 144,
                HeightSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                OnTabSwitched = _selectionPanel.ResetAnchor,
                ZIndex = int.MaxValue,
                //BackgroundColor = Color.Green * 0.2F,
            };

            TabbedRegionTab tab;

            _tabbedRegion.AddTab(new TabbedRegionTab(
                _notes = new AboutPage(_texturesService)
                {
                    HeightSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                    WidthSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                    Template = _template,
                })
            {
                Header = "About",
                Icon = AsyncTexture2D.FromAssetId(440023),
            });

            _tabbedRegion.AddTab(tab = new TabbedRegionTab(
                _build = new BuildPage(_texturesService))
            {
                Header = "Build",
                Icon = AsyncTexture2D.FromAssetId(156720),
            });
            _build.TemplatePresenter.Template = _template;

            _tabbedRegion.AddTab(new TabbedRegionTab(
                _gear = new GearPage(_texturesService)
                {
                    SelectionPanel = _selectionPanel,
                    Template = _template,
                })
            {
                Header = "Gear",
                Icon = AsyncTexture2D.FromAssetId(156714),
            });

            _tabbedRegion.SwitchTab(tab);
        }

        public event EventHandler<Template> TemplateChanged;

        public Template Template
        {
            get => _template; set => Common.SetProperty(ref _template, value, ApplyTemplate);
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            if (_tabbedRegion != null)
            {
                _selectionPanel.ZIndex = _tabbedRegion.ZIndex + 1;
            }
        }

        private void ApplyTemplate()
        {
            _build.TemplatePresenter.Template = _template;

            _gear.Template = _template;
            _notes.Template = _template;
            _selectionPanel.Template = _template;

            TemplateChanged?.Invoke(this, Template);
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();
            _tabbedRegion?.Dispose();
            _build?.Dispose();
            _gear?.Dispose();
            _notes?.Dispose();
            _selectionPanel?.Dispose();
        }
    }
}
