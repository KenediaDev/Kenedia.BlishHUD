using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Modules;
using Blish_HUD.Settings;
using Gw2Sharp.WebApi;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.Services;
using Kenedia.Modules.BuildsManager.Utility;
using Kenedia.Modules.BuildsManager.Views;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Res;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NotificationBadge = Kenedia.Modules.Core.Controls.NotificationBadge;
using CornerIcon = Kenedia.Modules.Core.Controls.CornerIcon;
using LoadingSpinner = Kenedia.Modules.Core.Controls.LoadingSpinner;
using AnchoredContainer = Kenedia.Modules.Core.Controls.AnchoredContainer;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Version = SemVer.Version;

namespace Kenedia.Modules.BuildsManager
{
    [Export(typeof(Module))]
    public class BuildsManager : BaseModule<BuildsManager, MainWindow, Settings, Paths>
    {
        private double _tick;
        private CancellationTokenSource _cancellationTokenSource;
        private CancellationTokenSource _fileAccessTokenSource;
        private CornerIcon _cornerIcon;
        private AnchoredContainer _cornerContainer;
        private NotificationBadge _notificationBadge;
        private LoadingSpinner _apiSpinner;

        [ImportingConstructor]
        public BuildsManager([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters)
        {
            ModuleInstance = this;
            HasGUI = true;

            Services.GameStateDetectionService.Enabled = false;

            CreateCornerIcons();
        }

        private GW2API GW2API { get; set; }

        private StaticHosting StaticHosting { get; set; }

        public static Data Data { get; set; }

        public ObservableCollection<Template> Templates { get; private set; } = new();

        public event ValueChangedEventHandler<bool> TemplatesLoadedDone;

        private bool _templatesLoaded = false;

        public bool TemplatesLoaded { get => _templatesLoaded; private set => Common.SetProperty(ref _templatesLoaded, value, OnTemplatesLoaded, value); }

        private void OnTemplatesLoaded(object sender, Core.Models.ValueChangedEventArgs<bool> e)
        {
            TemplatesLoadedDone?.Invoke(this, e);
        }

        public Template SelectedTemplate => MainWindow?.Template ?? null;

        protected override void DefineSettings(SettingCollection settings)
        {
            base.DefineSettings(settings);

            Settings = new Settings(settings);
            Settings.ShowCornerIcon.SettingChanged += ShowCornerIcon_SettingChanged; ;
        }

        protected override void Initialize()
        {
            base.Initialize();

            Paths = new(DirectoriesManager, Name);

            Logger.Info($"Starting {Name} v." + Version.BaseVersion());
            Data = new(ContentsManager, Paths);
        }

        protected override async void OnLocaleChanged(object sender, Blish_HUD.ValueChangedEventArgs<Locale> e)
        {
            if (e.NewValue is not Locale.Korean and not Locale.Chinese)
            {
                GW2API.Cancel();
                _apiSpinner?.Hide();

                if (!Data.Professions.TryGetValue(Gw2Sharp.Models.ProfessionType.Guardian, out DataModels.Professions.Profession profession) || !profession.Names.TryGetValue(e.NewValue, out string name) || string.IsNullOrEmpty(name))
                {
                    _apiSpinner?.Show();

                    Logger.Info($"No data for {e.NewValue} loaded yet. Fetching new data from the API.");
                    await GW2API.UpdateData();

                    if (!GW2API.IsCanceled())
                    {
                        Logger.Info($"Apply fresh {e.NewValue} data to the UI.");
                    }

                    if (Data.IsLoaded && !TemplatesLoaded) LoadTemplates();
                    _apiSpinner?.Hide();
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
            GW2API = new(Gw2ApiManager, Data, Paths, () => _notificationBadge)
            {
                Paths = Paths,
            };

            if (GameService.Overlay.UserLocale.Value is not Locale.Korean and not Locale.Chinese)
            {
                if (!Data.Professions.TryGetValue(Gw2Sharp.Models.ProfessionType.Guardian, out DataModels.Professions.Profession profession) || !profession.Names.TryGetValue(GameService.Overlay.UserLocale.Value, out string name) || string.IsNullOrEmpty(name))
                {
                    Logger.Info($"No data for {GameService.Overlay.UserLocale.Value} loaded yet. Fetching new data from the API.");
                    OnLocaleChanged(this, new(Locale.Chinese, GameService.Overlay.UserLocale.Value));
                }
            }

            if (Data.IsLoaded) LoadTemplates();
        }

        protected override void OnModuleLoaded(EventArgs e)
        {
            base.OnModuleLoaded(e);

            if (!Settings.ShowCornerIcon.Value)
            {
                DeleteCornerIcons();
            }

            Settings.ToggleWindowKey.Value.Enabled = true;
            Settings.ToggleWindowKey.Value.Activated += OnToggleWindowKey;

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
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            var version = new Version(0, 0, 1);
            var itemMapCollection = new ItemMapCollection(version);

            foreach (var item in Data.ItemMap.Utilities)
            {
                itemMapCollection.Enhancements.Add(item.MappedId, item.Id);
            }

            foreach (var item in Data.ItemMap.Armors)
            {
                itemMapCollection.Armors.Add(item.MappedId, item.Id);
            }

            foreach (var item in Data.ItemMap.Weapons)
            {
                itemMapCollection.Weapons.Add(item.MappedId, item.Id);
            }

            foreach (var item in Data.ItemMap.Backs)
            {
                itemMapCollection.Backs.Add(item.MappedId, item.Id);
            }

            foreach (var item in Data.ItemMap.Enrichments)
            {
                itemMapCollection.Enrichments.Add(item.MappedId, item.Id);
            }

            foreach (var item in Data.ItemMap.Infusions)
            {
                itemMapCollection.Infusions.Add(item.MappedId, item.Id);
            }

            foreach (var item in Data.ItemMap.Nourishments)
            {
                itemMapCollection.Nourishments.Add(item.MappedId, item.Id);
            }

            foreach (var item in Data.ItemMap.PowerCores)
            {
                itemMapCollection.PowerCores.Add(item.MappedId, item.Id);
            }

            foreach (var item in Data.ItemMap.PveRunes)
            {
                itemMapCollection.PveRunes.Add(item.MappedId, item.Id);
            }

            foreach (var item in Data.ItemMap.PveSigils)
            {
                itemMapCollection.PveSigils.Add(item.MappedId, item.Id);
            }

            foreach (var item in Data.ItemMap.PvpAmulets)
            {
                itemMapCollection.PvpAmulets.Add(item.MappedId, item.Id);
            }

            foreach (var item in Data.ItemMap.PvpRunes)
            {
                itemMapCollection.PvpRunes.Add(item.MappedId, item.Id);
            }

            foreach (var item in Data.ItemMap.PvpSigils)
            {
                itemMapCollection.PvpSigils.Add(item.MappedId, item.Id);
            }

            foreach (var item in Data.ItemMap.Relics)
            {
                itemMapCollection.Relics.Add(item.MappedId, item.Id);
            }

            foreach (var item in Data.ItemMap.Trinkets)
            {
                itemMapCollection.Trinkets.Add(item.MappedId, item.Id);
            }

            Debug.WriteLine($"Saving itemMapCollection {itemMapCollection.Infusions.Version}");

            foreach(var itemmap in itemMapCollection)
            {
                string json = JsonConvert.SerializeObject(itemmap.Value, Formatting.Indented);
                File.WriteAllText($@"{Paths.ModulePath}\data\itemmap\{itemmap.Key}.json", json);
            }

            string vjson = JsonConvert.SerializeObject(new StaticVersion(version), Formatting.Indented);
            File.WriteAllText($@"{Paths.ModulePath}\data\itemmap\Version.json", vjson);

            var versions = await StaticHosting.GetStaticVersion();

            Debug.WriteLine($"version {versions.Armors}");

            //LoadTemplates();
            //base.ReloadKey_Activated(sender, e);
        }

        protected override void LoadGUI()
        {
            if (!Data.IsLoaded || !TemplatesLoaded) return;

            base.LoadGUI();
            int Height = 670;
            int Width = 915;

            Logger.Info($"Building UI for {Name}");

            MainWindow = new MainWindow(
                Services.TexturesService.GetTexture(@"textures\mainwindow_background.png", "mainwindow_background"),
                new Rectangle(30, 30, Width, Height + 30),
                new Rectangle(30, 20, Width - 3, Height + 15),
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
                Width = 1200,
                Height = 900,
            };

#if DEBUG
            MainWindow.Show();
#endif

            MainWindow.SelectFirstTemplate();
        }

        protected override void UnloadGUI()
        {
            base.UnloadGUI();

            MainWindow?.Dispose();
        }

        protected override void Unload()
        {
            DeleteCornerIcons();
            Templates?.Clear();
            Data?.Dispose();

            base.Unload();
        }

        private void ShowCornerIcon_SettingChanged(object sender, Blish_HUD.ValueChangedEventArgs<bool> e)
        {
            if (e.NewValue)
            {
                CreateCornerIcons();
            }
            else
            {
                DeleteCornerIcons();
            }
        }

        private void LoadTemplates()
        {
            Logger.Info($"LoadTemplates");

            _ = Task.Run(() =>
            {
                TemplatesLoaded = false;
                var time = new Stopwatch();
                time.Start();

                try
                {
                    _fileAccessTokenSource?.Cancel();
                    _fileAccessTokenSource = new CancellationTokenSource();
                    string[] templates = Directory.GetFiles(Paths.TemplatesPath);

                    Templates.Clear();

                    JsonSerializerSettings settings = new();
                    settings.Converters.Add(new TemplateConverter());

                    Logger.Info($"Loading {templates.Length} Templates ...");
                    foreach (string file in templates)
                    {
                        string fileText = File.ReadAllText(file);
                        var template = JsonConvert.DeserializeObject<Template>(fileText, settings);

                        if (template is not null)
                        {
                            Templates.Add(template);
                        }
                    }

                    time.Stop();
                    Logger.Info($"Time to load {templates.Length} templates {time.ElapsedMilliseconds}ms. {Templates.Count} out of {templates.Length} templates got loaded.");
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex.Message);
                    Logger.Warn($"Loading Templates failed!");
                }

                TemplatesLoaded = true;
            });

        }

        private void CreateCornerIcons()
        {
            DeleteCornerIcons();

            _cornerIcon = new CornerIcon()
            {
                Icon = AsyncTexture2D.FromAssetId(156720),
                HoverIcon = AsyncTexture2D.FromAssetId(156721),
                SetLocalizedTooltip = () => string.Format(strings_common.ToggleItem, $"{Name}"),
                Parent = GameService.Graphics.SpriteScreen,
                Visible = Settings?.ShowCornerIcon?.Value ?? false,
                ClickAction = () => MainWindow?.ToggleWindow(),
            };

            _cornerContainer = new()
            {
                Parent = GameService.Graphics.SpriteScreen,
                WidthSizingMode = SizingMode.AutoSize,
                HeightSizingMode = SizingMode.AutoSize,
                Anchor = _cornerIcon,                
                AnchorPosition = AnchoredContainer.AnchorPos.Bottom,
                RelativePosition = new(0, -_cornerIcon.Height / 2),
                CaptureInput = CaptureType.Filter,
            };

            _notificationBadge = new NotificationBadge()
            {
                Location = new(_cornerIcon.Width - 15, 0),
                Parent = _cornerContainer,
                Size = new(20),
                Opacity = 0.6f,
                HoveredOpacity = 1f,
                CaptureInput = CaptureType.Filter,
                Anchor = _cornerIcon,
                Visible = false,
            };

            _apiSpinner = new LoadingSpinner()
            {
                Location = new Point(0, _notificationBadge.Bottom),
                Parent = _cornerContainer,
                Size = _cornerIcon.Size,
                BasicTooltipText = strings_common.FetchingApiData,
                Visible = false,
                CaptureInput = null,
            };
        }

        private void DeleteCornerIcons()
        {
            _cornerIcon?.Dispose();
            _cornerIcon = null;

            _cornerContainer?.Dispose();
            _cornerContainer = null;
        }

        private void OnToggleWindowKey(object sender, EventArgs e)
        {
            if (Control.ActiveControl is not TextBox)
            {
                MainWindow?.ToggleWindow();
            }
        }
    }
}