using Blish_HUD.Modules.Managers;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.Core.Converter;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Version = SemVer.Version;

namespace Kenedia.Modules.BuildsManager.Services
{
    [DataContract]
    public class BaseMappedDataEntry : IDisposable
    {
        protected bool DataLoaded = false;

        [DataMember]
        [JsonConverter(typeof(SemverVersionConverter))]
        public Version Version { get; set; } = new(0, 0, 0);

        [DataMember]
        public Dictionary<string, object> Items { get; set; } = [];

        public bool IsLoaded => DataLoaded;

        public ByteIntMap Map { get; set; }

        public virtual Task<bool> LoadCached(string name, string path, System.Threading.CancellationToken token)
        {
            return Task.FromResult(false);
        }

        // Conversion method
        public static BaseMappedDataEntry FromGeneric<Key, T>(MappedDataEntry<Key, T> entry) where T : IDataMember, new() where Key : notnull
        {
            return new()
            {
                Version = entry.Version,
                Items = entry.Items.ToDictionary(kvp => $"{kvp.Key}", kvp => (object)kvp.Value),
            };
        }

        public virtual Task<(bool, List<object>)> IsIncomplete(string name, ByteIntMap map, string path, Gw2ApiManager gw2ApiManager, System.Threading.CancellationToken token)
        {
            return Task.FromResult((false, new List<object>()));
        }

        public virtual Task<bool> Update(string name, ByteIntMap map, string path, Gw2ApiManager gw2ApiManager, System.Threading.CancellationToken token)
        {
            return Task.FromResult(false);
        }

        public void Dispose()
        {
            Items.Clear();
            Items = null;
        }
    }
}
