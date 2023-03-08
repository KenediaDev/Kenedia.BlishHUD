using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Modules;
using Blish_HUD.Settings;
using Gw2Sharp.WebApi;
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
using Microsoft.Xna.Framework.Graphics;
using Gw2Sharp.WebApi.V2.Models;
using BuildTemplate = Kenedia.Modules.BuildsManager.Models.Templates.BuildTemplate;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

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

        private Template SelectedTemplate { get; set; } = new();

        public static Data Data { get; set; }

        public SkillConnectionEditor SkillConnectionEditor { get; set; }

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

                if (!Data.Professions.TryGetValue(Gw2Sharp.Models.ProfessionType.Guardian, out DataModels.Professions.Profession profession) || !profession.Names.TryGetValue(e.NewValue, out string name) || string.IsNullOrEmpty(name))
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
            GW2API = new(Gw2ApiManager, Data, Paths)
            {
                Paths = Paths,
            };

            if(Data.BaseSkills.Count == 0)
            {
                await GW2API.FetchBaseSkills();
            }

            if (GameService.Overlay.UserLocale.Value is not Locale.Korean and not Locale.Chinese)
            {
                if (!Data.Professions.TryGetValue(Gw2Sharp.Models.ProfessionType.Guardian, out DataModels.Professions.Profession profession) || !profession.Names.TryGetValue(GameService.Overlay.UserLocale.Value, out string name) || string.IsNullOrEmpty(name))
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
            //await GW2API.FetchBaseSkills();

            await Data.LoadBaseSkills();
            await Data.LoadConnections();

            base.ReloadKey_Activated(sender, e);

            //await GW2API.UpdateData();

            //await GW2API.GetSkillConnections();
            SetDummyTemplate();
        }

        protected override void LoadGUI()
        {
            if (!Data.IsLoaded) return;

            base.LoadGUI();
            int Height = 670;
            int Width = 915;

            Logger.Info($"Building UI for {Name}");

            if (true)
            {
                MainWindow = new MainWindow(
                    Services.TexturesService.GetTexture(@"textures\mainwindow_background.png", "mainwindow_background"),
                    new Rectangle(30, 30, Width, Height + 30),
                    new Rectangle(30, 20, Width - 3, Height + 15),
                    Data,
                    Services.TexturesService)
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

            if (false)
            {
                SkillConnectionEditor?.Dispose();

                var settingsBg = AsyncTexture2D.FromAssetId(155997);
                Texture2D cutSettingsBg = settingsBg.Texture.GetRegion(0, 0, settingsBg.Width - 482, settingsBg.Height - 390);

                SkillConnectionEditor = new(
                settingsBg,
                    new Rectangle(30, 30, cutSettingsBg.Width + 10, cutSettingsBg.Height),
                    new Rectangle(30, 35, cutSettingsBg.Width - 5, cutSettingsBg.Height - 15))
                {
                    Parent = GameService.Graphics.SpriteScreen,
                    Title = "❤",
                    Subtitle = "❤",
                    SavesPosition = true,
                    Id = $"{Name} MainWindow",
                    CanResize = true,
                    Size = new(1024, 800),
                    Connections = Data.OldConnections,
                };
                SkillConnectionEditor.Show();
            }
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
            gear.Gear[GearTemplateSlot.Head].Stat = DataModels.Stats.EquipmentStat.Berserkers;
            gear.Gear[GearTemplateSlot.Head].Upgrades[UpgradeSlot.Rune] = 24836;
            gear.Gear[GearTemplateSlot.Head].Upgrades[UpgradeSlot.Infusion_1] = 37131;

            gear.Gear[GearTemplateSlot.Shoulder].Stat = DataModels.Stats.EquipmentStat.Berserkers;
            gear.Gear[GearTemplateSlot.Shoulder].Upgrades[UpgradeSlot.Rune] = 24836;
            gear.Gear[GearTemplateSlot.Shoulder].Upgrades[UpgradeSlot.Infusion_1] = 37131;

            gear.Gear[GearTemplateSlot.Chest].Stat = DataModels.Stats.EquipmentStat.Berserkers;
            gear.Gear[GearTemplateSlot.Chest].Upgrades[UpgradeSlot.Rune] = 24836;
            gear.Gear[GearTemplateSlot.Chest].Upgrades[UpgradeSlot.Infusion_1] = 37131;

            gear.Gear[GearTemplateSlot.Hand].Stat = DataModels.Stats.EquipmentStat.Berserkers;
            gear.Gear[GearTemplateSlot.Hand].Upgrades[UpgradeSlot.Rune] = 24836;
            gear.Gear[GearTemplateSlot.Hand].Upgrades[UpgradeSlot.Infusion_1] = 37131;

            gear.Gear[GearTemplateSlot.Leg].Stat = DataModels.Stats.EquipmentStat.Berserkers;
            gear.Gear[GearTemplateSlot.Leg].Upgrades[UpgradeSlot.Rune] = 24836;
            gear.Gear[GearTemplateSlot.Leg].Upgrades[UpgradeSlot.Infusion_1] = 37131;

            gear.Gear[GearTemplateSlot.Foot].Stat = DataModels.Stats.EquipmentStat.Berserkers;
            gear.Gear[GearTemplateSlot.Foot].Upgrades[UpgradeSlot.Rune] = 24836;
            gear.Gear[GearTemplateSlot.Foot].Upgrades[UpgradeSlot.Infusion_1] = 37131;

            gear.Gear[GearTemplateSlot.MainHand].Weapon = WeaponType.Staff;
            gear.Gear[GearTemplateSlot.MainHand].Stat = DataModels.Stats.EquipmentStat.Berserkers;
            gear.Gear[GearTemplateSlot.MainHand].Upgrades[UpgradeSlot.Sigil_1] = 24615;
            gear.Gear[GearTemplateSlot.MainHand].Upgrades[UpgradeSlot.Infusion_1] = 37131;
            gear.Gear[GearTemplateSlot.OffHand].Weapon = WeaponType.Staff;
            gear.Gear[GearTemplateSlot.OffHand].Stat = DataModels.Stats.EquipmentStat.Berserkers;
            gear.Gear[GearTemplateSlot.OffHand].Upgrades[UpgradeSlot.Sigil_1] = 24615;
            gear.Gear[GearTemplateSlot.OffHand].Upgrades[UpgradeSlot.Infusion_1] = 37131;
            gear.Gear[GearTemplateSlot.Aquatic].Weapon = WeaponType.Trident;
            gear.Gear[GearTemplateSlot.Aquatic].Stat = DataModels.Stats.EquipmentStat.Berserkers;
            gear.Gear[GearTemplateSlot.Aquatic].Upgrades[UpgradeSlot.Sigil_1] = 24615;
            gear.Gear[GearTemplateSlot.Aquatic].Upgrades[UpgradeSlot.Sigil_2] = 24618;
            gear.Gear[GearTemplateSlot.Aquatic].Upgrades[UpgradeSlot.Infusion_1] = 37131;
            gear.Gear[GearTemplateSlot.Aquatic].Upgrades[UpgradeSlot.Infusion_2] = 37131;

            gear.Gear[GearTemplateSlot.AltMainHand].Weapon = WeaponType.Scepter;
            gear.Gear[GearTemplateSlot.AltMainHand].Stat = DataModels.Stats.EquipmentStat.Berserkers;
            gear.Gear[GearTemplateSlot.AltMainHand].Upgrades[UpgradeSlot.Sigil_1] = 24615;
            gear.Gear[GearTemplateSlot.AltMainHand].Upgrades[UpgradeSlot.Infusion_1] = 37131;
            gear.Gear[GearTemplateSlot.AltOffHand].Weapon = WeaponType.Focus;
            gear.Gear[GearTemplateSlot.AltOffHand].Stat = DataModels.Stats.EquipmentStat.Berserkers;
            gear.Gear[GearTemplateSlot.AltOffHand].Upgrades[UpgradeSlot.Sigil_1] = 24615;
            gear.Gear[GearTemplateSlot.AltOffHand].Upgrades[UpgradeSlot.Infusion_1] = 37131;
            gear.Gear[GearTemplateSlot.AltAquatic].Weapon = WeaponType.Trident;
            gear.Gear[GearTemplateSlot.AltAquatic].Stat = DataModels.Stats.EquipmentStat.Berserkers;
            gear.Gear[GearTemplateSlot.AltAquatic].Upgrades[UpgradeSlot.Sigil_1] = 24615;
            gear.Gear[GearTemplateSlot.AltAquatic].Upgrades[UpgradeSlot.Sigil_2] = 24618;
            gear.Gear[GearTemplateSlot.AltAquatic].Upgrades[UpgradeSlot.Infusion_1] = 37131;
            gear.Gear[GearTemplateSlot.AltAquatic].Upgrades[UpgradeSlot.Infusion_2] = 37131;

            gear.Gear[GearTemplateSlot.Back].Stat = DataModels.Stats.EquipmentStat.Berserkers;
            gear.Gear[GearTemplateSlot.Back].Upgrades[UpgradeSlot.Infusion_1] = 37131;
            gear.Gear[GearTemplateSlot.Back].Upgrades[UpgradeSlot.Infusion_2] = 37131;
            gear.Gear[GearTemplateSlot.Amulet].Stat = DataModels.Stats.EquipmentStat.Berserkers;
            gear.Gear[GearTemplateSlot.Ring_1].Stat = DataModels.Stats.EquipmentStat.Berserkers;
            gear.Gear[GearTemplateSlot.Ring_1].Upgrades[UpgradeSlot.Infusion_1] = 37131;
            gear.Gear[GearTemplateSlot.Ring_1].Upgrades[UpgradeSlot.Infusion_2] = 37131;
            gear.Gear[GearTemplateSlot.Ring_1].Upgrades[UpgradeSlot.Infusion_3] = 37131;
            gear.Gear[GearTemplateSlot.Ring_2].Stat = DataModels.Stats.EquipmentStat.Berserkers;
            gear.Gear[GearTemplateSlot.Ring_2].Upgrades[UpgradeSlot.Infusion_1] = 37131;
            gear.Gear[GearTemplateSlot.Ring_2].Upgrades[UpgradeSlot.Infusion_2] = 37131;
            gear.Gear[GearTemplateSlot.Ring_2].Upgrades[UpgradeSlot.Infusion_3] = 37131;
            gear.Gear[GearTemplateSlot.Accessory_1].Stat = DataModels.Stats.EquipmentStat.Berserkers;
            gear.Gear[GearTemplateSlot.Accessory_1].Upgrades[UpgradeSlot.Infusion_1] = 37131;
            gear.Gear[GearTemplateSlot.Accessory_2].Stat = DataModels.Stats.EquipmentStat.Berserkers;
            gear.Gear[GearTemplateSlot.Accessory_2].Upgrades[UpgradeSlot.Infusion_1] = 37131;
            gear.Infusions[AttributeType.Power] = 4;

            string code = "[&DQgnNhM1PCYSAJsAiwDkAJkBdgBvAXABkgCVAAAAAAAAAAAAAAAAAAAAAAA=]";
            var build = new BuildTemplate(code);

            SelectedTemplate = new Template()
            {
                BuildTemplate = build,
                GearTemplate = gear,
                Race = Core.DataModels.Races.Human,
            };

            if (MainWindow != null) MainWindow.Template = SelectedTemplate;
        }
    }
}