using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Views;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Threading.Tasks;
using Map = Kenedia.Modules.Core.DataModels.Map;

namespace Kenedia.Modules.Dev
{
    [Export(typeof(Module))]
    public class Dev : BaseModule<Dev, StandardWindow, BaseSettingsModel, PathCollection>
    {
        private double _tick;
        private readonly Dictionary<int, Map> _maps = [];

        public static List<Skill> Skills { get; private set; } = [];

        public static RadialMenu RadialMenu { get; private set; }

        [ImportingConstructor]
        public Dev([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters)
        {

        }

        protected override void DefineSettings(SettingCollection settings)
        {
            base.DefineSettings(settings);

        }

        protected override void Initialize()
        {
            base.Initialize();

            HasGUI = false;
            Logger.Info($"Starting {Name} v." + Version.BaseVersion());
        }

        protected override async void ReloadKey_Activated(object sender, EventArgs e)
        {
            Logger.Debug($"ReloadKey_Activated: {Name}");

            base.ReloadKey_Activated(sender, e);

        }

        private async Task TestAPI()
        {
            var skills = await Gw2ApiManager.Gw2ApiClient.V2.Skills.ManyAsync(new List<int>() { 9168, 10701 });
            foreach (var skill2 in skills)
            {
                skill2.HttpResponseInfo = null;
            }
            string json = JsonConvert.SerializeObject(skills, SerializerSettings.Default);
            System.IO.File.WriteAllText($@"{Paths.ModulePath}\skills.json", json);

            //var skills = JsonConvert.DeserializeObject<List<Skill>>(await new StreamReader($@"{Paths.ModulePath}\skills.json").ReadToEndAsync());

            Debug.WriteLine($"{skills.Count}");
            var skill = skills[0];

            foreach (var item in skill.Facts)
            {
                Debug.WriteLine($"{item.Text}");
                Debug.WriteLine($"{item.Type.IsUnknown}");
                Debug.WriteLine($"{item.Type.RawValue}");
                Debug.WriteLine($"{item.Type.RawValue}");
            }
        }

        protected override async Task LoadAsync()
        {
            await base.LoadAsync();
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
            var r = new Microsoft.Xna.Framework.Rectangle(50, 50, 900, 625);

            MainWindow = new StandardWindow(AsyncTexture2D.FromAssetId(155985), r, r)
            {
                Title = "Dev",
                CanResize = true,
                ZIndex = int.MaxValue,
                Location = new(100, 100),
                Parent = GameService.Graphics.SpriteScreen,
            };

            var toggle = new ToggleControl()
            {
                Parent = MainWindow,
                TextLeft = "Homestead",
                TextRight = "Guild Hall",
                Width = 250,
            };

            MainWindow.Show();

            var base_color = new Microsoft.Xna.Framework.Color(172, 117, 74);
            var highlight_color = new Microsoft.Xna.Framework.Color(245, 175, 106);

            base_color = Microsoft.Xna.Framework.Color.Black;

            RadialMenu = new RadialMenu()
            {
                Parent = GameService.Graphics.SpriteScreen,
                Visible = true,
                ZIndex = int.MaxValue,
                DonutHolePercent = 0.5F,
            };

            RadialMenu.SetSize(new Point(800, 800));
            RadialMenu.SetCenter(GameService.Graphics.SpriteScreen.RelativeMousePosition);
            RadialMenu.Show();
        }

        protected override void UnloadGUI()
        {
            base.UnloadGUI();

            RadialMenu?.Dispose();
            MainWindow?.Dispose();
        }
    }
}