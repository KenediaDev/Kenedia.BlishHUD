using Newtonsoft.Json;
using SemVer;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Kenedia.Modules.BuildsManager.Models
{
    public class StaticVersion
    {
        public StaticVersion()
        {

        }

        public StaticVersion(Version version)
        {
            Nourishments = version;
            Enhancements = version;
            PveRunes = version;
            PvpRunes = version;
            PveSigils = version;
            PvpSigils = version;
            Infusions = version;
            Enrichments = version;
            Trinkets = version;
            Backs = version;
            Weapons = version;
            Armors = version;
            PowerCores = version;
            Relics = version;
            PvpAmulets = version;
        }

        [JsonProperty("Nourishments")]
        private string NourishmentsVersion
        {
            get => Nourishments.ToString();
            set => Nourishments = new Version(value);
        }

        [JsonIgnore]
        public Version Nourishments { get; set; } = new(0, 0, 0);

        [JsonProperty("Enhancements")]
        private string EnhancementsVersion
        {
            get => Enhancements.ToString();
            set => Enhancements = new Version(value);
        }

        [JsonIgnore]
        public Version Enhancements { get; set; } = new(0, 0, 0);

        [JsonProperty("PveRunes")]
        private string PveRunesVersion
        {
            get => PveRunes.ToString();
            set => PveRunes = new Version(value);
        }

        [JsonIgnore]
        public Version PveRunes { get; set; } = new(0, 0, 0);

        [JsonProperty("PvpRunes")]
        private string PvpRunesVersion
        {
            get => PvpRunes.ToString();
            set => PvpRunes = new Version(value);
        }

        [JsonIgnore]
        public Version PvpRunes { get; set; } = new(0, 0, 0);

        [JsonProperty("PveSigils")]
        private string PveSigilsVersion
        {
            get => PveSigils.ToString();
            set => PveSigils = new Version(value);
        }

        [JsonIgnore]
        public Version PveSigils { get; set; } = new(0, 0, 0);

        [JsonProperty("PvpSigils")]
        private string PvpSigilsVersion
        {
            get => PvpSigils.ToString();
            set => PvpSigils = new Version(value);
        }

        [JsonIgnore]
        public Version PvpSigils { get; set; } = new(0, 0, 0);

        [JsonProperty("Infusions")]
        private string InfusionsVersion
        {
            get => Infusions.ToString();
            set => Infusions = new Version(value);
        }

        [JsonIgnore]
        public Version Infusions { get; set; } = new(0, 0, 0);

        [JsonProperty("Enrichments")]
        private string EnrichmentsVersion
        {
            get => Enrichments.ToString();
            set => Enrichments = new Version(value);
        }

        [JsonIgnore]
        public Version Enrichments { get; set; } = new(0, 0, 0);

        [JsonProperty("Trinkets")]
        private string TrinketsVersion
        {
            get => Trinkets.ToString();
            set => Trinkets = new Version(value);
        }

        [JsonIgnore]
        public Version Trinkets { get; set; } = new(0, 0, 0);

        [JsonProperty("Backs")]
        private string BacksVersion
        {
            get => Backs.ToString();
            set => Backs = new Version(value);
        }

        [JsonIgnore]
        public Version Backs { get; set; } = new(0, 0, 0);

        [JsonProperty("Weapons")]
        private string WeaponsVersion
        {
            get => Weapons.ToString();
            set => Weapons = new Version(value);
        }

        [JsonIgnore]
        public Version Weapons { get; set; } = new(0, 0, 0);

        [JsonProperty("Armors")]
        private string ArmorsVersion
        {
            get => Armors.ToString();
            set => Armors = new Version(value);
        }

        [JsonIgnore]
        public Version Armors { get; set; } = new(0, 0, 0);

        [JsonProperty("PowerCores")]
        private string PowerCoresVersion
        {
            get => PowerCores.ToString();
            set => PowerCores = new Version(value);
        }

        [JsonIgnore]
        public Version PowerCores { get; set; } = new(0, 0, 0);

        [JsonProperty("Relics")]
        private string RelicsVersion
        {
            get => Relics.ToString();
            set => Relics = new Version(value);
        }

        [JsonIgnore]
        public Version Relics { get; set; } = new(0, 0, 0);

        [JsonProperty("PvpAmulets")]
        private string PvpAmuletsVersion
        {
            get => PvpAmulets.ToString();
            set => PvpAmulets = new Version(value);
        }

        [JsonIgnore]
        public Version PvpAmulets { get; set; } = new(0, 0, 0);

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
        }
    }

    public class ItemMap
    {
        public ItemMap() { }

        public ItemMap(Version version)
        {
            Version = version;
        }

        public Dictionary<byte, int> Items { get; } = new();

        [JsonProperty("Version")]
        public string VersionString
        {
            get => Version.ToString();
            set => Version = new Version(value);
        }

        [JsonIgnore]
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
    }

    public class ItemMapCollection
    {
        public ItemMapCollection()
        {

        }

        public ItemMapCollection(Version version)
        {
            foreach (var itemMap in this)
            {
                itemMap.Value.Version = version;
            }
        }

        public ItemMap Nourishments { get; set; } = new();
        public ItemMap Enhancements { get; set; } = new();
        public ItemMap PveRunes { get; set; } = new();
        public ItemMap PvpRunes { get; set; } = new();
        public ItemMap PveSigils { get; set; } = new();
        public ItemMap PvpSigils { get; set; } = new();
        public ItemMap Infusions { get; set; } = new();
        public ItemMap Enrichments { get; set; } = new();
        public ItemMap Trinkets { get; set; } = new();
        public ItemMap Backs { get; set; } = new();
        public ItemMap Weapons { get; set; } = new();
        public ItemMap Armors { get; set; } = new();
        public ItemMap PowerCores { get; set; } = new();
        public ItemMap Relics { get; set; } = new();
        public ItemMap PvpAmulets { get; set; } = new();

        // Implement the IEnumerable<Version> interface
        public IEnumerator<KeyValuePair<string, ItemMap>> GetEnumerator()
        {
            yield return new KeyValuePair<string, ItemMap>(nameof(Nourishments), Nourishments);
            yield return new KeyValuePair<string, ItemMap>(nameof(Enhancements), Enhancements);
            yield return new KeyValuePair<string, ItemMap>(nameof(PveRunes), PveRunes);
            yield return new KeyValuePair<string, ItemMap>(nameof(PvpRunes), PvpRunes);
            yield return new KeyValuePair<string, ItemMap>(nameof(PveSigils), PveSigils);
            yield return new KeyValuePair<string, ItemMap>(nameof(PvpSigils), PvpSigils);
            yield return new KeyValuePair<string, ItemMap>(nameof(Infusions), Infusions);
            yield return new KeyValuePair<string, ItemMap>(nameof(Enrichments), Enrichments);
            yield return new KeyValuePair<string, ItemMap>(nameof(Trinkets), Trinkets);
            yield return new KeyValuePair<string, ItemMap>(nameof(Backs), Backs);
            yield return new KeyValuePair<string, ItemMap>(nameof(Weapons), Weapons);
            yield return new KeyValuePair<string, ItemMap>(nameof(Armors), Armors);
            yield return new KeyValuePair<string, ItemMap>(nameof(PowerCores), PowerCores);
            yield return new KeyValuePair<string, ItemMap>(nameof(Relics), Relics);
            yield return new KeyValuePair<string, ItemMap>(nameof(PvpAmulets), PvpAmulets);
        }
    }
}
