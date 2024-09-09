using Blish_HUD.Content;
using Kenedia.Modules.BuildsManager.Controls.BuildPage;
using Kenedia.Modules.Core.Views;
using Microsoft.Xna.Framework;
using Kenedia.Modules.Core.Services;
using Kenedia.Modules.BuildsManager.Controls;
using Kenedia.Modules.BuildsManager.Controls.GearPage;
using Kenedia.Modules.BuildsManager.Controls.AboutPage;
using Kenedia.Modules.BuildsManager.Controls.Selection;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.BuildsManager.Res;
using System.Diagnostics;
using Kenedia.Modules.BuildsManager.Services;

namespace Kenedia.Modules.BuildsManager.Views
{
    public class MainWindow : StandardWindow
    {
        private readonly BuildPage _build;
        private readonly TabbedRegion _tabbedRegion;
        private readonly GearPage _gear;
        private readonly AboutPage _aboutPage;
        private readonly SelectionPanel _selectionPanel;

        public MainWindow(AsyncTexture2D background, Rectangle windowRegion, Rectangle contentRegion, TemplateTags templateTags) : base(background, windowRegion, contentRegion)
        {
            _selectionPanel = new(TemplatePresenter, this)
            {
                Parent = this,
                Location = new(0, 0),
                HeightSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                Width = 375,
            };

            _tabbedRegion = new()
            {
                Parent = this,
                Location = new(_selectionPanel.Right + 15, 0),
                Width = ContentRegion.Width - 144,
                HeightSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                OnTabSwitched = _selectionPanel.ResetAnchor,
            };

            TabbedRegionTab tab;

            _tabbedRegion.AddTab(new TabbedRegionTab(
                _aboutPage = new AboutPage(TemplatePresenter, templateTags)
                {
                    HeightSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                    WidthSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                })
            {
                Header = () => strings.About,
                Icon = AsyncTexture2D.FromAssetId(440023),
            });

            _tabbedRegion.AddTab(tab = new TabbedRegionTab(
                _build = new BuildPage(TemplatePresenter))
            {
                Header = () => strings.Build,
                Icon = AsyncTexture2D.FromAssetId(156720),
            });

            _tabbedRegion.AddTab(new TabbedRegionTab(
                _gear = new GearPage(TemplatePresenter)
                {
                    SelectionPanel = _selectionPanel,
                })
            {
                Header = () => strings.Equipment,
                Icon = AsyncTexture2D.FromAssetId(156714),
            });

            _tabbedRegion.SwitchTab(tab);

            TemplatePresenter.NameChanged += TemplatePresenter_NameChanged;
            SelectFirstTemplate();
        }

        private TemplatePresenter TemplatePresenter { get; } = new();

        private void TemplatePresenter_NameChanged(object sender, ValueChangedEventArgs<string> e)
        {
            SubName = e.NewValue;
        }

        public Template Template
        {
            get => TemplatePresenter?.Template; set
            {
                if (TemplatePresenter is null) return;

                TemplatePresenter.Template = value ?? new();
                TemplatePresenter.Template?.Load();
                SubName = TemplatePresenter.Template?.Name;
            }
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();
            _tabbedRegion?.Dispose();
            _build?.Dispose();
            _gear?.Dispose();
            _aboutPage?.Dispose();
            _selectionPanel?.Dispose();

            TemplatePresenter.Template = null;
        }

        public void SelectFirstTemplate()
        {
            _selectionPanel?.SelectFirstTemplate();
            TemplatePresenter.InvokeTemplateSwitch();
        }
    }
}
