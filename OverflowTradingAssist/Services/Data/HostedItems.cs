using Blish_HUD.Modules.Managers;
using Kenedia.Modules.Core.Attributes;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.ContractResolver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Version = SemVer.Version;

namespace Kenedia.Modules.OverflowTradingAssist.DataEntries
{
    public class HostedItems<T>
    {
        public HostedItems() { }

        public HostedItems(Version version)
        {
            Version = version;
        }

        public List<T> Items { get; } = new();

        [JsonSemverVersion]
        public Version Version { get; set; } = new(0, 0, 0);

        public T this[int key]
        {
            get => Items.ElementAt(key) ?? default;
            set => Items[key] = value;
        }

        public void Add(T value)
        {
            Items.Add(value);
        }

        public void Remove(T value)
        {
            _ = Items.Remove(value);
        }

        public void Clear()
        {
            Items.Clear();
        }

        [JsonIgnore]
        public int Count => Items.Count;

        // Implement the IEnumerable<Version> interface
        public IEnumerator<T> GetEnumerator()
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
}
