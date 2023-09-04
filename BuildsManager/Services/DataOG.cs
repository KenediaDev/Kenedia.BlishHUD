using Blish_HUD;
using Blish_HUD.Modules.Managers;
using Gw2Sharp.Models;
using Kenedia.Modules.BuildsManager.DataModels;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.BuildsManager.DataModels.Stats;
using Kenedia.Modules.Core.DataModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using System.Reflection;
using WeaponItem = Kenedia.Modules.BuildsManager.DataModels.Items.Weapon;
using Version = SemVer.Version;
using Kenedia.Modules.Core.Extensions;
using Gw2Sharp.WebApi;
using System.Diagnostics;
using System.Runtime.Serialization;
using Kenedia.Modules.Core.Utility;

namespace Kenedia.Modules.BuildsManager.Services
{
    [DataContract]
    public class MappedDataEntry : IDisposable
    {
        protected bool DataLoaded = false;

        [DataMember]
        [JsonSemverVersion]
        public Version Version { get; set; } = new(0, 0, 0);

        [DataMember]
        public Dictionary<string, object> Items { get; set; } = new();

        public bool IsLoaded => DataLoaded;

        public ByteIntMap Map { get; set; }

        public virtual Task<bool> LoadAndUpdate(string name, Version version, string path, Gw2ApiManager gw2ApiManager)
        {
            return Task.FromResult(false);
        }

        // Conversion method
        public static MappedDataEntry FromGeneric<Key, T>(MappedDataEntry<Key, T> entry) where T : IDataMember, new() where Key : notnull
        {
            return new()
            {
                Version = entry.Version,
                Items = entry.Items.ToDictionary(kvp => $"{kvp.Key}", kvp => (object)kvp.Value),
            };
        }

        public void Dispose()
        {
            Items.Clear();
            Items = null;
        }
    }

    public class MappedDataEntry<Key, T> : MappedDataEntry where T : IDataMember, new()
    {
        [DataMember]
        public new Dictionary<Key, T> Items { get; protected set; } = new();

        public T this[Key key]
        {
            get => Items.TryGetValue(key, out T value) ? value : default;
            set => Items[key] = value;
        }

        [JsonIgnore]
        public int Count => Items.Count;

        [JsonIgnore]
        public IEnumerable<Key> Keys => Items.Keys;

        [JsonIgnore]
        public IEnumerable<T> Values => Items.Values;

        public void Add(Key key, T value)
        {
            Items.Add(key, value);
        }

        public void Remove(Key key)
        {
            _ = Items.Remove(key);
        }

        public void Clear()
        {
            Items.Clear();
        }

        public bool ContainsKey(Key key)
        {
            return Items.ContainsKey(key);
        }

        public bool TryGetValue(Key key, out T value)
        {
            return Items.TryGetValue(key, out value);
        }

        public override async Task<bool> LoadAndUpdate(string name, Version version, string path, Gw2ApiManager gw2ApiManager)
        {
            return await Task.FromResult(DataLoaded);
        }
    }

    public class StatMappedDataEntry : MappedDataEntry<int, Stat>
    {
        public async override Task<bool> LoadAndUpdate(string name, Version version, string path, Gw2ApiManager gw2ApiManager)
        {
            try
            {
                bool saveRequired = false;
                MappedDataEntry<int, Stat> loaded = null;

                BuildsManager.Logger.Debug($"Load {name}.json");

                if (!DataLoaded && File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    loaded = JsonConvert.DeserializeObject<MappedDataEntry<int, Stat>>(json);
                    DataLoaded = true;
                }

                Items = loaded?.Items ?? Items;
                Version = loaded?.Version ?? Version;

                var lang = GameService.Overlay.UserLocale.Value is Locale.Korean or Locale.Chinese ? Locale.English : GameService.Overlay.UserLocale.Value;
                var fetchIds = Items.Values.Where(item => (item.Names[lang] == null) || (item.MappedId == 0))?.Select(e => e.Id);

                if (version > Version || Map is null)
                {
                    Map = await StaticHosting.GetItemMap(name);
                    if (Map is null)
                    {
                        BuildsManager.Logger.Debug($"Failed to map for {name}");
                        return false;
                    }

                    Version = Map.Version;
                    fetchIds = fetchIds.Concat(Map.Values.Except(Items.Keys));
                    saveRequired = true;
                }

                if (fetchIds.Count() > 0)
                {
                    var idSets = fetchIds.ToList().ChunkBy(200);

                    saveRequired = saveRequired || idSets.Count > 0;
                    BuildsManager.Logger.Debug($"Fetch a total of {fetchIds.Count()} in {idSets.Count} sets.");
                    foreach (var ids in idSets)
                    {
                        var items = await gw2ApiManager.Gw2ApiClient.V2.Itemstats.ManyAsync(ids);

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

    public class PvpAmuletMappedDataEntry : MappedDataEntry<int, PvpAmulet>
    {
        public async override Task<bool> LoadAndUpdate(string name, Version version, string path, Gw2ApiManager gw2ApiManager)
        {
            try
            {
                bool saveRequired = false;
                MappedDataEntry<int, PvpAmulet> loaded = null;

                BuildsManager.Logger.Debug($"Load {name}.json");

                if (!DataLoaded && File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    loaded = JsonConvert.DeserializeObject<MappedDataEntry<int, PvpAmulet>>(json);
                    DataLoaded = true;
                }

                Items = loaded?.Items ?? Items;
                Version = loaded?.Version ?? Version;

                var lang = GameService.Overlay.UserLocale.Value is Locale.Korean or Locale.Chinese ? Locale.English : GameService.Overlay.UserLocale.Value;
                var fetchIds = Items.Values.Where(item => item.Names[lang] == null)?.Select(e => e.Id);

                if (version > Version || Map is null)
                {
                    Map = await StaticHosting.GetItemMap(name);
                    if (Map is null)
                    {
                        BuildsManager.Logger.Debug($"Failed to map for {name}");
                        return false;
                    }

                    Version = Map.Version;
                    fetchIds = fetchIds.Concat(Map.Values.Except(Items.Keys));
                    saveRequired = true;
                }

                if (fetchIds.Count() > 0)
                {
                    var idSets = fetchIds.ToList().ChunkBy(200);

                    saveRequired = saveRequired || idSets.Count > 0;
                    BuildsManager.Logger.Debug($"Fetch a total of {fetchIds.Count()} in {idSets.Count} sets.");
                    foreach (var ids in idSets)
                    {
                        var items = await gw2ApiManager.Gw2ApiClient.V2.Pvp.Amulets.ManyAsync(ids);

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

    public class ItemMappedDataEntry<T> : MappedDataEntry<int, T> where T : BaseItem, new()
    {
        public async override Task<bool> LoadAndUpdate(string name, Version version, string path, Gw2ApiManager gw2ApiManager)
        {
            try
            {
                bool saveRequired = false;
                MappedDataEntry<int, T> loaded = null;

                BuildsManager.Logger.Debug($"Load {name}.json");

                if (!DataLoaded && File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    loaded = JsonConvert.DeserializeObject<MappedDataEntry<int, T>>(json);
                    DataLoaded = true;
                }

                Items = loaded?.Items ?? Items;
                Version = loaded?.Version ?? Version;

                var lang = GameService.Overlay.UserLocale.Value is Locale.Korean or Locale.Chinese ? Locale.English : GameService.Overlay.UserLocale.Value;
                var fetchIds = Items.Values.Where(item => item.Names[lang] == null)?.Select(e => e.Id);

                if (version > Version || Map is null)
                {
                    Map = await StaticHosting.GetItemMap(name);
                    if (Map is null)
                    {
                        BuildsManager.Logger.Debug($"Failed to map for {name}");
                        return false;
                    }

                    Version = Map.Version;
                    fetchIds = fetchIds.Concat(Map.Values.Except(Items.Keys));
                    saveRequired = true;
                }

                if (fetchIds.Count() > 0)
                {
                    var idSets = fetchIds.ToList().ChunkBy(200);

                    saveRequired = saveRequired || idSets.Count > 0;
                    BuildsManager.Logger.Debug($"Fetch a total of {fetchIds.Count()} {name} in {idSets.Count} sets.");
                    foreach (var ids in idSets)
                    {
                        var items = await gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(ids);

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

    public class ProfessionDataEntry : MappedDataEntry<ProfessionType, Profession>
    {
        public async override Task<bool> LoadAndUpdate(string name, Version version, string path, Gw2ApiManager gw2ApiManager)
        {
            try
            {
                bool saveRequired = false;
                ProfessionDataEntry loaded = null;

                BuildsManager.Logger.Debug($"Load {name}.json");

                if (!DataLoaded && File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    loaded = JsonConvert.DeserializeObject<ProfessionDataEntry>(json);
                    DataLoaded = true;
                }

                Items = loaded?.Items ?? Items;
                Version = loaded?.Version ?? Version;

                var professionIds = await gw2ApiManager.Gw2ApiClient.V2.Professions.IdsAsync();
                var professionTypes = professionIds.Select(value => Enum.TryParse(value, out ProfessionType profession) ? profession : ProfessionType.Guardian).Distinct();

                var lang = GameService.Overlay.UserLocale.Value is Locale.Korean or Locale.Chinese ? Locale.English : GameService.Overlay.UserLocale.Value;
                var localeMissing = Items.Values.Where(item => item.Names[lang] == null)?.Select(e => e.Id);
                var missing = professionTypes.Except(Items.Keys).Concat(localeMissing);

                if (version > Version)
                {
                    Version = version;
                    missing = professionTypes;
                }

                if (missing.Count() > 0)
                {
                    var idSets = missing.ToList().ChunkBy(200);
                    saveRequired = saveRequired || idSets.Count > 0;

                    BuildsManager.Logger.Debug($"Fetch a total of {missing.Count()} {name} in {idSets.Count} sets.");

                    var apiSpecializations = await gw2ApiManager.Gw2ApiClient.V2.Specializations.AllAsync();
                    var apiLegends = missing.Contains(ProfessionType.Revenant) ? await gw2ApiManager.Gw2ApiClient.V2.Legends.AllAsync() : null;
                    var apiTraits = await gw2ApiManager.Gw2ApiClient.V2.Traits.AllAsync();
                    var apiSkills = await gw2ApiManager.Gw2ApiClient.V2.Skills.AllAsync();

                    foreach (var ids in idSets)
                    {
                        var items = await gw2ApiManager.Gw2ApiClient.V2.Professions.ManyAsync(ids);

                        foreach (var item in items)
                        {
                            bool exists = Items.Values.TryFind(e => $"{e.Id}" == item.Id, out Profession entryItem);
                            entryItem ??= new();

                            entryItem.Apply(item, apiSpecializations, apiLegends, apiTraits, apiSkills);

                            if (!exists)
                                Items.Add(entryItem.Id, entryItem);
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

    public class RaceDataEntry : MappedDataEntry<Races, Race>
    {
        public RaceDataEntry()
        {
            Items.Add(Races.None, new() { Name = "None", Id = Races.None });
        }

        public async override Task<bool> LoadAndUpdate(string name, Version version, string path, Gw2ApiManager gw2ApiManager)
        {
            try
            {
                bool saveRequired = false;
                RaceDataEntry loaded = null;

                BuildsManager.Logger.Debug($"Load {name}.json");

                if (!DataLoaded && File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    loaded = JsonConvert.DeserializeObject<RaceDataEntry>(json);
                    DataLoaded = true;
                }

                Items = loaded?.Items ?? Items;
                Version = loaded?.Version ?? Version;

                var raceIds = await gw2ApiManager.Gw2ApiClient.V2.Races.IdsAsync();

                var lang = GameService.Overlay.UserLocale.Value is Locale.Korean or Locale.Chinese ? Locale.English : GameService.Overlay.UserLocale.Value;
                var localeMissing = Items.Values.Where(item => item.Names[lang] == null)?.Select(e => $"{e.Id}");
                var missing = raceIds.Except(Items.Keys.Select(e => $"{e}")).Concat(localeMissing);

                if (version > Version)
                {
                    Version = version;
                    missing = raceIds;
                }

                if (missing.Count() > 0)
                {
                    var idSets = missing.ToList().ChunkBy(200);
                    saveRequired = saveRequired || idSets.Count > 0;

                    BuildsManager.Logger.Debug($"Fetch a total of {missing.Count()} {name} in {idSets.Count} sets.");
                    var apiSkills = await gw2ApiManager.Gw2ApiClient.V2.Skills.AllAsync();
                    var profession = await gw2ApiManager.Gw2ApiClient.V2.Professions.GetAsync(ProfessionType.Guardian);

                    foreach (var ids in idSets)
                    {
                        var items = await gw2ApiManager.Gw2ApiClient.V2.Races.ManyAsync(ids);

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

    public class PetDataEntry : MappedDataEntry<int, Pet>
    {
        public async override Task<bool> LoadAndUpdate(string name, Version version, string path, Gw2ApiManager gw2ApiManager)
        {
            try
            {
                bool saveRequired = false;
                PetDataEntry loaded = null;

                BuildsManager.Logger.Debug($"Load {name}.json");

                if (!DataLoaded && File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    loaded = JsonConvert.DeserializeObject<PetDataEntry>(json);
                    DataLoaded = true;
                }

                Items = loaded?.Items ?? Items;
                Version = loaded?.Version ?? Version;

                var petIds = await gw2ApiManager.Gw2ApiClient.V2.Pets.IdsAsync();

                var lang = GameService.Overlay.UserLocale.Value is Locale.Korean or Locale.Chinese ? Locale.English : GameService.Overlay.UserLocale.Value;
                var localeMissing = Items.Values.Where(item => item.Names[lang] == null)?.Select(e => e.Id);
                var missing = petIds.Except(Items.Keys).Concat(localeMissing);

                if (version > Version)
                {
                    Version = version;
                    missing = petIds;
                }

                if (missing.Count() > 0)
                {
                    var idSets = missing.ToList().ChunkBy(200);
                    saveRequired = saveRequired || idSets.Count > 0;

                    BuildsManager.Logger.Debug($"Fetch a total of {missing.Count()} {name} in {idSets.Count} sets.");

                    foreach (var ids in idSets)
                    {
                        var items = await gw2ApiManager.Gw2ApiClient.V2.Pets.ManyAsync(ids);

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

    public class Data : IDisposable
    {
        public static readonly Dictionary<int, int?> SkinDictionary = new()
                {
                    { 85105, 5013 }, //Axe
                    { 85017, 4997 }, //Dagger
                    { 85251, 4995 }, //Greatsword
                    { 85060, 5022 }, //Hammer
                    { 85267, 5005 }, //Mace
                    { 85360, 5018 }, //Shield
                    { 85250, 5020 }, //Sword
                    { 84899, 5164 }, //Spear
                    { 85052, 5000 }, //Shortbow
                    { 84888, 4998 }, //Longbow
                    { 85010, 5008 }, //Pistol
                    { 85262, 5021 }, //Rifle
                    { 85307, 5001 }, //Warhorn
                    { 85323, 4992 }, //Torch
                    { 85341, 4990 }, //Harpoon Gun
                    { 84872, 4994 }, //Focus
                    { 85117, 4989 }, //Scepter
                    { 85026, 5019 }, //Staff
                    { 85265, 5129 }, //Trident
                    
                    { 79895, 854 }, //Aqua Breather (Heavy)
                    { 85193, 818 }, // Helm (Heavy)
                    { 84875, 808 }, // Shoulder (Heavy)
                    { 85084, 807 }, // Coat (Heavy)
                    { 85140, 812 }, // Gloves (Heavy)
                    { 84887, 797 }, // Leggings (Heavy)
                    { 85055, 801 },  // Boots (Heavy)
                    
                    { 79838, 856 }, //Aqua Breather (Medium)
                    { 80701, 817 }, // Helm (Medium)
                    { 80825 , 805 }, // Shoulder (Medium)
                    { 84977, 806 }, // Coat (Medium)
                    { 85169, 811 }, // Gloves (Medium)
                    { 85264, 796 }, // Leggings (Medium)
                    { 80836, 799 }, // Boots (Medium)
                    
                    { 79873, 855 }, //Aqua Breather (Light)
                    { 85128, 819 }, // Helm (Light)
                    { 84918, 810 }, // Shoulder (Light)
                    { 85333, 809 }, // Coat (Light)
                    { 85070, 813 }, // Gloves (Light)
                    { 85362, 798 }, // Leggings (Light)
                    { 80815, 803 },  // Boots (Light)                    
   
                    { 94947, 10161 }, //Back
                    { 79980, null }, // Amulet
                    { 80002, null }, // Accessory
                    { 80058, null },  // Ring

                    //{ 0, null },  // Relic
                };

        private bool _isDisposed;
        private Paths _paths;
        private Gw2ApiManager _gw2ApiManager;
        private ContentsManager _contentsManager;

        public Data(ContentsManager contentsManager, Paths paths, Gw2ApiManager gw2ApiManager)
        {
            _paths = paths;
            _gw2ApiManager = gw2ApiManager;
            _contentsManager = contentsManager;
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
        public ItemMappedDataEntry<Relic> Relics { get; } = new();

        [EnumeratorMember]
        public ItemMappedDataEntry<Enhancement> Enhancements { get; } = new();

        [EnumeratorMember]
        public ItemMappedDataEntry<Nourishment> Nourishments { get; } = new();

        [EnumeratorMember]
        public ItemMappedDataEntry<Infusion> Infusions { get; } = new();

        [EnumeratorMember]
        public ItemMappedDataEntry<Enrichment> Enrichments { get; } = new();

        public IEnumerator<(string name, MappedDataEntry map)> GetEnumerator()
        {
            var propertiesToEnumerate = GetType()
                .GetProperties()
                .Where(property => property.GetCustomAttribute<EnumeratorMemberAttribute>() != null);

            foreach (var property in propertiesToEnumerate)
            {
                yield return (property.Name, property.GetValue(this) as MappedDataEntry);
            }
        }

        public async Task<bool> Load()
        {
            //try
            //{
            StaticVersion versions = await StaticHosting.GetStaticVersion();
            if (versions is null)
                return false;

            bool failed = false;

            foreach (var (name, map) in this)
            {
                string path = Path.Combine(_paths.ModuleDataPath, $"{name}.json");
                bool success = await map.LoadAndUpdate(name, versions[name], path, _gw2ApiManager);
                failed = failed || !success;
            }

            if (!failed)
                Loaded?.Invoke(this, EventArgs.Empty);

            return !failed;
            //}
            //catch
            //{

            //}

            return false;
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

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    sealed class EnumeratorMemberAttribute : System.Attribute
    {
    }

    public class DataOG : IDisposable
    {
        private readonly Logger _logger = Logger.GetLogger(typeof(DataOG));
        private readonly ContentsManager _contentsManager;
        private readonly Paths _paths;
        private readonly Gw2ApiManager _gw2ApiManager;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isDisposed;

        public DataOG(ContentsManager contentsManager, Paths paths, Gw2ApiManager gw2ApiManager)
        {
            _paths = paths;
            _gw2ApiManager = gw2ApiManager;
            _contentsManager = contentsManager;

            ByteIntMaps = new(_paths);
        }

        public Dictionary<int, OldSkillConnection> OldConnections { get; set; } = new();

        public ByteIntMapCollection ByteIntMaps { get; set; }

        public StatMapping StatMap { get; set; } = new();

        public Dictionary<int, SkillConnection> SkillConnections { get; set; } = new();

        public Dictionary<int, BaseSkill> MissingSkills { get; set; } = new();

        public Dictionary<int, Armor> Armors { get; private set; } = new();

        public Dictionary<int, Trinket> Trinkets { get; private set; } = new();

        public Dictionary<int, PvpAmulet> PvpAmulets { get; private set; } = new();

        public Dictionary<int, Relic> Relics { get; private set; } = new();

        public Dictionary<int, PowerCore> PowerCores { get; private set; } = new();

        public Dictionary<int, DataModels.Items.Weapon> Weapons { get; private set; } = new();

        public Dictionary<ProfessionType, Profession> Professions { get; private set; } = new();

        public Dictionary<Races, Race> Races { get; private set; } = new();

        public Dictionary<int, Stat> Stats { get; private set; } = new();

        public Dictionary<int, Pet> Pets { get; private set; } = new();

        public Dictionary<int, int> PaletteBySkills { get; private set; } = new();

        public List<KeyValuePair<int, int>> SkillsByPalette { get; private set; } = new();

        public bool IsLoaded => Armors.Count > 0 && Professions.Count > 0 && Stats.Count > 0 && Pets.Count > 0 && Races.Count > 0 && SkillConnections.Count > 0;

        public Dictionary<int, Trinket> Backs { get; private set; } = new();

        public Dictionary<int, Sigil> PvpSigils { get; private set; } = new();

        public Dictionary<int, Sigil> PveSigils { get; private set; } = new();

        public Dictionary<int, Rune> PvpRunes { get; private set; } = new();

        public Dictionary<int, Rune> PveRunes { get; private set; } = new();

        public Dictionary<int, DataModels.Items.Enhancement> Utilities { get; private set; } = new();

        public Dictionary<int, Nourishment> Nourishments { get; private set; } = new();

        public Dictionary<int, Infusion> Infusions { get; private set; } = new();

        public Dictionary<int, Enrichment> Enrichments { get; private set; } = new();

        public async Task<bool> Load()
        {
            try
            {
                _logger.Debug("Load and update mapped ids ...");
                bool byteIntMapLoaded = await ByteIntMaps.FetchAndLoad();
                if (!byteIntMapLoaded)
                    return byteIntMapLoaded;

                _logger.Debug("Loading local data...");

                StatMap = JsonConvert.DeserializeObject<StatMapping>(await new StreamReader(_contentsManager.GetFileStream(@"data\stats_map.json")).ReadToEndAsync());

                _logger.Debug("Item Map loaded!");

                await LoadMissingSkills();
                await LoadConnections();

                foreach (var prop in GetType().GetProperties())
                {
                    if (prop.Name is not nameof(SkillsByPalette) and not nameof(SkillConnections) and not nameof(ByteIntMaps) and not nameof(OldConnections) and not nameof(ByteIntMaps) and not nameof(StatMap) and not nameof(IsLoaded))
                    {
                        string path = $@"{_paths.ModuleDataPath}{prop.Name}.json";

                        if (File.Exists(path))
                        {
                            _logger.Debug($"Loading data for property {prop.Name} from '{$@"{_paths.ModuleDataPath}{prop.Name}.json"}'");
                            string json = await new StreamReader($@"{_paths.ModuleDataPath}{prop.Name}.json").ReadToEndAsync();
                            object data = JsonConvert.DeserializeObject(json, prop.PropertyType);
                            prop.SetValue(this, data);
                        }
                        else
                        {
                            _logger.Debug($"File for property {prop.Name} does not exist at '{$@"{_paths.ModuleDataPath}{prop.Name}.json"}'!");
                        }
                    }
                }

                SkillsByPalette = PaletteBySkills.ToList();

                _logger.Debug($"Import missing Skills");
                foreach (var baseSkill in MissingSkills)
                {
                    Skill skill = new(baseSkill.Value);

                    foreach (var prof in skill.Professions)
                    {
                        if (Professions.TryGetValue(prof, out Profession profession) && !profession.Skills.ContainsKey(skill.Id))
                        {
                            profession.Skills.Add(skill.Id, skill);
                        }
                    }
                }

                _logger.Debug("All data loaded!");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Debug("Failed to load data!");
                _logger.Debug($"{ex}");
            }

            return false;
        }

        public async Task LoadMissingSkills()
        {
            MissingSkills = JsonConvert.DeserializeObject<Dictionary<int, BaseSkill>>(await new StreamReader(_contentsManager.GetFileStream(@"data\missing_skills.json")).ReadToEndAsync());

        }

        public async Task LoadConnections()
        {
            SkillConnections = (Dictionary<int, SkillConnection>)JsonConvert.DeserializeObject(await new StreamReader(_contentsManager.GetFileStream(@"data\skill_connections.json")).ReadToEndAsync(), typeof(Dictionary<int, SkillConnection>));
        }

        public Skill GetSkillById(int id)
        {

            foreach (var profession in Professions)
            {
                if (profession.Value.Skills.TryGetValue(id, out Skill skill)) return skill;
            }

            return null;
        }

        public async Task Save()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            await Task.Delay(1000, _cancellationTokenSource.Token);
            if (_cancellationTokenSource.IsCancellationRequested) return;

            string json = JsonConvert.SerializeObject(SkillConnections, Formatting.Indented);
            File.WriteAllText($@"{_paths.ModuleDataPath}\SkillConnections.json", json);

            json = JsonConvert.SerializeObject(OldConnections, Formatting.Indented);
            File.WriteAllText($@"{_paths.ModuleDataPath}\OldConnections.json", json);
        }

        public void Dispose()
        {
            if (_isDisposed) return;

            _isDisposed = true;

            OldConnections?.Clear();
            StatMap?.Clear();
            SkillConnections?.Clear();
            MissingSkills?.Clear();
            Armors?.Clear();
            Trinkets?.Clear();
            PvpAmulets?.Clear();
            Relics?.Clear();
            PowerCores?.Clear();
            Weapons?.Clear();
            Professions?.Clear();
            Races?.Clear();
            Stats?.Clear();
            Pets?.Clear();
            PaletteBySkills?.Clear();
            SkillsByPalette?.Clear();
            Backs?.Clear();
            PvpSigils?.Clear();
            PveSigils?.Clear();
            PvpRunes?.Clear();
            PveRunes?.Clear();
            Utilities?.Clear();
            Nourishments?.Clear();
            Infusions?.Clear();
            Enrichments?.Clear();
        }
    }
}
