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

namespace Kenedia.Modules.BuildsManager.Views
{
    public class MainWindow : StandardWindow
    {
        private readonly Data _data;
        private readonly TexturesService _texturesService;
        private readonly BuildPage _build;
        private readonly TabbedRegion _tabbedRegion;
        private readonly GearPage _gear;
        private readonly AboutPage _aboutPage;
        private readonly SelectionPanel _selectionPanel;
        private readonly TemplatePresenter _templatePresenter = new();

        public MainWindow(AsyncTexture2D background, Rectangle windowRegion, Rectangle contentRegion, Data data, TexturesService texturesService) : base(background, windowRegion, contentRegion)
        {
            _data = data;
            _texturesService = texturesService;

            _selectionPanel = new(_templatePresenter)
            {
                Parent = this,
                Location = new(0, 0),
                HeightSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                Width = 375,
                MainWindow = this,
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
                //BackgroundColor = Color.Green * 0.2F,
            };

            TabbedRegionTab tab;

            _tabbedRegion.AddTab(new TabbedRegionTab(
                _aboutPage = new AboutPage(_texturesService, _templatePresenter)
                {
                    HeightSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                    WidthSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                })
            {
                Header = "About",
                Icon = AsyncTexture2D.FromAssetId(440023),
            });

            _tabbedRegion.AddTab(tab = new TabbedRegionTab(
                _build = new BuildPage(_texturesService, _templatePresenter))
            {
                Header = "Build",
                Icon = AsyncTexture2D.FromAssetId(156720),
            });

            _tabbedRegion.AddTab(new TabbedRegionTab(
                _gear = new GearPage(_texturesService, _templatePresenter)
                {
                    SelectionPanel = _selectionPanel,
                })
            {
                Header = "Gear",
                Icon = AsyncTexture2D.FromAssetId(156714),
            });

            _tabbedRegion.SwitchTab(tab);
        }

        public event ValueChangedEventHandler<Template> TemplateChanged;

        public Template Template
        {
            get => _templatePresenter.Template; set
            {
                var prev = _templatePresenter.Template;

                if (_templatePresenter.Template != value)
                {
                    _templatePresenter.Template = value ?? new();
                    _templatePresenter.Template?.Load();

                    TemplateChanged?.Invoke(this, new(prev, _templatePresenter.Template));
                }
            }
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            if (_tabbedRegion != null)
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
        }
    }
}
