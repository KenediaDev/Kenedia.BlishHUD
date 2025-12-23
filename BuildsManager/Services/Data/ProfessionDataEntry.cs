using Blish_HUD;
using Blish_HUD.Modules.Managers;
using Gw2Sharp.Models;
using Gw2Sharp.WebApi;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Kenedia.Modules.BuildsManager.Services
{
    public class ProfessionDataEntry : MappedDataEntry<ProfessionType, Profession>
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

                var loaded = JsonConvert.DeserializeObject<ProfessionDataEntry>(json, SerializerSettings.Default);
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
            else
            {
                BuildsManager.Logger.Debug($"No local data for {name} found at '{Path.GetFileName(path)}'");
            }

            return false;
        }

        public override async Task<(bool, List<object>)> IsIncomplete(string name, ByteIntMap map, string path, Gw2ApiManager gw2ApiManager, CancellationToken token)
        {
            Map = map;

            try
            {
                Ids ??= [.. (await gw2ApiManager.Gw2ApiClient.V2.Professions.IdsAsync(token)).Select(value => Enum.TryParse(value, out ProfessionType profession) ? profession : ProfessionType.Guardian).Distinct()];
                if (token.IsCancellationRequested)
                {
                    return (true, new List<object>());
                }

                var lang = GameService.Overlay.UserLocale.Value is Locale.Korean or Locale.Chinese ? Locale.English : GameService.Overlay.UserLocale.Value;
                var missing = map.Version > Version ? Ids : Ids.Where(professionId => !Items.TryGetValue(professionId, out var prof) || prof.Names[lang] is null);

                return (missing.Any(), missing.Cast<object>().ToList());
            }
            catch (Exception ex)
            {
                BuildsManager.Logger.Warn(ex, $"Failed to check completeness of {name} data.");
            }

            return (true, new List<object>());
        }

        public async override Task<bool> Update(string name, ByteIntMap map, string path, Gw2ApiManager gw2ApiManager, CancellationToken token)
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    return false;
                }

                var professionIds = await gw2ApiManager.Gw2ApiClient.V2.Professions.IdsAsync(token);
                if (token.IsCancellationRequested)
                {
                    return false;
                }

                var (isIncomplete, missing) = await IsIncomplete(name, map, path, gw2ApiManager, token);
                if (missing.Any() && missing.All(item => item is ProfessionType))
                {
                    var idSets = missing.Cast<ProfessionType>().ToList().ChunkBy(200);

                    BuildsManager.Logger.Debug($"{name} updating {missing.Count()} entries in {idSets.Count} sets.");

                    var apiSpecializations = await gw2ApiManager.Gw2ApiClient.V2.Specializations.AllAsync(token);
                    var apiLegends = missing.Contains(ProfessionType.Revenant) ? await gw2ApiManager.Gw2ApiClient.V2.Legends.AllAsync(token) : null;
                    var apiTraits = await gw2ApiManager.Gw2ApiClient.V2.Traits.AllAsync(token);
                    var apiSkills = await gw2ApiManager.Gw2ApiClient.V2.Skills.AllAsync(token);
                    var allLegends = apiLegends.Append(new()
                    {
                        Id = "Legend7",
                        Swap = 62891,
                        Heal = 62719,
                        Elite = 62942,
                        Utilities =
                        [
                            62832,
                            62962,
                            62878,
                        ]
                    });

                    if (token.IsCancellationRequested)
                    {
                        return false;
                    }

                    foreach (var ids in idSets)
                    {
                        var items = await gw2ApiManager.Gw2ApiClient.V2.Professions.ManyAsync(ids, token);
                        if (token.IsCancellationRequested)
                        {
                            return false;
                        }

                        foreach (var item in items)
                        {
                            bool exists = Items.Values.TryFind(e => $"{e.Id}" == item.Id, out Profession entryItem);
                            entryItem ??= new();

                            entryItem.Apply(item, apiSpecializations, allLegends, apiTraits, apiSkills);

                            if (!exists)
                                Items.Add(entryItem.Id, entryItem);
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
