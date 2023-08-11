﻿using Blish_HUD;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Gw2Sharp.WebApi;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Views;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Map = Kenedia.Modules.Core.DataModels.Map;

namespace Kenedia.Modules.Dev
{
    [Export(typeof(Module))]
    public class Dev : BaseModule<Dev, StandardWindow, BaseSettingsModel, PathCollection>
    {
        private double _tick;
        private readonly Dictionary<int, Map> _maps = new();

        public static List<Skill> Skills { get; private set; } = new();

        [ImportingConstructor]
        public Dev([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters)
        {
            ModuleInstance = this;
        }

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

            await TestAPI();
        }

        private async Task TestAPI()
        {
            var skills = await Gw2ApiManager.Gw2ApiClient.V2.Skills.ManyAsync(new List<int>() { 9168, 10701 });
            foreach (var skill2 in skills)
            {
                skill2.HttpResponseInfo = null;
            }
            string json = JsonConvert.SerializeObject(skills, Formatting.Indented);
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
        }

        protected override void UnloadGUI()
        {
            base.UnloadGUI();

            MainWindow?.Dispose();
        }
    }
}