using Blish_HUD.Modules.Managers;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Services;
using Kenedia.Modules.FashionManager.Utility;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using File = System.IO.File;

namespace Kenedia.Modules.FashionManager.Services
{
    public abstract class DataEntry<TId, TValue> : IEnumerable<TValue>
    {
        protected Dictionary<TId, TValue> Dictionary = [];

        protected virtual string FilePath { get; set; }

        protected DataEntry(Gw2ApiManager gw2ApiManager, Paths paths)
        {
            Gw2ApiManager = gw2ApiManager;
            Paths = paths;
        }

        public Gw2ApiManager Gw2ApiManager { get; }

        public Paths Paths { get; }

        public TValue? this[TId id]
        {
            get => TryGetValue(id, out var value) ? value : default;
            set => Dictionary[id] = value;
        }

        public bool TryGetValue(TId id, out TValue value)
        {
            return Dictionary.TryGetValue(id, out value);
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            return Dictionary.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public abstract Task<bool> LoadDataFromWebAsync();

        public abstract Task<bool> LoadDataAsync();

        public virtual Task<bool> Load()
        {
            if (!File.Exists(FilePath))
            {
                return LoadDataFromWebAsync();
            }

            return LoadDataAsync();
        }
    }

    public class ColorDataEntry : DataEntry<int, Color>
    {
        public ColorDataEntry(Gw2ApiManager gw2ApiManager, Paths paths) : base(gw2ApiManager, paths)
        {
            FilePath = Path.Combine(Paths.DataPath, "colors.json");
        }

        public override async Task<bool> LoadDataAsync()
        {
            if (!File.Exists(FilePath))
            {
                return false;
            }

            string json = File.ReadAllText(Path.Combine(Paths.DataPath, "colors.json"));
            Dictionary = JsonConvert.DeserializeObject<Dictionary<int, Color>>(json, SerializerSettings.Default);

            return await Task.FromResult(true);
        }

        public override async Task<bool> LoadDataFromWebAsync()
        {
            var allids = await Gw2ApiManager.Gw2ApiClient.V2.Colors.IdsAsync();

            Debug.WriteLine($"Loading {allids.Count} Colors");

            var colors = new List<Color>();
            var idBatches = allids.ToList().ChunkBy(200);

            foreach (var ids in idBatches)
            {
                var colorChunk = await Gw2ApiManager.Gw2ApiClient.V2.Colors.ManyAsync(ids);

                Debug.WriteLine($"Loading {ids.Count} Colors ({colors.Count} / {allids.Count})");

                if (colorChunk is not null)
                {
                    colors.AddRange(colorChunk);
                }
            }

            Debug.WriteLine($"{colors.Count} / {allids.Count} Colors loaded.");
            Dictionary = colors.ToDictionary(c => c.Id, c => c);

            string json = JsonConvert.SerializeObject(Dictionary, SerializerSettings.Default);
            File.WriteAllText(Path.Combine(Paths.DataPath, "colors.json"), json);

            return true;
        }
    }

    public class Data
    {
        public Data(Gw2ApiManager gw2ApiManager, StaticHosting staticHosting, Paths paths)
        {
            Gw2ApiManager = gw2ApiManager;
            StaticHosting = staticHosting;
            Paths = paths;

            Colors = new ColorDataEntry(gw2ApiManager, paths);
        }

        public Gw2ApiManager Gw2ApiManager { get; }

        public StaticHosting StaticHosting { get; }

        public Paths Paths { get; }

        public DataEntry<int, Color> Colors { get; }

        public async Task<bool> LoadDataFromGw2ApiAsync()
        {
            await Colors.Load();

            return false;
        }
    }
}
