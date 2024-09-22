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
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NotificationBadge = Kenedia.Modules.Core.Controls.NotificationBadge;
using CornerIcon = Kenedia.Modules.Core.Controls.CornerIcon;
using LoadingSpinner = Kenedia.Modules.Core.Controls.LoadingSpinner;
using AnchoredContainer = Kenedia.Modules.Core.Controls.AnchoredContainer;
using Version = SemVer.Version;
using Microsoft.Extensions.DependencyInjection;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.BuildsManager.Controls_Old.Selection;
using Kenedia.Modules.BuildsManager.Controls.Tabs;
using System.Linq;
using Blish_HUD.Modules.Managers;

namespace Kenedia.Modules.BuildsManager
{
    public class ServiceProviderProxy
    {
        private BuildsManager _module;

        public BuildsManager Module
        {
            get => _module; set
            {
                _module = value;

                if (value is null) throw new NotImplementedException("MODULE VALUE IS NULL");

                BuildsManager.Data = Data ??= new(value.Paths, value.Gw2ApiManager, null, null);
                TemplatePresenter = new();
            }
        }

        public TemplatePresenter TemplatePresenter { get; private set; } 

        public TemplateCollection TemplateCollection { get; } = new();

        public TemplateTags TemplateTags { get; private set; }

        public Data Data { get; private set; }

        public GW2API GW2API { get; private set; }

        public ServiceProviderProxy()
        {

        }

        public T GetService<T>()
        {
            var serviceType = typeof(T);

            switch (serviceType)
            {
                case Type t when t == typeof(BuildsManager):
                case Type tt when tt == typeof(Module):
                    return (T)(object)Module;

                case Type t when t == typeof(Paths):
                    return (T)(object)Module.Paths;

                case Type t when t == typeof(ContentsManager):
                    return (T)(object)Module.ContentsManager;

                case Type t when t == typeof(TemplateCollection):
                    return (T)(object)TemplateCollection;

                case Type t when t == typeof(TemplatePresenter):
                    return (T)(object)TemplatePresenter;

                case Type t when t == typeof(TemplateTags):
                    TemplateTags ??= new(Module.ContentsManager, Module.Paths);
                    return (T)(object)TemplateTags;

                case Type t when t == typeof(Data):
                    Data ??= new(Module.Paths, Module.Gw2ApiManager, null, null);
                    return (T)(object)Data;

                case Type t when t == typeof(GW2API):
                    Data ??= new(Module.Paths, Module.Gw2ApiManager, null, null);
                    GW2API ??= new(Module.Gw2ApiManager, Data, Module.Paths, null);

                    return (T)(object)GW2API;

                case Type t when t == typeof(MainWindow):
                    Data ??= new(Module.Paths, Module.Gw2ApiManager, null, null);

                    var buildTab = new BuildTab(TemplatePresenter, Data);
                    var gearTab = new GearTab(TemplatePresenter);
                    var aboutTab = new AboutTab(TemplatePresenter);
                    var selectionPanel = new SelectionPanel(TemplatePresenter, TemplateCollection, TemplateTags, Data);

                    return (T)(object)new MainWindow(Module, TemplatePresenter, selectionPanel, aboutTab, buildTab, gearTab);
            }

            return (T)(object)null;
        }
    }

    //TODO: Check Texture Disposing
    //TODO: Add tag quick access panel
    //TODO: Tag ordering
    //TODO: Fix the absurd amount of events and UI rebuilds
    [Export(typeof(Module))]
    public class BuildsManager : BaseModule<BuildsManager, MainWindow, Settings, Paths>
    {
        public ServiceProviderProxy ServiceProvider { get; private set; }

        public static Data Data { get; set; }

        private double _tick;
        private CancellationTokenSource _cancellationTokenSource;
        private CornerIcon _cornerIcon;
        private CornerIconContainer _cornerContainer;
        private NotificationBadge _notificationBadge;
        private LoadingSpinner _apiSpinner;

        [ImportingConstructor]
        public BuildsManager([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters)
        {
            ModuleInstance = this;
            HasGUI = true;

            Services.GameStateDetectionService.Enabled = false;

            Paths = new(DirectoriesManager, Name);
            ServiceProvider = new ServiceProviderProxy()
            {
                Module = this,
            };

            //Data = ServiceProvider.GetService<Data>();
            TemplatePresenter = ServiceProvider.GetService<TemplatePresenter>();

            //CreateCornerIcons();
        }

        public void ConfigureServices()
        {
            var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();

            //services.AddSingleton(ContentsManager);
            //services.AddSingleton<Paths>(Paths);

            //services.AddSingleton<TemplateCollection>();
            //services.AddSingleton<TemplatePresenter>();
            //services.AddSingleton<TemplateTags>();
            //services.AddSingleton<Data>();
            //services.AddSingleton<GW2API>();

            //services.AddSingleton(Gw2ApiManager);

            //services.AddTransient<MainWindow>();
            //services.AddTransient<SelectionPanel>();
            //services.AddTransient<AboutTab>();
            //services.AddTransient<BuildTab>();
            //services.AddTransient<GearTab>();

            //services.AddSingleton<Module>(this);
            CreateCornerIcons();

            //services.BuildServiceProvider();
        }

        public event ValueChangedEventHandler<bool> TemplatesLoadedDone;

        private bool _templatesLoaded = false;

        public bool TemplatesLoaded { get => _templatesLoaded; private set => Common.SetProperty(ref _templatesLoaded, value, OnTemplatesLoaded, value); }

        private void OnTemplatesLoaded(object sender, Core.Models.ValueChangedEventArgs<bool> e)
        {
            TemplatesLoadedDone?.Invoke(this, e);
        }

        public Template? SelectedTemplate => TemplatePresenter.Template;

        public TemplatePresenter TemplatePresenter { get; private set; }

        protected override void DefineSettings(SettingCollection settings)
        {
            base.DefineSettings(settings);

            Settings = new Settings(settings);
            Settings.ShowCornerIcon.SettingChanged += ShowCornerIcon_SettingChanged;
        }

        protected override void Initialize()
        {
            base.Initialize();

            Logger.Info($"Starting {Name} v." + Version.BaseVersion());

            Data.Loaded += Data_Loaded;
        }

        private async void Data_Loaded(object sender, EventArgs e)
        {
            Debug.WriteLine($"Data_Loaded");

            if (!TemplatesLoaded)
                await LoadTemplates();

            LoadGUI();
        }

        protected override async void OnLocaleChanged(object sender, Blish_HUD.ValueChangedEventArgs<Locale> e)
        {
            if (ServiceProvider.GetService<GW2API>() is null) return;

            if (e.NewValue is not Locale.Korean and not Locale.Chinese)
            {
                bool success = await Data.Load();

                if (!success)
                {
                    //Set Failed Notification
                    //_notificationBadge.Show();
                    //_notificationBadge.SetLocalizedText = () => $"There was an issue when loading the Data for {e.NewValue} Localization.";
                }

                base.OnLocaleChanged(sender, e);
            }
        }

        protected override async Task LoadAsync()
        {
            _apiSpinner?.Show();

            await base.LoadAsync();

            var templateTags = ServiceProvider.GetService<TemplateTags>();
            await templateTags.Load();

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

                if (Data?.IsLoaded == false)
                {
                    _ = Task.Run(Data.Load);
                }
            }
        }

        protected override async void ReloadKey_Activated(object sender, EventArgs e)
        {
            Logger.Debug($"ReloadKey_Activated: {Name}");
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            base.ReloadKey_Activated(sender, e);

            //await LoadAsync();
            //await GW2API.UpdateMappedIds();
        }

        protected override void LoadGUI()
        {

            Debug.WriteLine($"TemplatesLoaded: {TemplatesLoaded} Data.IsLoaded : {Data.IsLoaded}");
            if (!Data.IsLoaded || !TemplatesLoaded) return;

            base.LoadGUI();

            Logger.Info($"Building UI for {Name}");

            MainWindow = ServiceProvider.GetService<MainWindow>();

#if DEBUG
            MainWindow.Show();
#endif

            var templates = ServiceProvider.GetService<TemplateCollection>();
            templates?.FirstOrDefault()?.Load();

            //TemplatePresenter.Template = templates?.FirstOrDefault();
        }

        protected override void UnloadGUI()
        {
            base.UnloadGUI();

            MainWindow?.Dispose();
        }

        protected override void Unload()
        {
            DeleteCornerIcons();

            var templates = ServiceProvider.GetService<TemplateCollection>();
            templates?.Clear();

            var data = ServiceProvider.GetService<Data>();
            data?.Dispose();

            base.Unload();
        }

        private void ShowCornerIcon_SettingChanged(object sender, Blish_HUD.ValueChangedEventArgs<bool> e)
        {
            if (e.NewValue)
            {
                //CreateCornerIcons();
                _cornerIcon.Parent = _cornerContainer.Parent = GameService.Graphics.SpriteScreen;
            }
            else
            {
                //DeleteCornerIcons();

                _cornerIcon.Parent = _cornerContainer.Parent = null;
            }
        }

        private async Task LoadTemplates()
        {
            var templates = ServiceProvider.GetService<TemplateCollection>();
            Logger.Info($"LoadTemplates");

            TemplatesLoaded = false;
            var time = new Stopwatch();
            time.Start();

            try
            {
                string[] templateFiles = Directory.GetFiles(Paths.TemplatesPath);

                templates.Clear();

                JsonSerializerSettings settings = new();
                settings.Converters.Add(new TemplateConverter());

                Logger.Info($"Loading {templateFiles.Length} Templates ...");
                foreach (string file in templateFiles)
                {
                    string fileText = File.ReadAllText(file);
                    var template = JsonConvert.DeserializeObject<Template>(fileText, settings);

                    if (template is not null)
                    {
                        templates.Add(template);
                    }
                }

                time.Stop();
                Logger.Info($"Time to load {templateFiles.Length} templates {time.ElapsedMilliseconds}ms. {templates.Count} out of {templateFiles.Length} templates got loaded.");
            }
            catch (Exception ex)
            {
                Logger.Warn(ex.Message);
                Logger.Warn($"Loading Templates failed!");
            }

            TemplatesLoaded = true;
        }

        private void CreateCornerIcons()
        {
            DeleteCornerIcons();

            _cornerIcon ??= new CornerIcon()
            {
                Icon = AsyncTexture2D.FromAssetId(156720),
                HoverIcon = AsyncTexture2D.FromAssetId(156721),
                SetLocalizedTooltip = () => string.Format(strings_common.ToggleItem, $"{Name}"),
                Parent = GameService.Graphics.SpriteScreen,
                Visible = Settings?.ShowCornerIcon?.Value ?? false,
                ClickAction = () =>
                {
                    if (!Data.IsLoaded)
                    {

                        Debug.WriteLine($"Data?.IsLoaded: {Data?.IsLoaded}");
                        _ = Task.Run(() => Data.Load(true));
                        return;
                    }

                    Debug.WriteLine($"SHOW WINDOW");
                    MainWindow?.ToggleWindow();

                    Debug.WriteLine($"{MainWindow?.Visible}");
                }
            };

            _cornerContainer ??= new()
            {
                Parent = GameService.Graphics.SpriteScreen,
                WidthSizingMode = SizingMode.AutoSize,
                HeightSizingMode = SizingMode.AutoSize,
                Anchor = _cornerIcon,
                AnchorPosition = AnchoredContainer.AnchorPos.Bottom,
                RelativePosition = new(0, -_cornerIcon.Height / 2),
                CaptureInput = CaptureType.Filter,
            };

            _notificationBadge ??= new NotificationBadge()
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

            _apiSpinner ??= new LoadingSpinner()
            {
                Location = new Point(0, _notificationBadge.Bottom),
                Parent = _cornerContainer,
                Size = _cornerIcon.Size,
                BasicTooltipText = strings_common.FetchingApiData,
                Visible = false,
                CaptureInput = null,
            };

            //serviceCollection.AddSingleton(_cornerContainer);
            //serviceCollection.AddSingleton(_cornerIcon);
            //serviceCollection.AddSingleton(_notificationBadge);
            //serviceCollection.AddSingleton(_apiSpinner);
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
            if (Control.ActiveControl is not Blish_HUD.Controls.TextBox)
            {
                MainWindow?.ToggleWindow();
            }
        }
    }
}