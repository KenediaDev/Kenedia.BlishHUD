using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Version = SemVer.Version;
using Kenedia.Modules.BuildsManager.Services;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Attributes;
using Kenedia.Modules.Core.ContractResolver;

namespace Kenedia.Modules.BuildsManager.Models
{
    public class StaticVersion
    {
        public StaticVersion()
        {

        }

        public StaticVersion(Version version)
        {
            foreach (var property in this)
            {
                this[property.Key] = version;
            }
        }

        [JsonSemverVersion]
        public Version Nourishments { get; set; } = new(0, 0, 0);

        [JsonSemverVersion]
        public Version Enhancements { get; set; } = new(0, 0, 0);

        [JsonSemverVersion]
        public Version PveRunes { get; set; } = new(0, 0, 0);

        [JsonSemverVersion]
        public Version PvpRunes { get; set; } = new(0, 0, 0);

        [JsonSemverVersion]
        public Version PveSigils { get; set; } = new(0, 0, 0);

        [JsonSemverVersion]
        public Version PvpSigils { get; set; } = new(0, 0, 0);

        [JsonSemverVersion]
        public Version Infusions { get; set; } = new(0, 0, 0);

        [JsonSemverVersion]
        public Version Enrichments { get; set; } = new(0, 0, 0);

        [JsonSemverVersion]
        public Version Trinkets { get; set; } = new(0, 0, 0);

        [JsonSemverVersion]
        public Version Backs { get; set; } = new(0, 0, 0);

        [JsonSemverVersion]
        public Version Weapons { get; set; } = new(0, 0, 0);

        [JsonSemverVersion]
        public Version Armors { get; set; } = new(0, 0, 0);

        [JsonSemverVersion]
        public Version PowerCores { get; set; } = new(0, 0, 0);

        [JsonSemverVersion]
        public Version Relics { get; set; } = new(0, 0, 0);

        [JsonSemverVersion]
        public Version PvpAmulets { get; set; } = new(0, 0, 0);

        [JsonSemverVersion]
        public Version Stats { get; set; } = new(0, 0, 0);

        [JsonSemverVersion]
        public Version Professions { get; set; } = new(0, 0, 0);

        [JsonSemverVersion]
        public Version Pets { get; set; } = new(0, 0, 0);

        [JsonSemverVersion]
        public Version Races { get; set; } = new(0, 0, 0);

        // Implement the IEnumerable<Version> interface
        public IEnumerator<KeyValuePair<string, Version>> GetEnumerator()
        {
            yield return new KeyValuePair<string, Version>(nameof(Nourishments), Nourishments);
            yield return new KeyValuePair<string, Version>(nameof(Enhancements), Enhancements);
            yield return new KeyValuePair<string, Version>(nameof(PveRunes), PveRunes);
            yield return new KeyValuePair<string, Version>(nameof(PvpRunes), PvpRunes);
            yield return new KeyValuePair<string, Version>(nameof(PveSigils), PveSigils);
            yield return new KeyValuePair<string, Version>(nameof(PvpSigils), PvpSigils);
            yield return new KeyValuePair<string, Version>(nameof(Infusions), Infusions);
            yield return new KeyValuePair<string, Version>(nameof(Enrichments), Enrichments);
            yield return new KeyValuePair<string, Version>(nameof(Trinkets), Trinkets);
            yield return new KeyValuePair<string, Version>(nameof(Backs), Backs);
            yield return new KeyValuePair<string, Version>(nameof(Weapons), Weapons);
            yield return new KeyValuePair<string, Version>(nameof(Armors), Armors);
            yield return new KeyValuePair<string, Version>(nameof(PowerCores), PowerCores);
            yield return new KeyValuePair<string, Version>(nameof(Relics), Relics);
            yield return new KeyValuePair<string, Version>(nameof(PvpAmulets), PvpAmulets);
            yield return new KeyValuePair<string, Version>(nameof(Stats), Stats);
            yield return new KeyValuePair<string, Version>(nameof(Professions), Professions);
            yield return new KeyValuePair<string, Version>(nameof(Pets), Pets);
            yield return new KeyValuePair<string, Version>(nameof(Races), Races);
        }

        public void SaveToJson(string path)
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new SemverVersionContractResolver(),
                Formatting = Formatting.Indented
            };

            string json = JsonConvert.SerializeObject(this, settings);
            System.IO.File.WriteAllText(path, json);
        }

        public Version this[string propertyName]
        {
            get
            {
                var propertyInfo = GetType().GetProperty(propertyName);

                return propertyInfo != null
                    ? (Version)propertyInfo.GetValue(this)
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
    }

    public class ByteIntMap
    {
        public ByteIntMap() { }

        public ByteIntMap(Version version)
        {
            Version = version;
        }

        public Dictionary<byte, int> Items { get; } = new();

        [JsonSemverVersion]
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
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new SemverVersionContractResolver(),
                Formatting = Formatting.Indented
            };

            string json = JsonConvert.SerializeObject(this, settings);
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

        public ByteIntMap Relics { get; } = new();

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
            yield return new KeyValuePair<string, ByteIntMap>(nameof(Relics), Relics);
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
