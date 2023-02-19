using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Gw2Sharp;
using Gw2Sharp.WebApi;
using Gw2Sharp.WebApi.V2.Clients;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.Core.DataModels;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Dev.Models;
using Kenedia.Modules.Dev.Views;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using SharpDX.Direct2D1.Effects;
using SharpDX.X3DAudio;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Map = Kenedia.Modules.Core.DataModels.Map;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.Dev
{
    [Export(typeof(Module))]
    public class Dev : BaseModule<Dev, MainWindow, BaseSettingsModel>
    {
        private double _tick;
        private IGw2WebApiClient _apiClient;
        private Dictionary<int, Map> _maps = new();
        private DeepObservableDictionary<int, CustomClass> o;
        private Dictionary<int, SkillConnection> _connections = new();
        private static CancellationTokenSource _cancellationTokenSource;

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

            HasGUI = true;
            Logger.Info($"Starting {Name} v." + Version.BaseVersion());
        }

        protected override async void ReloadKey_Activated(object sender, EventArgs e)
        {
            GetSkillConnections();

            base.ReloadKey_Activated(sender, e);

            //FetchSkillCategories();
            //GetSkillConnections
        }

        private void ObservableDictionaryTest()
        {
            o ??= new DeepObservableDictionary<int, CustomClass>
            {
                {1, new CustomClass() { Text = "Apple" } },
            };

            if (!o.ContainsKey(4))
            {
                o.ItemChanged += O_ItemChanged;
                o.CollectionChanged += O_ItemChanged; ;
                o[4] = new() { Text = "Salad", };
            }
            else
            {
                o[4].Text = "Juice";
            }
        }

        private void O_ItemChanged(object sender, PropertyChangedEventArgs e)
        {
            Debug.WriteLine($"Sender '[{sender}]' changed [{e.PropertyName}]");
        }

        public class CustomClass : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            private string _text;

            public string Text { get => _text; set => Common.SetProperty(ref _text, value, PropertyChanged); }
        }

        public async Task Save()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            Debug.WriteLine($"Request SAVE");
            await Task.Delay(1000, _cancellationTokenSource.Token);
            if (_cancellationTokenSource.IsCancellationRequested) return;

            Debug.WriteLine($"Perform SAVE");
            string json = JsonConvert.SerializeObject(_connections, Formatting.Indented);
            System.IO.File.WriteAllText($@"{Paths.ModulePath}\SkillConnections.json", json);
        }

        private async void GetSkillConnections()
        {
            var apiSkills = await Gw2ApiManager.Gw2ApiClient.V2.Skills.AllAsync();
            var apiTraits = await Gw2ApiManager.Gw2ApiClient.V2.Traits.AllAsync();
            var traits = apiTraits.ToList();
            Skills = apiSkills.ToList();

            List<int> getChain(Skill targetSkill, List<int> chain = null)
            {
                if (targetSkill == null) return null;

                chain ??= new List<int>();
                chain.Add(targetSkill.Id);

                if (targetSkill.NextChain != null)
                {
                    var s = Skills.Find(e => e != targetSkill && e.Id == targetSkill.NextChain);
                    if (s != null && !(chain?.Contains(s.Id) == true))
                    {
                        _ = getChain(s, chain);
                    }
                }

                return chain;
            }

            List<int> getFlips(Skill targetSkill, List<int> flips = null)
            {
                if (targetSkill == null) return null;

                flips ??= new List<int>();
                flips.Add(targetSkill.Id);

                if (targetSkill.NextChain != null)
                {
                    var s = Skills.Find(e => e != targetSkill && e.Id == targetSkill.NextChain);
                    if (s != null && !(flips?.Contains(s.Id) == true))
                    {
                        _ = getFlips(s, flips);
                    }
                }

                return flips;
            }

            _connections = new Dictionary<int, SkillConnection>();
            foreach (var skill in Skills)
            {
                if (skill.Type != SkillType.Monster && skill.Professions.Count > 0)
                {
                    var connection = new SkillConnection()
                    {
                        Id = skill.Id,
                        Weapon = skill.WeaponType?.ToEnum() ?? null,
                        DualWeapon = skill.DualWield != null && Enum.TryParse(skill.DualWield, out SkillWeaponType weapon) ? weapon : null,
                        Specialization = skill.Specialization != null ? (Specializations)skill.Specialization : null,
                        Attunement = skill.Attunement != null ? skill.Attunement?.ToEnum() : null,
                        DualAttunement = skill.DualAttunement != null ? skill.DualAttunement?.ToEnum() : null,
                        Enviroment = skill.Flags.Count() > 0 && skill.Flags.Aggregate((x, y) => x |= y.ToEnum()).Value.HasFlag(SkillFlag.NoUnderwater) ? Enviroment.Terrestrial : Enviroment.Any,
                    };

                    if (skill.ToolbeltSkill != null)
                    {
                        connection.Toolbelt = skill.ToolbeltSkill;
                    }

                    if (skill.NextChain != null)
                    {
                        connection.Chain = getChain(skill);
                    }

                    if (skill.BundleSkills != null)
                    {
                        connection.Bundle = skill.BundleSkills.ToList();
                    }

                    if (skill.TransformSkills != null)
                    {
                        connection.Transform = skill.TransformSkills.ToList();
                    }

                    if (skill.FlipSkill != null)
                    {
                        connection.FlipSkills = getFlips(skill);
                    }

                    if (skill.TraitedFacts != null)
                    {
                        foreach (var t in skill.TraitedFacts)
                        {
                            if (t.RequiresTrait != null)
                            {
                                var trait = traits.Find(e => e.Id == t.RequiresTrait);
                                if (trait != null && trait.Skills != null)
                                {
                                    connection.Traited ??= new();
                                    connection.Traited[trait.Id] = trait.Skills.Select(e => e.Id).ToList();
                                }
                            }
                        }
                    }

                    if (skill.Slot == SkillSlot.Weapon1 && skill.Professions.Contains("Thief"))
                    {
                        connection.Stealth = Skills.Find(e => e.Slot == skill.Slot && e.WeaponType == skill.WeaponType && e.Categories?.Contains("StealthAttack") == true)?.Id;
                    }

                    if (skill.Slot == SkillSlot.Weapon1 && skill.Professions.Contains("Mesmer"))
                    {
                        connection.Ambush = Skills.Find(e => e.Slot == skill.Slot && e.WeaponType == skill.WeaponType && e.Description?.Contains("Ambush") == true)?.Id;
                    }

                    _connections.Add(skill.Id, connection);
                }
            }

            var cnts = _connections.Values.ToList();
            foreach (var connection in _connections)
            {
                connection.Value.Parent = cnts.Find(e =>
                e.Chain?.Contains(connection.Value.Id) == true ||
                e.Bundle?.Contains(connection.Value.Id) == true ||
                e.Transform?.Contains(connection.Value.Id) == true ||
                e.FlipSkills?.Contains(connection.Value.Id) == true ||
                (e.Toolbelt != null && e.Toolbelt == connection.Value.Id)
                )?.Id;

                if (connection.Value.Parent == null)
                {
                    foreach (var s in cnts)
                    {
                        if (s.Traited != null)
                        {
                            foreach (var t in s.Traited)
                            {
                                if (t.Value.Contains(connection.Value.Id))
                                {
                                    connection.Value.Parent = s.Id;
                                    break;
                                }
                            }
                        }
                        break;
                    }
                }
            }

            string json = JsonConvert.SerializeObject(_connections, Formatting.Indented);
            System.IO.File.WriteAllText($@"{Paths.ModulePath}\SkillConnections.json", json);

            LoadGUI();
        }

        private async void FetchSkillCategories()
        {
            var skills = await Gw2ApiManager.Gw2ApiClient.V2.Skills.AllAsync();
            var categories = new List<string>();
            foreach (var skill in skills)
            {
                if (skill.Categories != null && skill.Categories.Count > 0) categories.AddRange(skill.Categories);
            }

            string json = JsonConvert.SerializeObject(categories.Distinct(), Formatting.Indented);
            System.IO.File.WriteAllText($@"{Paths.ModulePath}\skillcategories.json", json);
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
                    System.IO.File.WriteAllText($@"{Paths.ModulePath}\maps.json", json);

                    state[locale] = "Done";
                }

                await Task.Delay(500);

                Debug.WriteLine($"English: {state[Locale.English] ?? "Not Done"} | Spanish: {state[Locale.Spanish] ?? "Not Done"} | German: {state[Locale.German] ?? "Not Done"} | French: {state[Locale.French] ?? "Not Done"} ");
            }
        }

        protected override async Task LoadAsync()
        {
            await base.LoadAsync();

            _connections = JsonConvert.DeserializeObject<Dictionary<int, SkillConnection>>(await new StreamReader($@"{Paths.ModulePath}\SkillConnections.json").ReadToEndAsync());

            var apiSkills = await Gw2ApiManager.Gw2ApiClient.V2.Skills.AllAsync();
            Skills = apiSkills.ToList();
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
            if (Skills.Count <= 0) return;

            base.LoadGUI();

            var settingsBg = AsyncTexture2D.FromAssetId(155997);
            Texture2D cutSettingsBg = settingsBg.Texture.GetRegion(0, 0, settingsBg.Width - 482, settingsBg.Height - 390);

            MainWindow = new MainWindow(
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
                Connections = _connections,
            };

            MainWindow.Show();
        }

        protected override void UnloadGUI()
        {
            base.UnloadGUI();

            MainWindow?.Dispose();
        }
    }
}