using Blish_HUD;
using Blish_HUD.Settings;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Extensions;
using Microsoft.Xna.Framework;
using System;
using Kenedia.Modules.QoL.Res;
using Blish_HUD.Content;
using System.IO;
using SizingMode = Blish_HUD.Controls.SizingMode;
using ControlFlowDirection = Blish_HUD.Controls.ControlFlowDirection;

namespace Kenedia.Modules.QoL.SubModules.EnhancedCrosshair
{
    public class EnhancedCrosshair : SubModule
    {
        private SettingEntry<Point> _crosshairSize;
        private Image _crosshair;

        //1058519
        //1677342
        public EnhancedCrosshair(SettingCollection settings) : base(settings)
        {
            UI_Elements.Add(
            _crosshair = new Image()
            {
                Parent = GameService.Graphics.SpriteScreen,
                Size = _crosshairSize.Value,
                Texture = AsyncTexture2D.FromAssetId(1677342),
                Enabled = false,
            });

            string path = QoL.ModuleInstance.Paths.ModulePath + $"crosshair.png";
            if (File.Exists(path))
            {
                GameService.Graphics.QueueMainThreadRender((graphicsDevice) => _crosshair.Texture = TextureUtil.FromStreamPremultiplied(graphicsDevice, new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)));
            }
        }

        public override SubModuleType SubModuleType => SubModuleType.EnhancedCrosshair;

        public override void Update(GameTime gameTime)
        {
            if (!Enabled) return;

            _crosshairSize.Value = new(84);
            _crosshair.Texture = AsyncTexture2D.FromAssetId(1058519);
            _crosshair.Opacity = 0.5f;
            var r = QoL.ModuleInstance.Services.ClientWindowService.WindowBounds;
            var p = GameService.Graphics.SpriteScreen.AbsoluteBounds;
            _crosshair.Size = _crosshairSize.Value;
            _crosshair.Location = p.Center.Add(new(-_crosshairSize.Value.X / 2, -_crosshairSize.Value.Y / 2));
            _crosshair.Visible = Enabled && GameService.GameIntegration.Gw2Instance.IsInGame && !GameService.Gw2Mumble.UI.IsMapOpen;
        }

        protected override void Enable()
        {
            base.Enable();
            _crosshair.Visible = Enabled;
        }

        override protected void Disable()
        {
            base.Disable();
            _crosshair.Visible = Enabled;
        }

        protected override void DefineSettings(SettingCollection settings)
        {
            base.DefineSettings(settings);

            _crosshairSize = settings.DefineSetting(nameof(_crosshairSize),
                new Point(48),
                () => strings.DisableOnSearch_Name,
                () => strings.DisableOnSearch_Tooltip);
        }

        public override void CreateSettingsPanel(FlowPanel flowPanel, int width)
        {
            var headerPanel = new Panel()
            {
                Parent = flowPanel,
                Width = width,
                HeightSizingMode = SizingMode.AutoSize,
                ShowBorder = true,
                CanCollapse = true,
                TitleIcon = Icon.Texture,
                Title = SubModuleType.ToString(),
            };

            var contentFlowPanel = new FlowPanel()
            {
                Parent = headerPanel,
                HeightSizingMode = SizingMode.AutoSize,
                WidthSizingMode = SizingMode.Fill,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new(10),
            };

            UI.WrapWithLabel(() => string.Format(strings.ShowInHotbar_Name, $"{SubModuleType}"), () => string.Format(strings.ShowInHotbar_Description, $"{SubModuleType}"), contentFlowPanel, width - 16, new Checkbox()
            {
                Height = 20,
                Checked = ShowInHotbar.Value,
                CheckedChangedAction = (b) => ShowInHotbar.Value = b,
            });

            _ = new KeybindingAssigner()
            {
                Parent = contentFlowPanel,
                Width = width,
                KeyBinding = HotKey.Value,
                KeybindChangedAction = (kb) =>
                {
                    HotKey.Value = new()
                    {
                        ModifierKeys = kb.ModifierKeys,
                        PrimaryKey = kb.PrimaryKey,
                        Enabled = kb.Enabled,
                        IgnoreWhenInTextField = true,
                    };
                },
                SetLocalizedKeyBindingName = () => string.Format(strings.HotkeyEntry_Name, $"{SubModuleType}"),
                SetLocalizedTooltip = () => string.Format(strings.HotkeyEntry_Description, $"{SubModuleType}"),
            };
        }
    }
}
