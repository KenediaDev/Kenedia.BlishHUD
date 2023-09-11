using Blish_HUD;
using Blish_HUD.Modules.Managers;
using Kenedia.Modules.BuildsManager.DataModels.Stats;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Version = SemVer.Version;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Models;
using Gw2Sharp.WebApi;
using System.Threading;
using Kenedia.Modules.BuildsManager.Models;

namespace Kenedia.Modules.BuildsManager.Services
{
    public class StatMappedDataEntry : MappedDataEntry<int, Stat>
    {
        public async override Task<bool> LoadAndUpdate(string name, ByteIntMap map, string path, Gw2ApiManager gw2ApiManager, CancellationToken cancellationToken)
        {
            try
            {
                bool saveRequired = false;
                MappedDataEntry<int, Stat> loaded = null;

                BuildsManager.Logger.Debug($"Load {name}.json");

                if (!DataLoaded && File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    loaded = JsonConvert.DeserializeObject<MappedDataEntry<int, Stat>>(json, SerializerSettings.Default);
                    DataLoaded = true;
                }

                Map = map;
                Items = loaded?.Items ?? Items;
                Version = loaded?.Version ?? Version;

                BuildsManager.Logger.Debug($"{name} Version {Version} | version {map.Version}");

                var lang = GameService.Overlay.UserLocale.Value is Locale.Korean or Locale.Chinese ? Locale.English : GameService.Overlay.UserLocale.Value;
                var fetchIds = Items.Values.Where(item => (item.Names[lang] == null) || (item.MappedId == 0))?.Select(e => e.Id);

                bool fetchAll = map.Version > Version;

                if (map.Version > Version)
                {
                    Version = map.Version;
                    fetchIds = fetchIds.Concat(Map.Values.Except(Items.Keys).Except(Map.Ignored.Values));
                    saveRequired = true;

                    if (fetchAll)
                        BuildsManager.Logger.Debug($"The current version does not match the map version. Updating all values for {name}.");
                }

                if (fetchIds.Count() > 0)
                {
                    var idSets = fetchIds.ToList().ChunkBy(200);

                    saveRequired = saveRequired || idSets.Count > 0;
                    BuildsManager.Logger.Debug($"Fetch a total of {fetchIds.Count()} in {idSets.Count} sets.");
                    foreach (var ids in idSets)
                    {
                        var items = await gw2ApiManager.Gw2ApiClient.V2.Itemstats.ManyAsync(ids, cancellationToken);
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return false;
                        }

                        foreach (var item in items)
                        {
                            bool exists = Items.Values.TryFind(e => e.Id == item.Id, out Stat entryItem);
                            entryItem ??= new()
                            {
                                MappedId = Map?.Items?.FirstOrDefault(e => e.Value == item.Id).Key ?? 0,
                            };

                            entryItem?.Apply(item);

                            if (!exists)
                                Items.Add(item.Id, entryItem);
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
