using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Modules;
using Blish_HUD.Settings;
using Gw2Sharp.WebApi;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.BuildsManager.Services;
using Kenedia.Modules.BuildsManager.Views;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Res;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CornerIcon = Kenedia.Modules.Core.Controls.CornerIcon;
using LoadingSpinner = Kenedia.Modules.Core.Controls.LoadingSpinner;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.BuildsManager
{
    [Export(typeof(Module))]
    public class BuildsManager : BaseModule<BuildsManager, MainWindow, Settings, Paths>
    {
        private double _tick;
        private CancellationTokenSource _cancellationTokenSource;
        private CornerIcon _cornerIcon;
        private LoadingSpinner _apiSpinner;

        [ImportingConstructor]
        public BuildsManager([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters)
        {
            ModuleInstance = this;
            HasGUI = true;
        }

        private GW2API GW2API { get; set; }

        public static Data Data { get; set; }

        public ObservableCollection<Template> Templates { get; private set; } = new();

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
            };

            Logger.Info($"Starting {Name} v." + Version.BaseVersion());
            Data = new(ContentsManager, Paths);
        }

        protected override async void OnLocaleChanged(object sender, Blish_HUD.ValueChangedEventArgs<Locale> e)
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

            if (Data.IsLoaded) await LoadTemplates();
        }

        protected override void OnModuleLoaded(EventArgs e)
        {
            base.OnModuleLoaded(e);

            if (Settings.ShowCornerIcon.Value)
            {
                CreateCornerIcons();
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

                //var attribute = typeof(Gw2Client).Assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyFileVersionAttribute), true).FirstOrDefault();
                //Debug.WriteLine($"{((System.Reflection.AssemblyFileVersionAttribute)attribute).Version.ToString()}");
            }
        }

        protected override async void ReloadKey_Activated(object sender, EventArgs e)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            base.ReloadKey_Activated(sender, e);
        }

        protected override void LoadGUI()
        {
            if (!Data.IsLoaded) return;

            base.LoadGUI();
            int Height = 670;
            int Width = 915;

            Logger.Info($"Building UI for {Name}");

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
                Width = 1200,
                Height = 900,
            };

            //MainWindow.Show();
        }

        protected override void UnloadGUI()
        {
            base.UnloadGUI();

            MainWindow?.Dispose();
        }

        protected override void Unload()
        {
            base.Unload();

            DeleteCornerIcons();
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
            Logger.Info($"LoadTemplates");

            try
            {
                string[] templates = Directory.GetFiles(Paths.TemplatesPath);

                Logger.Info($"Loading {templates.Length} Templates ...");
                foreach (string file in templates)
                {
                    using var reader = File.OpenText(file);
                    string fileText = await reader.ReadToEndAsync();

                    var template = JsonConvert.DeserializeObject<Template>(fileText);

                    if (template != null)
                    {
                        Templates.Add(template);
                    }
                }

                //if (Templates.Count == 0 && SelectedTemplate != null) Templates.Add(SelectedTemplate);
                //SelectedTemplate = Templates.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Logger.Warn(ex.Message);
                Logger.Warn($"Loading Templates failed!");
            }
        }

        private void CreateCornerIcons()
        {
            DeleteCornerIcons();

            _cornerIcon = new CornerIcon()
            {
                Icon = AsyncTexture2D.FromAssetId(156720),
                HoverIcon = AsyncTexture2D.FromAssetId(156721),
                SetLocalizedTooltip = () => string.Format("Toggle {0}", $"{Name}"),
                Parent = GameService.Graphics.SpriteScreen,
                Visible = Settings.ShowCornerIcon.Value,
                ClickAction = () => MainWindow?.ToggleWindow(),
            };

            _apiSpinner = new LoadingSpinner()
            {
                Location = new Point(_cornerIcon.Left, _cornerIcon.Bottom + 3),
                Parent = GameService.Graphics.SpriteScreen,
                Size = new Point(_cornerIcon.Width, _cornerIcon.Height),
                BasicTooltipText = strings_common.FetchingApiData,
                Visible = false,
            };

            _cornerIcon.Moved += CornerIcon_Moved;
        }

        private void DeleteCornerIcons()
        {
            if (_cornerIcon != null) _cornerIcon.Moved -= CornerIcon_Moved;
            _cornerIcon?.Dispose();
            _cornerIcon = null;

            _apiSpinner?.Dispose();
            _apiSpinner = null;
        }

        private void CornerIcon_Moved(object sender, MovedEventArgs e)
        {
            if (_apiSpinner != null) _apiSpinner.Location = new Point(_cornerIcon.Left, _cornerIcon.Bottom + 3);
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