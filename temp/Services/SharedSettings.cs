using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Structs;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Kenedia.Modules.Core.Services
{
    [DataContract]
    public class SharedSettings
    {
        private bool _loaded = false;
        private string _path;
        private RectangleDimensions _windowOffset = new(8, 31, -8, -8);

        [DataMember]
        public RectangleDimensions WindowOffset { get => _windowOffset; set => SetValue(ref _windowOffset, value); }

        public bool Check { get; set; } = false;

        public async Task Load(string p, bool force = false)
        {
            if (!_loaded || force)
            {
                _path = p;

                if (File.Exists(_path))
                {
                    if (await FileExtension.WaitForFileUnlock(_path, 2500))
                    {
                        using StreamReader reader = File.OpenText(_path);
                        string content = await reader.ReadToEndAsync();

                        SharedSettings source = JsonConvert.DeserializeObject<SharedSettings>(content);
                    }
                }

                _loaded = true;
            }
        }

        private async void Save()
        {
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);

            if (await FileExtension.WaitForFileUnlock(_path, 2500))
            {
                using var writer = new StreamWriter(_path);
                await writer.WriteAsync(json);
            }
        }

        private void SetValue<T>(ref T prop, T value)
        {
            if (Equals(prop, value)) return;
            prop = value;

            if (_loaded) Save();
        }
    }
}
