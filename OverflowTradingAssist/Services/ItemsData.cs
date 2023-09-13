using Blish_HUD.Modules.Managers;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.OverflowTradingAssist.DataModels;
using Newtonsoft.Json;
using Blish_HUD;
using Gw2Sharp.WebApi;
using Kenedia.Modules.Core.Extensions;

namespace Kenedia.Modules.OverflowTradingAssist.Services
{
    public class ItemsData : DataEntry<Item>
    {
        public override async Task<bool> LoadAndUpdate(string name, SemVer.Version version, string path, Gw2ApiManager gw2ApiManager, CancellationToken cancellationToken)
        {
            try
            {
                var stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Start();
                bool saveRequired = false;
                ItemsData loaded = null;
                OverflowTradingAssist.Logger.Debug($"Load and if required update {name}");

                if (!DataLoaded && File.Exists(path))
                {
                    OverflowTradingAssist.Logger.Debug($"Load {name}.json");
                    string json = File.ReadAllText(path);
                    loaded = JsonConvert.DeserializeObject<ItemsData>(json, SerializerSettings.Default);
                    DataLoaded = true;
                }

                Items = loaded?.Items ?? Items;
                Version = loaded?.Version ?? Version;

                OverflowTradingAssist.Logger.Debug($"{name} Version {Version} | version {version}");

                OverflowTradingAssist.Logger.Debug($"Check for missing values for {name}");
                var itemIds = await gw2ApiManager.Gw2ApiClient.V2.Items.IdsAsync(cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                {
                    return false;
                }

                var lang = GameService.Overlay.UserLocale.Value is Locale.Korean or Locale.Chinese ? Locale.English : GameService.Overlay.UserLocale.Value;
                var localeMissing = Items.Where(item => item.Names[lang] == null)?.Select(e => e.Id);
                var missing = itemIds.Except(Items.Select(e => e.Id)).Concat(localeMissing);

                if (version > Version)
                {
                    OverflowTradingAssist.Logger.Debug($"The current version ({Version}) does not match the map version ({version}). Updating all values for {name}.");
                    Version = version;
                    missing = itemIds;
                }

                if (missing.Count() > 0)
                {
                    var idSets = missing.ToList().ChunkBy(200);
                    saveRequired = saveRequired || idSets.Count > 0;

                    OverflowTradingAssist.Logger.Debug($"Fetch a total of {missing.Count()} {name} in {idSets.Count} sets.");
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return false;
                    }

                    int count = 0;
                    foreach (var ids in idSets)
                    {
                        OverflowTradingAssist.Logger.Debug($"Fetch chunk {count}/{idSets.Count} for {name}.");
                        var items = await gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(ids, cancellationToken);
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return false;
                        }

                        foreach (var item in items)
                        {
                            bool exists = Items.TryFind(e => e.Id == item.Id, out Item entryItem);
                            entryItem ??= new();

                            entryItem.Apply(item);

                            if (!exists)
                                Items.Add(entryItem);
                        }

                        count++;
                    }
                }

                if (saveRequired)
                {
                    OverflowTradingAssist.Logger.Debug($"Saving {name}.json");
                    string json = JsonConvert.SerializeObject(this, SerializerSettings.Default);
                    File.WriteAllText(path, json);
                }
                
                stopwatch.Stop();
                OverflowTradingAssist.Logger.Debug($"Loaded {name} in {stopwatch.ElapsedMilliseconds}ms");

                DataLoaded = DataLoaded || Items.Count > 0;                
                return true;
            }
            catch (Exception ex)
            {
                OverflowTradingAssist.Logger.Warn(ex, $"Failed to load {name} data.");
                return false;
            }
        }
    }
}
