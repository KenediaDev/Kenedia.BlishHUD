using Blish_HUD.Modules.Managers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using Version = SemVer.Version;
using System.Runtime.Serialization;
using System.Threading;
using System.Linq;
using Kenedia.Modules.Core.Attributes;

namespace Kenedia.Modules.Core.Models
{
    [DataContract]
    public class BaseDataEntry
    {
        protected bool DataLoaded = false;

        [DataMember]
        [JsonSemverVersion]
        public Version Version { get; set; } = new(0, 0, 0);

        [DataMember]
        public Dictionary<string, object> Items { get; set; } = [];

        public bool IsLoaded => DataLoaded;

        public virtual Task<bool> LoadAndUpdate(string name, Version version, string path, Gw2ApiManager gw2ApiManager, System.Threading.CancellationToken token)
        {
            return Task.FromResult(false);
        }

        // Conversion method
        public static BaseDataEntry FromGeneric<Key, T>(DataEntry<Key, T> entry)
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

    public class DataEntry<Key, T> : BaseDataEntry
    {
        [DataMember]
        public new Dictionary<Key, T> Items { get; protected set; } = [];

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

        public override async Task<bool> LoadAndUpdate(string name, Version version, string path, Gw2ApiManager gw2ApiManager, CancellationToken cancellationToken)
        {
            return await Task.FromResult(DataLoaded);
        }
    }

    public class DataEntry<T> : BaseDataEntry
    {
        [DataMember]
        public new List<T> Items { get; protected set; } = [];

        public T this[int key]
        {
            get => Items.ElementAt(key) ?? default;
            set => Items[key] = value;
        }

        [JsonIgnore]
        public int Count => Items.Count;

        public void Add(T value)
        {
            Items.Add(value);
        }

        public void Remove(T item)
        {
            _ = Items.Remove(item);
        }

        public void Clear()
        {
            Items.Clear();
        }

        public override async Task<bool> LoadAndUpdate(string name, Version version, string path, Gw2ApiManager gw2ApiManager, CancellationToken cancellationToken)
        {
            return await Task.FromResult(DataLoaded);
        }
    }
}
