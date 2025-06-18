using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Structs;
using Kenedia.Modules.Core.Models;
using Newtonsoft.Json;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Diagnostics;
using System.ComponentModel;
using System;

namespace Kenedia.Modules.Core.Services
{
    [DataContract]
    public class SharedSettings : INotifyPropertyChanged
    {
        private bool _loaded = false;
        private string _path;
        private RectangleDimensions _windowOffset = new(8, 31, -8, -8);

        [DataMember]
        public RectangleDimensions WindowOffset { get => _windowOffset; set => Utility.Common.SetProperty(ref _windowOffset, value, OnPropertyChanged); }

        private void OnPropertyChanged(object sender, ValueChangedEventArgs<RectangleDimensions> e)
        {
            PropertyChanged?.Invoke(this, new(e.PropertyName));
            if (_loaded) Save();
        }

        public bool Check { get; set; } = false;

        public event PropertyChangedEventHandler PropertyChanged;

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

                        SharedSettings source = JsonConvert.DeserializeObject<SharedSettings>(content, SerializerSettings.Default);
                        WindowOffset = source.WindowOffset;

                        _loaded = true;
                    }
                }
            }
        }

        private async void Save()
        {
            string json = JsonConvert.SerializeObject(this, SerializerSettings.Default);

            if (await FileExtension.WaitForFileUnlock(_path, 2500))
            {
                using var writer = new StreamWriter(_path);
                await writer.WriteAsync(json);
            }
        }
    }
}
