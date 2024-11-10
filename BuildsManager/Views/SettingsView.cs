using Blish_HUD.Graphics.UI;
using Kenedia.Modules.BuildsManager.Res;
using Kenedia.Modules.BuildsManager.Services;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Res;

namespace Kenedia.Modules.BuildsManager.Views
{
    public class SettingsView : View
    {

        public SettingsView(Settings settings)
        {
            Settings = settings;
        }

        public Settings Settings { get; }

        protected override void Build(Blish_HUD.Controls.Container buildPanel)
        {
            base.Build(buildPanel);
            var p = new FlowPanel()
            {
                Parent = buildPanel,
                Location = new(50, 0),
                WidthSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                HeightSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                ContentPadding = new(10, 10),
                ControlPadding = new(10, 5),
                FlowDirection = Blish_HUD.Controls.ControlFlowDirection.TopToBottom,
            };

            var fp = new FlowPanel()
            {
                Title = "General",
                Parent = p,
                Location = new(50, 0),
                WidthSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                HeightSizingMode = Blish_HUD.Controls.SizingMode.AutoSize,
                AutoSizePadding = new(0, 20),
                ContentPadding = new(10, 10),
                ControlPadding = new(10, 5),
                FlowDirection = Blish_HUD.Controls.ControlFlowDirection.TopToBottom,
            };

            _ = new Checkbox()
            {
                Parent = fp,
                Checked = Settings.AutoSetFilterProfession.Value,
                CheckedChangedAction = (b) => Settings.AutoSetFilterProfession.Value = b,
                SetLocalizedText = () => strings.AutoSetProfession_Name,
                SetLocalizedTooltip = () => strings.AutoSetProfession_Tooltip,
            };

            _ = new Checkbox()
            {
                Parent = fp,
                Checked = Settings.AutoSetFilterSpecialization.Value,
                CheckedChangedAction = (b) => Settings.AutoSetFilterSpecialization.Value = b,
                SetLocalizedText = () => strings.AutoSetFilterSpecialization_Name,
                SetLocalizedTooltip = () => strings.AutoSetFilterSpecialization_Tooltip,
            };

            _ = new Checkbox()
            {
                Parent = fp,
                Checked = Settings.ShowCornerIcon.Value,
                CheckedChangedAction = (b) => Settings.ShowCornerIcon.Value = b,
                SetLocalizedText = () => string.Format(strings_common.ShowCornerIcon, BuildsManager.ModuleName),
                SetLocalizedTooltip = () => strings_common.ShowCornerIcon_ttp,
            };

            _ = new Checkbox()
            {
                Parent = fp,
                Checked = Settings.RequireVisibleTemplate.Value,
                CheckedChangedAction = (b) => Settings.RequireVisibleTemplate.Value = b,
                SetLocalizedText = () => string.Format(strings.RequireVisibleTemplate, BuildsManager.ModuleName),
                SetLocalizedTooltip = () => strings.RequireVisibleTemplate_Tooltip,
            };

           var setFilterOnTemplateCreate = new Checkbox()
            {
                Parent = fp,
                Checked = Settings.SetFilterOnTemplateCreate.Value,                
                SetLocalizedText = () => string.Format(strings.SetFilterOnTemplateCreate, BuildsManager.ModuleName),
                SetLocalizedTooltip = () => strings.SetFilterOnTemplateCreate_Tooltip,
            };

            var resetFilterOnTemplateCreate = new Checkbox()
            {
                Parent = fp,
                Checked = Settings.ResetFilterOnTemplateCreate.Value,
                SetLocalizedText = () => string.Format(strings.ResetFilterOnTemplateCreate, BuildsManager.ModuleName),
                SetLocalizedTooltip = () => strings.ResetFilterOnTemplateCreate_Tooltip,
            };

            setFilterOnTemplateCreate.CheckedChangedAction = (b) =>
            {
                Settings.SetFilterOnTemplateCreate.Value = b;

                if (Settings.ResetFilterOnTemplateCreate.Value && b)
                {
                    resetFilterOnTemplateCreate.Checked = false;
                }
            };
            resetFilterOnTemplateCreate.CheckedChangedAction = (b) =>
            {
                Settings.ResetFilterOnTemplateCreate.Value = b;

                if (Settings.SetFilterOnTemplateCreate.Value && b)
                {
                    setFilterOnTemplateCreate.Checked = false;
                }
            };

            _ = new KeybindingAssigner()
            {
                Parent = fp,
                Width = 300,
                KeyBinding = Settings.ToggleWindowKey.Value,
                KeybindChangedAction = (kb) =>
                {
                    Settings.ToggleWindowKey.Value = new()
                    {
                        ModifierKeys = kb.ModifierKeys,
                        PrimaryKey = kb.PrimaryKey,
                        Enabled = kb.Enabled,
                        IgnoreWhenInTextField = true,
                    };
                },
                SetLocalizedKeyBindingName = () => string.Format(strings_common.ToggleItem, BuildsManager.ModuleName),
            };

            fp = new FlowPanel()
            {
                Title = "Quick Filter Panel",
                Parent = p,
                WidthSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                HeightSizingMode = Blish_HUD.Controls.SizingMode.AutoSize,
                AutoSizePadding = new(0, 20),
                ContentPadding = new(10, 10),
                ControlPadding = new(10, 5),
                FlowDirection = Blish_HUD.Controls.ControlFlowDirection.TopToBottom,
            };

            _ = new Checkbox()
            {
                Parent = fp,
                Checked = Settings.QuickFiltersPanelFade.Value,
                CheckedChangedAction = (b) => Settings.QuickFiltersPanelFade.Value = b,
                SetLocalizedText = () => strings.FadeQuickFiltersPanel_Name,
                SetLocalizedTooltip = () => strings.FadeQuickFiltersPanel_Tooltip,
            };

            var subP = new Panel()
            {
                Parent = fp,
                WidthSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                HeightSizingMode = Blish_HUD.Controls.SizingMode.AutoSize,
            };

            var fadeDelayLabel = new Label()
            {
                Parent = subP,
                AutoSizeWidth = true,
                Height = 20,
                SetLocalizedText = () => string.Format(strings.QuickFiltersPanelFadeDelay_Name, Settings.QuickFiltersPanelFadeDelay.Value),
                SetLocalizedTooltip = () => strings.QuickFiltersPanelFadeDelay_Tooltip,
            };

            _ = new TrackBar()
            {
                Location = new(225, 2),
                Parent = subP,
                MinValue = 0,
                MaxValue = 240,
                SmallStep  = false,                
                Value = (float)(Settings.QuickFiltersPanelFadeDelay.Value / 0.25),
                Width = 300,
                ValueChangedAction = (num) =>
                {
                    Settings.QuickFiltersPanelFadeDelay.Value = num * 0.25;
                    fadeDelayLabel.UserLocale_SettingChanged(null, null);
                },
            };

            subP = new Panel()
            {
                Parent = fp,
                WidthSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                HeightSizingMode = Blish_HUD.Controls.SizingMode.AutoSize,
            };

            var fadeDurationLabel = new Label()
            {
                Parent = subP,
                AutoSizeWidth = true,
                Height = 20,
                SetLocalizedText = () => string.Format(strings.QuickFiltersPanelFadeDuration_Name, Settings.QuickFiltersPanelFadeDuration.Value),
                SetLocalizedTooltip = () => strings.QuickFiltersPanelFadeDuration_Tooltip,
            };

            _ = new TrackBar()
            {
                Location = new(225, 2),
                Parent = subP,
                MinValue = 0,
                MaxValue = 100,
                Value = (float)(Settings.QuickFiltersPanelFadeDuration.Value / 25),
                Width = 300,
                ValueChangedAction = (num) =>
                {
                    Settings.QuickFiltersPanelFadeDuration.Value = num * 25;
                    fadeDurationLabel.UserLocale_SettingChanged(null, null);
                },
            };

            _ = new Dummy()
            {
                Height = 10,
                Parent = fp
            };

            _ = new Checkbox()
            {
                Parent = fp,
                Checked = Settings.ShowQuickFilterPanelOnWindowOpen.Value,
                CheckedChangedAction = (b) => Settings.ShowQuickFilterPanelOnWindowOpen.Value = b,
                SetLocalizedText = () => strings.ShowQuickFilterPanelOnWindowOpen_Name,
                SetLocalizedTooltip = () => strings.ShowQuickFilterPanelOnWindowOpen_Tooltip,
            };

            _ = new Checkbox()
            {
                Parent = fp,
                Checked = Settings.ShowQuickFilterPanelOnTabOpen.Value,
                CheckedChangedAction = (b) => Settings.ShowQuickFilterPanelOnTabOpen.Value = b,
                SetLocalizedText = () => strings.ShowQuickFilterPanelOnTabOpen_Name,
                SetLocalizedTooltip = () => strings.ShowQuickFilterPanelOnTabOpen_Tooltip,
            };
        }
    }
}
