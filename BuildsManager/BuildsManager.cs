﻿using Blish_HUD;
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
using Kenedia.Modules.BuildsManager.Controls.Tabs;
using System.Linq;
using Kenedia.Modules.BuildsManager.Controls.Selection;
using Blish_HUD.Graphics.UI;
using Kenedia.Modules.Core.Extensions;

namespace Kenedia.Modules.BuildsManager
{
    //TODO: Check Texture Disposing
    [Export(typeof(Module))]
    public class BuildsManager : BaseModule<BuildsManager, MainWindow, Settings, Paths>
    {
        public static int MainThread = System.Threading.Thread.CurrentThread.ManagedThreadId;
        public static Data Data { get; set; }

        private double _tick;
        private CancellationTokenSource _cancellationTokenSource;

        [ImportingConstructor]
        public BuildsManager([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters)
        {
            HasGUI = true;
            AutoLoadGUI = false;

            CoreServices.GameStateDetectionService.Enabled = false;
        }

        protected override ServiceCollection DefineServices(ServiceCollection services)
        {
            base.DefineServices(services);

            services.AddSingleton<BuildsManager>(this);

            services.AddSingleton<TemplateCollection>();
            services.AddSingleton<TemplatePresenter>();
            services.AddSingleton<TemplateTags>();
            services.AddSingleton<TagGroups>();
            services.AddSingleton<Data>();
            services.AddSingleton<GW2API>();
            services.AddSingleton<Settings>();

            services.AddScoped<MainWindow>();
            services.AddScoped<MainWindowPresenter>();
            services.AddScoped<SelectionPanel>();
            services.AddScoped<AboutTab>();
            services.AddScoped<BuildTab>();
            services.AddScoped<GearTab>();
            services.AddScoped<QuickFiltersPanel>();

            services.AddSingleton<Func<CornerIcon>>(() => CornerIcon);
            services.AddSingleton<Func<LoadingSpinner>>(() => LoadingSpinner);
            services.AddSingleton<Func<NotificationBadge>>(() => NotificationBadge);
            services.AddSingleton<Func<CornerIconContainer>>(() => CornerContainer);

            services.AddTransient<TemplateFactory>();
            services.AddTransient<TemplateConverter>();

            return services;
        }

        protected override void AssignServiceInstaces(IServiceProvider serviceProvider)
        {
            base.AssignServiceInstaces(serviceProvider);

            Data = ServiceProvider.GetRequiredService<Data>();
            Templates = ServiceProvider.GetRequiredService<TemplateCollection>();
            TemplatePresenter = ServiceProvider.GetRequiredService<TemplatePresenter>();
            TemplateTags = ServiceProvider.GetRequiredService<TemplateTags>();
            GW2API = ServiceProvider.GetRequiredService<GW2API>();
        }

        //public event ValueChangedEventHandler<bool> TemplatesLoadedDone;

        private bool _templatesLoaded = false;

        public bool TemplatesLoaded { get => _templatesLoaded; private set => Common.SetProperty(ref _templatesLoaded, value); }

        public Template? SelectedTemplate => TemplatePresenter.Template;

        public GW2API GW2API { get; private set; }

        public TemplateTags TemplateTags { get; private set; }

        public TemplatePresenter TemplatePresenter { get; private set; }

        public TemplateCollection Templates { get; private set; }

        public CornerIcon CornerIcon { get; private set; }

        public CornerIconContainer CornerContainer { get; private set; }

        public LoadingSpinner LoadingSpinner { get; private set; }

        public NotificationBadge NotificationBadge { get; private set; }

        protected override void DefineSettings(SettingCollection settings)
        {
            base.DefineSettings(settings);

            Settings.ShowCornerIcon.SettingChanged += ShowCornerIcon_SettingChanged;
        }

        protected override void Initialize()
        {
            base.Initialize();

            CreateCornerIcons();
            Logger.Info($"Starting {Name} v." + Version.BaseVersion());

            LoadGUI();
        }

        public override IView GetSettingsView()
        {
            return new BlishSettingsView(() =>
            {
                if (!MainWindow.IsVisible())
                {
                    MainWindow.Show();
                }

                MainWindow.SelectedTab = MainWindow.SettingsViewTab;
            });
        }

        protected override async void OnLocaleChanged(object sender, Blish_HUD.ValueChangedEventArgs<Locale> e)
        {
            if (GW2API is null) return;

            if (e.NewValue is not Locale.Korean and not Locale.Chinese)
            {
                if (!Data.LoadedLocales.Contains(e.NewValue))
                {
                    bool _ = await Data.Load(e.NewValue);
                }
            }

            base.OnLocaleChanged(sender, e);
        }

        protected override async Task LoadAsync()
        {
            LoadingSpinner?.Show();

            await base.LoadAsync();

            var templateGroups = ServiceProvider.GetService<TagGroups>();
            await templateGroups.Load();

            var templateTags = TemplateTags;
            await templateTags.Load();

            await Templates.Load();

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

            //TemplatePresenter.SetTemplate(ServiceProvider.GetService<TemplateFactory>().CreateTemplate());
            //TemplatePresenter.SetTemplate(Templates.FirstOrDefault());

            //await LoadAsync();
            //await GW2API.UpdateMappedIds();

            //await TemplateTags.Load();

            //var templateGroups = ServiceProvider.GetService<TagGroups>();
            //await templateGroups.Load();

            base.ReloadKey_Activated(sender, e);
        }

        protected override void LoadGUI()
        {
            base.LoadGUI();

            Logger.Info($"Building UI for {Name}");
            var scope = ServiceProvider.CreateScope();
            MainWindow = scope.ServiceProvider.GetRequiredService<MainWindow>();

#if DEBUG
            //MainWindow.Show();
#endif

            //TemplatePresenter.SetTemplate(Templates?.FirstOrDefault());
        }

        protected override void UnloadGUI()
        {
            base.UnloadGUI();

            MainWindow?.Dispose();
        }

        protected override void Unload()
        {
            DeleteCornerIcons();

            Templates.Clear();
            Data.Dispose();

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

        private async Task LoadTemplates()
        {
        }

        private void CreateCornerIcons()
        {
            DeleteCornerIcons();

            CornerIcon ??= new CornerIcon()
            {
                Icon = AsyncTexture2D.FromAssetId(156720),
                HoverIcon = AsyncTexture2D.FromAssetId(156721),
                SetLocalizedTooltip = () => string.Format(strings_common.ToggleItem, $"{Name}"),
                Parent = GameService.Graphics.SpriteScreen,
                Priority = 51294257,
                Visible = Settings?.ShowCornerIcon?.Value ?? false,
                ClickAction = () =>
                {
                    if (!Data.IsLoaded)
                    {
                        _ = Task.Run(() => Data.Load(true));
                        return;
                    }

                    MainWindow?.ToggleWindow();
                }
            };

            CornerContainer ??= new CornerIconContainer()
            {
                Parent = GameService.Graphics.SpriteScreen,
                WidthSizingMode = SizingMode.AutoSize,
                HeightSizingMode = SizingMode.AutoSize,
                Anchor = CornerIcon,
                AnchorPosition = AnchoredContainer.AnchorPos.Bottom,
                RelativePosition = new(0, -CornerIcon.Height / 2),
                CaptureInput = CaptureType.Filter,
            };

            NotificationBadge ??= new NotificationBadge()
            {
                Location = new(CornerIcon.Width - 15, 0),
                Parent = CornerContainer,
                Size = new(20),
                Opacity = 0.6f,
                HoveredOpacity = 1f,
                CaptureInput = CaptureType.Filter,
                Anchor = CornerIcon,
                Visible = false,
            };

            LoadingSpinner ??= new LoadingSpinner()
            {
                Location = new Point(0, NotificationBadge.Bottom),
                Parent = CornerContainer,
                Size = CornerIcon.Size,
                BasicTooltipText = strings_common.FetchingApiData,
                Visible = false,
                CaptureInput = null,
            };
        }

        private void DeleteCornerIcons()
        {
            CornerIcon?.Dispose();
            CornerIcon = null;

            CornerContainer?.Dispose();
            CornerContainer = null;
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