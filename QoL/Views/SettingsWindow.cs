using Blish_HUD.Content;
using Kenedia.Modules.Core.Res;
using Kenedia.Modules.Core.Views;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.QoL.Res;
using Microsoft.Xna.Framework;
using SizingMode = Blish_HUD.Controls.SizingMode;
using ControlFlowDirection = Blish_HUD.Controls.ControlFlowDirection;
using Blish_HUD;
using System.Collections.Generic;
using Kenedia.Modules.QoL.SubModules;
using Kenedia.Modules.QoL.Services;
using System;
using Kenedia.Modules.Core.Models;

namespace Kenedia.Modules.QoL.Views
{
    public class SettingsWindow : BaseSettingsWindow
    {
        private readonly Settings _settings;
        private readonly SharedSettingsView _sharedSettingsView;
        private readonly Dictionary<SubModuleType, SubModule> _subModules;
        private readonly FlowPanel _contentPanel;
        private double _tick;

        public SettingsWindow(AsyncTexture2D background, Rectangle windowRegion, Rectangle contentRegion, Settings settings, SharedSettingsView sharedSettingsView, Dictionary<SubModuleType, SubModule> subModules) : base(background, windowRegion, contentRegion)
        {
            SubWindowEmblem = AsyncTexture2D.FromAssetId(156027);
            MainWindowEmblem = AsyncTexture2D.FromAssetId(156035);
            Name = string.Format(strings_common.ItemSettings, $"{QoL.ModuleName}");
            _settings = settings;
            _sharedSettingsView = sharedSettingsView;
            _subModules = subModules;

            _contentPanel = new()
            {
                Parent = this,
                Width = ContentRegion.Width,
                Height = ContentRegion.Height,
                ControlPadding = new(0, 10),
                CanScroll = true,
            };

            CreateGeneralSettings();
            
            foreach (var subModule in _subModules)
            {
                subModule.Value.CreateSettingsPanel(_contentPanel, ContentRegion.Width - 20);
            }

            CreateClientSettings();
        }

        private void CreateGeneralSettings()
        {
            var headerPanel = new Panel()
            {
                Parent = _contentPanel,
                Width = ContentRegion.Width - 20,
                HeightSizingMode = SizingMode.AutoSize,
                ShowBorder = true,
                CanCollapse = true,
                TitleIcon = AsyncTexture2D.FromAssetId(157109),
                SetLocalizedTitle = () => strings_common.GeneralSettings,
            };

            var contentFlowPanel = new FlowPanel()
            {
                Parent = headerPanel,
                HeightSizingMode = SizingMode.AutoSize,
                WidthSizingMode = SizingMode.Fill,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ContentPadding = new(5, 2),
                ControlPadding = new(0, 2),
            };

            UI.WrapWithLabel(() => strings.HotbarExpandDirection_Name, () => strings.HotbarExpandDirection_Tooltip, contentFlowPanel, ContentRegion.Width - 20 - 16, new Dropdown()
            {
                Location = new(250, 0),
                Parent = contentFlowPanel,
                SetLocalizedItems = () =>
                {
                    return new()
                    {
                        $"{ExpandType.LeftToRight}".SplitStringOnUppercase(),
                        $"{ExpandType.RightToLeft}".SplitStringOnUppercase(),
                        $"{ExpandType.TopToBottom}".SplitStringOnUppercase(),
                        $"{ExpandType.BottomToTop}".SplitStringOnUppercase(),
                    };
                },
                SelectedItem = $"{_settings.HotbarExpandDirection.Value}".SplitStringOnUppercase(),
                ValueChangedAction = (b) => _settings.HotbarExpandDirection.Value = Enum.TryParse(b.RemoveSpaces(), out ExpandType expandType) ? expandType : _settings.HotbarExpandDirection.Value,
            });

            UI.WrapWithLabel(() => strings.HotbarButtonSorting_Name, () => strings.HotbarButtonSorting_Tooltip, contentFlowPanel, ContentRegion.Width - 20 - 16, new Dropdown()
            {
                Location = new(250, 0),
                Parent = contentFlowPanel,
                SetLocalizedItems = () =>
                {
                    return new()
                    {
                        $"{SortType.ActivesFirst}".SplitStringOnUppercase(),
                        $"{SortType.ByModuleName}".SplitStringOnUppercase(),
                    };
                },
                SelectedItem = $"{_settings.HotbarButtonSorting.Value}".SplitStringOnUppercase(),
                ValueChangedAction = (b) => _settings.HotbarButtonSorting.Value = Enum.TryParse(b.RemoveSpaces(), out SortType sortType) ? sortType : _settings.HotbarButtonSorting.Value,
            });

            UI.WrapWithLabel(() => strings.KeyboardLayout_Name, () => strings.KeyboardLayout_Tooltip, contentFlowPanel, ContentRegion.Width - 20 - 16, new Dropdown()
            {
                Location = new(250, 0),
                Parent = contentFlowPanel,
                SetLocalizedItems = () =>
                {
                    return new()
                    {
                        $"{KeyboardLayoutType.QWETY}".SplitStringOnUppercase(),
                        $"{KeyboardLayoutType.AZERTY}".SplitStringOnUppercase(),
                        $"{KeyboardLayoutType.QWERTZ}".SplitStringOnUppercase(),
                    };
                },
                SelectedItem = $"{_settings.KeyboardLayout.Value}".SplitStringOnUppercase(),
                ValueChangedAction = (b) => _settings.KeyboardLayout.Value = Enum.TryParse(b.RemoveSpaces(), out KeyboardLayoutType sortType) ? sortType : _settings.KeyboardLayout.Value,
            });
        }

        private void CreateClientSettings()
        {
            var headerPanel = new Panel()
            {
                Parent = _contentPanel,
                Width = ContentRegion.Width - 20,
                HeightSizingMode = SizingMode.AutoSize,
                ShowBorder = true,
                CanCollapse = true,
                TitleIcon = AsyncTexture2D.FromAssetId(759447),
                SetLocalizedTitle = () => strings_common.SharedSettings,
            };

            var contentFlowPanel = new FlowPanel()
            {
                Parent = headerPanel,
                HeightSizingMode = SizingMode.AutoSize,
                WidthSizingMode = SizingMode.Fill,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new(10),
            };

            _sharedSettingsView.CreateLayout(contentFlowPanel, ContentRegion.Width - 20);
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

            if (gameTime.TotalGameTime.TotalMilliseconds - _tick >= 1000)
            {
                _tick = gameTime.TotalGameTime.TotalMilliseconds;

                if (GameService.GameIntegration.Gw2Instance.Gw2HasFocus)
                {
                    _sharedSettingsView?.UpdateOffset();
                }
            }
        }
    }
}
