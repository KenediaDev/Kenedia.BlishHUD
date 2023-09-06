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

        [JsonIgnore]
        public Version Nourishments { get; set; } = new(0, 0, 0);

        [JsonProperty("Nourishments")]
        private string NourishmentsString
        {
            get => Nourishments.ToString();
            set => Nourishments = new Version(value);
        }

        [JsonIgnore]
        public Version Enhancements { get; set; } = new(0, 0, 0);

        [JsonProperty("Enhancements")]
        private string EnhancementsString
        {
            get => Enhancements.ToString();
            set => Enhancements = new Version(value);
        }

        [JsonIgnore]
        public Version PveRunes { get; set; } = new(0, 0, 0);

        [JsonProperty("PveRunes")]
        private string PveRunesString
        {
            get => PveRunes.ToString();
            set => PveRunes = new Version(value);
        }

        [JsonIgnore]
        public Version PvpRunes { get; set; } = new(0, 0, 0);

        [JsonProperty("PvpRunes")]
        private string PvpRunesString
        {
            get => PvpRunes.ToString();
            set => PvpRunes = new Version(value);
        }

        [JsonIgnore]
        public Version PveSigils { get; set; } = new(0, 0, 0);

        [JsonProperty("PveSigils")]
        private string PveSigilsString
        {
            get => PveSigils.ToString();
            set => PveSigils = new Version(value);
        }

        [JsonIgnore]
        public Version PvpSigils { get; set; } = new(0, 0, 0);

        [JsonProperty("PvpSigils")]
        private string PvpSigilsString
        {
            get => PvpSigils.ToString();
            set => PvpSigils = new Version(value);
        }

        [JsonIgnore]
        public Version Infusions { get; set; } = new(0, 0, 0);

        [JsonProperty("Infusions")]
        private string InfusionsString
        {
            get => Infusions.ToString();
            set => Infusions = new Version(value);
        }

        [JsonIgnore]
        public Version Enrichments { get; set; } = new(0, 0, 0);

        [JsonProperty("Enrichments")]
        private string EnrichmentsString
        {
            get => Enrichments.ToString();
            set => Enrichments = new Version(value);
        }

        [JsonIgnore]
        public Version Trinkets { get; set; } = new(0, 0, 0);

        [JsonProperty("Trinkets")]
        private string TrinketsString
        {
            get => Trinkets.ToString();
            set => Trinkets = new Version(value);
        }

        [JsonIgnore]
        public Version Backs { get; set; } = new(0, 0, 0);

        [JsonProperty("Backs")]
        private string BacksString
        {
            get => Backs.ToString();
            set => Backs = new Version(value);
        }

        [JsonIgnore]
        public Version Weapons { get; set; } = new(0, 0, 0);

        [JsonProperty("Weapons")]
        private string WeaponsString
        {
            get => Weapons.ToString();
            set => Weapons = new Version(value);
        }

        [JsonIgnore]
        public Version Armors { get; set; } = new(0, 0, 0);

        [JsonProperty("Armors")]
        private string ArmorsString
        {
            get => Armors.ToString();
            set => Armors = new Version(value);
        }

        [JsonIgnore]
        public Version PowerCores { get; set; } = new(0, 0, 0);

        [JsonProperty("PowerCores")]
        private string PowerCoresString
        {
            get => PowerCores.ToString();
            set => PowerCores = new Version(value);
        }

        [JsonIgnore]
        public Version Relics { get; set; } = new(0, 0, 0);

        [JsonProperty("Relics")]
        private string RelicsString
        {
            get => Relics.ToString();
            set => Relics = new Version(value);
        }

        [JsonIgnore]
        public Version PvpAmulets { get; set; } = new(0, 0, 0);

        [JsonProperty("PvpAmulets")]
        private string PvpAmuletsString
        {
            get => PvpAmulets.ToString();
            set => PvpAmulets = new Version(value);
        }

        [JsonIgnore]
        public Version Stats { get; set; } = new(0, 0, 0);

        [JsonProperty("Stats")]
        private string StatsString
        {
            get => Stats.ToString();
            set => Stats = new Version(value);
        }

        [JsonIgnore]
        public Version Professions { get; set; } = new(0, 0, 0);

        [JsonProperty("Professions")]
        private string ProfessionsString
        {
            get => Professions.ToString();
            set => Professions = new Version(value);
        }

        [JsonIgnore]
        public Version Pets { get; set; } = new(0, 0, 0);

        [JsonProperty("Pets")]
        private string PetsString
        {
            get => Pets.ToString();
            set => Pets = new Version(value);
        }

        [JsonIgnore]
        public Version Races { get; set; } = new(0, 0, 0);

        [JsonProperty("Races")]
        private string RacesString
        {
            get => Races.ToString();
            set => Races = new Version(value);
        }

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
            string json = JsonConvert.SerializeObject(this, SerializerSettings.Default);
            File.WriteAllText(path, json);
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

        [JsonIgnore]
        public Version Version { get; set; } = new(0, 0, 0);

        [JsonProperty("Version")]
        private string VersionString
        {
            get => Version.ToString();
            set => Version = new Version(value);
        }

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
