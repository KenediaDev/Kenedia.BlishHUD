using Blish_HUD;
using Blish_HUD.GameServices.ArcDps.Models.UnofficialExtras;
using Blish_HUD.Modules.Managers;
using Gw2Sharp.WebApi;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.DataModels.Stats;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.Core.Attributes;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework.Content;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using WeaponItem = Kenedia.Modules.BuildsManager.DataModels.Items.Weapon;

namespace Kenedia.Modules.BuildsManager.Services
{
    public class Data : IDisposable, IEnumerable<(string name, BaseMappedDataEntry map)>, IEnumerable
    {
        public static readonly Dictionary<int, int?> SkinDictionary = new()
                {
                    { 30684, 5013 }, //Axe
                    { 30687, 4997 }, //Dagger
                    { 30689, 4995 }, //Greatsword
                    { 30690, 5022 }, //Hammer
                    { 30692, 5005 }, //Mace
                    { 30696, 5018 }, //Shield
                    { 30699, 5020 }, //Sword
                    { 30691, 5164 }, //Spear
                    { 30686, 5000 }, //Shortbow
                    { 30685, 4998 }, //Longbow
                    { 30693, 5008 }, //Pistol
                    { 30694, 5021 }, //Rifle
                    { 30702, 5001 }, //Warhorn
                    { 30700, 4992 }, //Torch
                    { 30697, 4990 }, //Harpoon Gun
                    { 30688, 4994 }, //Focus
                    { 30695, 4989 }, //Scepter
                    { 30698, 5019 }, //Staff
                    { 30701, 5129 }, //Trident
                    
                    { 79895, 854 }, //Aqua Breather (Heavy)
                    { 80384, 818 }, // Helm (Heavy)
                    { 80435, 808 }, // Shoulder (Heavy)
                    { 80254, 807 }, // Coat (Heavy)
                    { 80205, 812 }, // Gloves (Heavy)
                    { 80277, 797 }, // Leggings (Heavy)
                    { 80557, 801 },  // Boots (Heavy)
                    
                    { 79838, 856 }, //Aqua Breather (Medium)
                    { 80296, 817 }, // Helm (Medium)
                    { 80145 , 805 }, // Shoulder (Medium)
                    { 80578, 806 }, // Coat (Medium)
                    { 80161, 811 }, // Gloves (Medium)
                    { 80252, 796 }, // Leggings (Medium)
                    { 80281, 799 }, // Boots (Medium)
                    
                    { 79873, 855 }, //Aqua Breather (Light)
                    { 80248, 819 }, // Helm (Light)
                    { 80131, 810 }, // Shoulder (Light)
                    { 80190, 809 }, // Coat (Light)
                    { 80111, 813 }, // Gloves (Light)
                    { 80356, 798 }, // Leggings (Light)
                    { 80399, 803 },  // Boots (Light)                    
   
                    { 74155, 10161 }, //Back
                    { 92991, 1614376}, // Amulet
                    { 81908, 1614709 }, // Accessory
                    { 91234, 1614682},  // Ring

                    //{ 0, null },  // Relic
                };

        private readonly Func<NotificationBadge> _getNotificationBadge;
        private readonly Func<LoadingSpinner> _getSpinner;

        public NotificationBadge NotificationBadge => _getNotificationBadge?.Invoke() is NotificationBadge badge ? badge : null;

        public LoadingSpinner Spinner => _getSpinner?.Invoke() is LoadingSpinner spinner ? spinner : null;

        private CancellationTokenSource _cancellationTokenSource;
        private bool _isDisposed;

        public Data(Paths paths, Gw2ApiManager gw2ApiManager, Func<NotificationBadge> notificationBadge, Func<LoadingSpinner> spinner, StaticHosting staticHosting, ContentsManager contentsManager)
        {
            Paths = paths;
            Gw2ApiManager = gw2ApiManager;
            StaticHosting = staticHosting;
            ContentsManager = contentsManager;
            _getSpinner = spinner;
            _getNotificationBadge = notificationBadge;
        }

        public event EventHandler Loaded;

        public bool IsLoaded
        {
            get
            {
                foreach (var (name, map) in this)
                {
                    if (!map.IsLoaded)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public double LastLoadAttempt { get; private set; } = double.MinValue;

        [EnumeratorMember]
        public ProfessionDataEntry Professions { get; } = new();

        [EnumeratorMember]
        public RaceDataEntry Races { get; } = new();

        [EnumeratorMember]
        public PetDataEntry Pets { get; } = new();

        [EnumeratorMember]
        public StatMappedDataEntry Stats { get; } = new();

        [EnumeratorMember]
        public PvpAmuletMappedDataEntry PvpAmulets { get; } = new();

        [EnumeratorMember]
        public ItemMappedDataEntry<Armor> Armors { get; } = new();

        [EnumeratorMember]
        public ItemMappedDataEntry<WeaponItem> Weapons { get; } = new();

        [EnumeratorMember]
        public ItemMappedDataEntry<Trinket> Trinkets { get; } = new();

        [EnumeratorMember]
        public ItemMappedDataEntry<Trinket> Backs { get; } = new();

        [EnumeratorMember]
        public ItemMappedDataEntry<Sigil> PvpSigils { get; } = new();

        [EnumeratorMember]
        public ItemMappedDataEntry<Sigil> PveSigils { get; } = new();

        [EnumeratorMember]
        public ItemMappedDataEntry<Rune> PvpRunes { get; } = new();

        [EnumeratorMember]
        public ItemMappedDataEntry<Rune> PveRunes { get; } = new();

        [EnumeratorMember]
        public ItemMappedDataEntry<PowerCore> PowerCores { get; } = new();

        [EnumeratorMember]
        public ItemMappedDataEntry<Relic> PveRelics { get; } = new();

        [EnumeratorMember]
        public ItemMappedDataEntry<Relic> PvpRelics { get; } = new();

        [EnumeratorMember]
        public ItemMappedDataEntry<Enhancement> Enhancements { get; } = new();

        [EnumeratorMember]
        public ItemMappedDataEntry<Nourishment> Nourishments { get; } = new();

        [EnumeratorMember]
        public ItemMappedDataEntry<Infusion> Infusions { get; } = new();

        [EnumeratorMember]
        public ItemMappedDataEntry<Enrichment> Enrichments { get; } = new();

        public Paths Paths { get; }

        public Gw2ApiManager Gw2ApiManager { get; }

        public StaticHosting StaticHosting { get; }

        public ContentsManager ContentsManager { get; }

        public StaticStats? StatsMap { get; private set; }

        public StaticVersion? Versions { get; private set; }

        public IEnumerator<(string name, BaseMappedDataEntry map)> GetEnumerator()
        {
            yield return (nameof(Professions), Professions);
            yield return (nameof(Races), Races);
            yield return (nameof(Pets), Pets);
            yield return (nameof(Stats), Stats);
            yield return (nameof(PvpAmulets), PvpAmulets);
            yield return (nameof(Armors), Armors);
            yield return (nameof(Weapons), Weapons);
            yield return (nameof(Trinkets), Trinkets);
            yield return (nameof(Backs), Backs);
            yield return (nameof(PvpSigils), PvpSigils);
            yield return (nameof(PveSigils), PveSigils);
            yield return (nameof(PvpRunes), PvpRunes);
            yield return (nameof(PveRunes), PveRunes);
            yield return (nameof(PowerCores), PowerCores);
            yield return (nameof(PveRelics), PveRelics);
            yield return (nameof(PvpRelics), PvpRelics);
            yield return (nameof(Enhancements), Enhancements);
            yield return (nameof(Nourishments), Nourishments);
            yield return (nameof(Infusions), Infusions);
            yield return (nameof(Enrichments), Enrichments);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            // Forward non-generic enumerator to the generic one
            return GetEnumerator();
        }

        public async Task<bool> Load(bool force)
        {
            if (force)
            {
                LastLoadAttempt = double.MinValue;
            }

            return await Load();
        }

        public async Task<bool> Load(Locale _)
        {
            return await Load(true);
        }

        public async Task<bool> Load()
        {
            // Don't try to load more than once every 3 minutes
            if (Common.Now - LastLoadAttempt <= 180000)
            {
                return false;
            }

            LoadingSpinner spinner = Spinner;
            LastLoadAttempt = Common.Now;

            BuildsManager.Logger.Info("Loading data");
            spinner?.Show();
            NotificationBadge?.Notifications?.Clear();

            try
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource = new CancellationTokenSource();

                Versions ??= await StaticHosting.GetStaticVersion();
                StatsMap ??= await StaticHosting.GetStaticStats();

                var endTime = DateTime.Now.AddMinutes(3);

                string error_text =
                    Versions is null ? $"Failed to get the version file. Using locally cached data only.\nRetry at {DateTime.Now.AddMinutes(3):T}" :
                    StatsMap is null ? $"Failed to get the stats file. Using locally cached data only.\nRetry at {DateTime.Now.AddMinutes(3):T}" :
                    null;

                if (!string.IsNullOrEmpty(error_text))
                {
                    NotificationBadge?.AddNotification(new()
                    {
                        NotificationText = error_text,
                        Condition = () => DateTime.Now >= endTime,
                    });
                }

                BuildsManager.Logger.Info(Versions is null ? "Version file could not be fetched." : "Versions file fetched from static hosting.");
                BuildsManager.Logger.Info(StatsMap is null ? "Stats file could not be fetched." : $"Stats file fetched from static hosting.");

                // Export shipped default data if no local data exists
                List<string> files = ["stat_map.png", "static_stats.json"];
                files = [.. files, .. this.Select(e => $"{e.name}.json")];

                foreach (string file in files)
                {
                    string path = Path.Combine(Paths.ModuleDataPath, file);

                    if (!File.Exists(path))
                    {
                        BuildsManager.Logger.Info($"Exporting default data for {file}...");
                        using Stream target = File.Create(path);
                        using Stream source = ContentsManager.GetFileStream($@"data\default_api_data\{file}");
                        _ = source.Seek(0, SeekOrigin.Begin);
                        source.CopyTo(target);
                    }
                }

                // Load all local data first
                bool localDataLoaded = true;
                List<string> failedLocalParts = [];

                var localStats = File.Exists(Path.Combine(Paths.ModuleDataPath, "static_stats.json")) ? JsonConvert.DeserializeObject<StaticStats>(File.ReadAllText(Path.Combine(Paths.ModuleDataPath, "static_stats.json"))) : null;
                bool statsImageExists = File.Exists(Path.Combine(BuildsManager.Data.Paths.ModuleDataPath, "stat_map.png"));
                if (localStats is not null && statsImageExists)
                {
                    Stat.StatTextureMap = localStats.TextureMapInfo;
                }

                foreach (var (name, map) in this)
                {
                    if (_cancellationTokenSource.IsCancellationRequested)
                    {
                        localDataLoaded = false;
                        failedLocalParts.Add(name);
                        continue;
                    }

                    if (map.IsLoaded)
                    {
                        continue;
                    }

                    string path = Path.Combine(Paths.ModuleDataPath, $"{name}.json");
                    bool success = await map.LoadCached(name, path, _cancellationTokenSource.Token);
                    localDataLoaded = localDataLoaded && success;

                    if (!success)
                    {
                        failedLocalParts.Add(name);
                    }
                }

                if (!localDataLoaded)
                {
                    BuildsManager.Logger.Info($"Local data failed to load: {string.Join(", ", failedLocalParts)}");
                }
                else
                {
                    BuildsManager.Logger.Info("All local data loaded successfully.");
                }

                // Check and update data if necessary
                bool remoteDataLoaded = true;
                List<string> remoteFailedParts = [];

                if (Versions is not null && StatsMap is not null)
                {
                    BuildsManager.Logger.Info("Loading remote data...");
                    if (localStats is null || localStats.Version < StatsMap.Version)
                    {
                        File.WriteAllText(Path.Combine(Paths.ModuleDataPath, "static_stats.json"), JsonConvert.SerializeObject(StatsMap, Formatting.Indented));

                        //Download image from stats.ImageUrl
                        string url = StatsMap.ImageUrl;
                        using var client = new System.Net.Http.HttpClient();

                        BuildsManager.Logger.Info($"Downloading stat map image from {url}...");
                        byte[] data = await client.GetByteArrayAsync(url);
                        File.WriteAllBytes(Path.Combine(Paths.ModuleDataPath, "stat_map.png"), data);
                        Stat.StatTextureMap = StatsMap.TextureMapInfo;
                    }

                    var lang = GameService.Overlay.UserLocale.Value is Locale.Korean or Locale.Chinese ? Locale.English : GameService.Overlay.UserLocale.Value;
                    await PreFetchItemIds(Versions);

                    foreach (var (name, map) in this)
                    {
                        if (_cancellationTokenSource.IsCancellationRequested)
                        {
                            remoteDataLoaded = false;
                            remoteFailedParts.Add(name);
                            continue;
                        }

                        string path = Path.Combine(Paths.ModuleDataPath, $"{name}.json");

                        if (await map.IsIncomplete(name, Versions[name], path, Gw2ApiManager, _cancellationTokenSource.Token) is (true, var _))
                        {
                            if (_cancellationTokenSource.IsCancellationRequested)
                            {
                                remoteDataLoaded = false;
                                remoteFailedParts.Add(name);
                                continue;
                            }

                            BuildsManager.Logger.Info($"Updating data for {name} {(map.Version < Versions[name].Version ? $"from version {map.Version} to {Versions[name].Version}" : $"for {lang} locale...")}");
                            bool updated = await map.Update(name, Versions[name], path, Gw2ApiManager, _cancellationTokenSource.Token);
                            remoteDataLoaded = remoteDataLoaded && updated;
                            if (!updated)
                            {
                                remoteFailedParts.Add(name);
                            }
                        }
                    }
                
                    if (!remoteDataLoaded)
                    {
                        BuildsManager.Logger.Info($"Remote data failed to load: {string.Join(", ", remoteFailedParts)}");
                    }
                    else
                    {
                        BuildsManager.Logger.Info("All remote data loaded successfully.");
                    }
                }

                // Raise Loaded event if all data entries loaded successfully at least locally
                if (localDataLoaded || remoteDataLoaded)
                {
                    GameService.Graphics.QueueMainThreadRender((graphicsDevice) => Loaded?.Invoke(this, EventArgs.Empty));
                    spinner?.Hide();
                    return true;
                }
                else
                {
                    string txt = $"Failed to load local and remote data. Click to retry.{Environment.NewLine}Automatic retry at {endTime:T}" +
                        $"{Environment.NewLine}Failed to load locally: {string.Join(", ", failedLocalParts)}" +
                        $"{Environment.NewLine}Failed to load remotely: {string.Join(", ", remoteFailedParts)}";

                    NotificationBadge?.AddNotification(new()
                    {
                        NotificationText = txt,
                        Condition = () => DateTime.Now >= endTime || IsLoaded,
                    });

                    BuildsManager.Logger.Info(txt);

                }
            }
            catch (Exception ex)
            {
                BuildsManager.Logger.Info(ex, "An error occurred while loading data.");
                BuildsManager.Logger.Info(ex.Message);
                BuildsManager.Logger.Info(ex.InnerException.Message);
            }

            return false;
        }

        private async Task<bool> PreFetchItemIds(StaticVersion versions)
        {
            List<int> itemIds = [];
            foreach (var (name, map) in this)
            {
                if (map.GetType().IsGenericType && map.GetType().GetGenericTypeDefinition() == typeof(ItemMappedDataEntry<>))
                {
                    var innerType = map.GetType().GetGenericArguments()[0];

                    if (typeof(BaseItem).IsAssignableFrom(innerType))
                    {

                        var (isIncomplete, ids) = await map.IsIncomplete(
                            name,
                            versions[name],
                            Path.Combine(Paths.ModuleDataPath, $"{name}.json"),
                            Gw2ApiManager,
                            _cancellationTokenSource.Token
                        );

                        if (ids.Any() && ids.All(e => e is int))
                        {
                            itemIds = isIncomplete ? [.. itemIds.Union(ids.Cast<int>())] : itemIds;
                        }
                    }
                }
            }

            var idSets = itemIds.ToList().ChunkBy(200);
            if (itemIds.Count > 0)
            {
                BuildsManager.Logger.Info($"Pre-Fetching {itemIds.Count} items from API...");
                int count = 0;
                foreach (var idSet in idSets)
                {
                    if (_cancellationTokenSource.IsCancellationRequested)
                    {
                        return false;
                    }

                    count += idSet.Count;
                    BuildsManager.Logger.Info($"Fetching {count}/{itemIds.Count} items...");
                    await Gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(idSet);
                }
            }

            return true;
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            foreach (var (_, map) in this)
            {
                map?.Dispose();
            }
        }
    }
}
