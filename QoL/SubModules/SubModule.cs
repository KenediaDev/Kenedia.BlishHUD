using Blish_HUD.Input;
using Blish_HUD.Settings;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Services;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.QoL.Res;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using Kenedia.Modules.QoL.Views;
using Kenedia.Modules.QoL.Controls;
using System.Diagnostics;

namespace Kenedia.Modules.QoL.SubModules
{
    public abstract class SubModule
    {
        private readonly SettingCollection _settings;

        private bool _loaded;
        private bool _unloaded;
        private bool _enabled;
        private Func<string> _localizedName;
        private Func<string> _localizedDescription;

        protected SettingCollection Settings;
        protected SubModuleUI UI_Elements = new();

        public SubModule(SettingCollection settings)
        {
            _settings = settings;
            DefineSettings(_settings);

            Name = SubModuleType.ToString();

            Icon = new()
            {
                Texture = QoL.ModuleInstance.ContentsManager.GetTexture($@"textures\{SubModuleType}.png"),
                HoveredTexture = QoL.ModuleInstance.ContentsManager.GetTexture($@"textures\{SubModuleType}_Hovered.png"),
            };

            ToggleControl = new()
            {
                Icon = Icon,
                BasicTooltipText = SubModuleType.ToString(),
                Checked = EnabledSetting.Value,
                Size = new(32),
                Visible = EnabledSetting.Value,
                OnCheckChanged = (b) => Enabled = b,
                Module = this,
            };
        }

        public abstract SubModuleType SubModuleType { get; }

        public bool Enabled { get => _enabled; set => Common.SetProperty(ref _enabled, value, OnEnabledChanged); }

        public Func<string> LocalizedName { get => _localizedName; set => Common.SetProperty(ref _localizedName, value); }

        public Func<string> LocalizedDescription { get => _localizedDescription; set => Common.SetProperty(ref _localizedDescription, value); }

        public ModuleButton ToggleControl { get; }

        public DetailedTexture Icon { get; }

        public string Name { get; set; }

        public string Description { get; set; }

        public SettingEntry<bool> EnabledSetting { get; set; }

        public SettingEntry<bool> ShowInHotbar { get; set; }

        public SettingEntry<KeyBinding> HotKey { get; set; }

        public abstract void Update(GameTime gameTime);

        public abstract void CreateSettingsPanel(FlowPanel flowPanel, int width);

        private void OnEnabledChanged(object sender, ValueChangedEventArgs<bool> e)
        {

            (e.NewValue ? (Action)Enable : Disable)();
            EnabledSetting.Value = e.NewValue;

            if (ToggleControl is HotbarButton toggle)
            {
                toggle.Checked = Enabled;
                ToggleControl?.Parent?.RecalculateLayout();
            }
        }

        private void LocalizingService_LocaleChanged(object sender = null, EventArgs e = null)
        {
            SwitchLanguage();
        }

        protected virtual void Enable()
        {
            Enabled = true;
        }

        protected virtual void Disable()
        {
            Enabled = false;
        }

        protected virtual void SwitchLanguage()
        {
            Name = LocalizedName?.Invoke() ?? Name;
            Description = LocalizedDescription?.Invoke() ?? Description;
        }

        protected virtual void DefineSettings(SettingCollection settings)
        {
            Settings = settings.AddSubCollection($"{SubModuleType}", true);
            Settings.RenderInUi = false;

            EnabledSetting = Settings.DefineSetting(nameof(EnabledSetting), false);

            HotKey = Settings.DefineSetting(nameof(HotKey), new KeyBinding(Keys.None),
                () => string.Format(strings.HotkeyEntry_Name, $"{SubModuleType}"),
                () => string.Format(strings.HotkeyEntry_Description, $"{SubModuleType}"));

            ShowInHotbar = Settings.DefineSetting(nameof(ShowInHotbar), true,
                () => string.Format(strings.ShowInHotbar_Name, $"{SubModuleType}"),
                () => string.Format(strings.ShowInHotbar_Description, $"{SubModuleType}"));

            HotKey.Value.Enabled = true;
            HotKey.Value.Activated += HotKey_Activated;

            ShowInHotbar.SettingChanged += ShowInHotbar_SettingChanged;
        }

        private void ShowInHotbar_SettingChanged(object sender, Blish_HUD.ValueChangedEventArgs<bool> e)
        {
            if (ToggleControl is HotbarButton _)
            {
                if(ToggleControl?.Parent?.Parent is ModuleHotbar moduleHotbar)
                {
                    moduleHotbar.SetButtonsExpanded();
                    moduleHotbar.RecalculateLayout();
                }
            }
        }

        private void HotKey_Activated(object sender, EventArgs e)
        {
            Enabled = !Enabled;
        }

        public virtual void Load()
        {
            if (_loaded) return;
            _loaded = true;

            LocalizingService.LocaleChanged += LocalizingService_LocaleChanged;
            LocalizingService_LocaleChanged();

            Enabled = EnabledSetting.Value;
        }

        public virtual void Unload()
        {
            if (_unloaded) return;
            _unloaded = true;

            ToggleControl?.Dispose();
            UI_Elements.DisposeAll();
            HotKey.Value.Activated -= HotKey_Activated;
            LocalizingService.LocaleChanged -= LocalizingService_LocaleChanged;
        }
    }
}
