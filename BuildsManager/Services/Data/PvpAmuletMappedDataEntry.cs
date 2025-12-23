using Blish_HUD;
using Blish_HUD.Modules.Managers;
using Gw2Sharp.WebApi;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.Res;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kenedia.Modules.BuildsManager.Services
{
    public class PvpAmuletMappedDataEntry : MappedDataEntry<int, PvpAmulet>
    {
        public override async Task<bool> LoadCached(string name, string path, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return false;
            }

            if (!File.Exists(path))
            {
                BuildsManager.Logger.Debug($"No local data for {name} found at '{Path.GetFileName(path)}'");
                return false;
            }

            BuildsManager.Logger.Debug($"Loading local data for {name} from '{Path.GetFileName(path)}'");

            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                if (token.IsCancellationRequested)
                {
                    return false;
                }

                var loaded = JsonConvert.DeserializeObject<PvpAmuletMappedDataEntry>(json, SerializerSettings.Default);
                if (loaded != null)
                {
                    Map = loaded.Map;
                    Items = loaded.Items;
                    Version = loaded.Version;
                    DataLoaded = true;

                    BuildsManager.Logger.Debug($"Loaded local data for {name} with {Items.Count} entries. Version {Version}");
                    return true;
                }
            }

            return false;
        }

        public override async Task<(bool, List<object>)> IsIncomplete(string name, ByteIntMap map, string path, Gw2ApiManager gw2ApiManager, CancellationToken token)
        {
            Map = map;

            var lang = GameService.Overlay.UserLocale.Value is Locale.Korean or Locale.Chinese ? Locale.English : GameService.Overlay.UserLocale.Value;
            var missing = map.Version > Version ? map.Items.Values : Map.Items.Values.Where(id => !Items.TryGetValue(id, out var entry) || entry.Names[lang] is null);

            return (missing.Any(), missing.Cast<object>().ToList());
        }

        public override async Task<bool> Update(string name, ByteIntMap map, string path, Gw2ApiManager gw2ApiManager, CancellationToken token)
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    return false;
                }

                var (incomplete, missing) = await IsIncomplete(name, map, path, gw2ApiManager, token);

                if (missing.Any() && missing.All(e => e is int))
                {
                    var idSets = missing.Cast<int>().ToList().ChunkBy(200);
                    BuildsManager.Logger.Debug($"{name} updating {missing.Count()} entries in {idSets.Count} sets.");

                    foreach (var ids in idSets)
                    {
                        var items = await gw2ApiManager.Gw2ApiClient.V2.Pvp.Amulets.ManyAsync(ids, token);
                        if (token.IsCancellationRequested)
                        {
                            return false;
                        }

                        foreach (var item in items)
                        {
                            bool exists = Items.Values.TryFind(e => e.Id == item.Id, out PvpAmulet entryItem);
                            entryItem ??= new()
                            {
                                MappedId = Map?.Items?.FirstOrDefault(e => e.Value == item.Id).Key ?? 0,
                            };

                            entryItem?.Apply(item);

                            if (!exists)
                                Items.Add(item.Id, entryItem);
                        }
                    }

                    Version = Map.Version;
                    BuildsManager.Logger.Debug($"Saving updated {name} data with {missing.Count()} updated entries. Version {Version}");
                    string json = JsonConvert.SerializeObject(this, SerializerSettings.Default);
                    File.WriteAllText(path, json);
                    DataLoaded = true;

                    return true;
                }
            }
            catch (Exception ex)
            {
                BuildsManager.Logger.Warn(ex, $"Failed to update {name} data.");
            }

            return false;
        }
    }
}
