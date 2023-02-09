using Blish_HUD.Content;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Views;
using Kenedia.Modules.ReleaseTheChoya.Models;
using Kenedia.Modules.ReleaseTheChoya.Res;
using Microsoft.Xna.Framework;
using System;
using SizingMode = Blish_HUD.Controls.SizingMode;
using ControlFlowDirection = Blish_HUD.Controls.ControlFlowDirection;
using Container = Blish_HUD.Controls.Container;
using Kenedia.Modules.Core.Res;
using Blish_HUD.Settings;
using Kenedia.Modules.Core.Services;
using Kenedia.Modules.ReleaseTheChoya.Services;

namespace Kenedia.Modules.ReleaseTheChoya.Views
{
    public class SettingsWindow : BaseSettingsWindow
    {
        private readonly Settings _settings;
        private readonly TexturesService _texturesService;

        public SettingsWindow(AsyncTexture2D background, Rectangle windowRegion, Rectangle contentRegion, Settings settings, TexturesService texturesService) : base(background, windowRegion, contentRegion)
        {
            _settings = settings;
            _texturesService = texturesService;
            var behaviorPanel = new Panel()
            {
                Parent = ContentPanel,
                Width = ContentRegion.Width - 20,
                HeightSizingMode = SizingMode.AutoSize,
                ShowBorder = true,
                CanCollapse = true,
                TitleIcon = AsyncTexture2D.FromAssetId(155051),
                SetLocalizedTitle = () => strings.Behavior,
            };

            var behaviorContentPanel = new FlowPanel()
            {
                Parent = behaviorPanel,
                Location = new(5),
                HeightSizingMode = SizingMode.AutoSize,
                WidthSizingMode = SizingMode.Fill,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new(3, 3),
            };

            var appearancePanel = new Panel()
            {
                Parent = ContentPanel,
                Width = ContentRegion.Width - 20,
                HeightSizingMode = SizingMode.AutoSize,
                ShowBorder = true,
                CanCollapse = true,
                TitleIcon = AsyncTexture2D.FromAssetId(156740),
                SetLocalizedTitle = () => strings.Appearance,
            };

            var appearanceContentPanel = new FlowPanel()
            {
                Parent = appearancePanel,
                Location = new(5),
                HeightSizingMode = SizingMode.AutoSize,
                WidthSizingMode = SizingMode.Fill,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new(3, 3),
            };

            var delayPanel = new Panel()
            {
                Parent = ContentPanel,
                Width = ContentRegion.Width - 20,
                HeightSizingMode = SizingMode.AutoSize,
                ShowBorder = true,
                CanCollapse = true,
                TitleIcon = AsyncTexture2D.FromAssetId(155035),
                SetLocalizedTitle = () => strings.Delays,
            };

            var delayContentPanel = new FlowPanel()
            {
                Parent = delayPanel,
                Location = new(5),
                HeightSizingMode = SizingMode.AutoSize,
                WidthSizingMode = SizingMode.Fill,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new(3, 3),
            };

            var keybindPanel = new Panel()
            {
                Parent = ContentPanel,
                Width = ContentRegion.Width - 20,
                HeightSizingMode = SizingMode.AutoSize,
                ShowBorder = true,
                CanCollapse = true,
                TitleIcon = AsyncTexture2D.FromAssetId(156734),
                SetLocalizedTitle = () => strings.Behavior,
            };

            var keybindContentPanel = new FlowPanel()
            {
                Parent = keybindPanel,
                Location = new(5),
                HeightSizingMode = SizingMode.AutoSize,
                WidthSizingMode = SizingMode.Fill,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new(3, 3),
            };

            var staticChoyaPanel = new Panel()
            {
                Parent = ContentPanel,
                Width = ContentRegion.Width - 20,
                HeightSizingMode = SizingMode.AutoSize,
                ShowBorder = true,
                CanCollapse = true,
                TitleIcon = _texturesService.GetTexture(@"textures\choya_corner_bg.png", "choya_corner_bg"),
                SetLocalizedTitle = () => "Choya",
            };

            var staticChoyaContentPanel = new FlowPanel()
            {
                Parent = staticChoyaPanel,
                Location = new(5),
                WidthSizingMode = SizingMode.Fill,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new(3, 3),
                CanScroll = true,
                Height = 500,
            };

            _ = new Checkbox()
            {
                Parent = behaviorContentPanel,
                SetLocalizedText = () => strings.ShowRandomly,
                SetLocalizedTooltip = () => strings.ShowRandomly_ttp,
                Checked = _settings.ShowRandomly.Value,
                CheckedChangedAction = (b) => _settings.ShowRandomly.Value = b,
            };

            _ = new Checkbox()
            {
                Parent = behaviorContentPanel,
                SetLocalizedText = () => strings.ShowWhenIdle,
                SetLocalizedTooltip = () => strings.ShowWhenIdle_ttp,
                Checked = _settings.ShowWhenIdle.Value,
                CheckedChangedAction = (b) => _settings.ShowWhenIdle.Value = b,
            };

            _ = new Checkbox()
            {
                Parent = behaviorContentPanel,
                SetLocalizedText = () => strings.ShowWhenStandingStill,
                SetLocalizedTooltip = () => strings.ShowWhenStandingStill_ttp,
                Checked = _settings.ShowWhenStandingStill.Value,
                CheckedChangedAction = (b) => _settings.ShowWhenStandingStill.Value = b,
            };

            _ = new Checkbox()
            {
                Parent = behaviorContentPanel,
                SetLocalizedText = () => strings.AvoidCombat,
                SetLocalizedTooltip = () => strings.AvoidCombat_ttp,
                Checked = _settings.AvoidCombat.Value,
                CheckedChangedAction = (b) => _settings.AvoidCombat.Value = b,
            };

            var idleDelay = CreateNumberSetting(delayContentPanel, () => string.Format(strings.IdleDelay, _settings.IdleDelay.Value), () => strings.IdleDelay_ttp);
            idleDelay.Item2.Value = _settings.IdleDelay.Value;
            idleDelay.Item2.MaxValue = 3600;
            idleDelay.Item2.MinValue = 0;
            idleDelay.Item2.ValueChangedAction = (v) =>
            {
                _settings.IdleDelay.Value = v;
                idleDelay.Item1.UserLocale_SettingChanged(null, null);
                idleDelay.Item2.UserLocale_SettingChanged(null, null);
            };

            var noMoveDelay = CreateNumberSetting(delayContentPanel, () => string.Format(strings.NoMoveDelay, _settings.NoMoveDelay.Value), () => strings.NoMoveDelay_ttp);
            noMoveDelay.Item2.Value = _settings.NoMoveDelay.Value;
            noMoveDelay.Item2.MaxValue = 3600;
            noMoveDelay.Item2.MinValue = 0;
            noMoveDelay.Item2.ValueChangedAction = (v) =>
            {
                _settings.NoMoveDelay.Value = v;
                noMoveDelay.Item2.UserLocale_SettingChanged(null, null);
                noMoveDelay.Item1.UserLocale_SettingChanged(null, null);
            };

            _ = new Checkbox()
            {
                Parent = appearanceContentPanel,
                SetLocalizedText = () => string.Format(strings_common.ShowCornerIcon, Name),
                SetLocalizedTooltip = () => strings_common.ShowCornerIcon_ttp,
                Checked = _settings.ShowCornerIcon.Value,
                CheckedChangedAction = (b) => _settings.ShowCornerIcon.Value = b,
            };

            var choyaDelay = CreateRangeSetting(appearanceContentPanel, () => string.Format(strings.ChoyaDelay, _settings.ChoyaDelay.Value.Start, _settings.ChoyaDelay.Value.End), () => strings.ChoyaDelay_ttp);
            choyaDelay.Item2.Value = _settings.ChoyaDelay.Value.Start;
            choyaDelay.Item3.Value = _settings.ChoyaDelay.Value.End;
            choyaDelay.Item2.ValueChangedAction = (v) =>
            {
                _settings.ChoyaDelay.Value = new(v, _settings.ChoyaDelay.Value.End);
                choyaDelay.Item1.UserLocale_SettingChanged(null, null);
            };
            choyaDelay.Item3.ValueChangedAction = (v) =>
            {
                _settings.ChoyaDelay.Value = new(_settings.ChoyaDelay.Value.Start, v);
                choyaDelay.Item1.UserLocale_SettingChanged(null, null);
            };

            var choyaIdleDelay = CreateRangeSetting(appearanceContentPanel, () => string.Format(strings.ChoyaIdleDelay, _settings.ChoyaIdleDelay.Value.Start, _settings.ChoyaIdleDelay.Value.End), () => strings.ChoyaIdleDelay_ttp);
            choyaIdleDelay.Item2.Value = _settings.ChoyaIdleDelay.Value.Start;
            choyaIdleDelay.Item3.Value = _settings.ChoyaIdleDelay.Value.End;
            choyaIdleDelay.Item2.ValueChangedAction = (v) =>
            {
                _settings.ChoyaIdleDelay.Value = new(v, _settings.ChoyaIdleDelay.Value.End);
                choyaIdleDelay.Item1.UserLocale_SettingChanged(null, null);
            };
            choyaIdleDelay.Item3.ValueChangedAction = (v) =>
            {
                _settings.ChoyaIdleDelay.Value = new(_settings.ChoyaIdleDelay.Value.Start, v);
                choyaIdleDelay.Item1.UserLocale_SettingChanged(null, null);
            };

            var choyaSpeed = CreateRangeSetting(appearanceContentPanel, () => string.Format(strings.ChoyaSpeed, _settings.ChoyaSpeed.Value.Start, _settings.ChoyaSpeed.Value.End), () => strings.ChoyaSpeed_ttp);
            choyaSpeed.Item2.Value = _settings.ChoyaSpeed.Value.Start;
            choyaSpeed.Item3.Value = _settings.ChoyaSpeed.Value.End;
            choyaSpeed.Item2.ValueChangedAction = (v) =>
            {
                _settings.ChoyaSpeed.Value = new(v, _settings.ChoyaSpeed.Value.End);
                choyaSpeed.Item1.UserLocale_SettingChanged(null, null);
            };
            choyaSpeed.Item3.ValueChangedAction = (v) =>
            {
                _settings.ChoyaSpeed.Value = new(_settings.ChoyaSpeed.Value.Start, v);
                choyaSpeed.Item1.UserLocale_SettingChanged(null, null);
            };

            var choyaTravelDistance = CreateRangeSetting(appearanceContentPanel, () => string.Format(strings.ChoyaTravelDistance, _settings.ChoyaTravelDistance.Value.Start, _settings.ChoyaTravelDistance.Value.End), () => strings.ChoyaTravelDistance_ttp);
            choyaTravelDistance.Item2.Value = _settings.ChoyaTravelDistance.Value.Start;
            choyaTravelDistance.Item3.Value = _settings.ChoyaTravelDistance.Value.End;
            choyaTravelDistance.Item2.ValueChangedAction = (v) =>
            {
                _settings.ChoyaTravelDistance.Value = new(v, _settings.ChoyaTravelDistance.Value.End);
                choyaTravelDistance.Item1.UserLocale_SettingChanged(null, null);
            };
            choyaTravelDistance.Item3.ValueChangedAction = (v) =>
            {
                _settings.ChoyaTravelDistance.Value = new(_settings.ChoyaTravelDistance.Value.Start, v);
                choyaTravelDistance.Item1.UserLocale_SettingChanged(null, null);
            };

            var choyaSize = CreateRangeSetting(appearanceContentPanel, () => string.Format(strings.ChoyaSize, _settings.ChoyaSize.Value.Start, _settings.ChoyaSize.Value.End), () => strings.ChoyaSize_ttp);
            choyaSize.Item2.Value = _settings.ChoyaSize.Value.Start;
            choyaSize.Item3.Value = _settings.ChoyaSize.Value.End;
            choyaSize.Item2.ValueChangedAction = (v) =>
            {
                _settings.ChoyaSize.Value = new(v, _settings.ChoyaSize.Value.End);
                choyaSize.Item1.UserLocale_SettingChanged(null, null);
            };
            choyaSize.Item3.ValueChangedAction = (v) =>
            {
                _settings.ChoyaSize.Value = new(_settings.ChoyaSize.Value.Start, v);
                choyaSize.Item1.UserLocale_SettingChanged(null, null);
            };

            //Keybinds 
            _ = new KeybindingAssigner()
            {
                Parent = keybindContentPanel,
                Width = ContentRegion.Width - 35,
                KeyBinding = _settings.SpawnChoyaKey.Value,
                KeybindChangedAction = (kb) =>
                {
                    _settings.SpawnChoyaKey.Value = new()
                    {
                        ModifierKeys = kb.ModifierKeys,
                        PrimaryKey = kb.PrimaryKey,
                        Enabled = kb.Enabled,
                        IgnoreWhenInTextField = true,
                    };
                },
                SetLocalizedKeyBindingName = () => strings.SpawnChoyaKey,
                SetLocalizedTooltip = () => strings.SpawnChoyaKey_ttp,
            };

            _ = new KeybindingAssigner()
            {
                Parent = keybindContentPanel,
                Width = ContentRegion.Width - 35,
                KeyBinding = _settings.ToggleChoyaHunt.Value,
                KeybindChangedAction = (kb) =>
                {
                    _settings.ToggleChoyaHunt.Value = new()
                    {
                        ModifierKeys = kb.ModifierKeys,
                        PrimaryKey = kb.PrimaryKey,
                        Enabled = kb.Enabled,
                        IgnoreWhenInTextField = true,
                    };
                },
                SetLocalizedKeyBindingName = () => strings.ToggleChoyaHunt,
                SetLocalizedTooltip = () => strings.ToggleChoyaHunt_ttp,
            };

            _ = new Button()
            {
                Parent = staticChoyaContentPanel,
                SetLocalizedText = () => strings.CreateStaticChoya,
                SetLocalizedTooltip = () => strings.CreateStaticChoya,
                Width = ContentRegion.Width - 35,
                ClickAction = () =>
                {
                    var c = new Choya();
                    c.Initialize(_settings.StaticChoya, _texturesService.GetTexture(textures_common.RollingChoya, nameof(textures_common.RollingChoya)), staticChoyaContentPanel);
                    c.ToggleEdit();
                },
            };

            foreach (SettingEntry<Choya> choya in _settings.StaticChoya)
            {
                choya.Value.Initialize(_settings.StaticChoya, _texturesService.GetTexture(textures_common.RollingChoya, nameof(textures_common.RollingChoya)), staticChoyaContentPanel);
            }
        }

        private (Label, NumberBox, NumberBox) CreateRangeSetting(Container parent, Func<string> setLocalizedText, Func<string> setLocalizedTooltip)
        {
            var p = new Panel()
            {
                Parent = parent,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
            };

            var label = new Label()
            {
                Parent = p,
                Width = 225,
                Height = 20,
                SetLocalizedText = setLocalizedText,
                SetLocalizedTooltip = setLocalizedTooltip,
            };

            var min = new NumberBox()
            {
                Location = new(250, 0),
                Width = 125,
                Parent = p,
                MinValue = 0,
                SetLocalizedTooltip = setLocalizedTooltip,
            };

            var max = new NumberBox()
            {
                Location = new(min.Right + 5, 0),
                Parent = p,
                Width = 125,
                MinValue = 0,
                SetLocalizedTooltip = setLocalizedTooltip,
            };

            return (label, min, max);
        }

        private (Label, NumberBox) CreateNumberSetting(Container parent, Func<string> setLocalizedText, Func<string> setLocalizedTooltip)
        {
            var p = new Panel()
            {
                Parent = parent,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
            };

            var label = new Label()
            {
                Parent = p,
                Width = 225,
                Height = 20,
                SetLocalizedText = setLocalizedText,
                SetLocalizedTooltip = setLocalizedTooltip,
            };

            var num = new NumberBox()
            {
                Location = new(250, 0),
                Width = 125,
                Parent = p,
                MinValue = 0,
                SetLocalizedTooltip = setLocalizedTooltip,
            };

            return (label, num);
        }
    }
}
