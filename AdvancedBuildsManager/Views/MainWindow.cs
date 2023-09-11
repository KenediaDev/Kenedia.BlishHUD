using Blish_HUD.Content;
using Kenedia.Modules.AdvancedBuildsManager.Controls.BuildPage;
using Kenedia.Modules.AdvancedBuildsManager.Models.Templates;
using Kenedia.Modules.AdvancedBuildsManager.Services;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Views;
using Microsoft.Xna.Framework;
using Kenedia.Modules.Core.Services;
using Kenedia.Modules.AdvancedBuildsManager.Controls;
using Kenedia.Modules.AdvancedBuildsManager.Controls.GearPage;
using Kenedia.Modules.AdvancedBuildsManager.Controls.NotesPage;
using Kenedia.Modules.AdvancedBuildsManager.Controls.Selection;

namespace Kenedia.Modules.AdvancedBuildsManager.Views
{
    public class MainWindow : StandardWindow
    {
        private readonly Data _data;
        private readonly BuildPage _build;
        private readonly TabbedRegion _tabbedRegion;
        private readonly GearPage _gear;
        private readonly AboutPage _notes;
        private readonly RotationPage _rotation;
        private readonly SelectionPanel _selectionPanel;
        private Template _template;

        public MainWindow(AsyncTexture2D background, Rectangle windowRegion, Rectangle contentRegion, Data data) : base(background, windowRegion, contentRegion)
        {
            _data = data;
            _selectionPanel = new()
            {
                Parent = this,
                Location = new(0, 0),
                HeightSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                Width = 375,
                //BackgroundColor = Color.Yellow * 0.2F,
            };

            _tabbedRegion = new()
            {
                Parent = this,
                Location = new(_selectionPanel.Right + 15, 0),
                Width = ContentRegion.Width - 144,
                HeightSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                OnTabSwitched = _selectionPanel.ResetAnchor,
                //BackgroundColor = Color.Green * 0.2F,
            };

            TabbedRegionTab tab;

            _tabbedRegion.AddTab(new TabbedRegionTab(
                _notes = new AboutPage()
                {
                    HeightSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                    WidthSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                })
            {
                Header = "About",
                Icon = AsyncTexture2D.FromAssetId(440023),
            });

            _tabbedRegion.AddTab(new TabbedRegionTab(
                _build = new BuildPage())
            {
                Header = "Build",
                Icon = AsyncTexture2D.FromAssetId(156720),
            });

            _tabbedRegion.AddTab(tab = new TabbedRegionTab(
                _gear = new GearPage()
                {
                    SelectionPanel = _selectionPanel,
                })
            {
                Header = "Gear",
                Icon = AsyncTexture2D.FromAssetId(156714),
            });

            _tabbedRegion.AddTab(new TabbedRegionTab(
                _rotation = new RotationPage()
                {
                    HeightSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                    WidthSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                    SelectionPanel = _selectionPanel,
                })
            {
                Header = "Rotation",
                Icon = AsyncTexture2D.FromAssetId(155054),
            });

            _tabbedRegion.SwitchTab(tab);
        }

        public Template Template
        {
            get => _template; set => Common.SetProperty(ref _template, value, ApplyTemplate);
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            if(_tabbedRegion is not null)
            {
                _selectionPanel.ZIndex = _tabbedRegion.ZIndex + 1;
            }
        }

        private void ApplyTemplate()
        {
            _build.Template = _template;
            _gear.Template = _template;
            _notes.Template = _template;
            _rotation.Template = _template;
            _selectionPanel.Template = _template;
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();
            _tabbedRegion?.Dispose();
            _build?.Dispose();
            _gear?.Dispose();
            _notes?.Dispose();
            _rotation?.Dispose();
            _selectionPanel?.Dispose();
        }
    }
}
