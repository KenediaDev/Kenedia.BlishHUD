using Blish_HUD;
using Blish_HUD.Modules.Managers;
using Gw2Sharp.Models;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using Kenedia.Modules.Core.Models;
using System.Threading.Tasks;
using Version = SemVer.Version;
using Kenedia.Modules.Core.Extensions;
using Gw2Sharp.WebApi;
using System.Threading;
using System.Collections.Generic;
using Kenedia.Modules.BuildsManager.Models;

namespace Kenedia.Modules.BuildsManager.Services
{
    public class ProfessionDataEntry : MappedDataEntry<ProfessionType, Profession>
    {
        public async override Task<bool> LoadAndUpdate(string name, ByteIntMap map, string path, Gw2ApiManager gw2ApiManager, CancellationToken cancellationToken)
        {
            try
            {
                bool saveRequired = false;
                ProfessionDataEntry loaded = null;

                BuildsManager.Logger.Debug($"Load and if required update {name}");

                if (!DataLoaded && File.Exists(path))
                {
                    BuildsManager.Logger.Debug($"Load {name}.json");
                    string json = File.ReadAllText(path);
                    loaded = JsonConvert.DeserializeObject<ProfessionDataEntry>(json, SerializerSettings.Default);
                    DataLoaded = true;
                }

                Map = map;
                Items = loaded?.Items ?? Items;
                Version = loaded?.Version ?? Version;

                BuildsManager.Logger.Debug($"{name} Version {Version} | version {map.Version}");

                BuildsManager.Logger.Debug($"Check for missing values for {name}");
                var professionIds = await gw2ApiManager.Gw2ApiClient.V2.Professions.IdsAsync(cancellationToken);
                if(cancellationToken.IsCancellationRequested)
                {
                    return false;
                }

                var professionTypes = professionIds?.Select(value => Enum.TryParse(value, out ProfessionType profession) ? profession : ProfessionType.Guardian).Distinct();

                var lang = GameService.Overlay.UserLocale.Value is Locale.Korean or Locale.Chinese ? Locale.English : GameService.Overlay.UserLocale.Value;
                var localeMissing = Items.Values.Where(item => item.Names[lang] == null)?.Select(e => e.Id);
                var missing = professionTypes.Except(Items.Keys).Concat(localeMissing);

                if (map.Version > Version)
                {
                    missing = professionTypes;
                    BuildsManager.Logger.Debug($"The current version does not match the map version. Updating all values for {name}.");
                }

                if (missing.Count() > 0)
                {
                    var idSets = missing.ToList().ChunkBy(200);
                    saveRequired = saveRequired || idSets.Count > 0;

                    BuildsManager.Logger.Debug($"Fetch a total of {missing.Count()} {name} in {idSets.Count} sets.");

                    var apiSpecializations = await gw2ApiManager.Gw2ApiClient.V2.Specializations.AllAsync(cancellationToken);
                    var apiLegends = missing.Contains(ProfessionType.Revenant) ? await gw2ApiManager.Gw2ApiClient.V2.Legends.AllAsync(cancellationToken) : null;
                    var apiTraits = await gw2ApiManager.Gw2ApiClient.V2.Traits.AllAsync(cancellationToken);
                    var apiSkills = await gw2ApiManager.Gw2ApiClient.V2.Skills.AllAsync(cancellationToken);
                    var allLegends = apiLegends.Append(new()
                    {
                        Id = "Legend7",
                        Swap = 62891,
                        Heal = 62719,
                        Elite = 62942,
                        Utilities = new List<int>()
                    {
                        62832,
                        62962,
                        62878,
                    }
                    });

                    if (cancellationToken.IsCancellationRequested)
                    {
                        return false;
                    }

                    foreach (var ids in idSets)
                    {
                        var items = await gw2ApiManager.Gw2ApiClient.V2.Professions.ManyAsync(ids, cancellationToken);
                        if (cancellationToken.IsCancellationRequested)
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
