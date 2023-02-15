using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Modules;
using Blish_HUD.Settings;
using Gw2Sharp.WebApi;
using ItemWeaponType = Gw2Sharp.WebApi.V2.Models.ItemWeaponType;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.BuildsManager.Services;
using Kenedia.Modules.BuildsManager.Views;
using Kenedia.Modules.Core.Models;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Threading.Tasks;
using static Kenedia.Modules.BuildsManager.DataModels.Professions.Weapon;

namespace Kenedia.Modules.BuildsManager
{
    [Export(typeof(Module))]
    public class BuildsManager : BaseModule<BuildsManager, MainWindow, Settings>
    {
        private double _tick;

        [ImportingConstructor]
        public BuildsManager([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters)
        {
            ModuleInstance = this;
            HasGUI = true;
        }

        private GW2API GW2API { get; set; }

        private Template SelectedTemplate { get; set; }

        public static Data Data { get; set; }

        protected override void DefineSettings(SettingCollection settings)
        {
            base.DefineSettings(settings);

        }

        protected override void Initialize()
        {
            base.Initialize();

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
            };

            Logger.Info($"Starting {Name} v." + Version.BaseVersion());
            Data = new(ContentsManager, Paths);
        }

        protected override async void OnLocaleChanged(object sender, ValueChangedEventArgs<Locale> e)
        {
            if (e.NewValue is not Locale.Korean and not Locale.Chinese)
            {
                GW2API.Cancel();

                if (!Data.Professions.TryGetValue(Gw2Sharp.Models.ProfessionType.Guardian, out Profession profession) || !profession.Names.TryGetValue(e.NewValue, out string name) || string.IsNullOrEmpty(name))
                {
                    Logger.Info($"No data for {e.NewValue} loaded yet. Fetching new data from the API.");
                    await GW2API.UpdateData();

                    if (!GW2API.IsCanceled())
                    {
                        Logger.Info($"Apply fresh {e.NewValue} data to the UI.");
                    }
                }
                else
                {
                    Logger.Info($"Apply {e.NewValue} data to the UI.");
                }

                base.OnLocaleChanged(sender, e);
            }
        }

        protected override async Task LoadAsync()
        {
            await base.LoadAsync();

            await Data.Load();
            GW2API = new(Gw2ApiManager, Data)
            {
                Paths = Paths,
            };

            if (GameService.Overlay.UserLocale.Value is not Locale.Korean and not Locale.Chinese)
            {
                if (!Data.Professions.TryGetValue(Gw2Sharp.Models.ProfessionType.Guardian, out Profession profession) || !profession.Names.TryGetValue(GameService.Overlay.UserLocale.Value, out string name) || string.IsNullOrEmpty(name))
                {
                    Logger.Info($"No data for {GameService.Overlay.UserLocale.Value} loaded yet. Fetching new data from the API.");
                    OnLocaleChanged(this, new(Locale.Chinese, GameService.Overlay.UserLocale.Value));
                }
            }
        }

        protected override void OnModuleLoaded(EventArgs e)
        {
            base.OnModuleLoaded(e);
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (gameTime.TotalGameTime.TotalMilliseconds - _tick > 500)
            {
                _tick = gameTime.TotalGameTime.TotalMilliseconds;

            }
        }

        protected override async void ReloadKey_Activated(object sender, EventArgs e)
        {
            base.ReloadKey_Activated(sender, e);
            //await GW2API.UpdateData();

            SetDummyTemplate();
        }

        protected override void LoadGUI()
        {
            base.LoadGUI();
            int Height = 670;
            int Width = 915;

            Logger.Info($"Building UI for {Name}");

            MainWindow = new MainWindow(
                Services.TexturesService.GetTexture(@"textures\mainwindow_background.png", "mainwindow_background"),
                new Rectangle(30, 30, Width, Height + 30),
                new Rectangle(30, 20, Width - 3, Height + 25),
                Data)
            {
                Parent = GameService.Graphics.SpriteScreen,
                Title = "❤",
                Subtitle = "❤",
                SavesPosition = true,
                Id = $"{Name} MainWindow",
                MainWindowEmblem = AsyncTexture2D.FromAssetId(156020),
                Name = Name,
                Version = ModuleVersion,
                Template = SelectedTemplate,
                Width = 1120,
                Height = 900,
            };

            MainWindow.Show();
        }

        protected override void UnloadGUI()
        {
            base.UnloadGUI();

            MainWindow?.Dispose();
        }

        protected override void Unload()
        {
            base.Unload();
        }

        private void SetDummyTemplate()
        {
            var gear = new GearTemplate();
            gear.Gear[GearSlot.Head].Stat = DataModels.Stats.EquipmentStat.Berserkers;
            gear.Gear[GearSlot.Head].Upgrades[UpgradeSlot.Rune] = 24836;
            gear.Gear[GearSlot.Head].Upgrades[UpgradeSlot.Infusion_1] = 37131;

            gear.Gear[GearSlot.Shoulder].Stat = DataModels.Stats.EquipmentStat.Berserkers;
            gear.Gear[GearSlot.Shoulder].Upgrades[UpgradeSlot.Rune] = 24836;
            gear.Gear[GearSlot.Shoulder].Upgrades[UpgradeSlot.Infusion_1] = 37131;

            gear.Gear[GearSlot.Chest].Stat = DataModels.Stats.EquipmentStat.Berserkers;
            gear.Gear[GearSlot.Chest].Upgrades[UpgradeSlot.Rune] = 24836;
            gear.Gear[GearSlot.Chest].Upgrades[UpgradeSlot.Infusion_1] = 37131;

            gear.Gear[GearSlot.Hand].Stat = DataModels.Stats.EquipmentStat.Berserkers;
            gear.Gear[GearSlot.Hand].Upgrades[UpgradeSlot.Rune] = 24836;
            gear.Gear[GearSlot.Hand].Upgrades[UpgradeSlot.Infusion_1] = 37131;

            gear.Gear[GearSlot.Leg].Stat = DataModels.Stats.EquipmentStat.Berserkers;
            gear.Gear[GearSlot.Leg].Upgrades[UpgradeSlot.Rune] = 24836;
            gear.Gear[GearSlot.Leg].Upgrades[UpgradeSlot.Infusion_1] = 37131;

            gear.Gear[GearSlot.Foot].Stat = DataModels.Stats.EquipmentStat.Berserkers;
            gear.Gear[GearSlot.Foot].Upgrades[UpgradeSlot.Rune] = 24836;
            gear.Gear[GearSlot.Foot].Upgrades[UpgradeSlot.Infusion_1] = 37131;

            gear.Gear[GearSlot.MainHand].WeaponType = WeaponType.Axe;
            gear.Gear[GearSlot.MainHand].Stat = DataModels.Stats.EquipmentStat.Berserkers;
            gear.Gear[GearSlot.MainHand].Upgrades[UpgradeSlot.Sigil_1] = 24615;
            gear.Gear[GearSlot.MainHand].Upgrades[UpgradeSlot.Infusion_1] = 37131;
            gear.Gear[GearSlot.OffHand].WeaponType = WeaponType.Axe;
            gear.Gear[GearSlot.OffHand].Stat = DataModels.Stats.EquipmentStat.Berserkers;
            gear.Gear[GearSlot.OffHand].Upgrades[UpgradeSlot.Sigil_1] = 24615;
            gear.Gear[GearSlot.OffHand].Upgrades[UpgradeSlot.Infusion_1] = 37131;
            gear.Gear[GearSlot.Aquatic].WeaponType = WeaponType.Harpoon;
            gear.Gear[GearSlot.Aquatic].Stat = DataModels.Stats.EquipmentStat.Berserkers;
            gear.Gear[GearSlot.Aquatic].Upgrades[UpgradeSlot.Sigil_1] = 24615;
            gear.Gear[GearSlot.Aquatic].Upgrades[UpgradeSlot.Sigil_2] = 24618;
            gear.Gear[GearSlot.Aquatic].Upgrades[UpgradeSlot.Infusion_1] = 37131;
            gear.Gear[GearSlot.Aquatic].Upgrades[UpgradeSlot.Infusion_2] = 37131;

            gear.Gear[GearSlot.AltMainHand].WeaponType = WeaponType.Greatsword;
            gear.Gear[GearSlot.AltMainHand].Stat = DataModels.Stats.EquipmentStat.Berserkers;
            gear.Gear[GearSlot.AltMainHand].Upgrades[UpgradeSlot.Sigil_1] = 24615;
            gear.Gear[GearSlot.AltMainHand].Upgrades[UpgradeSlot.Infusion_1] = 37131;
            gear.Gear[GearSlot.AltOffHand].WeaponType = WeaponType.Greatsword;
            gear.Gear[GearSlot.AltOffHand].Stat = DataModels.Stats.EquipmentStat.Berserkers;
            gear.Gear[GearSlot.AltOffHand].Upgrades[UpgradeSlot.Sigil_1] = 24615;
            gear.Gear[GearSlot.AltOffHand].Upgrades[UpgradeSlot.Infusion_1] = 37131;
            gear.Gear[GearSlot.AltAquatic].WeaponType = WeaponType.Speargun;
            gear.Gear[GearSlot.AltAquatic].Stat = DataModels.Stats.EquipmentStat.Berserkers;
            gear.Gear[GearSlot.AltAquatic].Upgrades[UpgradeSlot.Sigil_1] = 24615;
            gear.Gear[GearSlot.AltAquatic].Upgrades[UpgradeSlot.Sigil_2] = 24618;
            gear.Gear[GearSlot.AltAquatic].Upgrades[UpgradeSlot.Infusion_1] = 37131;
            gear.Gear[GearSlot.AltAquatic].Upgrades[UpgradeSlot.Infusion_2] = 37131;

            gear.Gear[GearSlot.Back].Stat = DataModels.Stats.EquipmentStat.Berserkers;
            gear.Gear[GearSlot.Back].Upgrades[UpgradeSlot.Infusion_1] = 37131;
            gear.Gear[GearSlot.Back].Upgrades[UpgradeSlot.Infusion_2] = 37131;
            gear.Gear[GearSlot.Amulet].Stat = DataModels.Stats.EquipmentStat.Berserkers;
            gear.Gear[GearSlot.Ring_1].Stat = DataModels.Stats.EquipmentStat.Berserkers;
            gear.Gear[GearSlot.Ring_1].Upgrades[UpgradeSlot.Infusion_1] = 37131;
            gear.Gear[GearSlot.Ring_1].Upgrades[UpgradeSlot.Infusion_2] = 37131;
            gear.Gear[GearSlot.Ring_1].Upgrades[UpgradeSlot.Infusion_3] = 37131;
            gear.Gear[GearSlot.Ring_2].Stat = DataModels.Stats.EquipmentStat.Berserkers;
            gear.Gear[GearSlot.Ring_2].Upgrades[UpgradeSlot.Infusion_1] = 37131;
            gear.Gear[GearSlot.Ring_2].Upgrades[UpgradeSlot.Infusion_2] = 37131;
            gear.Gear[GearSlot.Ring_2].Upgrades[UpgradeSlot.Infusion_3] = 37131;
            gear.Gear[GearSlot.Accessory_1].Stat = DataModels.Stats.EquipmentStat.Berserkers;
            gear.Gear[GearSlot.Accessory_1].Upgrades[UpgradeSlot.Infusion_1] = 37131;
            gear.Gear[GearSlot.Accessory_2].Stat = DataModels.Stats.EquipmentStat.Berserkers;
            gear.Gear[GearSlot.Accessory_2].Upgrades[UpgradeSlot.Infusion_1] = 37131;

            string code = "[&DQQePSA/Nzp5AHgANBaaAPoWpQGsAawBLhbtABE7BwEAAAAAAAAAAAAAAAA=]";
            var build = new BuildTemplate(code);

            SelectedTemplate = new Template()
            {
                BuildTemplate = build,
                GearTemplate = gear,
            };

            MainWindow.Template = SelectedTemplate;
        }
    }
}