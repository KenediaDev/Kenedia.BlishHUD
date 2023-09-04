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
using Newtonsoft.Json.Serialization;

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

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new SemverVersionContractResolver()
            };

            Logger.Info($"Starting {Name} v." + Version.BaseVersion());

            GW2API = new(Gw2ApiManager, () => Data, Paths, () => _notificationBadge);
            Data = new(ContentsManager, Paths, Gw2ApiManager);
            Data.Loaded += Data_Loaded;
        }

        private void Data_Loaded(object sender, EventArgs e)
        {
            if (!TemplatesLoaded) 
                LoadTemplates();

            _apiSpinner?.Hide();
        }

        protected override async void OnLocaleChanged(object sender, Blish_HUD.ValueChangedEventArgs<Locale> e)
        {
            if (GW2API is null) return;

            if (e.NewValue is not Locale.Korean and not Locale.Chinese)
            {
                _apiSpinner?.Show();
                bool success = await Data.Load();

                if (!success)
                {
                    //Set Failed Notification
                    _notificationBadge.Show();
                    _notificationBadge.SetLocalizedText = () => $"There was an issue when loading the Data for {e.NewValue} Localization.";
                }

                _apiSpinner?.Hide();

                base.OnLocaleChanged(sender, e);
            }
        }

        protected override async Task LoadAsync()
        {
            _apiSpinner?.Show();
            await base.LoadAsync();
            _ = await Data.Load();
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

            //await GW2API.UpdateMappedIds("0.0.4");

            //LoadTemplates();
            base.ReloadKey_Activated(sender, e);
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