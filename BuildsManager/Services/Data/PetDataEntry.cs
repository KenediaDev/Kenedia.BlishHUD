using Blish_HUD;
using Blish_HUD.Modules.Managers;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Version = SemVer.Version;
using Kenedia.Modules.Core.Extensions;
using Gw2Sharp.WebApi;
using System.Threading;

namespace Kenedia.Modules.BuildsManager.Services
{
    public class PetDataEntry : MappedDataEntry<int, Pet>
    {
        public async override Task<bool> LoadAndUpdate(string name, Version version, string path, Gw2ApiManager gw2ApiManager, CancellationToken cancellationToken)
        {
            try
            {
                bool saveRequired = false;
                PetDataEntry loaded = null;
                BuildsManager.Logger.Debug($"Load and if required update {name}");

                if (!DataLoaded && File.Exists(path))
                {
                    BuildsManager.Logger.Debug($"Load {name}.json");
                    string json = File.ReadAllText(path);
                    loaded = JsonConvert.DeserializeObject<PetDataEntry>(json);
                    DataLoaded = true;
                }

                Items = loaded?.Items ?? Items;
                Version = loaded?.Version ?? Version;

                BuildsManager.Logger.Debug($"Check for missing values for {name}");
                var petIds = await gw2ApiManager.Gw2ApiClient.V2.Pets.IdsAsync(cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                {
                    return false;
                }

                var lang = GameService.Overlay.UserLocale.Value is Locale.Korean or Locale.Chinese ? Locale.English : GameService.Overlay.UserLocale.Value;
                var localeMissing = Items.Values.Where(item => item.Names[lang] == null)?.Select(e => e.Id);
                var missing = petIds.Except(Items.Keys).Concat(localeMissing);

                if (version > Version)
                {
                    Version = version;
                    missing = petIds;
                    BuildsManager.Logger.Debug($"The current version does not match the map version. Updating all values for {name}.");
                }

                if (missing.Count() > 0)
                {
                    var idSets = missing.ToList().ChunkBy(200);
                    saveRequired = saveRequired || idSets.Count > 0;

                    BuildsManager.Logger.Debug($"Fetch a total of {missing.Count()} {name} in {idSets.Count} sets.");

                    foreach (var ids in idSets)
                    {
                        var items = await gw2ApiManager.Gw2ApiClient.V2.Pets.ManyAsync(ids, cancellationToken);
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return false;
                        }

                        foreach (var item in items)
                        {
                            bool exists = Items.TryGetValue(item.Id, out Pet entryItem);
                            entryItem ??= new();

                            entryItem.Apply(item);

                            if (!exists)
                                Items.Add(item.Id, entryItem);
                        }
                    }
                }

                if (saveRequired)
                {
                    BuildsManager.Logger.Debug($"Saving {name}.json");
                    string json = JsonConvert.SerializeObject(this);
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
