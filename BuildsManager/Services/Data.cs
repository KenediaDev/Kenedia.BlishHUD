using Blish_HUD;
using Blish_HUD.Modules.Managers;
using Gw2Sharp.Models;
using Kenedia.Modules.BuildsManager.DataModels;
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
using System.Threading;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Extensions;

namespace Kenedia.Modules.BuildsManager.Services
{
    public class StatMap
    {
        public EquipmentStat Stat { get; set; }

        public string Name { get; set; }

        public List<int> Ids { get; set; }
    }

    public class StatMapping : List<StatMap>
    {

    }

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

        public StatMapping StatMap { get; set; } = new();

        public Dictionary<int, SkillConnection> SkillConnections { get; set; } = new();

        public Dictionary<int, BaseSkill> BaseSkills { get; set; } = new();

        public Dictionary<int, BaseSkill> MissingSkills { get; set; } = new();

        public Dictionary<int, Armor> Armors { get; private set; } = new();

        public Dictionary<int, Trinket> Trinkets { get; private set; } = new();

        public Dictionary<int, DataModels.Items.Weapon> Weapons { get; private set; } = new();

        public Dictionary<ProfessionType, Profession> Professions { get; private set; } = new();

        public Dictionary<Races, Race> Races { get; private set; } = new();

        public Dictionary<int, Stat> Stats { get; private set; } = new();

        public Dictionary<int, Pet> Pets { get; private set; } = new();

        public Dictionary<int, int> PaletteBySkills { get; private set; } = new();

        public List<KeyValuePair<int, int>> SkillsByPalette { get; private set; } = new();

        public bool IsLoaded => Armors.Count > 0 && Professions.Count > 0 && Stats.Count > 0 && Pets.Count > 0 && Races.Count > 0 && SkillConnections.Count > 0;

        public Dictionary<int, Trinket> Backs { get; private set; } = new();

        public Dictionary<int, Sigil> PvpSigils { get; private set; } = new();

        public Dictionary<int, Sigil> PveSigils { get; private set; } = new();

        public Dictionary<int, Rune> PvpRunes { get; private set; } = new();

        public Dictionary<int, Rune> PveRunes { get; private set; } = new();

        public Dictionary<int, DataModels.Items.Utility> Utilities { get; private set; } = new();

        public Dictionary<int, Nourishment> Nourishments { get; private set; } = new();

        public Dictionary<int, Infusion> Infusions { get; private set; } = new();

        public Dictionary<int, Enrichment> Enrichments { get; private set; } = new();

        public bool TryGetItemsFor<T>(GearTemplateSlot slot, out Dictionary<int, T> dict) where T : BaseItem
        {
            var type = typeof(BaseItem);
            switch (slot)
            {
                case GearTemplateSlot.Head:
                case GearTemplateSlot.Shoulder:
                case GearTemplateSlot.Chest:
                case GearTemplateSlot.Hand:
                case GearTemplateSlot.Leg:
                case GearTemplateSlot.Foot:
                case GearTemplateSlot.AquaBreather:
                    dict = Armors.ToDictionary(e => e.Key, e => e.Value as T);
                    return true;

                //case GearTemplateSlot.PvpAmulet:
                //    dict = PvpAmulets.ToDictionary(e => e.Key, e => e.Value as T);
                //    return true;

                case GearTemplateSlot.MainHand:
                case GearTemplateSlot.AltMainHand:
                case GearTemplateSlot.OffHand:
                case GearTemplateSlot.AltOffHand:
                case GearTemplateSlot.Aquatic:
                case GearTemplateSlot.AltAquatic:
                    dict = Weapons.ToDictionary(e => e.Key, e => e.Value as T);
                    return true;

                case GearTemplateSlot.Back:
                    dict = Backs.ToDictionary(e => e.Key, e => e.Value as T);
                    return true;

                case GearTemplateSlot.Nourishment:
                    dict = Nourishments.ToDictionary(e => e.Key, e => e.Value as T);
                    return true;

                case GearTemplateSlot.Utility:
                    dict = Utilities.ToDictionary(e => e.Key, e => e.Value as T);
                    return true;

                default:
                    dict = null;
                    return false;
            }
        }

        public Dictionary<int, BaseItem> GetUpgradesFor<item>(GearTemplateSlot slot, bool pve = true)
        {
            return slot switch
            {
                GearTemplateSlot.PvpAmulet => PvpRunes.ToDictionary(e => e.Key, e => (BaseItem)e.Value),
                GearTemplateSlot.MainHand or GearTemplateSlot.AltMainHand or GearTemplateSlot.OffHand or GearTemplateSlot.AltOffHand or GearTemplateSlot.Aquatic or GearTemplateSlot.AltAquatic => (pve ? PveSigils : PvpSigils).ToDictionary(e => e.Key, e => (BaseItem)e.Value),
                GearTemplateSlot.Head or GearTemplateSlot.Shoulder or GearTemplateSlot.Chest or GearTemplateSlot.Hand or GearTemplateSlot.Leg or GearTemplateSlot.Foot or GearTemplateSlot.AquaBreather => (pve ? PveRunes : PvpRunes).ToDictionary(e => e.Key, e => (BaseItem)e.Value),
                _ => null,
            };
        }

        public async Task Load()
        {
            try
            {
                _logger.Debug("Loading local data...");

                ItemMap = JsonConvert.DeserializeObject<ItemMapping>(await new StreamReader(_contentsManager.GetFileStream(@"data\ItemMapping.json")).ReadToEndAsync());
                StatMap = JsonConvert.DeserializeObject<StatMapping>(await new StreamReader(_contentsManager.GetFileStream(@"data\stats_map.json")).ReadToEndAsync());

                _logger.Debug("Item Map loaded!");

                await LoadMissingSkills();
                await LoadBaseSkills();
                await LoadConnections();

                foreach (var prop in GetType().GetProperties())
                {
                    if (prop.Name is not nameof(SkillsByPalette) and not nameof(BaseSkills) and not nameof(SkillConnections) and not nameof(OldConnections) and not nameof(ItemMap) and not nameof(StatMap) and not nameof(IsLoaded))
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

                _logger.Debug($"Import missing Skills");
                foreach (var baseSkill in MissingSkills)
                {
                    Skill skill = new(baseSkill.Value); 

                    foreach(var prof in skill.Professions)
                    {
                        if (!Professions[prof].Skills.ContainsKey(skill.Id))
                        {
                            Professions[prof].Skills.Add(skill.Id, skill);
                        }
                    }
                }

                _logger.Debug("All data loaded!");
            }
            catch (Exception ex)
            {
                _logger.Debug("Failed to load data!");
                _logger.Debug($"{ex}");
            }
        }

        public async Task LoadMissingSkills()
        {
            MissingSkills = JsonConvert.DeserializeObject<Dictionary<int, BaseSkill>>(await new StreamReader(_contentsManager.GetFileStream(@"data\missing_skills.json")).ReadToEndAsync());

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
                    if (!BaseSkills.ContainsKey(item.Key))
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
            string path = $@"{_paths.ModuleDataPath}{nameof(OldConnections)}.json";

            if (File.Exists(path))
            {
                _logger.Debug($"Loading data for property {nameof(OldConnections)} from '{$@"{_paths.ModuleDataPath}{nameof(OldConnections)}.json"}'");
                string json = await new StreamReader($@"{_paths.ModuleDataPath}{nameof(OldConnections)}.json").ReadToEndAsync();
                object data = JsonConvert.DeserializeObject(json, typeof(Dictionary<int, OldSkillConnection>));
                OldConnections = (Dictionary<int, OldSkillConnection>)data;
                SkillConnections.Clear();

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
                _logger.Debug($"File for property {nameof(OldConnections)} does not exist at '{$@"{_paths.ModuleDataPath}{nameof(OldConnections)}.json"}'!");
            }
        }

        public async Task LoadConnections()
        {
            string oldpath = $@"{_paths.ModuleDataPath}{nameof(OldConnections)}.json";
            SkillConnections = (Dictionary<int, SkillConnection>)JsonConvert.DeserializeObject(await new StreamReader(_contentsManager.GetFileStream(@"data\skill_connections.json")).ReadToEndAsync(), typeof(Dictionary<int, SkillConnection>));

            //Adding missing skills, prob obsolete atm
            foreach (var item in BaseSkills)
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

            if (File.Exists(oldpath))
            {
                string json;
                object data;

                _logger.Debug($"Loading data for property {nameof(OldConnections)} from '{$@"{_paths.ModuleDataPath}{nameof(OldConnections)}.json"}'");
                json = await new StreamReader($@"{_paths.ModuleDataPath}{nameof(OldConnections)}.json").ReadToEndAsync();
                data = JsonConvert.DeserializeObject(json, typeof(Dictionary<int, OldSkillConnection>));
                OldConnections = (Dictionary<int, OldSkillConnection>)data;
            }
        }

        public Skill GetSkillById(int id)
        {

            foreach (var profession in Professions)
            {
                if (profession.Value.Skills.TryGetValue(id, out Skill skill)) return skill;
            }

            return null;
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
