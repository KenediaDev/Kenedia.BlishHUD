using Blish_HUD.Modules.Managers;
using Kenedia.Modules.Core.Attributes;
using Kenedia.Modules.OverflowTradingAssist.DataEntries;
using Kenedia.Modules.OverflowTradingAssist.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Utility;
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
                    Version = version;
                    missing = itemIds;
                    OverflowTradingAssist.Logger.Debug($"The current version does not match the map version. Updating all values for {name}.");
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

    public class Data
    {
        private readonly Gw2ApiManager _gw2ApiManager;
        private readonly Func<NotificationBadge> _notificationBadge;
        private readonly Func<LoadingSpinner> _spinner;
        private readonly Paths _paths;
        private CancellationTokenSource _cancellationTokenSource;

        public Data(Paths paths, Gw2ApiManager gw2ApiManager, Func<Core.Controls.NotificationBadge> notificationBadge, Func<LoadingSpinner> spinner)
        {
            _paths = paths;
            _gw2ApiManager = gw2ApiManager;
            _notificationBadge = notificationBadge;
            _spinner = spinner;
        }

        public event EventHandler Loaded;

        [EnumeratorMember]
        public static ItemsData Items { get; set; } = new();

        public bool IsLoaded { get; internal set; }
        public double LastLoadAttempt { get; private set; }

        public IEnumerator<(string name, ItemsData map)> GetEnumerator()
        {
            var propertiesToEnumerate = GetType()
                .GetProperties()
                .Where(property => property.GetCustomAttribute<EnumeratorMemberAttribute>() != null);

            foreach (var property in propertiesToEnumerate)
            {
                yield return (property.Name, property.GetValue(this) as ItemsData);
            }
        }

        public async Task<bool> Load()
        {
            if (Common.Now - LastLoadAttempt <= 180000)
            {
                return false;
            }

            LoadingSpinner spinner = _spinner?.Invoke();

            try
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource = new CancellationTokenSource();

                StaticVersion versions = await StaticHosting.GetStaticVersion();
                versions = new()
                {
                    Items = new("0.0.1")
                };

                if (versions is null)
                {
                    if (_notificationBadge?.Invoke() is NotificationBadge badge)
                    {
                        badge.Show();
                        badge.SetLocalizedText = () => $"Failed to get the version file. Retry at {DateTime.Now.AddMinutes(3):T}";
                    }

                    spinner?.Hide();
                    return false;
                }

                bool failed = false;
                string loadStatus = string.Empty;

                foreach (var (name, map) in this)
                {
                    string path = Path.Combine(_paths.ModuleDataPath, $"{name}.json");
                    bool success = await map?.LoadAndUpdate(name, versions[name], path, _gw2ApiManager, _cancellationTokenSource.Token);
                    failed = failed || !success;

                    loadStatus += $"{Environment.NewLine}{name}: {success} [{map?.Version?.ToString() ?? "0.0.0"} | {versions[name]}] ";
                }

                if (!failed)
                {
                    OverflowTradingAssist.Logger.Info("All data loaded!");
                    Loaded?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    if (_notificationBadge?.Invoke() is NotificationBadge badge)
                    {
                        badge.Show();
                        string txt = $"Failed to load some data. Click to retry.{Environment.NewLine}Automatic retry at {DateTime.Now.AddMinutes(3):T}{loadStatus}";
                        badge.SetLocalizedText = () => txt;
                        OverflowTradingAssist.Logger.Info(txt);
                    }
                }

                spinner?.Hide();
                return true;
            }
            catch
            {
            }

            return false;
        }

        public async Task<bool> Load(bool force)
        {
            if (force)
            {
                LastLoadAttempt = double.MinValue;
            }

            return await Load();
        }
    }
}
