using Blish_HUD.Content;
using ScreenNotification = Blish_HUD.Controls.ScreenNotification;
using Kenedia.Modules.BuildsManager.Controls.BuildPage;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.BuildsManager.Services;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Views;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Kenedia.Modules.Core.Services;
using Kenedia.Modules.BuildsManager.Controls;
using Kenedia.Modules.BuildsManager.Controls.GearPage;
using Kenedia.Modules.BuildsManager.Controls.NotesPage;
using Kenedia.Modules.BuildsManager.Controls.Selection;

namespace Kenedia.Modules.BuildsManager.Views
{
    public class MainWindow : StandardWindow
    {
        private readonly Data _data;
        private readonly TexturesService _texturesService;
        private readonly BuildPage _build;
        private readonly TabbedRegion _tabbedRegion;
        private readonly GearPage _gear;
        private readonly NotesPage _notes;
        private readonly SelectionPanel _selectionPanel;
        private Template _template;

        public MainWindow(AsyncTexture2D background, Rectangle windowRegion, Rectangle contentRegion, Data data, TexturesService texturesService) : base(background, windowRegion, contentRegion)
        {
            _data = data;
            _texturesService = texturesService;
            _selectionPanel = new()
            {
                Parent = this,
                Location = new(0, 0),
                HeightSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                Width = 330,
                //BackgroundColor = Color.Yellow * 0.2F,
            };

            _tabbedRegion = new()
            {
                Parent = this,
                Location = new(_selectionPanel.Right + 15, 0),
                Width = ContentRegion.Width - 144,
                HeightSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                OnTabSwitched = () => _selectionPanel.SetAnchor(null, Rectangle.Empty),
                //BackgroundColor = Color.Green * 0.2F,
            };

            TabbedRegionTab tab;
            _tabbedRegion.AddTab(new TabbedRegionTab(
                _build = new BuildPage(_texturesService))
            {
                Header = "Build",
                Icon = AsyncTexture2D.FromAssetId(156720),
            });

            _tabbedRegion.AddTab(tab = new TabbedRegionTab(
                _gear = new GearPage(_texturesService)
                {
                    SelectionPanel = _selectionPanel,
                })
            {
                Header = "Gear & Consumables",
                Icon = AsyncTexture2D.FromAssetId(156714),
            });

            _tabbedRegion.AddTab(new TabbedRegionTab(
                _notes = new NotesPage(_texturesService)
                {
                    HeightSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                    WidthSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                })
            {
                Header = "Notes",
                Icon = AsyncTexture2D.FromAssetId(440023),
            });

            _tabbedRegion.SwitchTab(tab);
        }

        public Template Template
        {
            get => _template; set
            {
                var prev = _template;

                if (Common.SetProperty(ref _template, value, ApplyTemplate))
                {
                    if (prev != null)
                    {
                        //prev.Changed -= BuildChanged;
                    }

                    //_template.Changed += BuildChanged;
                }
            }
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            if(_tabbedRegion != null)
            {
                _selectionPanel.ZIndex = _tabbedRegion.ZIndex + 1;
            }
        }

        private void ApplyTemplate()
        {
            _build.Template = _template;
            _gear.Template = _template;
            _selectionPanel.Template = _template;
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
