using Blish_HUD.Content;
using Kenedia.Modules.BuildsManager.Controls.BuildPage;
using Kenedia.Modules.BuildsManager.Services;
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

namespace Kenedia.Modules.BuildsManager.Views
{
    public class MainWindow : StandardWindow
    {
        private readonly TexturesService _texturesService;
        private readonly BuildPage _build;
        private readonly TabbedRegion _tabbedRegion;
        private readonly GearPage _gear;
        private readonly AboutPage _aboutPage;
        private readonly SelectionPanel _selectionPanel;

        public MainWindow(AsyncTexture2D background, Rectangle windowRegion, Rectangle contentRegion, TexturesService texturesService) : base(background, windowRegion, contentRegion)
        {
            _texturesService = texturesService;

            _selectionPanel = new(TemplatePresenter, this)
            {
                Parent = this,
                Location = new(0, 0),
                HeightSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                Width = 375,
                ZIndex = int.MaxValue,
            };

            _tabbedRegion = new()
            {
                Parent = this,
                Location = new(_selectionPanel.Right + 15, 0),
                Width = ContentRegion.Width - 144,
                HeightSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                OnTabSwitched = _selectionPanel.ResetAnchor,
                ZIndex = int.MaxValue - 5,
            };

            TabbedRegionTab tab;

            _tabbedRegion.AddTab(new TabbedRegionTab(
                _aboutPage = new AboutPage(_texturesService, TemplatePresenter)
                {
                    HeightSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                    WidthSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                })
            {
                Header = () => strings.About,
                Icon = AsyncTexture2D.FromAssetId(440023),
            });

            _tabbedRegion.AddTab(tab = new TabbedRegionTab(
                _build = new BuildPage(_texturesService, TemplatePresenter))
            {
                Header = () => strings.Build,
                Icon = AsyncTexture2D.FromAssetId(156720),
            });

            _tabbedRegion.AddTab(new TabbedRegionTab(
                _gear = new GearPage(_texturesService, TemplatePresenter)
                {
                    SelectionPanel = _selectionPanel,
                })
            {
                Header = () => strings.Equipment,
                Icon = AsyncTexture2D.FromAssetId(156714),
            });

            _tabbedRegion.SwitchTab(tab);

            TemplatePresenter.NameChanged += TemplatePresenter_NameChanged;
            //Template = BuildsManager.ModuleInstance?.Templates.FirstOrDefault();
        }

        private TemplatePresenter TemplatePresenter { get; set; } = new();

        private void TemplatePresenter_NameChanged(object sender, ValueChangedEventArgs<string> e)
        {
            SubName = e.NewValue;
        }

        public event ValueChangedEventHandler<Template> TemplateChanged;

        public Template Template
        {
            get => TemplatePresenter?.Template; set
            {
                var prev = TemplatePresenter.Template;

                SubName = value?.Name;

                if (TemplatePresenter.Template != value)
                {
                    TemplatePresenter.Template = value ?? new();
                    TemplatePresenter.Template?.Load();

                    TemplateChanged?.Invoke(this, new(prev, TemplatePresenter.Template));
                }
            }
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            if (_tabbedRegion is not null)
            {
                _selectionPanel.ZIndex = _tabbedRegion.ZIndex + 1;
            }
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
            TemplatePresenter = null;
        }

        public void SelectFirstTemplate()
        {
            _selectionPanel?.SelectFirstTemplate();
        }
    }
}
