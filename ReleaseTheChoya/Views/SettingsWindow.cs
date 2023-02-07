using Blish_HUD.Content;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Views;
using Kenedia.Modules.ReleaseTheChoya.Models;
using Kenedia.Modules.ReleaseTheChoya.Res;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SizingMode = Blish_HUD.Controls.SizingMode;
using ControlFlowDirection = Blish_HUD.Controls.ControlFlowDirection;
using Container = Blish_HUD.Controls.Container;
using Kenedia.Modules.Core.Res;

namespace Kenedia.Modules.ReleaseTheChoya.Views
{
    public class SettingsWindow : BaseSettingsWindow
    {
        private readonly Settings _settings;

        public SettingsWindow(AsyncTexture2D background, Rectangle windowRegion, Rectangle contentRegion, Settings settings) : base(background, windowRegion, contentRegion)
        {
            _settings = settings;
            
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

            var idleDelay = LabeledTrackbar(delayContentPanel, () => string.Format(strings.IdleDelay, _settings.IdleDelay.Value), () => strings.IdleDelay_ttp);
            idleDelay.Item3.Value = _settings.IdleDelay.Value;
            idleDelay.Item3.MaxValue = 1800;
            idleDelay.Item3.MinValue = 0;
            idleDelay.Item3.ValueChangedAction = (v) =>
            {
                _settings.IdleDelay.Value = v;
                idleDelay.Item2.UserLocale_SettingChanged(null, null);
                idleDelay.Item3.UserLocale_SettingChanged(null, null);
            };

            var noMoveDelay = LabeledTrackbar(delayContentPanel, () => string.Format(strings.NoMoveDelay, _settings.NoMoveDelay.Value), () => strings.NoMoveDelay_ttp);
            noMoveDelay.Item3.Value = _settings.NoMoveDelay.Value;
            noMoveDelay.Item3.MaxValue = 300;
            noMoveDelay.Item3.MinValue = 0;
            noMoveDelay.Item3.ValueChangedAction = (v) =>
            {
                _settings.NoMoveDelay.Value = v;
                noMoveDelay.Item3.UserLocale_SettingChanged(null, null);
                noMoveDelay.Item2.UserLocale_SettingChanged(null, null);
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
    }
}
