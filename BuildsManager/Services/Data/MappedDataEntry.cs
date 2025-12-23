using Blish_HUD.Modules.Managers;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Kenedia.Modules.BuildsManager.Services
{
    public static class MappedDataReflection
    {
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> s_cache = new();

        public static PropertyInfo[] GetMappedEntries(Type dataType)
        {
            return s_cache.GetOrAdd(dataType, static type =>
            {
                return type
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(p =>
                        p.PropertyType.IsGenericType &&
                        IsMappedDataEntry(p.PropertyType))
                    .ToArray();
            });
        }

        private static bool IsMappedDataEntry(Type type)
        {
            while (type != null)
            {
                if (type.IsGenericType &&
                    type.GetGenericTypeDefinition() == typeof(MappedDataEntry<,>))
                    return true;

                type = type.BaseType;
            }

            return false;
        }
    }

    public class MappedDataEntry<Key, T> : BaseMappedDataEntry where T : IDataMember, new()
    {
        protected List<Key>? Ids { get; set; } = null;

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
    }
}
