using Blish_HUD.Modules.Managers;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Threading;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Kenedia.Modules.BuildsManager.Models;

namespace Kenedia.Modules.BuildsManager.Services
{
    public class MappedDataEntry<Key, T> : BaseMappedDataEntry where T : IDataMember, new()
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

        public override async Task<bool> LoadAndUpdate(string name, ByteIntMap map, string path, Gw2ApiManager gw2ApiManager, CancellationToken cancellationToken)
        {
            return await Task.FromResult(DataLoaded);
        }
    }
}
