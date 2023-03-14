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
using System.Threading;
using Gw2Sharp;
using System.Linq;

namespace Kenedia.Modules.BuildsManager
{
    [Export(typeof(Module))]
    public class BuildsManager : BaseModule<BuildsManager, MainWindow, Settings>
    {
        private double _tick;
        private CancellationTokenSource _cancellationTokenSource;

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

            if (Data.BaseSkills.Count == 0)
            {
                _cancellationTokenSource ??= new();
                await GW2API.FetchBaseSkills(_cancellationTokenSource.Token);
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

                //var attribute = typeof(Gw2Client).Assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyFileVersionAttribute), true).FirstOrDefault();
                //Debug.WriteLine($"{((System.Reflection.AssemblyFileVersionAttribute)attribute).Version.ToString()}");
            }
        }

        protected override async void ReloadKey_Activated(object sender, EventArgs e)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            //await GW2API.CreateItemMap(_cancellationTokenSource.Token);
            //await GW2API.GetItems(_cancellationTokenSource.Token);

            //await GW2API.FetchBaseSkills();

            //Data.Stats.Clear();
            //await GW2API.GetStats(CancellationToken.None, Data.Stats);

            //await GW2API.UpdateData();

            await Data.LoadBaseSkills();
            await Data.LoadConnections();

            base.ReloadKey_Activated(sender, e);

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
            string gearcode = "[9|584|-1|0][4|584|-1|0][7|584|-1|0][3|584|-1|0][15|584|-1|0][17|584|-1|0][1|584|-1|0][53|584|-1|-1|-1][50|584|-1|-1|-1][7|584|-1|-1|-1|-1][-1|584|-1|-1|-1][16|584|-1|-1|-1][17|584|-1|-1|-1|-1][0|584|-1|-1][8|584|-1][3|584|-1][4|584|-1][5|584|-1|-1|-1][7|584|-1|-1|-1][-1][-1][-1]";
            string code = "[&DQgnNhM1PCYSAJsAiwDkAJkBdgBvAXABkgCVAAAAAAAAAAAAAAAAAAAAAAA=]";
            var build = new BuildTemplate(code);

            SelectedTemplate = new Template()
            {
                BuildTemplate = build,
                GearTemplate = new(gearcode),
                Race = Core.DataModels.Races.Human,
            };

            var template = SelectedTemplate.GearTemplate;

            foreach (var armor in template.Armors)
            {
                armor.Value.Stat = DataModels.Stats.EquipmentStat.Assassins;

                for (int i = 0; i < armor.Value.InfusionIds.Count; i++)
                {
                    armor.Value.InfusionIds[i] = 255;
                }

                for (int i = 0; i < armor.Value.RuneIds.Count; i++)
                {
                    armor.Value.RuneIds[i] = 1;
                }
            }

            foreach (var weapon in template.Weapons)
            {
                weapon.Value.Stat = DataModels.Stats.EquipmentStat.Berserkers;

                for (int i = 0; i < weapon.Value.InfusionIds.Count; i++)
                {
                    weapon.Value.InfusionIds[i] = 26;
                }

                for (int i = 0; i < weapon.Value.SigilIds.Count; i++)
                {
                    weapon.Value.SigilIds[i] = 1;
                }
            }

            foreach (var juwellery in template.Juwellery)
            {
                juwellery.Value.Stat = DataModels.Stats.EquipmentStat.Berserkers;

                for (int i = 0; i <( juwellery.Value.InfusionIds?.Count ?? 0); i++)
                {
                    juwellery.Value.InfusionIds[i] = 340;
                }

                if(juwellery.Value.EnrichmentIds != null)
                {
                    for (int i = 0; i < (juwellery.Value.EnrichmentIds?.Count ?? 0); i++)
                    {
                        juwellery.Value.EnrichmentIds[i] = 1;
                    }
                }
            }

            if (MainWindow != null) MainWindow.Template = SelectedTemplate;
        }
    }
}