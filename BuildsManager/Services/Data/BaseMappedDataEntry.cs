using Blish_HUD.Modules.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Version = SemVer.Version;
using System.Runtime.Serialization;

namespace Kenedia.Modules.BuildsManager.Services
{
    [DataContract]
    public class BaseMappedDataEntry : IDisposable
    {
        protected bool DataLoaded = false;

        [DataMember]
        [JsonSemverVersion]
        public Version Version { get; set; } = new(0, 0, 0);

        [DataMember]
        public Dictionary<string, object> Items { get; set; } = new();

        public bool IsLoaded => DataLoaded;

        public ByteIntMap Map { get; set; }

        public virtual Task<bool> LoadAndUpdate(string name, Version version, string path, Gw2ApiManager gw2ApiManager, System.Threading.CancellationToken token)
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

        public void Dispose()
        {
            Items.Clear();
            Items = null;
        }
    }
}
