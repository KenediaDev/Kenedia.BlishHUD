using Blish_HUD.Input;
using Blish_HUD.Settings;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Res;
using Microsoft.Xna.Framework.Input;
using Kenedia.Modules.BuildsManager.Res;
using Kenedia.Modules.Core.Utility;
using System;
using Kenedia.Modules.BuildsManager.Models;

namespace Kenedia.Modules.BuildsManager.Services
{
    public class Settings : BaseSettingsModel
    {
        private SettingCollection _settingCollection;

        public Settings()
        {

        }

        private void InitializeSettings(SettingCollection settings)
        {
            SettingCollection = settings;
            SettingCollection internalSettings = settings.AddSubCollection("Internal", false, false);

            SortBehavior = internalSettings.DefineSetting(nameof(SortBehavior), TemplateSortBehavior.ByProfession);
            ShowQuickFilterPanelOnWindowOpen = internalSettings.DefineSetting(nameof(ShowQuickFilterPanelOnWindowOpen), false);
            ShowQuickFilterPanelOnTabOpen = internalSettings.DefineSetting(nameof(ShowQuickFilterPanelOnTabOpen), true);

            ShowCornerIcon = internalSettings.DefineSetting(nameof(ShowCornerIcon), true);
            QuickFiltersPanelFade = internalSettings.DefineSetting(nameof(QuickFiltersPanelFade), true);
            QuickFiltersPanelFadeDuration = internalSettings.DefineSetting(nameof(QuickFiltersPanelFadeDuration), 1000.00);
            QuickFiltersPanelFadeDelay = internalSettings.DefineSetting(nameof(QuickFiltersPanelFadeDelay), 5000.00);

            AutoSetFilterProfession = internalSettings.DefineSetting(nameof(AutoSetFilterProfession), false, () => strings.AutoSetProfession_Name, () => strings.AutoSetProfession_Tooltip);
            AutoSetFilterSpecialization = internalSettings.DefineSetting(nameof(AutoSetFilterSpecialization), false, () => strings.AutoSetFilterSpecialization_Name, () => strings.AutoSetFilterSpecialization_Tooltip);

            //Temporary
            ToggleWindowKey = internalSettings.DefineSetting(nameof(ToggleWindowKey), new KeyBinding(ModifierKeys.Shift, Keys.B),
                () => string.Format(strings_common.ToggleItem, BuildsManager.ModuleName),
                () => string.Format(strings_common.ToggleItem, BuildsManager.ModuleName));
        }

        public SettingEntry<TemplateSortBehavior> SortBehavior{ get; set; }

        public SettingEntry<bool> ShowCornerIcon { get; set; }

        public SettingEntry<bool> ShowQuickFilterPanelOnTabOpen { get; set; }

        public SettingEntry<bool> ShowQuickFilterPanelOnWindowOpen { get; set; }

        public SettingEntry<bool> AutoSetFilterProfession { get; set; }

        public SettingEntry<bool> AutoSetFilterSpecialization { get; private set; }

        public SettingEntry<double> QuickFiltersPanelFadeDelay { get; private set; }

        public SettingEntry<double> QuickFiltersPanelFadeDuration { get; private set; }

        public SettingEntry<KeyBinding> ToggleWindowKey { get; set; }

        public SettingCollection SettingCollection { get => _settingCollection; set => Common.SetProperty(ref _settingCollection, value, OnSettingCollectionChanged); }

        public SettingEntry<bool> QuickFiltersPanelFade { get; private set; }

        private void OnSettingCollectionChanged(object sender, ValueChangedEventArgs<SettingCollection> e)
        {
            if (e.NewValue == null)
            {
                return;
            }

            InitializeSettings(e.NewValue);
        }
    }
}
