using Kenedia.Modules.Core.Converter;
using Kenedia.Modules.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using static Kenedia.Modules.BuildsManager.DataModels.Stats.Stat;
using Version = SemVer.Version;

namespace Kenedia.Modules.BuildsManager.Models
{
    public class StaticStats
    {
        public List<StatTextureMapInfo> TextureMapInfo { get; set; }

        public string ImageUrl { get; set; }

        [JsonConverter(typeof(SemverVersionConverter))]
        public Version Version { get; set; } = new(0, 0, 0);
    }

    public class StaticVersion
    {
        private Version _version = new(0, 0, 0);

        public StaticVersion()
        {

        }

        public StaticVersion(Version version)
        {
            foreach (var property in this)
            {
                this[property.Key].Version = version;
            }
        }

        [JsonIgnore]
        public Version Version 
        {
            set
            {
                if(value is Version version && version > _version)
                {
                    _version = version;

                    foreach (var property in this)
                    {
                        this[property.Key].Version = version;
                    }
                }
            }
        }

        public ByteIntMap Armors { get; set; } = new(new(0, 0, 0));

        public ByteIntMap Backs { get; set; } = new(new(0, 0, 0));

        public ByteIntMap Enhancements { get; set; } = new(new(0, 0, 0));

        public ByteIntMap Enrichments { get; set; } = new(new(0, 0, 0));

        public ByteIntMap Infusions { get; set; } = new(new(0, 0, 0));

        public ByteIntMap Nourishments { get; set; } = new(new(0, 0, 0));

        public ByteIntMap Pets { get; set; } = new(new(0, 0, 0));

        public ByteIntMap PowerCores { get; set; } = new(new(0, 0, 0));

        public ByteIntMap Professions { get; set; } = new(new(0, 0, 0));

        public ByteIntMap PveRunes { get; set; } = new(new(0, 0, 0));

        public ByteIntMap PvpRunes { get; set; } = new(new(0, 0, 0));

        public ByteIntMap PveSigils { get; set; } = new(new(0, 0, 0));

        public ByteIntMap PvpSigils { get; set; } = new(new(0, 0, 0));

        public ByteIntMap PvpAmulets { get; set; } = new(new(0, 0, 0));

        public ByteIntMap Races { get; set; } = new(new(0, 0, 0));

        public ByteIntMap PveRelics { get; set; } = new(new(0, 0, 0));

        public ByteIntMap PvpRelics { get; set; } = new(new(0, 0, 0));

        public ByteIntMap Stats { get; set; } = new(new(0, 0, 0));

        public ByteIntMap Trinkets { get; set; } = new(new(0, 0, 0));
        
        public ByteIntMap Weapons { get; set; } = new(new(0, 0, 0));

        // Implement the IEnumerable<Version> interface
        public IEnumerator<KeyValuePair<string, ByteIntMap>> GetEnumerator()
        {
            yield return new (nameof(Nourishments), Nourishments);
            yield return new (nameof(Enhancements), Enhancements);
            yield return new (nameof(PveRunes), PveRunes);
            yield return new (nameof(PvpRunes), PvpRunes);
            yield return new (nameof(PveSigils), PveSigils);
            yield return new (nameof(PvpSigils), PvpSigils);
            yield return new (nameof(Infusions), Infusions);
            yield return new (nameof(Enrichments), Enrichments);
            yield return new (nameof(Trinkets), Trinkets);
            yield return new (nameof(Backs), Backs);
            yield return new (nameof(Weapons), Weapons);
            yield return new (nameof(Armors), Armors);
            yield return new (nameof(PowerCores), PowerCores);
            yield return new (nameof(PveRelics), PveRelics);
            yield return new (nameof(PvpRelics), PvpRelics);
            yield return new (nameof(PvpAmulets), PvpAmulets);
            yield return new (nameof(Stats), Stats);
            yield return new (nameof(Professions), Professions);
            yield return new (nameof(Pets), Pets);
            yield return new (nameof(Races), Races);
        }

        public void Save(string path)
        {
            string json = JsonConvert.SerializeObject(this, SerializerSettings.Default);
            File.WriteAllText(path, json);
        }

        public static StaticVersion LoadFromFile(Version version, string path)
        {
            var staticVersion = JsonConvert.DeserializeObject<StaticVersion>(File.ReadAllText(path), SerializerSettings.Default) ?? new(version);
            staticVersion.Version = version;

            foreach (var property in staticVersion)
            {
                staticVersion[property.Key].Name = property.Key;
            }

            return staticVersion;   
        }

        public static StaticVersion LoadFromFile(string path)
        {
            var staticVersion = JsonConvert.DeserializeObject<StaticVersion>(File.ReadAllText(path), SerializerSettings.Default) ?? new();

            foreach (var property in staticVersion)
            {
                staticVersion[property.Key].Name = property.Key;
            }

            return staticVersion;   
        }

        public ByteIntMap this[string propertyName]
        {
            get
            {
                var propertyInfo = GetType().GetProperty(propertyName);

                return propertyInfo != null
                    ? (ByteIntMap)propertyInfo.GetValue(this)
                    : throw new ArgumentException($"Property '{propertyName}' not found in StaticVersion class.");
            }
            set
            {
                var propertyInfo = GetType().GetProperty(propertyName);
                if (propertyInfo != null)
                {
                    propertyInfo.SetValue(this, value);
                }
                else
                {
                    throw new ArgumentException($"Property '{propertyName}' not found in StaticVersion class.");
                }
            }
        }

        public Dictionary<string, Version> GetVersions()
        {
            var versions = new Dictionary<string, Version>();

            foreach (var property in this)
            {
                versions.Add(property.Key, new Version(property.Value.Version.ToString()));
            }

            return versions;
        }
    }

    public class ByteIntMap
    {
        public ByteIntMap() { }

        public ByteIntMap(Version version)
        {
            Version = version;
        }

        public Dictionary<byte, int> Items { get; } = [];

        public Dictionary<byte, int> Ignored { get; } = [];

        [JsonIgnore]
        public string Name { get; set; }

        [JsonConverter(typeof(SemverVersionConverter))]
        public Version Version { get; set; } = new(0, 0, 0);

        public int this[byte key]
        {
            get => Items[key];
            set => Items[key] = value;
        }

        public void Add(byte key, int value)
        {
            Items.Add(key, value);
        }

        public void Remove(byte key)
        {
            _ = Items.Remove(key);
        }

        public void Clear()
        {
            Items.Clear();
        }

        public bool ContainsKey(byte key)
        {
            return Items.ContainsKey(key);
        }

        public bool TryGetValue(byte key, out int value)
        {
            return Items.TryGetValue(key, out value);
        }

        [JsonIgnore]
        public int Count => Items.Count;

        [JsonIgnore]
        public IEnumerable<byte> Keys => Items.Keys;

        [JsonIgnore]
        public IEnumerable<int> Values => Items.Values;

        // Implement the IEnumerable<Version> interface
        public IEnumerator<KeyValuePair<byte, int>> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        public void SaveToJson(string path)
        {
            string json = JsonConvert.SerializeObject(this, SerializerSettings.Default);
            File.WriteAllText(path, json);
        }
    }

    public class ByteIntMapCollection
    {
        private readonly Paths _paths;

        public ByteIntMapCollection(Paths paths)
        {
            _paths = paths;
        }

        public ByteIntMapCollection(Version version, Paths paths) : this(paths)
        {
            foreach (var itemMap in this)
            {
                itemMap.Value.Version = version;
            }
        }

        public ByteIntMap Nourishments { get; } = new();

        public ByteIntMap Enhancements { get; } = new();

        public ByteIntMap PveRunes { get; } = new();

        public ByteIntMap PvpRunes { get; } = new();

        public ByteIntMap PveSigils { get; } = new();

        public ByteIntMap PvpSigils { get; } = new();

        public ByteIntMap Infusions { get; } = new();

        public ByteIntMap Enrichments { get; } = new();

        public ByteIntMap Trinkets { get; } = new();

        public ByteIntMap Backs { get; } = new();

        public ByteIntMap Weapons { get; } = new();

        public ByteIntMap Armors { get; } = new();

        public ByteIntMap PowerCores { get; } = new();

        public ByteIntMap PveRelics { get; } = new();

        public ByteIntMap PvpRelics { get; } = new();

        public ByteIntMap PvpAmulets { get; } = new();

        public ByteIntMap Stats { get; } = new();

        // Implement the IEnumerable<Version> interface
        public IEnumerator<KeyValuePair<string, ByteIntMap>> GetEnumerator()
        {
            yield return new KeyValuePair<string, ByteIntMap>(nameof(Nourishments), Nourishments);
            yield return new KeyValuePair<string, ByteIntMap>(nameof(Enhancements), Enhancements);
            yield return new KeyValuePair<string, ByteIntMap>(nameof(PveRunes), PveRunes);
            yield return new KeyValuePair<string, ByteIntMap>(nameof(PvpRunes), PvpRunes);
            yield return new KeyValuePair<string, ByteIntMap>(nameof(PveSigils), PveSigils);
            yield return new KeyValuePair<string, ByteIntMap>(nameof(PvpSigils), PvpSigils);
            yield return new KeyValuePair<string, ByteIntMap>(nameof(Infusions), Infusions);
            yield return new KeyValuePair<string, ByteIntMap>(nameof(Enrichments), Enrichments);
            yield return new KeyValuePair<string, ByteIntMap>(nameof(Trinkets), Trinkets);
            yield return new KeyValuePair<string, ByteIntMap>(nameof(Backs), Backs);
            yield return new KeyValuePair<string, ByteIntMap>(nameof(Weapons), Weapons);
            yield return new KeyValuePair<string, ByteIntMap>(nameof(Armors), Armors);
            yield return new KeyValuePair<string, ByteIntMap>(nameof(PowerCores), PowerCores);
            yield return new KeyValuePair<string, ByteIntMap>(nameof(PveRelics), PveRelics);
            yield return new KeyValuePair<string, ByteIntMap>(nameof(PvpRelics), PvpRelics);
            yield return new KeyValuePair<string, ByteIntMap>(nameof(PvpAmulets), PvpAmulets);
            yield return new KeyValuePair<string, ByteIntMap>(nameof(Stats), Stats);
        }

        public void Save()
        {
            try
            {
                foreach (var itemMap in this)
                {
                    string filePath = Path.Combine(_paths.ItemMapPath, $"{itemMap.Key}.json");

                    itemMap.Value?.SaveToJson(filePath);
                }
            }
            catch { }
        }
    }
}
