using Blish_HUD;
using Blish_HUD.Modules.Managers;
using Gw2Sharp.Models;
using Kenedia.Modules.BuildsManager.DataModels;
using Kenedia.Modules.Core.DataModels;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Models;
using Gw2Sharp.WebApi;
using System.Threading;
using Kenedia.Modules.BuildsManager.Models;

namespace Kenedia.Modules.BuildsManager.Services
{
    public class RaceDataEntry : MappedDataEntry<Races, Race>
    {
        //TODO: Check how to set to race selection
        public RaceDataEntry()
        {
            Race race = null;
            Items.Add(Races.None, race = new() { Id = Races.None });

            race.Names[Locale.English] =  "None";
            race.Names[Locale.German] = "Keine";
            race.Names[Locale.French] = "Aucun";
            race.Names[Locale.Spanish] = "Ninguno";
        }

        public async override Task<bool> LoadAndUpdate(string name, ByteIntMap map, string path, Gw2ApiManager gw2ApiManager, CancellationToken cancellationToken)
        {
            try
            {
                bool saveRequired = false;
                RaceDataEntry loaded = null;
                BuildsManager.Logger.Debug($"Load and if required update {name}");

                if (!DataLoaded && File.Exists(path))
                {
                    BuildsManager.Logger.Debug($"Load {name}.json");
                    string json = File.ReadAllText(path);
                    loaded = JsonConvert.DeserializeObject<RaceDataEntry>(json, SerializerSettings.Default);
                    DataLoaded = true;
                }

                Map = map;
                Items = loaded?.Items ?? Items;
                Version = loaded?.Version ?? Version;

                BuildsManager.Logger.Debug($"{name} Version {Version} | version {map.Version}");

                BuildsManager.Logger.Debug($"Check for missing values for {name}");
                var raceIds = await gw2ApiManager.Gw2ApiClient.V2.Races.IdsAsync(cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                {
                    return false;
                }

                var lang = GameService.Overlay.UserLocale.Value is Locale.Korean or Locale.Chinese ? Locale.English : GameService.Overlay.UserLocale.Value;
                var localeMissing = Items.Values.Where(item => item.Names[lang] == null)?.Select(e => $"{e.Id}");
                var missing = raceIds.Except(Items.Keys.Select(e => $"{e}")).Concat(localeMissing).Except(new string[] {$"{Races.None}"});

                if (map.Version > Version)
                {
                    Version = map.Version;
                    missing = raceIds;
                    BuildsManager.Logger.Debug($"The current version does not match the map version. Updating all values for {name}.");
                }

                if (missing.Count() > 0)
                {
                    var idSets = missing.ToList().ChunkBy(200);
                    saveRequired = saveRequired || idSets.Count > 0;

                    BuildsManager.Logger.Debug($"Fetch a total of {missing.Count()} {name} in {idSets.Count} sets.");
                    var apiSkills = await gw2ApiManager.Gw2ApiClient.V2.Skills.AllAsync(cancellationToken);
                    var profession = await gw2ApiManager.Gw2ApiClient.V2.Professions.GetAsync(ProfessionType.Guardian, cancellationToken);
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return false;
                    }

                    foreach (var ids in idSets)
                    {
                        var items = await gw2ApiManager.Gw2ApiClient.V2.Races.ManyAsync(ids, cancellationToken);

                        if (cancellationToken.IsCancellationRequested)
                        {
                            return false;
                        }

                        foreach (var item in items)
                        {
                            bool exists = Items.Values.TryFind(e => $"{e.Id}" == item.Id, out Race entryItem);
                            entryItem ??= new();

                            entryItem.Apply(item, apiSkills, profession.SkillsByPalette);

                            if (!exists)
                                Items.Add((Races)Enum.Parse(typeof(Races), item.Id), entryItem);
                        }
                    }
                }

                if (saveRequired)
                {
                    BuildsManager.Logger.Debug($"Saving {name}.json");
                    string json = JsonConvert.SerializeObject(this, SerializerSettings.Default);
                    File.WriteAllText(path, json);
                }

                DataLoaded = DataLoaded || Items.Count > 0;
                return true;
            }
            catch (Exception ex)
            {
                BuildsManager.Logger.Warn(ex, $"Failed to load {name} data.");
                return false;
            }
        }
    }
}
