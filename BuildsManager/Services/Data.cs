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

namespace Kenedia.Modules.BuildsManager.Services
{
    public class Data
    {
        private readonly Logger _logger = Logger.GetLogger(typeof(Data));
        private readonly ContentsManager _contentsManager;
        private readonly PathCollection _paths;

        public Data(ContentsManager contentsManager, PathCollection paths)
        {
            _contentsManager = contentsManager;
            _paths = paths;
        }

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

        public bool IsLoaded => Armors.Count > 0 && Professions.Count > 0 && Stats.Count > 0 && Sigils.Count > 0 && Runes.Count > 0 && Pets.Count > 0 && PaletteBySkills.Count > 0 && Races.Count > 0;

        public async Task Load()
        {
            try
            {
                _logger.Debug("Loading local data...");

                RuneIds = JsonConvert.DeserializeObject<List<int>>(await new StreamReader(_contentsManager.GetFileStream(@"data\rune_ids.json")).ReadToEndAsync());
                _logger.Debug("RuneIds loaded!");

                SigilIds = JsonConvert.DeserializeObject<List<int>>(await new StreamReader(_contentsManager.GetFileStream(@"data\sigil_ids.json")).ReadToEndAsync());
                _logger.Debug("SigilIds loaded!");

                foreach (var prop in GetType().GetProperties())
                {
                    if (prop.Name is not nameof(RuneIds) and not nameof(SigilIds) and not nameof(SkillsByPalette) and not nameof(IsLoaded))
                    {
                        string path = $@"{_paths.ModulePath}\data\{prop.Name}.json";

                        if (File.Exists(path))
                        {
                            _logger.Debug($"Loading data for property {prop.Name} from '{$@"{_paths.ModulePath}\data\{prop.Name}.json"}'");
                            string json = await new StreamReader($@"{_paths.ModulePath}\data\{prop.Name}.json").ReadToEndAsync();
                            object data = JsonConvert.DeserializeObject(json, prop.PropertyType);
                            prop.SetValue(this, data);
                        }
                        else
                        {
                            _logger.Debug($"File for property {prop.Name} does not exist at '{$@"{_paths.ModulePath}\data\{prop.Name}.json"}'!");
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
    }
}
