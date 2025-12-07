using Blish_HUD;
using Kenedia.Modules.Core.Converter;
using Newtonsoft.Json;
using SemVer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Version = SemVer.Version;

namespace Kenedia.Modules.Core.Models
{
    public interface IDataDictionary
    {
        Version Version { get; set; }

        bool IsOutdated(Version version);

        string FilePath { get; }

        string FileName { get; }

        Task Update(Version version);

        Task<bool> Load();

        Task Save();
    }
    
    public class DataDictionaryDto<TKey, TValue>
    {
        [JsonConverter(typeof(SemverVersionConverter))]
        public Version Version { get; set; } = new Version("0.0.0");

        public Dictionary<TKey, TValue> Data { get; set; }
    }

    public class DataDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IDataDictionary
    {
        public DataDictionary()
        {
            
        }

        public DataDictionary(string filePath)
        {
            FilePath = filePath;
        }
        // Allow lambda expression to be passed for async update operations
        public DataDictionary(string filePath, Func<Task> update) : this(filePath)
        {
            OnUpdate = update;
        }

        [JsonConverter(typeof(SemverVersionConverter))]
        public Version Version { get; set; } = new Version("0.0.0");

        public string FilePath { get; }

        public string FileName => Path.GetFileNameWithoutExtension(FilePath);

        public Func<Task> OnUpdate { get; set; }

        public bool IsOutdated(Version version)
        {
            return Version < version;
        }

        public virtual async Task<bool> Load()
        {
            if (!File.Exists(FilePath))
                return false;

            using var stream = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new StreamReader(stream);
            string content = await reader.ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(content))
                return false;

            var dto = JsonConvert.DeserializeObject<DataDictionaryDto<TKey, TValue>>(
                content,
                SerializerSettings.Default
            );

            if (dto == null)
                return false;

            Clear();

            foreach (var kv in dto.Data)
                Add(kv.Key, kv.Value);

            Version = dto.Version;
            return true;
        }

        public virtual async Task Save()
        {
            var dto = new DataDictionaryDto<TKey, TValue>
            {
                Version = Version,
                Data = new Dictionary<TKey, TValue>(this)
            };

            string content = JsonConvert.SerializeObject(dto, SerializerSettings.Default);

            using var stream = new FileStream(FilePath, FileMode.Create, FileAccess.Write, FileShare.None);
            using var writer = new StreamWriter(stream, Encoding.UTF8);
            await writer.WriteAsync(content);
        }

        public virtual async Task Update(Version? version = null)
        {
            try
            {
                if (OnUpdate != null)
                {
                    await OnUpdate();
                }

                if (version != null)
                {
                    Version = version;
                }
            }
            catch (Exception ex)
            {
                Logger.GetLogger<DataDictionary<TKey, TValue>>().Warn($"{ex}");
            }
        }
    }
}
