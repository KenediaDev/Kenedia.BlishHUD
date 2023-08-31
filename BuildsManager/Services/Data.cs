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

namespace Kenedia.Modules.BuildsManager.Services
{
    public class Data : IDisposable
    {
        private readonly Logger _logger = Logger.GetLogger(typeof(Data));
        private readonly ContentsManager _contentsManager;
        private readonly PathCollection _paths;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isDisposed;

        public Data(ContentsManager contentsManager, PathCollection paths)
        {
            _contentsManager = contentsManager;
            _paths = paths;

            ItemMaps = new(_paths);
        }

        public Dictionary<int, OldSkillConnection> OldConnections { get; set; } = new();

        public ItemMapCollection ItemMaps { get; set; } 

        public ItemMapping ItemMap { get; set; } = new();

        public StatMapping StatMap { get; set; } = new();

        public Dictionary<int, SkillConnection> SkillConnections { get; set; } = new();

        public Dictionary<int, BaseSkill> MissingSkills { get; set; } = new();

        public Dictionary<int, Armor> Armors { get; private set; } = new();

        public Dictionary<int, Trinket> Trinkets { get; private set; } = new();

        public Dictionary<int, PvpAmulet> PvpAmulets { get; private set; } = new();

        public Dictionary<int, Relic> Relics { get; private set; } = new();

        public Dictionary<int, PowerCore> PowerCores { get; private set; } = new();

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

        public Dictionary<int, DataModels.Items.Enhancement> Utilities { get; private set; } = new();

        public Dictionary<int, Nourishment> Nourishments { get; private set; } = new();

        public Dictionary<int, Infusion> Infusions { get; private set; } = new();

        public Dictionary<int, Enrichment> Enrichments { get; private set; } = new();

        public bool TryGetItemsFor<T>(TemplateSlotType slot, out Dictionary<int, T> dict) where T : BaseItem
        {
            var type = typeof(BaseItem);
            switch (slot)
            {
                case TemplateSlotType.Head:
                case TemplateSlotType.Shoulder:
                case TemplateSlotType.Chest:
                case TemplateSlotType.Hand:
                case TemplateSlotType.Leg:
                case TemplateSlotType.Foot:
                case TemplateSlotType.AquaBreather:
                    dict = Armors.ToDictionary(e => e.Key, e => e.Value as T);
                    return true;

                //case GearTemplateSlot.PvpAmulet:
                //    dict = PvpAmulets.ToDictionary(e => e.Key, e => e.Value as T);
                //    return true;

                case TemplateSlotType.MainHand:
                case TemplateSlotType.AltMainHand:
                case TemplateSlotType.OffHand:
                case TemplateSlotType.AltOffHand:
                case TemplateSlotType.Aquatic:
                case TemplateSlotType.AltAquatic:
                    dict = Weapons.ToDictionary(e => e.Key, e => e.Value as T);
                    return true;

                case TemplateSlotType.Back:
                    dict = Backs.ToDictionary(e => e.Key, e => e.Value as T);
                    return true;

                case TemplateSlotType.Nourishment:
                    dict = Nourishments.ToDictionary(e => e.Key, e => e.Value as T);
                    return true;

                case TemplateSlotType.Enhancement:
                    dict = Utilities.ToDictionary(e => e.Key, e => e.Value as T);
                    return true;

                default:
                    dict = null;
                    return false;
            }
        }

        public Dictionary<int, BaseItem> GetUpgradesFor<item>(TemplateSlotType slot, bool pve = true)
        {
            return slot switch
            {
                TemplateSlotType.PvpAmulet => PvpRunes.ToDictionary(e => e.Key, e => (BaseItem)e.Value),
                TemplateSlotType.MainHand or TemplateSlotType.AltMainHand or TemplateSlotType.OffHand or TemplateSlotType.AltOffHand or TemplateSlotType.Aquatic or TemplateSlotType.AltAquatic => (pve ? PveSigils : PvpSigils).ToDictionary(e => e.Key, e => (BaseItem)e.Value),
                TemplateSlotType.Head or TemplateSlotType.Shoulder or TemplateSlotType.Chest or TemplateSlotType.Hand or TemplateSlotType.Leg or TemplateSlotType.Foot or TemplateSlotType.AquaBreather => (pve ? PveRunes : PvpRunes).ToDictionary(e => e.Key, e => (BaseItem)e.Value),
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
                await LoadConnections();

                await ItemMaps.FetchAndLoad();

                foreach (var prop in GetType().GetProperties())
                {
                    if (prop.Name is not nameof(SkillsByPalette) and not nameof(SkillConnections) and not nameof(ItemMaps) and not nameof(OldConnections) and not nameof(ItemMap) and not nameof(StatMap) and not nameof(IsLoaded))
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
                        if (Professions.TryGetValue(prof, out Profession profession) && !profession.Skills.ContainsKey(skill.Id))
                        {
                            profession.Skills.Add(skill.Id, skill);
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

        public async Task LoadConnections()
        {
            SkillConnections = (Dictionary<int, SkillConnection>)JsonConvert.DeserializeObject(await new StreamReader(_contentsManager.GetFileStream(@"data\skill_connections.json")).ReadToEndAsync(), typeof(Dictionary<int, SkillConnection>));
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

        public void Dispose()
        {
            if (_isDisposed) return;

            _isDisposed = true;

            ItemMap?.Dispose();

            OldConnections?.Clear();
            StatMap?.Clear();
            SkillConnections?.Clear();
            MissingSkills?.Clear();
            Armors?.Clear();
            Trinkets?.Clear();
            PvpAmulets?.Clear();
            Relics?.Clear();
            PowerCores?.Clear();
            Weapons?.Clear();
            Professions?.Clear();
            Races?.Clear();
            Stats?.Clear();
            Pets?.Clear();
            PaletteBySkills?.Clear();
            SkillsByPalette?.Clear();
            Backs?.Clear();
            PvpSigils?.Clear();
            PveSigils?.Clear();
            PvpRunes?.Clear();
            PveRunes?.Clear();
            Utilities?.Clear();
            Nourishments?.Clear();
            Infusions?.Clear();
            Enrichments?.Clear();
        }
    }
}
