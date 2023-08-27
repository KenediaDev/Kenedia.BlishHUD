using Blish_HUD;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Views;
using Kenedia.Modules.FashionManager.Models;
using Kenedia.Modules.FashionManager.Views;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Kenedia.Modules.FashionManager
{
    [Export(typeof(Module))]
    public class FashionManager : BaseModule<FashionManager, MainWindow, BaseSettingsModel, Paths>
    {
        private double _tick;
        private CancellationTokenSource _fileAccessTokenSource;
        private readonly List<FashionTemplate> _fashionTemplates = new();

        [ImportingConstructor]
        public FashionManager([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters)
        {
            ModuleInstance = this;
        }

        public static FashionTemplate FashionTemplate { get; private set; } = new();
        public bool TemplatesLoaded { get; private set; }

        protected override void DefineSettings(SettingCollection settings)
        {
            base.DefineSettings(settings);

        }

        protected override void Initialize()
        {
            base.Initialize();
            Paths = new(DirectoriesManager, Name);

            HasGUI = false;
            Logger.Info($"Starting {Name} v." + Version.BaseVersion());
        }

        protected override async void ReloadKey_Activated(object sender, EventArgs e)
        {
            base.ReloadKey_Activated(sender, e);

            FashionTemplate.Name ??= "Test";

            FashionTemplate.Name = "My Template";

            LoadTemplates();
        }

        protected override async Task LoadAsync()
        {
            await base.LoadAsync();
            LoadTemplates();
        }

        private void LoadTemplates()
        {
            _ = Task.Run(() =>
            {

                TemplatesLoaded = false;
                var time = new Stopwatch();
                time.Start();

                try
                {
                    _fileAccessTokenSource?.Cancel();
                    _fileAccessTokenSource = new CancellationTokenSource();
                    string[] templates = Directory.GetDirectories(Paths.TemplatesPath);

                    _fashionTemplates.Clear();

                    JsonSerializerSettings settings = new();
                    settings.Converters.Add(new FashionTemplateConverter());

                    Logger.Info($"Loading {templates.Length} Templates ...");
                    foreach (string directory in templates)
                    {
                        string fileName = Path.GetFileName(directory);

                        string fileText = File.ReadAllText(@$"{directory}\{fileName}.json");
                        var template = JsonConvert.DeserializeObject<FashionTemplate>(fileText, settings);

                        if (template is not null)
                        {
                            _fashionTemplates.Add(template);
                        }
                    }

                    time.Stop();
                    Logger.Info($"Time to load {templates.Length} templates {time.ElapsedMilliseconds}ms. {_fashionTemplates.Count} out of {templates.Length} templates got loaded.");
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex.Message);
                    Logger.Warn($"Loading Templates failed!");
                }

                TemplatesLoaded = true;
            });
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

        protected override void Unload()
        {
            base.Unload();
        }

        protected override void LoadGUI()
        {
            base.LoadGUI();
        }

        protected override void UnloadGUI()
        {
            base.UnloadGUI();

            MainWindow?.Dispose();
        }
    }
}