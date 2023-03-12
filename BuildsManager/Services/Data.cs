using Blish_HUD;
using Blish_HUD.Modules.Managers;
using Gw2Sharp.Models;
using Kenedia.Modules.BuildsManager.DataModels;
using Kenedia.Modules.BuildsManager.DataModels.ItemUpgrades;
using Kenedia.Modules.BuildsManager.DataModels.LegendaryItems;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.BuildsManager.DataModels.Stats;
using Kenedia.Modules.Core.DataModels;
using Kenedia.Modules.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ApiSkill = Gw2Sharp.WebApi.V2.Models.Skill;
using ApiTraits = Gw2Sharp.WebApi.V2.Models.Trait;
using ApiPet = Gw2Sharp.WebApi.V2.Models.Pet;
using System.Diagnostics;
using System.Threading;
using SharpDX.Direct2D1.Effects;
using Microsoft.Xna.Framework.Content;

namespace Kenedia.Modules.BuildsManager.Services
{
    public class Data
    {
        private readonly Logger _logger = Logger.GetLogger(typeof(Data));
        private readonly ContentsManager _contentsManager;
        private readonly PathCollection _paths;
        private CancellationTokenSource _cancellationTokenSource;

        public Data(ContentsManager contentsManager, PathCollection paths)
        {
            _contentsManager = contentsManager;
            _paths = paths;
        }

        public Dictionary<int, OldSkillConnection> OldConnections { get; set; } = new();

        public ItemMapping ItemMap { get; set; } = new();

        public Dictionary<int, SkillConnection> SkillConnections { get; set; } = new();

        public Dictionary<int, BaseSkill> BaseSkills { get; set; } = new();

        public List<int> RuneIds { get; set; } = new();

        public List<int> SigilIds { get; set; } = new();

        public Dictionary<int, LegendaryArmor> Armors { get; private set; } = new();

        public Dictionary<int, LegendaryTrinket> Trinkets { get; private set; } = new();

        public Dictionary<int, LegendaryWeapon> Weapons { get; private set; } = new();

        public Dictionary<int, LegendaryUpgrade> Upgrades { get; private set; } = new();

        public Dictionary<ProfessionType, Profession> Professions { get; private set; } = new();

        public Dictionary<Races, Race> Races { get; private set; } = new();

        public Dictionary<int, Stat> Stats { get; private set; } = new();

        public Dictionary<int, Sigil> Sigils { get; private set; } = new();

        public Dictionary<int, Rune> Runes { get; private set; } = new();

        public Dictionary<int, Pet> Pets { get; private set; } = new();

        public Dictionary<int, int> PaletteBySkills { get; private set; } = new();

        public List<KeyValuePair<int, int>> SkillsByPalette { get; private set; } = new();

        public bool IsLoaded => Armors.Count > 0 && Professions.Count > 0 && Stats.Count > 0 && Sigils.Count > 0 && Runes.Count > 0 && Pets.Count > 0 && PaletteBySkills.Count > 0 && Races.Count > 0 && SkillConnections.Count > 0;
               
        public async Task Load()
        {
            try
            {
                _logger.Debug("Loading local data...");

                RuneIds = JsonConvert.DeserializeObject<List<int>>(await new StreamReader(_contentsManager.GetFileStream(@"data\rune_ids.json")).ReadToEndAsync());
                _logger.Debug("RuneIds loaded!");

                ItemMap = JsonConvert.DeserializeObject<ItemMapping>(await new StreamReader(_contentsManager.GetFileStream(@"data\ItemMapping.json")).ReadToEndAsync());
                _logger.Debug("Item Map loaded!");

                SigilIds = JsonConvert.DeserializeObject<List<int>>(await new StreamReader(_contentsManager.GetFileStream(@"data\sigil_ids.json")).ReadToEndAsync());
                _logger.Debug("SigilIds loaded!");

                await LoadBaseSkills();
                await LoadConnections();

                foreach (var prop in GetType().GetProperties())
                {
                    if (prop.Name is not nameof(RuneIds) and not nameof(SigilIds) and not nameof(SkillsByPalette) and not nameof(BaseSkills) and not nameof(SkillConnections) and not nameof(OldConnections) and not nameof(ItemMap) and not nameof(IsLoaded))
                    {
                        string path = $@"{_paths.ModuleDataPath}{prop.Name}.json";

                        if (File.Exists(path))
                        {
                            _logger.Debug($"Loading data for property {prop.Name} from '{$@"{_paths.ModuleDataPath}{prop.Name}.json"}'");
                            string json = await new StreamReader($@"{_paths.ModuleDataPath}{prop.Name}.json").ReadToEndAsync();
                            object data = JsonConvert.DeserializeObject(json, prop.PropertyType);
                            prop.SetValue(this, data);
                        }
                        else
                        {
                            _logger.Debug($"File for property {prop.Name} does not exist at '{$@"{_paths.ModuleDataPath}{prop.Name}.json"}'!");
                        }
                    }
                }

                SkillsByPalette = PaletteBySkills.ToList();

                _logger.Debug("All data loaded!");
            }
            catch (Exception ex)
            {
                _logger.Debug("Failed to load data!");
                _logger.Debug($"{ex}");
            }
        }

        public async Task LoadBaseSkills()
        {
            string path = $@"{_paths.ModuleDataPath}{nameof(BaseSkills)}.json";

            if (File.Exists(path))
            {
                _logger.Debug($"Loading data for property {nameof(BaseSkills)} from '{$@"{_paths.ModuleDataPath}{nameof(BaseSkills)}.json"}'");
                string json = await new StreamReader($@"{_paths.ModuleDataPath}{nameof(BaseSkills)}.json").ReadToEndAsync();
                object data = JsonConvert.DeserializeObject(json, typeof(Dictionary<int, BaseSkill>));

                BaseSkills = JsonConvert.DeserializeObject<Dictionary<int, BaseSkill>>(await new StreamReader(_contentsManager.GetFileStream(@"data\missing_skills.json")).ReadToEndAsync());
                foreach (var item in (Dictionary<int, BaseSkill>)data)
                {
                    if(!BaseSkills.ContainsKey(item.Key))
                    {
                        BaseSkills.Add(item.Key, item.Value);
                    }
                }
            }
            else
            {
                _logger.Debug($"File for property {nameof(BaseSkills)} does not exist at '{$@"{_paths.ModuleDataPath}{nameof(BaseSkills)}.json"}'!");
            }
        }

        public async Task ImportOldConnections()
        {
            string path = $@"{_paths.ModuleDataPath}{nameof(SkillConnections)}.json";

            if (File.Exists(path))
            {
                _logger.Debug($"Loading data for property {nameof(OldConnections)} from '{$@"{_paths.ModuleDataPath}{nameof(OldConnections)}.json"}'");
                string json = await new StreamReader($@"{_paths.ModuleDataPath}{nameof(OldConnections)}.json").ReadToEndAsync();
                object data = JsonConvert.DeserializeObject(json, typeof(Dictionary<int, OldSkillConnection>));
                OldConnections = (Dictionary<int, OldSkillConnection>)data;

                foreach (var skillConnection in OldConnections)
                {
                    if (!SkillConnections.ContainsKey(skillConnection.Key))
                    {
                        SkillConnections.Add(skillConnection.Key, new SkillConnection(skillConnection.Value));
                    }
                }

                foreach (var item in BaseSkills)
                {
                    if (!SkillConnections.TryGetValue(item.Key, out SkillConnection connection))
                    {
                        SkillConnections.Add(item.Key, new SkillConnection() { Id = item.Value.Id, AssetId = item.Value.AssetId });
                    }
                    
                    if (connection != null && connection.Professions.Count <= 0)
                    {
                        foreach (string p in item.Value.Professions)
                        {
                            if (Enum.TryParse(p, out ProfessionType pt))
                            {
                                connection.Professions.Add(pt);
                            }
                        }
                    }
                    
                    if (connection != null)
                    {
                        connection.Slot = item.Value.Slot;
                    }
                }

                await Save();
            }
            else
            {
                _logger.Debug($"File for property {nameof(SkillConnections)} does not exist at '{$@"{_paths.ModuleDataPath}{nameof(SkillConnections)}.json"}'!");
            }
        }

        public async Task LoadConnections()
        {
            string path = $@"{_paths.ModuleDataPath}{nameof(SkillConnections)}.json";
            string oldpath = $@"{_paths.ModuleDataPath}{nameof(SkillConnections)}.json";

            if (File.Exists(path))
            {
                string json;
                object data;

                if (File.Exists(oldpath))
                {
                    _logger.Debug($"Loading data for property {nameof(OldConnections)} from '{$@"{_paths.ModuleDataPath}{nameof(OldConnections)}.json"}'");
                    json = await new StreamReader($@"{_paths.ModuleDataPath}{nameof(OldConnections)}.json").ReadToEndAsync();
                    data = JsonConvert.DeserializeObject(json, typeof(Dictionary<int, OldSkillConnection>));
                    OldConnections = (Dictionary<int, OldSkillConnection>)data;
                }

                _logger.Debug($"Loading data for property {nameof(SkillConnections)} from '{$@"{_paths.ModuleDataPath}{nameof(SkillConnections)}.json"}'");
                json = await new StreamReader($@"{_paths.ModuleDataPath}{nameof(SkillConnections)}.json").ReadToEndAsync();
                data = JsonConvert.DeserializeObject(json, typeof(Dictionary<int, SkillConnection>));
                SkillConnections = (Dictionary<int, SkillConnection>)data;

                foreach(var item in BaseSkills)
                {
                    if (!SkillConnections.TryGetValue(item.Key, out SkillConnection connection))
                    {
                        SkillConnection cn;
                        SkillConnections.Add(item.Key, cn = new SkillConnection() { Id = item.Value.Id, AssetId = item.Value.AssetId, Slot = item.Value.Slot });

                        foreach (string p in item.Value.Professions)
                        {
                            if (Enum.TryParse(p, out ProfessionType pt) && !cn.Professions.Contains(pt))
                            {
                                cn.Professions.Add(pt);
                            }
                        }
                    }
                }
            }
            else
            {
                _logger.Debug($"File for property {nameof(SkillConnections)} does not exist at '{$@"{_paths.ModuleDataPath}{nameof(SkillConnections)}.json"}'!");
            }
        }

        public async Task Save()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            await Task.Delay(1000, _cancellationTokenSource.Token);
            if (_cancellationTokenSource.IsCancellationRequested) return;

            string json = JsonConvert.SerializeObject(SkillConnections, Formatting.Indented);
            File.WriteAllText($@"{_paths.ModuleDataPath}\SkillConnections.json", json);

            json = JsonConvert.SerializeObject(OldConnections, Formatting.Indented);
            File.WriteAllText($@"{_paths.ModuleDataPath}\OldConnections.json", json);
        }
    }
}
