using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Structs;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Kenedia.Modules.Core.Services
{
    public static class SharedSettings
    {
        private static bool s_loaded = false;
        private static string s_path;
        private static RectangleDimensions s_windowOffset = new(31, 8, -8, -8);

        public static RectangleDimensions WindowOffset { get => s_windowOffset; set => SetValue(ref s_windowOffset, value); }

        public static bool Check { get; set; } = false;

        public static async Task Load(string p, bool force = false)
        {
            if (!s_loaded || force)
            {
                s_path = $@"{p}\shared_settings.json";

                if (Directory.Exists(p) && File.Exists(s_path))
                {
                    using var reader = File.OpenText(s_path);
                    string content = await reader.ReadToEndAsync();

                    var source = JsonConvert.DeserializeObject<SharedSettingsModel>(content);
                    MapToStatic(source);
                }

                s_loaded = true;
            }
        }

        public static T ObjectFromStaticClass<T>()
        {
            var o = (T)Activator.CreateInstance(typeof(T));

            var destinationProperties = o.GetType().GetProperties();
            var sourceProperties = typeof(SharedSettings).GetProperties();

            foreach (PropertyInfo prop in sourceProperties)
            {
                var destinationProp = destinationProperties.SingleOrDefault(p => p.Name.Equals(prop.Name, StringComparison.OrdinalIgnoreCase));
                destinationProp?.SetValue(o, prop.GetValue(null));
            }

            return o;
        }

        private static void MapToStatic<sT>(sT source)
        {
            var sourceProperties = source.GetType().GetProperties();

            //Key thing here is to specify we want the static properties only
            var destinationProperties = typeof(SharedSettings).GetProperties(BindingFlags.Public | BindingFlags.Static);

            foreach (var prop in sourceProperties)
            {
                //Find matching property by name
                var destinationProp = destinationProperties.Single(p => p.Name == prop.Name);

                //Set the static property value
                destinationProp?.SetValue(null, prop.GetValue(source));
            }
        }

        private static async void Save()
        {
            var sm = ObjectFromStaticClass<SharedSettingsModel>();
            string json = JsonConvert.SerializeObject(sm, Formatting.Indented);

            using var writer = new StreamWriter(s_path);
            await writer.WriteAsync(json);
        }

        private static void SetValue<T>(ref T prop, T value)
        {
            if (Equals(prop, value)) return;
            prop = value;

            if(s_loaded) Save();
        }
    }
}
