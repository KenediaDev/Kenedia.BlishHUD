using Blish_HUD;
using Blish_HUD.Modules.Managers;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Version = SemVer.Version;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Models;
using Gw2Sharp.WebApi;
using Kenedia.Modules.Core.Utility;
using System.Threading;

namespace Kenedia.Modules.BuildsManager.Services
{
    public class ItemMappedDataEntry<T> : MappedDataEntry<int, T> where T : BaseItem, new()
    {
        public async override Task<bool> LoadAndUpdate(string name, Version version, string path, Gw2ApiManager gw2ApiManager, CancellationToken cancellationToken)
        {
            try
            {
                bool saveRequired = false;
                MappedDataEntry<int, T> loaded = null;
                BuildsManager.Logger.Debug($"Load and if required update {name}");

                if (!DataLoaded && File.Exists(path))
                {
                    BuildsManager.Logger.Debug($"Load {name}.json");
                    string json = File.ReadAllText(path);
                    loaded = JsonConvert.DeserializeObject<MappedDataEntry<int, T>>(json);
                    DataLoaded = true;
                }

                Items = loaded?.Items ?? Items;
                Version = loaded?.Version ?? Version;

                BuildsManager.Logger.Debug($"Check for missing values for {name}");
                var lang = GameService.Overlay.UserLocale.Value is Locale.Korean or Locale.Chinese ? Locale.English : GameService.Overlay.UserLocale.Value;
                var fetchIds = Items.Values.Where(item => item.Names[lang] == null)?.Select(e => e.Id);

                if (version > Version || Map is null)
                {
                    BuildsManager.Logger.Debug($"Get the map for {name}");
                    Map = await StaticHosting.GetItemMap(name, cancellationToken);
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return false;
                    }

                    if (Map is null)
                    {
                        BuildsManager.Logger.Debug($"Failed to get the map for {name}");
                        return false;
                    }

                    Version = Map.Version;
                    fetchIds = fetchIds.Concat(Map.Values.Except(Items.Keys));
                    saveRequired = true;
                    BuildsManager.Logger.Debug($"The current version does not match the map version. Updating all values for {name}.");
                }

                if (fetchIds.Count() > 0)
                {
                    var idSets = fetchIds.ToList().ChunkBy(200);

                    saveRequired = saveRequired || idSets.Count > 0;
                    BuildsManager.Logger.Debug($"Fetch a total of {fetchIds.Count()} {name} in {idSets.Count} sets.");
                    foreach (var ids in idSets)
                    {
                        var items = await gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(ids, cancellationToken);
                        if(cancellationToken.IsCancellationRequested)
                        {
                            return false;
                        }

                        foreach (var item in items)
                        {
                            bool exists = Items.Values.TryFind(e => e.Id == item.Id, out T entryItem);
                            entryItem ??= new()
                            {
                                MappedId = Map?.Items?.FirstOrDefault(e => e.Value == item.Id).Key ?? 0,
                            };

                            entryItem?.Apply(item);

                            if (entryItem is not null && Data.SkinDictionary.TryGetValue(item.Id, out int? assetId) && assetId is not null)
                            {
                                var skin = await gw2ApiManager.Gw2ApiClient.V2.Skins.GetAsync(assetId.Value);
                                entryItem.AssetId = skin?.Icon.GetAssetIdFromRenderUrl() ?? 0;
                            }

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
