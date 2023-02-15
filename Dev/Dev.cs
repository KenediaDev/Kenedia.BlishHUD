using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Gw2Sharp.WebApi;
using Kenedia.Modules.Core.Models;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Map = Kenedia.Modules.Core.DataModels.Map;

namespace Kenedia.Modules.Dev
{
    [Export(typeof(Module))]
    public class Dev : BaseModule<Dev, StandardWindow, BaseSettingsModel>
    {
        private double _tick;
        private IGw2WebApiClient _apiClient;
        private Dictionary<int, Map> _maps = new();

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

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
            };

            Logger.Info($"Starting {Name} v." + Version.BaseVersion());
        }

        protected override void ReloadKey_Activated(object sender, EventArgs e)
        {
            base.ReloadKey_Activated(sender, e);
            FetchSkillCategories();
        }

        private async void FetchSkillCategories()
        {
            var skills = await Gw2ApiManager.Gw2ApiClient.V2.Skills.AllAsync();
            var categories = new List<string>();
            foreach (var skill in skills)
            {
                if(skill.Categories != null && skill.Categories.Count > 0) categories.AddRange(skill.Categories);
            }

            string json = JsonConvert.SerializeObject(categories.Distinct(), Formatting.Indented);
            File.WriteAllText($@"{Paths.ModulePath}\skillcategories.json", json);
        }

        private async void FetchAPI()
        {
            Debug.WriteLine($"FetchAPI!");
            LocalizedString state = new();

            bool done = state[Locale.English] != null && state[Locale.Spanish] != null && state[Locale.German] != null && state[Locale.French] != null;

            while (!done)
            {
                done = state[Locale.English] != null && state[Locale.Spanish] != null && state[Locale.German] != null && state[Locale.French] != null;

                Locale locale = GameService.Overlay.UserLocale.Value;

                if (state[locale] == null)
                {
                    var maps = await Gw2ApiManager.Gw2ApiClient.V2.Maps.AllAsync();

                    foreach (var m in maps)
                    {
                        bool exists = _maps.TryGetValue(m.Id, out Map map);

                        map ??= new Map();
                        map.Name = m.Name;
                        map.Id = m.Id;

                        if (!exists) _maps.Add(m.Id, map);
                        Debug.WriteLine($"{m.Name} [{m.Id}]");
                    }

                    string json = JsonConvert.SerializeObject(_maps, Formatting.Indented);
                    File.WriteAllText($@"{Paths.ModulePath}\maps.json", json);

                    state[locale] = "Done";
                }

                await Task.Delay(500);

                Debug.WriteLine($"English: {state[Locale.English] ?? "Not Done"} | Spanish: {state[Locale.Spanish] ?? "Not Done"} | German: {state[Locale.German] ?? "Not Done"} | French: {state[Locale.French] ?? "Not Done"} ");
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
    }
}