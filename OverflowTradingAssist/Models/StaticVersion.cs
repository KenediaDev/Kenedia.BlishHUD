using Kenedia.Modules.Core.Attributes;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.ContractResolver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Version = SemVer.Version;

namespace Kenedia.Modules.OverflowTradingAssist.Models
{

    public class StaticVersion
    {
        public StaticVersion()
        {

        }

        public StaticVersion(Version version)
        {
            foreach (var property in this)
            {
                this[property.Key] = version;
            }
        }

        [JsonSemverVersion]
        public Version Items { get; set; } = new(0, 0, 0);

        public IEnumerator<KeyValuePair<string, Version>> GetEnumerator()
        {
            yield return new KeyValuePair<string, Version>(nameof(Items), Items);
        }

        public void SaveToJson(string path)
        {
            string json = JsonConvert.SerializeObject(this, SerializerSettings.SemverSerializer);
            System.IO.File.WriteAllText(path, json);
        }

        public Version this[string propertyName]
        {
            get
            {
                var propertyInfo = GetType().GetProperty(propertyName);

                return propertyInfo != null
                    ? (Version)propertyInfo.GetValue(this)
                    : throw new ArgumentException($"Property '{propertyName}' not found in StaticVersion class.");
            }
            set
            {
                var propertyInfo = GetType().GetProperty(propertyName);
                if (propertyInfo != null)
                {
                    propertyInfo.SetValue(this, value);
                }
                else
                {
                    throw new ArgumentException($"Property '{propertyName}' not found in StaticVersion class.");
                }
            }
        }
    }

}
