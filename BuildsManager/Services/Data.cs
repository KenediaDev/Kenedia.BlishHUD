using Blish_HUD;
using Blish_HUD.Modules.Managers;
using Kenedia.Modules.BuildsManager.DataModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kenedia.Modules.BuildsManager.Services
{
    public class Data
    {
        private readonly Logger _logger = Logger.GetLogger(typeof(Data));
        private readonly ContentsManager _contentsManager;

        public Data(ContentsManager contentsManager)
        {
            _contentsManager = contentsManager;
        }

        public Dictionary<int, LegendaryArmor> Armors{ get; set; } = new();

        public Dictionary<int, LegendaryTrinket> Trinkets{ get; set; } = new();

        public Dictionary<int, LegendaryWeapon> Weapons { get; set; } = new();

        public Dictionary<int, LegendaryUpgrade> Upgrades{ get; set; } = new();

        public async Task Load() 
        {
            try
            {
                Armors = JsonConvert.DeserializeObject<Dictionary<int, LegendaryArmor>>(await new StreamReader(_contentsManager.GetFileStream($@"data\{"armors"}.json")).ReadToEndAsync());
                _logger.Info("Armors loaded!");
                Trinkets = JsonConvert.DeserializeObject<Dictionary<int, LegendaryTrinket>>(await new StreamReader(_contentsManager.GetFileStream($@"data\{"trinkets"}.json")).ReadToEndAsync());
                _logger.Info("Trinkets loaded!");
                Weapons = JsonConvert.DeserializeObject<Dictionary<int, LegendaryWeapon>>(await new StreamReader(_contentsManager.GetFileStream($@"data\{"weapons"}.json")).ReadToEndAsync());
                _logger.Info("Weapons loaded!");
                Upgrades = JsonConvert.DeserializeObject<Dictionary<int, LegendaryUpgrade>>(await new StreamReader(_contentsManager.GetFileStream($@"data\{"upgrades"}.json")).ReadToEndAsync());
                _logger.Info("Upgrades loaded!");

                _logger.Info("All data loaded!");
            }
            catch (Exception ex)
            {
                _logger.Info("Failed to load data!");
                Debug.WriteLine($"{ex}");
            }
        }
    }
}
